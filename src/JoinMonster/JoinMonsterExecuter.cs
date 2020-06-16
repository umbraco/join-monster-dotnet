using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using JoinMonster.Configs;
using JoinMonster.Data;
using JoinMonster.Language;
using NestHydration;

namespace JoinMonster
{
    /// <summary>
    /// The entry class for JoinMonster.
    /// </summary>
    public class JoinMonsterExecuter
    {
        private readonly QueryToSqlConverter _converter;
        private readonly SqlCompiler _compiler;
        private readonly Hydrator _hydrator;
        private readonly ObjectShaper _objectShaper;
        private readonly ArrayToConnectionConverter _arrayToConnectionConverter;

        /// <summary>
        /// Creates a new instance of <see cref="JoinMonsterExecuter"/>.
        /// </summary>
        /// <param name="converter">The <see cref="QueryToSqlConverter"/>.</param>
        /// <param name="compiler">The <see cref="SqlCompiler"/>.</param>
        /// <param name="hydrator">The <see cref="Hydrator"/>.</param>
        public JoinMonsterExecuter(QueryToSqlConverter converter, SqlCompiler compiler, Hydrator hydrator)
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            _hydrator = hydrator ?? throw new ArgumentNullException(nameof(hydrator));

            _objectShaper = new ObjectShaper(new SqlAstValidator());
            _arrayToConnectionConverter = new ArrayToConnectionConverter();
        }

        /// <summary>
        /// Takes a <see cref="IResolveFieldContext"/> and returns a hydrated object with the data.
        /// </summary>
        /// <param name="context">The <see cref="IResolveFieldContext"/>.</param>
        /// <param name="databaseCall">A <see cref="DatabaseCallDelegate"/> that is passed the compiled SQL and calls the database and returns a <see cref="IDataReader"/>.</param>
        /// <returns>The correctly nested data from the database.</returns>
        /// <exception cref="ArgumentNullException">If <c>context</c> or <c>databaseCall</c> is null.</exception>
        public async Task<object?> ExecuteAsync(IResolveFieldContext context, DatabaseCallDelegate databaseCall)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (databaseCall == null) throw new ArgumentNullException(nameof(databaseCall));

            var sqlAst = _converter.Convert(context);
            var sql = _compiler.Compile(sqlAst, context);

            // TODO: Run batches and map result
            using var reader = await databaseCall(sql, new Dictionary<string, object>()).ConfigureAwait(false);

            var data = new List<Dictionary<string, object?>>();
            while (reader.Read())
            {
                var item = new Dictionary<string, object?>();
                for (var i = 0; i < reader.FieldCount; ++i)
                {
                    item[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }

                data.Add(item);
            }

            var objectShape = _objectShaper.DefineObjectShape(sqlAst);
#pragma warning disable 8620
            var nested = _hydrator.Nest(data, objectShape);
            var result = _arrayToConnectionConverter.Convert(nested, sqlAst, context);
#pragma warning restore 8620

            return sqlAst.GrabMany
                ? result
                : result.FirstOrDefault();
        }

    }
}

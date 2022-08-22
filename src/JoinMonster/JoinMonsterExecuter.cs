using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
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
        private readonly ISqlCompiler _compiler;
        private readonly IBatchPlanner _batchPlanner;
        private readonly Hydrator _hydrator;
        private readonly ObjectShaper _objectShaper;
        private readonly ArrayToConnectionConverter _arrayToConnectionConverter;

        /// <summary>
        /// Creates a new instance of <see cref="JoinMonsterExecuter"/>.
        /// </summary>
        /// <param name="converter">The <see cref="QueryToSqlConverter"/>.</param>
        /// <param name="compiler">The <see cref="ISqlCompiler"/>.</param>
        /// <param name="batchPlanner">The <see cref="IBatchPlanner"/>.</param>
        /// <param name="hydrator">The <see cref="Hydrator"/>.</param>
        public JoinMonsterExecuter(QueryToSqlConverter converter, ISqlCompiler compiler, IBatchPlanner batchPlanner, Hydrator hydrator)
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            _batchPlanner = batchPlanner ?? throw new ArgumentNullException(nameof(batchPlanner));
            _hydrator = hydrator ?? throw new ArgumentNullException(nameof(hydrator));

            _objectShaper = new ObjectShaper(new SqlAstValidator());
            _arrayToConnectionConverter = new ArrayToConnectionConverter();
        }

        /// <summary>
        /// Takes a <see cref="IResolveFieldContext"/> and returns a hydrated object with the data.
        /// </summary>
        /// <param name="context">The <see cref="IResolveFieldContext"/>.</param>
        /// <param name="databaseCall">A <see cref="DatabaseCallDelegate"/> that is passed the compiled SQL and calls the database and returns a <see cref="IDataReader"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The correctly nested data from the database.</returns>
        /// <exception cref="ArgumentNullException">If <c>context</c> or <c>databaseCall</c> is null.</exception>
        public async Task<object?> ExecuteAsync(IResolveFieldContext context, DatabaseCallDelegate databaseCall,
            CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (databaseCall == null) throw new ArgumentNullException(nameof(databaseCall));

            var sqlAst = _converter.Convert(context);
            var sqlResult = _compiler.Compile(sqlAst, context);

            var data = new List<Dictionary<string, object?>>();

            using (var reader = await databaseCall(sqlResult.Sql, sqlResult.Parameters).ConfigureAwait(false))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    var item = new Dictionary<string, object?>();

                    for (var i = 0; i < reader.FieldCount; ++i)
                    {
                        var value = await reader.IsDBNullAsync(i, cancellationToken)
                            ? null
                            : await reader.GetFieldValueAsync<object>(i, cancellationToken);

                        item[reader.GetName(i)] = value;
                    }

                    data.Add(item);
                }
            }

            var objectShape = _objectShaper.DefineObjectShape(sqlAst);
#pragma warning disable 8620
            var nested = _hydrator.Nest(data, objectShape);
            var result = _arrayToConnectionConverter.Convert(nested, sqlAst, context);
#pragma warning restore 8620

            if (result == null) return null;

            await _batchPlanner.NextBatch(sqlAst, result, databaseCall, context, cancellationToken).ConfigureAwait(false);

            return result;
        }
    }
}

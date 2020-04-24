using System;
using System.Threading.Tasks;
using GraphQL.Types;
using JoinMonster.Data;
using JoinMonster.Language;

namespace JoinMonster
{
    /// <summary>
    /// The entry class for JoinMonster.
    /// </summary>
    public class JoinMonsterExecuter
    {
        private readonly QueryToSqlConverter _converter;
        private readonly SqlCompiler _compiler;

        /// <summary>
        /// Creates a new instance of <see cref="JoinMonsterExecuter"/>.
        /// </summary>
        /// <param name="converter">The <see cref="QueryToSqlConverter"/>.</param>
        /// <param name="compiler">The <see cref="SqlCompiler"/>.</param>
        public JoinMonsterExecuter(QueryToSqlConverter converter, SqlCompiler compiler)
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        }

        /// <summary>
        /// Takes a <see cref="IResolveFieldContext"/> and returns a hydrated object with the data.
        /// </summary>
        /// <param name="context">The <see cref="IResolveFieldContext"/>.</param>
        /// <param name="databaseCall">A <see cref="DatabaseCallDelegate"/> that is passed the compiled SQL and calls the database and returns the data.</param>
        /// <returns>The correctly nested data from the database.</returns>
        /// <exception cref="ArgumentNullException">If <see cref="context"/> or <see cref="databaseCall"/> is null.</exception>
        public async Task<object?> Execute(IResolveFieldContext context, DatabaseCallDelegate databaseCall)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (databaseCall == null) throw new ArgumentNullException(nameof(databaseCall));

            var sqlAst = _converter.Convert(context);
            var sql = await _compiler.Compile(sqlAst, context).ConfigureAwait(false);

            // TODO: Run batches and map result
            return await databaseCall(sql).ConfigureAwait(false);
        }
    }
}

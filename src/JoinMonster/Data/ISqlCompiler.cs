using System;
using System.Collections.Generic;
using GraphQL;
using JoinMonster.Language.AST;

namespace JoinMonster.Data
{
    /// <summary>
    /// The <see cref="ISqlCompiler"/> is responsible for converting SQL Ast to SQL.
    /// </summary>
    public interface ISqlCompiler
    {
        /// <summary>
        /// Compiles the SQL Ast to SQL.
        /// </summary>
        /// <param name="node">The <see cref="Node"/>.</param>
        /// <param name="context">The <see cref="IResolveFieldContext"/>.</param>
        /// <returns>The compiled SQL.</returns>
        /// <exception cref="ArgumentNullException">If <c>node</c> or <c>context</c> is null.</exception>
        SqlResult Compile(Node node, IResolveFieldContext context);
    }

    public class SqlResult
    {
        public SqlResult(string sql, IReadOnlyDictionary<string, object> parameters)
        {
            Sql = sql ?? throw new ArgumentNullException(nameof(sql));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public string Sql { get; }
        public IReadOnlyDictionary<string, object> Parameters { get; }
    }
}

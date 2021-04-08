using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GraphQL;
using JoinMonster.Builders.Clauses;
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
        /// <param name="batchScope">The batch scope.</param>
        /// <returns>The compiled SQL.</returns>
        /// <exception cref="ArgumentNullException">If <c>node</c> or <c>context</c> is null.</exception>
        SqlResult Compile(Node node, IResolveFieldContext context, IEnumerable? batchScope = null);
    }

    public class SqlCompilerContext
    {
        private readonly Dictionary<string, object> _parameters;

        public SqlCompilerContext(SqlCompiler compiler)
        {
            Compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            _parameters = new Dictionary<string, object>();
        }

        public ISqlCompiler Compiler { get; }
        public IReadOnlyDictionary<string, object> Parameters => new ReadOnlyDictionary<string, object>(_parameters);

        public string AddParameter(object value)
        {
            var parameterName = $"@p{_parameters.Count}";

            _parameters.Add(parameterName, value);

            return parameterName;
        }
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

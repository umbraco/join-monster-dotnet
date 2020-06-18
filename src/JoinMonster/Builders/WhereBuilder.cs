using System;
using System.Collections.Generic;
using JoinMonster.Data;

namespace JoinMonster.Builders
{
    /// <summary>
    /// The <c>Where</c> clause builder.
    /// </summary>
    public class WhereBuilder
    {
        private readonly ISqlDialect _dialect;
        private readonly string _parentTable;
        private readonly ICollection<string> _whereConditions;
        private readonly IDictionary<string, object> _parameters;

        internal WhereBuilder(ISqlDialect dialect, string table,  ICollection<string> whereConditions, IDictionary<string, object> parameters)
        {
            _dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            _parentTable = table ?? throw new ArgumentNullException(nameof(table));
            _whereConditions = whereConditions ?? throw new ArgumentNullException(nameof(whereConditions));
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        /// <summary>
        /// Compares two columns.
        /// </summary>
        /// <param name="first">The first column.</param>
        /// <param name="second">The second column.</param>
        /// <param name="op">The compare operator.</param>
        /// <returns>The <see cref="WhereBuilder"/>.</returns>
        public WhereBuilder Columns(string first, string second, string op = "=")
        {
            _whereConditions.Add($"{_parentTable}.{_dialect.Quote(first)} {op} {_parentTable}.{_dialect.Quote(second)}");
            return this;
        }

        /// <summary>
        /// Compares a column with a value.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <param name="op">The compare operator.</param>
        /// <returns>The <see cref="WhereBuilder"/>.</returns>
        public WhereBuilder Column(string column, object value, string op = "=")
        {
            var parameterName = $"@p{_parameters.Count}";
            _parameters.Add(parameterName, value);
            _whereConditions.Add($"{_parentTable}.{_dialect.Quote(column)} {op} {parameterName}");
            return this;
        }
    }
}

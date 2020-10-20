using System;
using System.Collections.Generic;
using JoinMonster.Builders.Clauses;

namespace JoinMonster.Builders
{
    /// <summary>
    /// The <c>Where</c> clause builder.
    /// </summary>
    public class WhereBuilder
    {
        private readonly ICollection<WhereCondition> _whereConditions;

        private bool _not;
        private bool _or;

        internal WhereBuilder(string table, ICollection<WhereCondition> whereConditions)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            _whereConditions = whereConditions ?? throw new ArgumentNullException(nameof(whereConditions));
        }

        /// <summary>
        /// The table name, already quoted.
        /// </summary>
        public string Table { get; }

        public WhereBuilder And()
        {
            _or = false;
            return this;
        }

        public WhereBuilder Or()
        {
            _or = true;
            return this;
        }

        public WhereBuilder Not()
        {
            _not = true;
            return this;
        }

        public WhereBuilder Grouped(Action<WhereBuilder> where)
        {
            if (where == null) throw new ArgumentNullException(nameof(where));

            var conditions = new List<WhereCondition>();
            var nestedCondition = new NestedCondition(conditions);
            var nestedBuilder = new WhereBuilder(Table, conditions);

            AddCondition(nestedCondition);

            where(nestedBuilder);

            return this;
        }

        /// <summary>
        /// Compares two columns.
        /// </summary>
        /// <param name="first">The first column.</param>
        /// <param name="second">The second column.</param>
        /// <param name="op">The compare operator.</param>
        /// <returns>The <see cref="WhereBuilder"/>.</returns>
        public WhereBuilder Columns(string first, string second, string op = "=") =>
            AddCondition(new CompareColumnsCondition(Table, first, op, Table, second));

        /// <summary>
        /// Compares a column with a value.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <param name="op">The compare operator.</param>
        /// <returns>The <see cref="WhereBuilder"/>.</returns>
        public WhereBuilder Column(string column, object value, string op = "=") =>
            AddCondition(new CompareCondition(Table, column, op, value));

        /// <summary>
        /// Compares a column with a value.
        /// </summary>
        /// <param name="sql">The raw sql condition.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The <see cref="WhereBuilder"/>.</returns>
        public WhereBuilder Raw(string sql, IDictionary<string, object> parameters) =>
            AddCondition(new RawCondition(sql, parameters));

        private WhereBuilder AddCondition(WhereCondition condition)
        {
            condition.IsNot = _not;
            condition.IsOr = _or;

            _or = false;
            _not = false;

            _whereConditions.Add(condition);

            return this;
        }
    }
}

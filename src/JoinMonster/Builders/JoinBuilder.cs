using System;
using System.Collections.Generic;
using JoinMonster.Builders.Clauses;
using JoinMonster.Data;

namespace JoinMonster.Builders
{
    /// <summary>
    /// The <c>Join</c> condition builder.
    /// </summary>
    public class JoinBuilder
    {
        internal JoinBuilder(string parentTableName, string parentTableAlias, string childTableName, string childTableAlias)
        {
            ParentTableName = parentTableName ?? throw new ArgumentNullException(nameof(parentTableName));
            ParentTableAlias = parentTableAlias ?? throw new ArgumentNullException(nameof(parentTableAlias));
            ChildTableName = childTableName ?? throw new ArgumentNullException(nameof(childTableName));
            ChildTableAlias = childTableAlias ?? throw new ArgumentNullException(nameof(childTableAlias));

            From = $"LEFT JOIN {ChildTableName} AS {ChildTableAlias}";
        }

        internal string From { get; private set; }

        internal WhereCondition? Condition { get; private set; }

        /// <summary>
        /// The parent table name. Already quoted.
        /// </summary>
        public string ParentTableName { get; }

        /// <summary>
        /// An auto-generated table alias for the parent table. Already quoted.
        /// </summary>
        public string ParentTableAlias { get; }

        /// <summary>
        /// The parent child name. Already quoted.
        /// </summary>
        public string ChildTableName { get; }

        /// <summary>
        /// An auto-generated table alias for the child table. Already quoted.
        /// </summary>
        public string ChildTableAlias { get; }

        /// <summary>
        /// Defines a <c>JOIN</c> between two tables.
        /// </summary>
        /// <param name="parentColumn">The column on the parent table.</param>
        /// <param name="childColumn">The column on the child table</param>
        /// <param name="op">The join operator.</param>
        /// <returns>The <see cref="JoinBuilder"/> instance.</returns>
        public void On(string parentColumn, string childColumn, string op = "=")
        {
            if (parentColumn == null) throw new ArgumentNullException(nameof(parentColumn));
            if (childColumn == null) throw new ArgumentNullException(nameof(childColumn));
            if (op == null) throw new ArgumentNullException(nameof(op));

            Condition = new CompareColumnsCondition(ParentTableAlias, parentColumn, op, ChildTableAlias, childColumn);
        }

        /// <summary>
        /// Defines a raw <c>JOIN</c> condition.
        /// </summary>
        /// <param name="joinCondition">The join condition.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="from">E.g. LEFT JOIN {join.ChildTableName} AS {join.ChildTableAlias}.</param>
        public void Raw(string joinCondition, object? parameters = null, string from = null!) =>
            Raw(joinCondition, parameters?.ToDictionary(), from);

        /// <summary>
        /// Defines a raw <c>JOIN</c> condition.
        /// </summary>
        /// <param name="joinCondition">The join condition.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="from">E.g. LEFT JOIN {join.ChildTableName} AS {join.ChildTableAlias}.</param>
        public void Raw(string joinCondition, IDictionary<string, object>? parameters, string? from = null)
        {
            if (joinCondition == null) throw new ArgumentNullException(nameof(joinCondition));
            if (@from != null)
                From = @from;

            Condition = new RawCondition(joinCondition, parameters);
        }
    }
}

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
        internal JoinBuilder(string parentTable, string childTable)
        {
            ParentTableAlias = parentTable ?? throw new ArgumentNullException(nameof(parentTable));
            ChildTableAlias = childTable ?? throw new ArgumentNullException(nameof(childTable));
        }

        internal WhereCondition? Condition { get; private set; }

        /// <summary>
        /// An auto-generated table alias for the parent table. Already quoted.
        /// </summary>
        public string ParentTableAlias { get; }

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
        /// <param name="joinCondition">The raw join condition.</param>
        /// <param name="parameters">The parameters.</param>
        public void Raw(string joinCondition, object? parameters = null) =>
            Raw(joinCondition, parameters?.ToDictionary());

        /// <summary>
        /// Defines a raw <c>JOIN</c> condition.
        /// </summary>
        /// <param name="joinCondition">The raw join condition.</param>
        /// <param name="parameters">The parameters.</param>
        public void Raw(string joinCondition, IDictionary<string, object>? parameters)
        {
            if (joinCondition == null) throw new ArgumentNullException(nameof(joinCondition));
            Condition = new RawCondition(joinCondition, parameters);
        }
    }
}

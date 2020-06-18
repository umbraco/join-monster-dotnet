using System;
using JoinMonster.Data;

namespace JoinMonster.Builders
{
    /// <summary>
    /// The <c>Join</c> condition builder.
    /// </summary>
    public class JoinBuilder
    {
        private readonly ISqlDialect _dialect;

        internal JoinBuilder(ISqlDialect dialect, string parentTable, string childTable)
        {
            _dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            ParentTableAlias = parentTable ?? throw new ArgumentNullException(nameof(parentTable));
            ChildTableAlias = childTable ?? throw new ArgumentNullException(nameof(childTable));
        }

        internal string? Condition { get; private set; }
        internal string? RawCondition { get; private set; }

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

            var parent = parentColumn.IndexOf(ParentTableAlias, StringComparison.Ordinal) == -1 ?
                $"{ParentTableAlias}.{_dialect.Quote(parentColumn)}" : parentColumn;

            var child = childColumn.IndexOf(ChildTableAlias, StringComparison.Ordinal) == -1 ?
                $"{ChildTableAlias}.{_dialect.Quote(childColumn)}" : childColumn;

            Condition = $"{parent} {op} {child}";
        }

        /// <summary>
        /// Defines a raw <c>JOIN</c> condition.
        /// </summary>
        /// <param name="joinCondition">The raw join condition.</param>
        public void Raw(string joinCondition)
        {
            RawCondition = joinCondition ?? throw new ArgumentNullException(nameof(joinCondition));
        }
    }
}

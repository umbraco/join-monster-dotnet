using System;

namespace JoinMonster.Configs
{
    /// <summary>
    /// SQL junction configuration.
    /// </summary>
    public class SqlJunctionConfig
    {
        /// <summary>
        /// Creates a new instance of <see cref="SqlJunctionConfig"/>.
        /// </summary>
        /// <param name="table">The table name.</param>
        /// <param name="fromParent">The JOIN condition when joining from the parent table to the junction table.</param>
        /// <param name="toChild">The JOIN condition when joining from the junction table to the child table.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="table"/>, <paramref name="fromParent"/> or <paramref name="toChild"/> is <c>null</c>.</exception>
        public SqlJunctionConfig(string table, JoinDelegate fromParent, JoinDelegate toChild)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            FromParent = fromParent ?? throw new ArgumentNullException(nameof(fromParent));
            ToChild = toChild ?? throw new ArgumentNullException(nameof(toChild));
        }

        /// <summary>
        /// The Junction table name.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// The JOIN condition when joining from the parent table to the junction table.
        /// First argument is the parent table, second argument is the junction table.
        /// </summary>
        public JoinDelegate FromParent { get; }

        /// <summary>
        /// The JOIN condition when joining from the junction table to the child table.
        /// First argument is the junction table, second argument is the child table.
        /// </summary>
        public JoinDelegate ToChild { get; }

        /// <summary>
        /// The WHERE condition.
        /// </summary>
        public WhereDelegate? Where { get; set; }

        // TODO: Add Include (Dictionary/Expression?)
    }
}

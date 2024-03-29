using System;

namespace JoinMonster.Configs
{
    /// <summary>
    /// SQL junction configuration.
    /// </summary>
    public class SqlJunctionConfig
    {
        /// <summary>
        /// Creates a new instance of <see cref="SqlJunctionConfig"/> configured for joining the junction table.
        /// </summary>
        /// <param name="table">The junction table name.</param>
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
        /// Creates a new instance of <see cref="SqlJunctionConfig"/> configured for batching the many-to-many query.
        /// </summary>
        /// <param name="table">The junction table name.</param>
        /// <param name="uniqueKey">The unique key columns.</param>
        /// <param name="batchConfig">The batch configuration.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="table"/>, <paramref name="uniqueKey"/> or <paramref name="batchConfig"/> is <c>null</c>.</exception>
        public SqlJunctionConfig(string table, string[] uniqueKey, SqlBatchConfig batchConfig)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            UniqueKey = uniqueKey ?? throw new ArgumentNullException(nameof(uniqueKey));
            BatchConfig = batchConfig ?? throw new ArgumentNullException(nameof(batchConfig));
        }

        /// <summary>
        /// The Junction table name.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// The unique key columns.
        /// </summary>
        public string[]? UniqueKey { get; }

        /// <summary>
        /// The SQL batch configuration.
        /// </summary>
        public SqlBatchConfig? BatchConfig { get; }

        /// <summary>
        /// The JOIN condition when joining from the parent table to the junction table.
        /// First argument is the parent table, second argument is the junction table.
        /// </summary>
        public JoinDelegate? FromParent { get; }

        /// <summary>
        /// The JOIN condition when joining from the junction table to the child table.
        /// First argument is the junction table, second argument is the child table.
        /// </summary>
        public JoinDelegate? ToChild { get; }

        /// <summary>
        /// The WHERE condition.
        /// </summary>
        public WhereDelegate? Where { get; set; }

        /// <summary>
        /// the ORDER condition.
        /// </summary>
        public OrderByDelegate? OrderBy { get; set; }

        /// <summary>
        /// The Sort Key,
        /// </summary>
        public SortKeyDelegate? SortKey { get; set; }

        // TODO: Add Include (Dictionary/Expression?)
    }
}

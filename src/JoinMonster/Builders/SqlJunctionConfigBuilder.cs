using System;
using JoinMonster.Configs;

namespace JoinMonster.Builders
{
    /// <summary>
    /// A helper class for fluently creating a <see cref="SqlJunctionConfig"/> object.
    /// </summary>
    public class SqlJunctionConfigBuilder
    {
        private SqlJunctionConfigBuilder(SqlJunctionConfig sqlJunctionConfig)
        {
            SqlJunctionConfig = sqlJunctionConfig;
        }

        /// <summary>
        /// The SQL Junction configuration.
        /// </summary>
        public SqlJunctionConfig SqlJunctionConfig { get; }

        /// <summary>
        /// Create a new instance of the <see cref="SqlJunctionConfigBuilder"/> configured for joining the junction table.
        /// </summary>
        /// <param name="tableName">The junction table name.</param>
        /// <param name="fromParent">The JOIN condition when joining from the parent table to the junction table.</param>
        /// <param name="toChild">The JOIN condition when joining from the junction table to the child table.</param>
        /// <returns>The <see cref="SqlJunctionConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tableName"/>, <paramref name="fromParent"/> or <paramref name="toChild"/> is <c>null</c>.</exception>
        public static SqlJunctionConfigBuilder Create(string tableName, JoinDelegate fromParent, JoinDelegate toChild)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            if (fromParent == null) throw new ArgumentNullException(nameof(fromParent));
            if (toChild == null) throw new ArgumentNullException(nameof(toChild));

            var config = new SqlJunctionConfig(tableName, fromParent, toChild);

            return new SqlJunctionConfigBuilder(config);
        }

        /// <summary>
        /// Create a new instance of the <see cref="SqlJunctionConfigBuilder"/> configured for batching the many-to-many query.
        /// </summary>
        /// <param name="tableName">The junction table name.</param>
        /// <param name="uniqueKey">The unique key columns.</param>
        /// <param name="thisKey">The column to match on the current table.</param>
        /// <param name="parentKey">The column to match in the parent table.</param>
        /// <param name="keyType">The type of the keys.</param>
        /// <param name="join">The JOIN condition when joining from the junction table to the related table.</param>
        /// <returns>The <see cref="SqlJunctionConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tableName"/>, <paramref name="uniqueKey"/>, <paramref name="thisKey"/>, <paramref name="parentKey"/> or <paramref name="join"/> is <c>null</c>.</exception>
        public static SqlJunctionConfigBuilder Create(string tableName, string[] uniqueKey, string thisKey, string parentKey, Type keyType, JoinDelegate join)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            if (thisKey == null) throw new ArgumentNullException(nameof(thisKey));
            if (parentKey == null) throw new ArgumentNullException(nameof(parentKey));
            if (join == null) throw new ArgumentNullException(nameof(join));

            var sqlBatchBuilder = SqlBatchConfigBuilder
                .Create(thisKey, parentKey, keyType)
                .Join(join);

            var config = new SqlJunctionConfig(tableName, uniqueKey, sqlBatchBuilder.SqlBatchConfig);

            return new SqlJunctionConfigBuilder(config);
        }

        /// <summary>
        /// Sets the <c>WHERE</c> condition for the junction configuration.
        /// </summary>
        /// <param name="where">The WHERE condition.</param>
        /// <returns>The <see cref="SqlJunctionConfig"/>.</returns>
        public SqlJunctionConfigBuilder Where(WhereDelegate where)
        {
            SqlJunctionConfig.Where = where;
            return this;
        }

        /// <summary>
        /// Sets the <c>ORDER BY</c> clause for the junction configuration.
        /// </summary>
        /// <param name="orderBy">The ORDER BY clause.</param>
        /// <returns>The <see cref="SqlJunctionConfig"/>.</returns>
        public SqlJunctionConfigBuilder OrderBy(OrderByDelegate orderBy)
        {
            SqlJunctionConfig.OrderBy = orderBy;
            return this;
        }

        /// <summary>
        /// Sets the <c>Sort Key</c> for the junction configuration.
        /// </summary>
        /// <param name="sortKey">The Sort Key.</param>
        /// <returns>The <see cref="SqlJunctionConfig"/>.</returns>
        public SqlJunctionConfigBuilder SortKey(SortKeyDelegate sortKey)
        {
            SqlJunctionConfig.SortKey = sortKey;
            return this;
        }
    }
}

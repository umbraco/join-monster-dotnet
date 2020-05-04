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
        /// <returns>The <see cref="SqlJunctionConfig"/>.</returns>
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
        /// Sets the <c>WHERE</c> condition for the junction configuration.
        /// </summary>
        /// <param name="where">The WHERE condition condition.</param>
        /// <returns>The <see cref="SqlJunctionConfig"/>.</returns>
        public SqlJunctionConfigBuilder Where(WhereDelegate where)
        {
            SqlJunctionConfig.Where = where;
            return this;
        }

        public SqlJunctionConfigBuilder OrderBy(OrderByDelegate orderBy)
        {
            SqlJunctionConfig.OrderBy = orderBy;
            return this;
        }
    }
}

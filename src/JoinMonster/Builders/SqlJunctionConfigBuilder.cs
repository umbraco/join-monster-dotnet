using JoinMonster.Configs;

namespace JoinMonster.Builders
{
    /// <summary>
    /// A helper class for creating a <see cref="SqlJunctionConfig"/> object.
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
        /// Create a new instance of the <see cref="SqlJunctionConfig"/>
        /// </summary>
        /// <param name="table">The junction table name.</param>
        /// <param name="fromParent">The JOIN condition when joining from the parent table to the junction table.</param>
        /// <param name="toChild">The JOIN condition when joining from the junction table to the child table.</param>
        /// <returns>The <see cref="SqlJunctionConfig"/>.</returns>
        public static SqlJunctionConfigBuilder Create(string table, JoinDelegate fromParent, JoinDelegate toChild)
        {
            var config = new SqlJunctionConfig(table, fromParent, toChild);

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
    }
}

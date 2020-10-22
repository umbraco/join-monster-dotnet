using System;
using GraphQL;
using JoinMonster.Configs;

namespace JoinMonster.Builders
{
    /// <summary>
    /// A helper class for fluently creating a <see cref="SqlTableConfig"/> object.
    /// </summary>
    public class SqlTableConfigBuilder
    {
        private SqlTableConfigBuilder(SqlTableConfig sqlTableConfig)
        {
            SqlTableConfig = sqlTableConfig;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SqlTableConfigBuilder"/>.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="uniqueKey">The unique key columns.</param>
        /// <returns>The <see cref="SqlTableConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tableName"/> or <paramref name="uniqueKey"/> is <c>null</c>.</exception>
        public static SqlTableConfigBuilder Create(TableExpressionDelegate tableName, string[] uniqueKey)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            if (uniqueKey == null) throw new ArgumentNullException(nameof(uniqueKey));

            var config = new SqlTableConfig(tableName, uniqueKey);

            return new SqlTableConfigBuilder(config);
        }

        /// <summary>
        /// The SQL Table configuration.
        /// </summary>
        public SqlTableConfig SqlTableConfig { get; }

        /// <summary>
        /// Set columns that should always be fetched.
        /// </summary>
        /// <param name="columnNames">The column names.</param>
        /// <returns>The <see cref="SqlTableConfigBuilder"/>.</returns>
        public SqlTableConfigBuilder AlwaysFetch(params string[] columnNames)
        {
            SqlTableConfig.AlwaysFetch = columnNames;
            return this;
        }

        /// <summary>
        /// Set a custom column expression.
        /// </summary>
        /// <param name="resolve">The column resolver.</param>
        /// <returns>The <see cref="SqlTableConfigBuilder"/>.</returns>
        public SqlTableConfigBuilder ColumnExpression(ColumnExpressionDelegate resolve)
        {
            SqlTableConfig.ColumnExpression = resolve;
            return this;
        }

        /// <summary>
        /// Set metadata on the table config.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="SqlTableConfigBuilder"/>.</returns>
        public SqlTableConfigBuilder WithMetadata(string key, object value)
        {
            SqlTableConfig.WithMetadata(key, value);
            return this;
        }
    }
}

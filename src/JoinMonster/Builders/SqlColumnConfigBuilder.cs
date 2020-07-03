using System;
using JoinMonster.Configs;

namespace JoinMonster.Builders
{
    /// <summary>
    /// A helper class for fluently creating a <see cref="SqlColumnConfig"/> object.
    /// </summary>
    public class SqlColumnConfigBuilder
    {
        private SqlColumnConfigBuilder(SqlColumnConfig sqlColumnConfig)
        {
            SqlColumnConfig = sqlColumnConfig;
        }

        /// <summary>
        /// Create a new instance of the <see cref="SqlColumnConfigBuilder"/>
        /// </summary>
        /// <param name="columnName">A column name.</param>
        /// <returns>The <see cref="SqlColumnConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="columnName"/> is <c>null</c>.</exception>
        public static SqlColumnConfigBuilder Create(string columnName)
        {
            if (columnName == null) throw new ArgumentNullException(nameof(columnName));

            var config = new SqlColumnConfig(columnName);

            return new SqlColumnConfigBuilder(config);
        }

        /// <summary>
        /// The SQL column configuration.
        /// </summary>
        public SqlColumnConfig SqlColumnConfig { get; }

        /// <summary>
        /// Set the dependant columns, a custom resolver must be specified on the field.
        /// </summary>
        /// <param name="columnNames">The column names to select.</param>
        /// <returns>The <see cref="SqlColumnConfigBuilder"/>.</returns>
        public SqlColumnConfigBuilder Dependencies(params string[] columnNames)
        {
            SqlColumnConfig.Dependencies = columnNames;
            return this;
        }

        /// <summary>
        /// Set a method that resolves to a RAW SQL expression.
        /// </summary>
        /// <param name="expression">The expression resolver.</param>
        /// <returns>The <see cref="SqlColumnConfigBuilder"/>.</returns>
        public SqlColumnConfigBuilder Expression(ExpressionDelegate expression)
        {
            SqlColumnConfig.Expression = expression;
            return this;
        }
    }
}

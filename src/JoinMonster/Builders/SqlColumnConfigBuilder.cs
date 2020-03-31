namespace JoinMonster.Builders
{
    public class SqlColumnConfigBuilder
    {
        private SqlColumnConfigBuilder(SqlColumnConfig sqlColumnConfig)
        {
            SqlColumnConfig = sqlColumnConfig;
        }

        /// <summary>
        /// Create a new instance of the <see cref="SqlColumnConfigBuilder"/>
        /// </summary>
        /// <param name="columnName">A column name, if <c>null</c> the fields name is used instead.</param>
        /// <returns>The <see cref="SqlColumnConfigBuilder"/>.</returns>
        public static SqlColumnConfigBuilder Create(string? columnName = null)
        {
            var config = new SqlColumnConfig
            {
                Column = columnName
            };

            return new SqlColumnConfigBuilder(config);
        }

        /// <summary>
        /// The SQL column configuration.
        /// </summary>
        public SqlColumnConfig SqlColumnConfig { get; }

        /// <summary>
        /// Set the column name.
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The <see cref="SqlColumnConfigBuilder"/>.</returns>
        public SqlColumnConfigBuilder Name(string columnName)
        {
            SqlColumnConfig.Column = columnName;
            return this;
        }

        /// <summary>
        /// Set whether the column should be ignored from the generated SQL query.
        /// </summary>
        /// <param name="ignored"><c>true</c> if the column should be ignored, otherwise <c>false</c>.</param>
        /// <returns>The <see cref="SqlColumnConfigBuilder"/>.</returns>
        public SqlColumnConfigBuilder Ignore(bool ignored)
        {
            SqlColumnConfig.Ignored = ignored;
            return this;
        }

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
        /// Set a method that resolves to a raw SQL expression.
        /// </summary>
        /// <param name="expressionResolver">The expression resolver.</param>
        /// <returns>The <see cref="SqlColumnConfigBuilder"/>.</returns>
        public SqlColumnConfigBuilder Expression(ExpressionDelegate expressionResolver)
        {
            SqlColumnConfig.Expression = expressionResolver;
            return this;
        }
    }
}

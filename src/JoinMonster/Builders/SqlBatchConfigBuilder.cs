using System;
using JoinMonster.Configs;

namespace JoinMonster.Builders
{
    /// <summary>
    /// A helper class for fluently creating a <see cref="SqlBatchConfig"/> object.
    /// </summary>
    public class SqlBatchConfigBuilder
    {
        private SqlBatchConfigBuilder(SqlBatchConfig sqlBatchConfig)
        {
            SqlBatchConfig = sqlBatchConfig;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SqlBatchConfigBuilder"/>.
        /// </summary>
        /// <param name="thisKey">The column to match on the current table.</param>
        /// <param name="parentKey">The column to match on the other table.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="thisKey"/> or <paramref name="parentKey"/> is null.</exception>
        /// <returns>The <see cref="SqlBatchConfigBuilder"/>.</returns>
        public static SqlBatchConfigBuilder Create(string thisKey, string parentKey)
        {
            if (thisKey == null) throw new ArgumentNullException(nameof(thisKey));
            if (parentKey == null) throw new ArgumentNullException(nameof(parentKey));

            var config = new SqlBatchConfig(thisKey, parentKey);

            return new SqlBatchConfigBuilder(config);
        }

        /// <summary>
        /// The SQL batch configuration.
        /// </summary>
        public SqlBatchConfig SqlBatchConfig { get; }

        /// <summary>
        /// Set a method that resolves to a RAW SQL expression.
        /// </summary>
        /// <param name="expression">The expression resolver.</param>
        /// <returns>The <see cref="SqlBatchConfigBuilder"/>.</returns>
        public SqlBatchConfigBuilder ThisKey(ExpressionDelegate expression)
        {
            SqlBatchConfig.ThisKeyExpression = expression;
            return this;
        }

        /// <summary>
        /// Set a method that resolves to a RAW SQL expression.
        /// </summary>
        /// <param name="expression">The expression resolver.</param>
        /// <returns>The <see cref="SqlBatchConfigBuilder"/>.</returns>
        public SqlBatchConfigBuilder ParentKey(ExpressionDelegate expression)
        {
            SqlBatchConfig.ParentKeyExpression = expression;
            return this;
        }

        /// <summary>
        /// Set a method that resolves the WHERE condition.
        /// </summary>
        /// <param name="where">The WHERE condition.</param>
        public SqlBatchConfigBuilder Where(BatchWhereDelegate where)
        {
            SqlBatchConfig.Where = where;
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="join">The JOIN condition when joining from the junction table to the related table.</param>
        /// <returns>The <see cref="SqlBatchConfigBuilder"/>.</returns>
        public SqlBatchConfigBuilder Join(JoinDelegate join)
        {
            SqlBatchConfig.Join = join;
            return this;
        }
    }
}

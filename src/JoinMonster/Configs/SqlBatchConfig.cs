using System;

namespace JoinMonster.Configs
{
    /// <summary>
    /// SQL batch configuration.
    /// </summary>
    public class SqlBatchConfig
    {
        /// <summary>
        /// Creates a new instance of <see cref="SqlBatchConfig"/>.
        /// </summary>
        /// <param name="thisKey">The column to match on the current table.</param>
        /// <param name="parentKey">The column to match on the other table.</param>
        /// <param name="keyType">The type of the keys</param>
        /// <exception cref="ArgumentNullException">If <paramref name="thisKey"/> or <paramref name="parentKey"/> is null.</exception>
        public SqlBatchConfig(string thisKey, string parentKey, Type keyType)
        {
            ThisKey = thisKey ?? throw new ArgumentNullException(nameof(thisKey));
            ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));
            KeyType = keyType ?? throw new ArgumentNullException(nameof(keyType));
        }

        /// <summary>
        /// The column to match on the current table.
        /// </summary>
        public string ThisKey { get; }

        /// <summary>
        /// Custom SQL expression for matching the column on the current table.
        /// </summary>
        public ExpressionDelegate? ThisKeyExpression { get; set; }

        /// <summary>
        /// The column to match on the other table.
        /// </summary>
        public string ParentKey { get; }

        /// <summary>
        /// The type of the keys to match on
        /// </summary>
        public Type KeyType { get; }

        /// <summary>
        /// Custom SQL expression for matching the column on the other table.
        /// </summary>
        public ExpressionDelegate? ParentKeyExpression { get; set; }

        /// <summary>
        /// The WHERE condition.
        /// </summary>
        public BatchWhereDelegate? Where { get; set; }

        /// <summary>
        /// The JOIN condition when joining from the junction table to the related table.
        /// </summary>
        public JoinDelegate? Join { get; set; }
    }
}

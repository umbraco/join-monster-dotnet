using System;
using GraphQL.Utilities;

namespace JoinMonster.Configs
{
    /// <summary>
    /// SQL table configuration.
    /// </summary>
    public class SqlTableConfig : MetadataProvider
    {
        /// <summary>
        /// Creates a new instance of <see cref="SqlTableConfig"/>.
        /// </summary>
        /// <param name="table">The table name.</param>
        /// <param name="uniqueKey">The unique key columns.</param>
        public SqlTableConfig(TableExpressionDelegate table, string[] uniqueKey)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            UniqueKey = uniqueKey ?? throw new ArgumentNullException(nameof(uniqueKey));
        }

        /// <summary>
        /// The table name.
        /// </summary>
        public TableExpressionDelegate Table { get; }

        /// <summary>
        /// The unique key columns.
        /// </summary>
        public string[] UniqueKey { get; }

        /// <summary>
        /// Columns to always fetch.
        /// </summary>
        public string[]? AlwaysFetch { get; set; }

        /// <summary>
        /// Custom column SQL expression.
        /// </summary>
        public ColumnExpressionDelegate? ColumnExpression { get; set; }
    }
}

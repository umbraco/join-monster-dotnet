using System;

namespace JoinMonster
{
    public class SqlTableConfig
    {
        public SqlTableConfig(string table, string[] uniqueKey)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            UniqueKey = uniqueKey ?? throw new ArgumentNullException(nameof(uniqueKey));
        }

        /// <summary>
        /// The table name.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// The unique keys.
        /// </summary>
        public string[] UniqueKey { get; }

        /// <summary>
        /// Columns to always fetch.
        /// </summary>
        public string[]? AlwaysFetch { get; set; }
    }
}

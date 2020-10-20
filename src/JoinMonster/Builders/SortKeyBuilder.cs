using System;
using System.Linq;
using JoinMonster.Configs;
using JoinMonster.Language.AST;

namespace JoinMonster.Builders
{
    /// <summary>
    /// A helper object for fluently creating a <c>Sort Key</c> configuration.
    /// </summary>
    public class SortKeyBuilder
    {
        private readonly IAliasGenerator _aliasGenerator;

        internal SortKeyBuilder(string table, IAliasGenerator aliasGenerator)
        {
            _aliasGenerator = aliasGenerator ?? throw new ArgumentNullException(nameof(aliasGenerator));
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public string Table { get; }
        internal SortKey? SortKey { get; private set; }

        /// <summary>
        /// Sort the result by the <paramref name="column"/> in ascending order.
        /// </summary>
        /// <param name="column">The column name.</param>
        public void By(string column) => By(new[] {column});

        /// <summary>
        /// Sort the result by the <paramref name="columns"/> in ascending order.
        /// </summary>
        /// <param name="columns">The column names.</param>
        public void By(string[] columns)
        {
            if (columns == null) throw new ArgumentNullException(nameof(columns));
            SortKey = new SortKey(Table, columns.ToDictionary(x => x, _aliasGenerator.GenerateColumnAlias), SortDirection.Ascending);
        }

        /// <summary>
        /// Sort the result by the <paramref name="column"/> in descending order.
        /// </summary>
        /// <param name="column">The column name.</param>
        public void ByDescending(string column) => ByDescending(new[] {column});

        /// <summary>
        /// Sort the result by the <paramref name="columns"/> in descending order.
        /// </summary>
        /// <param name="columns">The column names.</param>
        public void ByDescending(string[] columns)
        {
            if (columns == null) throw new ArgumentNullException(nameof(columns));
            SortKey = new SortKey(Table, columns.ToDictionary(x => x, _aliasGenerator.GenerateColumnAlias), SortDirection.Descending);
        }
    }
}

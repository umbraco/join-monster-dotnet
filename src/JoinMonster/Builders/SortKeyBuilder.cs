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
        public ThenSortKeyBuilder By(string column) => By(column, SortDirection.Ascending);


        /// <summary>
        /// Sort the result by the <paramref name="column"/> in descending order.
        /// </summary>
        /// <param name="column">The column name.</param>
        public ThenSortKeyBuilder ByDescending(string column) => By(column, SortDirection.Descending);

        private ThenSortKeyBuilder By(string column, SortDirection direction)
        {
            SortKey = new SortKey(Table, column, _aliasGenerator.GenerateColumnAlias(column), direction);
            return new ThenSortKeyBuilder(Table, SortKey, _aliasGenerator);
        }
    }

    /// <summary>
    /// A helper object for fluently creating a <c>Sort Key</c> configuration.
    /// </summary>
    public class ThenSortKeyBuilder
    {
        private readonly IAliasGenerator _aliasGenerator;

        internal ThenSortKeyBuilder(string table, SortKey sortKey, IAliasGenerator aliasGenerator)
        {
            _aliasGenerator = aliasGenerator ?? throw new ArgumentNullException(nameof(aliasGenerator));
            Table = table ?? throw new ArgumentNullException(nameof(table));
            SortKey = sortKey ?? throw new ArgumentNullException(nameof(sortKey));
        }

        public string Table { get; }
        internal SortKey SortKey { get; private set; }

        /// <summary>
        /// Sort the result by the <paramref name="column"/> in ascending order.
        /// </summary>
        /// <param name="column">The column name.</param>
        public ThenSortKeyBuilder ThenBy(string column) => ThenBy(column, SortDirection.Ascending);


        /// <summary>
        /// Sort the result by the <paramref name="column"/> in descending order.
        /// </summary>
        /// <param name="column">The column name.</param>
        public ThenSortKeyBuilder ThenByDescending(string column) => ThenBy(column, SortDirection.Descending);

        private ThenSortKeyBuilder ThenBy(string column, SortDirection direction)
        {
            SortKey.ThenBy = new SortKey(Table, column, _aliasGenerator.GenerateColumnAlias(column), direction);
            return new ThenSortKeyBuilder(Table, SortKey.ThenBy, _aliasGenerator);
        }
    }
}

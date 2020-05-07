using System;
using JoinMonster.Configs;
using JoinMonster.Language.AST;

namespace JoinMonster.Builders
{
    /// <summary>
    /// A helper object for fluently creating a <c>Sort Key</c> configuration.
    /// </summary>
    public class SortKeyBuilder
    {
        internal SortKeyBuilder()
        {
        }

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
            SortKey = new SortKey(columns, SortDirection.Ascending);
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
            SortKey = new SortKey(columns, SortDirection.Descending);
        }
    }
}

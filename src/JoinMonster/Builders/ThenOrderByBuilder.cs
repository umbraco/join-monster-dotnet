using System;
using JoinMonster.Configs;
using JoinMonster.Data;
using JoinMonster.Language.AST;

namespace JoinMonster.Builders
{
    /// <summary>
    /// A helper object for fluently chaining the <c>Then By</c> objects.
    /// </summary>
    public class ThenOrderByBuilder
    {
        internal ThenOrderByBuilder(OrderBy orderBy)
        {
            OrderBy = orderBy ?? throw new ArgumentNullException(nameof(orderBy));
        }

        internal OrderBy OrderBy { get; private set; }

        /// <summary>
        /// Subsequent sort the result by the <paramref name="column"/> in ascending order.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <returns>The <see cref="ThenOrderByBuilder"/>.</returns>
        public ThenOrderByBuilder ThenBy(string column)
        {
            OrderBy = OrderBy.ThenBy = new OrderBy(column, SortDirection.Ascending);
            return this;
        }

        /// <summary>
        /// Subsequent sort the result by the <paramref name="column"/> in descending order.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <returns>The <see cref="ThenOrderByBuilder"/>.</returns>
        public ThenOrderByBuilder ThenByDescending(string column)
        {
            OrderBy = OrderBy.ThenBy = new OrderBy(column, SortDirection.Descending);
            return this;
        }
    }
}

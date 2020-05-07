using JoinMonster.Configs;
using JoinMonster.Data;
using JoinMonster.Language.AST;

namespace JoinMonster.Builders
{
    /// <summary>
    /// A helper object for fluently creating a <c>Order By</c> configuration.
    /// </summary>
    public class OrderByBuilder
    {
        internal OrderByBuilder()
        {
        }

        internal OrderBy? OrderBy { get; private set; }

        /// <summary>
        /// Sort the result by the <paramref name="column"/> in ascending order.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <returns>The <see cref="ThenOrderByBuilder"/>.</returns>
        public ThenOrderByBuilder By(string column)
        {
            OrderBy = new OrderBy(column, SortDirection.Ascending);
            return new ThenOrderByBuilder(OrderBy);
        }

        /// <summary>
        /// Sort the result by the <paramref name="column"/> in descending order.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <returns>The <see cref="ThenOrderByBuilder"/>.</returns>
        public ThenOrderByBuilder ByDescending(string column)
        {
            OrderBy = new OrderBy(column, SortDirection.Descending);
            return new ThenOrderByBuilder(OrderBy);
        }
    }
}

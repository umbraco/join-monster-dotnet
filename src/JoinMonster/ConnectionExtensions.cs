using System;
using GraphQL.Builders;
using JoinMonster.Configs;

namespace JoinMonster
{
    public static class ConnectionExtensions
    {
        /// <summary>
        /// Set a method that resolves the WHERE condition.
        /// </summary>
        /// <param name="builder">The connection builder.</param>
        /// <param name="where">The WHERE condition condition.</param>
        /// <returns>The <see cref="ConnectionBuilder{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="builder"/> or <see cref="where"/> is <c>null</c>.</exception>
        public static ConnectionBuilder<T> SqlWhere<T>(this ConnectionBuilder<T> builder, WhereDelegate where)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (where == null) throw new ArgumentNullException(nameof(where));

            builder.FieldType.SqlWhere(where);

            return builder;
        }

        /// <summary>
        /// Set a method that resolves the <c>ORDER BY</c> clause.
        /// </summary>
        /// <param name="builder">The connection builder.</param>
        /// <param name="orderBy">The <c>ORDER BY</c> builder.</param>
        /// <returns>The <see cref="ConnectionBuilder{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="builder"/> or <paramref name="orderBy"/> is <c>NULL</c>.</exception>
        public static ConnectionBuilder<T> SqlOrder<T>(this ConnectionBuilder<T> builder, OrderByDelegate orderBy)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (orderBy == null) throw new ArgumentNullException(nameof(orderBy));

            builder.FieldType.SqlOrder(orderBy);

            return builder;
        }

        /// <summary>
        /// Set a method that resolves the LEFT JOIN condition.
        /// </summary>
        /// <param name="builder">The connection builder.</param>
        /// <param name="join">The JOIN condition resolver.</param>
        /// <returns>The <see cref="ConnectionBuilder{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="builder"/> or <paramref name="join"/> is <c>null</c>.</exception>
        public static ConnectionBuilder<T> SqlJoin<T>(this ConnectionBuilder<T> builder, JoinDelegate join)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (join == null) throw new ArgumentNullException(nameof(join));

            builder.FieldType.SqlJoin(join);

            return builder;
        }

        /// <summary>
        /// Sets whether the result set should be paginated.
        /// </summary>
        /// <param name="builder">The connection builder.</param>
        /// <param name="paginate">Should the result be paginated?</param>
        /// <returns>The <see cref="ConnectionBuilder{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="builder"/> is <c>null</c>.</exception>
        public static ConnectionBuilder<T> SqlPaginate<T>(this ConnectionBuilder<T> builder, bool paginate = true)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.FieldType.SqlPaginate(paginate);

            return builder;
        }
    }
}

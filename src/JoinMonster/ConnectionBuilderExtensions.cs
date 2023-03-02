using System;
using GraphQL.Builders;
using JoinMonster.Builders;
using JoinMonster.Configs;

namespace JoinMonster
{
    /// <summary>
    /// Extension methods for <see cref="ConnectionBuilder"/>.
    /// </summary>
    public static class ConnectionBuilderExtensions
    {
        /// <summary>
        /// Set a method that resolves the <c>WHERE</c> condition.
        /// </summary>
        /// <param name="connectionBuilder">The field builder.</param>
        /// <param name="where">The WHERE condition resolver.</param>
        /// <returns>The <see cref="ConnectionBuilder{TSourceType}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="connectionBuilder"/> or <paramref name="where"/> is <c>null</c>.</exception>
        public static ConnectionBuilder<TSourceType> SqlWhere<TSourceType>(this ConnectionBuilder<TSourceType> connectionBuilder, WhereDelegate where)
        {
            if (connectionBuilder == null) throw new ArgumentNullException(nameof(connectionBuilder));
            if (where == null) throw new ArgumentNullException(nameof(where));

            connectionBuilder.FieldType.SqlWhere(where);

            return connectionBuilder;
        }

        /// <summary>
        /// Set a method that resolves the <c>JOIN</c> condition.
        /// </summary>
        /// <param name="connectionBuilder">The field builder.</param>
        /// <param name="join">The JOIN condition resolver.</param>
        /// <returns>The <see cref="ConnectionBuilder{TSourceType}"/></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="connectionBuilder"/> or <paramref name="join"/> is <c>null</c>.</exception>
        public static ConnectionBuilder<TSourceType> SqlJoin<TSourceType>(this ConnectionBuilder<TSourceType> connectionBuilder, JoinDelegate join)
        {
            if (connectionBuilder == null) throw new ArgumentNullException(nameof(connectionBuilder));
            if (join == null) throw new ArgumentNullException(nameof(join));

            connectionBuilder.FieldType.SqlJoin(join);

            return connectionBuilder;
        }

        /// <summary>
        /// Configure a SQL Junction.
        /// </summary>
        /// <param name="connectionBuilder">The field builder.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="fromParent">The JOIN condition when joining from the parent table to the junction table.</param>
        /// <param name="toChild">The JOIN condition when joining from the junction table to the child table.</param>
        /// <param name="configure">The configuration action.</param>
        /// <returns>The <see cref="ConnectionBuilder{TSourceType}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="connectionBuilder"/> is <c>null</c>.</exception>
        public static ConnectionBuilder<TSourceType> SqlJunction<TSourceType>(this ConnectionBuilder<TSourceType> connectionBuilder, string tableName, JoinDelegate fromParent, JoinDelegate toChild, Action<SqlJunctionConfigBuilder>? configure = null)
        {
            if (connectionBuilder == null) throw new ArgumentNullException(nameof(connectionBuilder));

            var junctionBuilder = connectionBuilder.FieldType.SqlJunction(tableName, fromParent, toChild);

            if (configure is not null)
                configure(junctionBuilder);

            return connectionBuilder;
        }

        /// <summary>
        /// Configure one to many SQL batching.
        /// </summary>
        /// <param name="connectionBuilder">The field builder.</param>
        /// <param name="thisKey">The column in this table.</param>
        /// <param name="parentKey">The column in the other table.</param>
        /// <param name="configure">The configuration action.</param>
        /// <returns>The <see cref="ConnectionBuilder{TSourceType}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="connectionBuilder"/> is <c>null</c>.</exception>
        public static ConnectionBuilder<TSourceType> SqlBatch<TSourceType>(this ConnectionBuilder<TSourceType> connectionBuilder, string thisKey, string parentKey, Action<SqlBatchConfigBuilder>? configure = null)
        {
            if (connectionBuilder == null) throw new ArgumentNullException(nameof(connectionBuilder));

            var columnBuilder = connectionBuilder.FieldType.SqlBatch(thisKey, parentKey);

            if (configure is not null)
                configure(columnBuilder);

            return connectionBuilder;
        }

        /// <summary>
        /// Set a method that resolves the <c>ORDER BY</c> clause.
        /// </summary>
        /// <param name="connectionBuilder">The field builder.</param>
        /// <param name="orderBy">The <c>ORDER BY</c> builder.</param>
        /// <returns>The <see cref="ConnectionBuilder{TSourceType}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="connectionBuilder"/> or <paramref name="orderBy"/> is <c>NULL</c>.</exception>
        public static ConnectionBuilder<TSourceType> SqlOrder<TSourceType>(this ConnectionBuilder<TSourceType> connectionBuilder, OrderByDelegate orderBy)
        {
            if (connectionBuilder == null) throw new ArgumentNullException(nameof(connectionBuilder));
            if (orderBy == null) throw new ArgumentNullException(nameof(orderBy));

            connectionBuilder.FieldType.SqlOrder(orderBy);

            return connectionBuilder;
        }

        /// <summary>
        /// Set a method that resolves the sort key used for keyset based pagination.
        /// </summary>
        /// <param name="connectionBuilder">The field builder.</param>
        /// <param name="sort">The <c>Sort Key</c> builder.</param>
        /// <returns>The <see cref="ConnectionBuilder{TSourceType}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="connectionBuilder"/> or <paramref name="sort"/> is <c>NULL</c>.</exception>
        public static ConnectionBuilder<TSourceType> SqlSortKey<TSourceType>(this ConnectionBuilder<TSourceType> connectionBuilder, SortKeyDelegate sort)
        {
            if (connectionBuilder == null) throw new ArgumentNullException(nameof(connectionBuilder));
            if (sort == null) throw new ArgumentNullException(nameof(sort));

            connectionBuilder.FieldType.SqlSortKey(sort);

            return connectionBuilder;
        }

        /// <summary>
        /// Sets whether the result set should be paginated.
        /// </summary>
        /// <param name="connectionBuilder">The field builder.</param>
        /// <param name="paginate">Should the result be paginated?</param>
        /// <returns>The <see cref="ConnectionBuilder{TSourceType}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="connectionBuilder"/> is <c>null</c>.</exception>
        public static ConnectionBuilder<TSourceType> SqlPaginate<TSourceType>(this ConnectionBuilder<TSourceType> connectionBuilder, bool paginate = true)
        {
            if (connectionBuilder == null) throw new ArgumentNullException(nameof(connectionBuilder));

            connectionBuilder.FieldType.SqlPaginate(paginate);
            return connectionBuilder;
        }
    }

}

using System;
using GraphQL.Builders;
using JoinMonster.Builders;
using JoinMonster.Configs;

namespace JoinMonster
{
    /// <summary>
    /// Extension methods for <see cref="FieldBuilder"/>.
    /// </summary>
    public static class FieldBuilderExtensions
    {
        /// <summary>
        /// Configure the SQL column for the field.
        /// </summary>
        /// <param name="fieldBuilder">The <see cref="FieldBuilder{TSourceType, TTargetType}"/>.</param>
        /// <param name="columnName">The column name, if null the field name is used.</param>
        /// <param name="ignore"><c>true</c> if the column should be ignored, otherwise <c>false</c>.</param>
        /// <param name="configure">The configuration action.</param>
        /// <returns>The <see cref="FieldBuilder{TSourceType, TTargetType}"/></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldBuilder"/> is <c>null</c>.</exception>
        public static FieldBuilder<TSourceType, TTargetType> SqlColumn<TSourceType, TTargetType>(
                this FieldBuilder<TSourceType, TTargetType> fieldBuilder,
                string? columnName = null,
                bool ignore = false,
                Action<SqlColumnConfigBuilder>? configure = null)
        {
            if (fieldBuilder == null) throw new ArgumentNullException(nameof(fieldBuilder));

            var columnBuilder = fieldBuilder.FieldType.SqlColumn(columnName, ignore);

            if (configure is not null)
                configure(columnBuilder);

            return fieldBuilder;
        }

        /// <summary>
        /// Set a method that resolves the <c>WHERE</c> condition.
        /// </summary>
        /// <param name="fieldBuilder">The field builder.</param>
        /// <param name="where">The WHERE condition resolver.</param>
        /// <returns>The <see cref="FieldBuilder{TSourceType, TTargetType}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldBuilder"/> or <paramref name="where"/> is <c>null</c>.</exception>
        public static FieldBuilder<TSourceType, TTargetType> SqlWhere<TSourceType, TTargetType>(this FieldBuilder<TSourceType, TTargetType> fieldBuilder, WhereDelegate where)
        {
            if (fieldBuilder == null) throw new ArgumentNullException(nameof(fieldBuilder));
            if (where == null) throw new ArgumentNullException(nameof(where));

            fieldBuilder.FieldType.SqlWhere(where);

            return fieldBuilder;
        }

        /// <summary>
        /// Set a method that resolves the <c>JOIN</c> condition.
        /// </summary>
        /// <param name="fieldBuilder">The field builder.</param>
        /// <param name="join">The JOIN condition resolver.</param>
        /// <returns>The <see cref="FieldBuilder{TSourceType, TTargetType}"/></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldBuilder"/> or <paramref name="join"/> is <c>null</c>.</exception>
        public static FieldBuilder<TSourceType, TTargetType> SqlJoin<TSourceType, TTargetType>(this FieldBuilder<TSourceType, TTargetType> fieldBuilder, JoinDelegate join)
        {
            if (fieldBuilder == null) throw new ArgumentNullException(nameof(fieldBuilder));
            if (join == null) throw new ArgumentNullException(nameof(join));

            fieldBuilder.FieldType.SqlJoin(join);

            return fieldBuilder;
        }

        /// <summary>
        /// Configure a SQL Junction.
        /// </summary>
        /// <param name="fieldBuilder">The field builder.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="fromParent">The JOIN condition when joining from the parent table to the junction table.</param>
        /// <param name="toChild">The JOIN condition when joining from the junction table to the child table.</param>
        /// <param name="configure">The configuration action.</param>
        /// <returns>The <see cref="FieldBuilder{TSourceType, TTargetType}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldBuilder"/> is <c>null</c>.</exception>
        public static FieldBuilder<TSourceType, TTargetType> SqlJunction<TSourceType, TTargetType>(this FieldBuilder<TSourceType, TTargetType> fieldBuilder, string tableName, JoinDelegate fromParent, JoinDelegate toChild, Action<SqlJunctionConfigBuilder>? configure = null)
        {
            if (fieldBuilder == null) throw new ArgumentNullException(nameof(fieldBuilder));

            var junctionBuilder = fieldBuilder.FieldType.SqlJunction(tableName, fromParent, toChild);

            if (configure is not null)
                configure(junctionBuilder);

            return fieldBuilder;
        }

        /// <summary>
        /// Configure one to many SQL batching.
        /// </summary>
        /// <param name="fieldBuilder">The field builder.</param>
        /// <param name="thisKey">The column in this table.</param>
        /// <param name="parentKey">The column in the other table.</param>
        /// <param name="configure">The configuration action.</param>
        /// <returns>The <see cref="FieldBuilder{TSourceType, TTargetType}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldBuilder"/> is <c>null</c>.</exception>
        public static FieldBuilder<TSourceType, TTargetType> SqlBatch<TSourceType, TTargetType>(this FieldBuilder<TSourceType, TTargetType> fieldBuilder, string thisKey, string parentKey, Type keyType, Action<SqlBatchConfigBuilder>? configure = null)
        {
            if (fieldBuilder == null) throw new ArgumentNullException(nameof(fieldBuilder));

            var columnBuilder = fieldBuilder.FieldType.SqlBatch(thisKey, parentKey, keyType);

            if (configure is not null)
                configure(columnBuilder);

            return fieldBuilder;
        }

        /// <summary>
        /// Set a method that resolves the <c>ORDER BY</c> clause.
        /// </summary>
        /// <param name="fieldBuilder">The field builder.</param>
        /// <param name="orderBy">The <c>ORDER BY</c> builder.</param>
        /// <returns>The <see cref="FieldBuilder{TSourceType, TTargetType}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldBuilder"/> or <paramref name="orderBy"/> is <c>NULL</c>.</exception>
        public static FieldBuilder<TSourceType, TTargetType> SqlOrder<TSourceType, TTargetType>(this FieldBuilder<TSourceType, TTargetType> fieldBuilder, OrderByDelegate orderBy)
        {
            if (fieldBuilder == null) throw new ArgumentNullException(nameof(fieldBuilder));
            if (orderBy == null) throw new ArgumentNullException(nameof(orderBy));

            fieldBuilder.FieldType.SqlOrder(orderBy);

            return fieldBuilder;
        }

        /// <summary>
        /// Set a method that resolves the sort key used for keyset based pagination.
        /// </summary>
        /// <param name="fieldBuilder">The field builder.</param>
        /// <param name="sort">The <c>Sort Key</c> builder.</param>
        /// <returns>The <see cref="FieldBuilder{TSourceType, TTargetType}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldBuilder"/> or <paramref name="sort"/> is <c>NULL</c>.</exception>
        public static FieldBuilder<TSourceType, TTargetType> SqlSortKey<TSourceType, TTargetType>(this FieldBuilder<TSourceType, TTargetType> fieldBuilder, SortKeyDelegate sort)
        {
            if (fieldBuilder == null) throw new ArgumentNullException(nameof(fieldBuilder));
            if (sort == null) throw new ArgumentNullException(nameof(sort));

            fieldBuilder.FieldType.SqlSortKey(sort);

            return fieldBuilder;
        }

        /// <summary>
        /// Sets whether the result set should be paginated.
        /// </summary>
        /// <param name="fieldBuilder">The field builder.</param>
        /// <param name="paginate">Should the result be paginated?</param>
        /// <returns>The <see cref="FieldBuilder{TSourceType, TTargetType}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldBuilder"/> is <c>null</c>.</exception>
        public static FieldBuilder<TSourceType, TTargetType> SqlPaginate<TSourceType, TTargetType>(this FieldBuilder<TSourceType, TTargetType> fieldBuilder, bool paginate = true)
        {
            if (fieldBuilder == null) throw new ArgumentNullException(nameof(fieldBuilder));

            fieldBuilder.FieldType.SqlPaginate(paginate);
            return fieldBuilder;
        }
    }

}

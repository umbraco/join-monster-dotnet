using System;
using GraphQL;
using GraphQL.Utilities;
using JoinMonster.Builders;
using JoinMonster.Configs;
using JoinMonster.Resolvers;

namespace JoinMonster
{
    /// <summary>
    /// Extension methods for <see cref="FieldConfig"/>.
    /// </summary>
    public static class FieldConfigExtensions
    {
        /// <summary>
        /// Configure the SQL column for the field.
        /// </summary>
        /// <param name="fieldConfig">The <see cref="FieldConfig"/>.</param>
        /// <param name="columnName">The column name, if null the field name is used.</param>
        /// <param name="ignore"><c>true</c> if the column should be ignored, otherwise <c>false</c>.</param>
        /// <returns>The <see cref="SqlColumnConfigBuilder"/></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldConfig"/> is <c>null</c>.</exception>
        public static SqlColumnConfigBuilder SqlColumn(this FieldConfig fieldConfig, string? columnName = null, bool ignore = false)
        {
            if (fieldConfig == null) throw new ArgumentNullException(nameof(fieldConfig));

            var builder = SqlColumnConfigBuilder.Create(columnName ?? fieldConfig.Name);
            fieldConfig.WithMetadata(nameof(SqlColumnConfig), builder.SqlColumnConfig);

            if (ignore)
            {
                builder.SqlColumnConfig.Ignored = true;
            }
            else if (fieldConfig.Resolver == null)
            {
                fieldConfig.Resolver = DictionaryFieldResolver.Instance;
            }

            return builder;
        }

        /// <summary>
        /// Set a method that resolves the <c>WHERE</c> condition.
        /// </summary>
        /// <param name="fieldConfig">The field config.</param>
        /// <param name="where">The WHERE condition resolver.</param>
        /// <returns>The <see cref="FieldConfig"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldConfig"/> or <paramref name="where"/> is <c>null</c>.</exception>
        public static FieldConfig SqlWhere(this FieldConfig fieldConfig, WhereDelegate where)
        {
            if (fieldConfig == null) throw new ArgumentNullException(nameof(fieldConfig));
            if (where == null) throw new ArgumentNullException(nameof(where));

            return fieldConfig.WithMetadata(nameof(WhereDelegate), where);
        }

        /// <summary>
        /// Set a method that resolves the <c>JOIN</c> condition.
        /// </summary>
        /// <param name="fieldConfig">The field config.</param>
        /// <param name="join">The JOIN condition resolver.</param>
        /// <returns>The <see cref="FieldConfig"/></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldConfig"/> or <paramref name="join"/> is <c>null</c>.</exception>
        public static FieldConfig SqlJoin(this FieldConfig fieldConfig, JoinDelegate join)
        {
            if (fieldConfig == null) throw new ArgumentNullException(nameof(fieldConfig));
            if (join == null) throw new ArgumentNullException(nameof(join));

            fieldConfig.Resolver ??= DictionaryFieldResolver.Instance;

            return fieldConfig.WithMetadata(nameof(JoinDelegate), join);
        }

        /// <summary>
        /// Configure a SQL Junction.
        /// </summary>
        /// <param name="fieldConfig">The field config.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="fromParent">The JOIN condition when joining from the parent table to the junction table.</param>
        /// <param name="toChild">The JOIN condition when joining from the junction table to the child table.</param>
        /// <returns>The <see cref="SqlJunctionConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldConfig"/> is <c>null</c>.</exception>
        public static SqlJunctionConfigBuilder SqlJunction(this FieldConfig fieldConfig, string tableName, JoinDelegate fromParent, JoinDelegate toChild)
        {
            if (fieldConfig == null) throw new ArgumentNullException(nameof(fieldConfig));

            var builder = SqlJunctionConfigBuilder.Create(tableName, fromParent, toChild);
            fieldConfig.WithMetadata(nameof(SqlJunctionConfig), builder.SqlJunctionConfig);
            fieldConfig.Resolver ??= DictionaryFieldResolver.Instance;

            return builder;
        }

        /// <summary>
        /// Configure one to many SQL batching.
        /// </summary>
        /// <param name="fieldConfig">The field config.</param>
        /// <param name="thisKey">The column in this table.</param>
        /// <param name="parentKey">The column in the other table.</param>
        /// <param name="keyType">The type of the keys.</param>
        /// <returns>The <see cref="SqlBatchConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldConfig"/> is <c>null</c>.</exception>
        public static SqlBatchConfigBuilder SqlBatch(this FieldConfig fieldConfig, string thisKey, string parentKey, Type keyType)
        {
            if (fieldConfig == null) throw new ArgumentNullException(nameof(fieldConfig));

            var builder = SqlBatchConfigBuilder.Create(thisKey, parentKey, keyType);
            fieldConfig.WithMetadata(nameof(SqlBatchConfig), builder.SqlBatchConfig);
            fieldConfig.Resolver ??= DictionaryFieldResolver.Instance;

            return builder;
        }

        /// <summary>
        /// Set a method that resolves the <c>ORDER BY</c> clause.
        /// </summary>
        /// <param name="fieldConfig">The field config.</param>
        /// <param name="orderBy">The <c>ORDER BY</c> builder.</param>
        /// <returns>The <see cref="FieldConfig"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldConfig"/> or <paramref name="orderBy"/> is <c>NULL</c>.</exception>
        public static FieldConfig SqlOrder(this FieldConfig fieldConfig, OrderByDelegate orderBy)
        {
            if (fieldConfig == null) throw new ArgumentNullException(nameof(fieldConfig));
            if (orderBy == null) throw new ArgumentNullException(nameof(orderBy));

            return fieldConfig.WithMetadata(nameof(OrderByDelegate), orderBy);
        }

        /// <summary>
        /// Set a method that resolves the sort key used for keyset based pagination.
        /// </summary>
        /// <param name="fieldConfig">The field config.</param>
        /// <param name="sort">The <c>Sort Key</c> builder.</param>
        /// <returns>The <see cref="FieldConfig"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldConfig"/> or <paramref name="sort"/> is <c>NULL</c>.</exception>
        public static FieldConfig SqlSortKey(this FieldConfig fieldConfig, SortKeyDelegate sort)
        {
            if (fieldConfig == null) throw new ArgumentNullException(nameof(fieldConfig));
            if (sort == null) throw new ArgumentNullException(nameof(sort));

            return fieldConfig.WithMetadata(nameof(SortKeyDelegate), sort);
        }

        /// <summary>
        /// Sets whether the result set should be paginated.
        /// </summary>
        /// <param name="fieldConfig">The field config.</param>
        /// <param name="paginate">Should the result be paginated?</param>
        /// <returns>The <see cref="FieldConfig"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldConfig"/> is <c>null</c>.</exception>
        public static FieldConfig SqlPaginate(this FieldConfig fieldConfig, bool paginate = true)
        {
            if (fieldConfig == null) throw new ArgumentNullException(nameof(fieldConfig));

            fieldConfig.WithMetadata("JoinMonster.SqlPaginate", paginate);
            return fieldConfig;
        }
    }
}

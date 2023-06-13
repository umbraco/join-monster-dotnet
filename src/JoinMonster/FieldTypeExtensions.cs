using System;
using GraphQL;
using GraphQL.Types;
using JoinMonster.Builders;
using JoinMonster.Configs;
using JoinMonster.Resolvers;

namespace JoinMonster
{
    /// <summary>
    /// Extension methods for <see cref="FieldType"/>.
    /// </summary>
    public static class FieldTypeExtensions
    {
        /// <summary>
        /// Configure the SQL column for the field.
        /// </summary>
        /// <param name="fieldType">The <see cref="FieldType"/>.</param>
        /// <param name="columnName">The column name, if null the field name is used.</param>
        /// <param name="ignore"><c>true</c> if the column should be ignored, otherwise <c>false</c>.</param>
        /// <returns>The <see cref="SqlColumnConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> is <c>null</c>.</exception>
        public static SqlColumnConfigBuilder SqlColumn(this FieldType fieldType, string? columnName = null, bool ignore = false)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            var builder = SqlColumnConfigBuilder.Create(columnName ?? fieldType.Name);
            fieldType.WithMetadata(nameof(SqlColumnConfig), builder.SqlColumnConfig);

            if (ignore)
            {
                builder.SqlColumnConfig.Ignored = true;
            }
            else if(fieldType.Resolver == null)
            {
                fieldType.Resolver = DictionaryFieldResolver.Instance;
            }

            return builder;
        }

        /// <summary>
        /// Get the SQL column config.
        /// </summary>
        /// <param name="fieldType">The <see cref="FieldType"/>.</param>
        /// <returns>The <see cref="SqlColumnConfig"/> if set, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> is <c>null</c>.</exception>
        public static SqlColumnConfig? GetSqlColumnConfig(this FieldType fieldType)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            return fieldType.GetMetadata<SqlColumnConfig?>(nameof(SqlColumnConfig));
        }

        /// <summary>
        /// Set a method that resolves the WHERE condition.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <param name="where">The WHERE condition.</param>
        /// <returns>The <see cref="FieldType"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> or <paramref name="where"/> is <c>null</c>.</exception>
        public static FieldType SqlWhere(this FieldType fieldType, WhereDelegate where)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));
            if (where == null) throw new ArgumentNullException(nameof(where));

            return fieldType.WithMetadata(nameof(WhereDelegate), where);
        }

        /// <summary>
        /// Get the SQL WHERE condition resolver.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <returns>A <see cref="WhereDelegate"/> if one is set, otherwise null.</returns>
        public static WhereDelegate? GetSqlWhere(this FieldType fieldType)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            return fieldType.GetMetadata<WhereDelegate?>(nameof(WhereDelegate));
        }

        /// <summary>
        /// Set a method that resolves the <c>JOIN</c> condition.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <param name="join">The JOIN condition resolver.</param>
        /// <returns><see cref="FieldType"/>See <see cref="FieldType"/></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> or <paramref name="join"/> is <c>null</c>.</exception>
        public static FieldType SqlJoin(this FieldType fieldType, JoinDelegate join)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));
            if (join == null) throw new ArgumentNullException(nameof(join));

            fieldType.Resolver ??= DictionaryFieldResolver.Instance;

            return fieldType.WithMetadata(nameof(JoinDelegate), join);
        }

        /// <summary>
        /// Get the SQL JOIN condition resolver.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <returns>A <see cref="JoinDelegate"/> if one is set, otherwise null.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> is <c>null</c>.</exception>
        public static JoinDelegate? GetSqlJoin(this FieldType fieldType)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            return fieldType.GetMetadata<JoinDelegate?>(nameof(JoinDelegate));
        }

        /// <summary>
        /// Configure a SQL Junction.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="fromParent">The JOIN condition when joining from the parent table to the junction table.</param>
        /// <param name="toChild">The JOIN condition when joining from the junction table to the child table.</param>
        /// <returns>The <see cref="SqlJunctionConfig"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> is <c>null</c>.</exception>
        public static SqlJunctionConfigBuilder SqlJunction(this FieldType fieldType, string tableName, JoinDelegate fromParent, JoinDelegate toChild)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            var builder = SqlJunctionConfigBuilder.Create(tableName, fromParent, toChild);
            fieldType.WithMetadata(nameof(SqlJunctionConfig), builder.SqlJunctionConfig);
            fieldType.Resolver ??= DictionaryFieldResolver.Instance;

            return builder;
        }

        /// <summary>
        /// Create a new instance of the <see cref="SqlJunctionConfigBuilder"/> configured for batching the many-to-many query.
        /// </summary>
        /// <param name="fieldType">The <see cref="FieldType"/>.</param>
        /// <param name="tableName">The junction table name.</param>
        /// <param name="uniqueKey">The unique key columns.</param>
        /// <param name="thisKey">The column to match on the current table.</param>
        /// <param name="parentKey">The column to match in the parent table.</param>
        /// <param name="keyType">The type of the keys,</param>
        /// <param name="join">The JOIN condition when joining from the junction table to the related table.</param>
        /// <returns>The <see cref="SqlJunctionConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/>, <paramref name="tableName"/>, <paramref name="uniqueKey"/>, <paramref name="thisKey"/>, <paramref name="parentKey"/> or <paramref name="join"/> is <c>null</c>.</exception>
        public static SqlJunctionConfigBuilder SqlJunction(this FieldType fieldType, string tableName, string[] uniqueKey, string thisKey, string parentKey, Type keyType, JoinDelegate join)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            if (thisKey == null) throw new ArgumentNullException(nameof(thisKey));
            if (parentKey == null) throw new ArgumentNullException(nameof(parentKey));
            if (join == null) throw new ArgumentNullException(nameof(join));

            var builder = SqlJunctionConfigBuilder.Create(tableName, uniqueKey, thisKey, parentKey, keyType, join);
            fieldType.WithMetadata(nameof(SqlJunctionConfig), builder.SqlJunctionConfig);
            return builder;
        }

        /// <summary>
        /// Get the SQL Junction config.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <returns>A <see cref="SqlJunctionConfig"/> if one is set, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> is <c>null</c>.</exception>
        public static SqlJunctionConfig? GetSqlJunction(this FieldType fieldType)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            return fieldType.GetMetadata<SqlJunctionConfig?>(nameof(SqlJunctionConfig));
        }

        /// <summary>
        /// Configure one to many SQL batching.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <param name="thisKey">The column in this table.</param>
        /// <param name="parentKey">The column in the other table.</param>
        /// <param name="keyType">The type of the keys</param>
        /// <returns>The <see cref="SqlBatchConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> is <c>null</c>.</exception>
        public static SqlBatchConfigBuilder SqlBatch(this FieldType fieldType, string thisKey, string parentKey, Type keyType)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            var builder = SqlBatchConfigBuilder.Create(thisKey, parentKey, keyType);
            fieldType.WithMetadata(nameof(SqlBatchConfig), builder.SqlBatchConfig);
            fieldType.Resolver ??= DictionaryFieldResolver.Instance;

            return builder;
        }

        /// <summary>
        /// Get the SQL Batch config.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <returns>A <see cref="SqlBatchConfig"/> if one is set, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> is <c>null</c>.</exception>
        public static SqlBatchConfig? GetSqlBatch(this FieldType fieldType)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            return fieldType.GetMetadata<SqlBatchConfig?>(nameof(SqlBatchConfig));
        }

        /// <summary>
        /// Set a method that resolves the <c>ORDER BY</c> clause.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <param name="orderBy">The <c>ORDER BY</c> builder.</param>
        /// <returns>The <see cref="FieldType"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> or <paramref name="orderBy"/> is <c>NULL</c>.</exception>
        public static FieldType SqlOrder(this FieldType fieldType, OrderByDelegate orderBy)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));
            if (orderBy == null) throw new ArgumentNullException(nameof(orderBy));

            return fieldType.WithMetadata(nameof(OrderByDelegate), orderBy);
        }

        /// <summary>
        /// Get the SQL <c>ORDER BY</c> resolver.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <returns>A <see cref="OrderByDelegate"/> if one is set, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> is <c>null</c>.</exception>
        public static OrderByDelegate? GetSqlOrder(this FieldType fieldType)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            return fieldType.GetMetadata<OrderByDelegate?>(nameof(OrderByDelegate));
        }

        /// <summary>
        /// Set a method that resolves the sort key used for keyset based pagination.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <param name="sort">The <c>Sort Key</c> builder.</param>
        /// <returns>The <see cref="FieldType"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> or <paramref name="sort"/> is <c>NULL</c>.</exception>
        public static FieldType SqlSortKey(this FieldType fieldType, SortKeyDelegate sort)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));
            if (sort == null) throw new ArgumentNullException(nameof(sort));

            return fieldType.WithMetadata(nameof(SortKeyDelegate), sort);
        }

        /// <summary>
        /// Get the SQL <c>Sort Key</c> resolver.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <returns>A <see cref="SortKeyDelegate"/> if one is set, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> is <c>null</c>.</exception>
        public static SortKeyDelegate? GetSqlSortKey(this FieldType fieldType)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            return fieldType.GetMetadata<SortKeyDelegate?>(nameof(SortKeyDelegate));
        }

        /// <summary>
        /// Sets whether the result set should be paginated.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <param name="paginate">Should the result be paginated?</param>
        /// <returns>The <see cref="FieldType"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> is <c>null</c>.</exception>
        public static FieldType SqlPaginate(this FieldType fieldType, bool paginate = true)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            fieldType.WithMetadata("JoinMonster.SqlPaginate", paginate);
            return fieldType;
        }

        /// <summary>
        /// Get whether the SQL query should be paginated.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <returns><c>true</c> if the SQL query should be paginated, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> is <c>null</c>.</exception>
        public static bool? GetSqlPaginate(this FieldType fieldType)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            return fieldType.GetMetadata<bool?>("JoinMonster.SqlPaginate");
        }
    }
}

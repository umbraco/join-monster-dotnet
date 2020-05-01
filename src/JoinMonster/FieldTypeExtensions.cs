using System;
using GraphQL;
using GraphQL.Types;
using JoinMonster.Builders;
using JoinMonster.Configs;

namespace JoinMonster
{
    public static class FieldTypeExtensions
    {
        /// <summary>
        /// Configure the SQL column for the field.
        /// </summary>
        /// <param name="fieldType">The <see cref="FieldType"/>.</param>
        /// <param name="columnName">The column name, if null the field name is used.</param>
        /// <returns>The <see cref="SqlColumnConfigBuilder"/></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> is <c>null</c>.</exception>
        public static SqlColumnConfigBuilder SqlColumn(this FieldType fieldType, string? columnName = null)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            var builder = SqlColumnConfigBuilder.Create(columnName ?? fieldType.Name);
            fieldType.WithMetadata(nameof(SqlColumnConfig), builder.SqlColumnConfig);
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

            return fieldType.GetMetadata<SqlColumnConfig>(nameof(SqlColumnConfig));
        }

        /// <summary>
        /// Set a method that resolves the WHERE condition.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <param name="where">The WHERE condition condition.</param>
        /// <returns>The <see cref="FieldType"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> or <see cref="where"/> is <c>null</c>.</exception>
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

            return fieldType.GetMetadata<WhereDelegate>(nameof(WhereDelegate));
        }

        /// <summary>
        /// Set a method that resolves the LEFT JOIN condition.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <param name="join">The JOIN condition resolver.</param>
        /// <returns><see cref="FieldType"/>See <see cref="FieldType"/></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldType"/> or <paramref name="join"/> is <c>null</c>.</exception>
        public static FieldType SqlJoin(this FieldType fieldType, JoinDelegate join)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));
            if (join == null) throw new ArgumentNullException(nameof(join));

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

            return fieldType.GetMetadata<JoinDelegate>(nameof(JoinDelegate));
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

            return fieldType.GetMetadata<SqlJunctionConfig>(nameof(SqlJunctionConfig));
        }
    }
}

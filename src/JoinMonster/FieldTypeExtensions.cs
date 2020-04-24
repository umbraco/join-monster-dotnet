using System;
using GraphQL;
using GraphQL.Types;
using JoinMonster.Builders;

namespace JoinMonster
{
    public static class FieldTypeExtensions
    {
        public static SqlColumnConfigBuilder SqlColumn(this FieldType fieldType, string? columnName = null)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            var builder = SqlColumnConfigBuilder.Create(columnName ?? fieldType.Name);
            fieldType.WithMetadata(nameof(SqlColumnConfig), builder.SqlColumnConfig);
            return builder;
        }

        public static SqlColumnConfig? GetSqlColumnConfig(this FieldType fieldType)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            return fieldType.GetMetadata<SqlColumnConfig>(nameof(SqlColumnConfig));
        }


        /// <summary>
        /// Set a method that resolves the where clause.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <param name="where">The where clause resolver.</param>
        /// <returns>The <see cref="FieldType"/>.</returns>
        public static FieldType SqlWhere(this FieldType fieldType, WhereDelegate where)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));
            if (@where == null) throw new ArgumentNullException(nameof(where));

            return fieldType.WithMetadata(nameof(WhereDelegate), where);
        }

        /// <summary>
        /// Get the SQL Where expression.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <returns>A <see cref="WhereDelegate"/> if one is set, otherwise null.</returns>
        public static WhereDelegate? GetSqlWhere(this FieldType fieldType)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            return fieldType.GetMetadata<WhereDelegate>(nameof(WhereDelegate));
        }
    }
}

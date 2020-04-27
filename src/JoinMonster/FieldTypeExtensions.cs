using System;
using GraphQL;
using GraphQL.Types;
using JoinMonster.Builders;
using JoinMonster.Configs;

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
        /// Set a method that resolves the WHERE condition.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <param name="where">The WHERE condition condition.</param>
        /// <returns>The <see cref="FieldType"/>.</returns>
        public static FieldType SqlWhere(this FieldType fieldType, WhereDelegate where)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));
            if (@where == null) throw new ArgumentNullException(nameof(where));

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
        public static JoinDelegate? GetSqlJoin(this FieldType fieldType)
        {
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            return fieldType.GetMetadata<JoinDelegate>(nameof(JoinDelegate));
        }
    }
}

using GraphQL;
using GraphQL.Types;
using JoinMonster.Builders;

namespace JoinMonster
{
    public static class FieldTypeExtensions
    {
        public static SqlColumnConfigBuilder SqlColumn(this FieldType fieldType, string? columnName = null)
        {
            var builder = SqlColumnConfigBuilder.Create(columnName);
            fieldType.WithMetadata(nameof(SqlColumnConfig), builder.SqlColumnConfig);
            return builder;
        }

        public static SqlColumnConfig? GetSqlColumnConfig(this FieldType fieldType) =>
            fieldType.GetMetadata<SqlColumnConfig>(nameof(SqlColumnConfig));

        /// <summary>
        /// Get the SQL Where expression.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        /// <returns>A <see cref="WhereDelegate"/> if one is set, otherwise null.</returns>
        public static WhereDelegate? GetSqlWhere(this FieldType fieldType) =>
            fieldType.GetMetadata<WhereDelegate>(nameof(WhereDelegate));
    }
}

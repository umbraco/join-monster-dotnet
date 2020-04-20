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
    }
}

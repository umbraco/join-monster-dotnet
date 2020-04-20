using GraphQL;
using GraphQL.Utilities;
using JoinMonster.Builders;

namespace JoinMonster
{
    public static class FieldConfigExtensions
    {
        public static SqlColumnConfigBuilder SqlColumn(this FieldConfig fieldConfig, string? columnName = null)
        {
            var builder = SqlColumnConfigBuilder.Create(columnName);
            fieldConfig.WithMetadata(nameof(SqlColumnConfig), builder.SqlColumnConfig);
            return builder;
        }
    }
}

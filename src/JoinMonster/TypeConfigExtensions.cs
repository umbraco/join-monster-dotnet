using GraphQL;
using GraphQL.Utilities;
using JoinMonster.Builders;

namespace JoinMonster
{
    public static class TypeConfigExtensions
    {
        public static SqlTableConfigBuilder SqlTable(this TypeConfig typeConfig, string tableName, string uniqueKey) =>
            SqlTable(typeConfig, tableName, new[] {uniqueKey});

        public static SqlTableConfigBuilder SqlTable(this TypeConfig typeConfig, string tableName, string[] uniqueKey)
        {
            var builder = SqlTableConfigBuilder.Create(tableName, uniqueKey);
            typeConfig.WithMetadata(nameof(SqlTableConfig), builder.SqlTableConfig);
            return builder;
        }
    }
}

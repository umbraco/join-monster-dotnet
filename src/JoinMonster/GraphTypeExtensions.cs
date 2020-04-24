using System;
using GraphQL;
using GraphQL.Types;
using JoinMonster.Builders;

namespace JoinMonster
{
    public static class GraphTypeExtensions
    {
        public static SqlTableConfigBuilder SqlTable(this IGraphType graphType, string tableName, string uniqueKey)
        {
            if (graphType == null) throw new ArgumentNullException(nameof(graphType));

            return SqlTable(graphType, tableName, new[] {uniqueKey});
        }

        public static SqlTableConfigBuilder SqlTable(this IGraphType graphType, string tableName, string[] uniqueKey)
        {
            if (graphType == null) throw new ArgumentNullException(nameof(graphType));

            var builder = SqlTableConfigBuilder.Create(tableName, uniqueKey);
            graphType.WithMetadata(nameof(SqlTableConfig) ,builder.SqlTableConfig);
            return builder;
        }

        public static SqlTableConfig? GetSqlTableConfig(this IGraphType graphType) =>
            graphType?.GetMetadata<SqlTableConfig>(nameof(SqlTableConfig));

        public static bool IsListType(this IGraphType graphType)
        {
            if (graphType == null) throw new ArgumentNullException(nameof(graphType));

            if (graphType is NonNullGraphType nonNullGraphType)
                graphType = nonNullGraphType.ResolvedType;

            if (graphType is ListGraphType)
                return true;

            return false;
        }
    }
}

using System;
using GraphQL;
using GraphQL.Types;
using GraphQL.Types.Relay;
using JoinMonster.Builders;
using JoinMonster.Configs;

namespace JoinMonster
{
    public static class GraphTypeExtensions
    {
        public static SqlTableConfigBuilder SqlTable(this IGraphType graphType, string tableName, string uniqueKey) =>
            SqlTable(graphType, tableName, new[] {uniqueKey});

        public static SqlTableConfigBuilder SqlTable(this IGraphType graphType, string tableName, string[] uniqueKey)
        {
            if (graphType == null) throw new ArgumentNullException(nameof(graphType));

            var builder = SqlTableConfigBuilder.Create(tableName, uniqueKey);
            graphType.WithMetadata(nameof(SqlTableConfig) ,builder.SqlTableConfig);
            return builder;
        }

        public static SqlTableConfig? GetSqlTableConfig(this IGraphType graphType)
        {
            if (graphType == null) throw new ArgumentNullException(nameof(graphType));

            return graphType.GetMetadata<SqlTableConfig>(nameof(SqlTableConfig));
        }

        public static bool IsListType(this IGraphType graphType)
        {
            if (graphType == null) throw new ArgumentNullException(nameof(graphType));

            if (graphType is NonNullGraphType nonNullGraphType)
                graphType = nonNullGraphType.ResolvedType;

            if (graphType is ListGraphType)
                return true;

            return false;
        }

        public static bool IsConnectionType(this IGraphType graphType)
        {
            if (graphType == null) throw new ArgumentNullException(nameof(graphType));

            var type = graphType.GetType();

            var isConnection = type.ImplementsGenericType(typeof(ConnectionType<,>));
            if (isConnection)
                return true;

            if (graphType is IComplexGraphType complexGraphType && complexGraphType.HasField("edges"))
            {
                if (complexGraphType.GetField("edges").ResolvedType.GetNamedType() is IComplexGraphType edgesType)
                    return edgesType.HasField("node");
            }

            return false;
        }
    }
}

using System;
using GraphQL;
using GraphQL.Types;
using GraphQL.Types.Relay;
using JoinMonster.Builders;
using JoinMonster.Configs;

namespace JoinMonster
{
    /// <summary>
    /// Extension methods for <see cref="IGraphType"/>.
    /// </summary>
    public static class GraphTypeExtensions
    {
        /// <summary>
        /// Configure the SQL table for the graph type.
        /// </summary>
        /// <param name="graphType">The graph type.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="uniqueKey">The unique key column.</param>
        /// <returns>The <see cref="SqlTableConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="graphType"/>, <paramref name="tableName"/> or <paramref name="uniqueKey"/> is <c>null</c>.</exception>
        public static SqlTableConfigBuilder SqlTable(this IGraphType graphType, string tableName, string uniqueKey) =>
            SqlTable(graphType, tableName, new[] {uniqueKey});

        /// <summary>
        /// Configure the SQL table for the graph type.
        /// </summary>
        /// <param name="graphType">The graph type.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="uniqueKey">The unique key column.</param>
        /// <returns>The <see cref="SqlTableConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="graphType"/>, <paramref name="tableName"/> or <paramref name="uniqueKey"/> is <c>null</c>.</exception>
        public static SqlTableConfigBuilder SqlTable(this IGraphType graphType, string tableName, string[] uniqueKey)
        {
            if (graphType == null) throw new ArgumentNullException(nameof(graphType));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            if (uniqueKey == null) throw new ArgumentNullException(nameof(uniqueKey));

            var builder = SqlTableConfigBuilder.Create(tableName, uniqueKey);
            graphType.WithMetadata(nameof(SqlTableConfig) ,builder.SqlTableConfig);
            return builder;
        }

        /// <summary>
        /// Get the SQL table configuration.
        /// </summary>
        /// <param name="graphType">The graph type.</param>
        /// <returns>The <see cref="SqlTableConfig"/> if any, otherwise <c>null</c></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="graphType"/> is <c>null</c>.</exception>
        public static SqlTableConfig? GetSqlTableConfig(this IGraphType graphType)
        {
            if (graphType == null) throw new ArgumentNullException(nameof(graphType));

            return graphType.GetMetadata<SqlTableConfig>(nameof(SqlTableConfig));
        }

        /// <summary>
        /// Checks if the <see cref="IGraphType"/> is a <see cref="ListGraphType"/>.
        /// </summary>
        /// <param name="graphType">The graph type to check.</param>
        /// <returns><c>true</c> if the <see cref="IGraphType"/> is a <see cref="ListGraphType"/>, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="graphType"/> is <c>null</c>.</exception>
        public static bool IsListType(this IGraphType graphType)
        {
            if (graphType == null) throw new ArgumentNullException(nameof(graphType));

            if (graphType is NonNullGraphType nonNullGraphType)
                graphType = nonNullGraphType.ResolvedType;

            if (graphType is ListGraphType)
                return true;

            return false;
        }

        /// <summary>
        /// Checks if the <see cref="IGraphType"/> is a Connection.
        /// </summary>
        /// <remarks>
        /// A <see cref="IGraphType"/> is considered a Connection if it either inherits from <see cref="ConnectionType{TNodeType,TEdgeType}"/> or has a <c>edges</c> field which has a <c>node</c> field.
        /// </remarks>
        /// <param name="graphType">The graph type to check.</param>
        /// <returns><c>true</c> if the <see cref="IGraphType"/> is a Connection, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="graphType"/> is <c>null</c>.</exception>
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

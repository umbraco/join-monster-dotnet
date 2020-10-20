using System;
using GraphQL;
using GraphQL.Utilities;
using JoinMonster.Builders;
using JoinMonster.Configs;

namespace JoinMonster
{
    /// <summary>
    /// Extension methods for <see cref="TypeConfig"/>.
    /// </summary>
    public static class TypeConfigExtensions
    {
        /// <summary>
        /// Configure the SQL table for the graph type.
        /// </summary>
        /// <param name="typeConfig">The type config.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="uniqueKey">The unique key column.</param>
        /// <returns>The <see cref="SqlTableConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="typeConfig"/>, <paramref name="tableName"/> or <paramref name="uniqueKey"/> is <c>null</c>.</exception>
        public static SqlTableConfigBuilder SqlTable(this TypeConfig typeConfig, string tableName, string uniqueKey) =>
            SqlTable(typeConfig, tableName, new[] {uniqueKey});

        /// <summary>
        /// Configure the SQL table for the graph type.
        /// </summary>
        /// <param name="typeConfig">The type config.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="uniqueKey">The unique key column.</param>
        /// <returns>The <see cref="SqlTableConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="typeConfig"/>, <paramref name="tableName"/> or <paramref name="uniqueKey"/> is <c>null</c>.</exception>
        public static SqlTableConfigBuilder SqlTable(this TypeConfig typeConfig, string tableName, string[] uniqueKey) =>
            SqlTable(typeConfig, (_, __) => tableName, uniqueKey);

        /// <summary>
        /// Configure the SQL table for the graph type.
        /// </summary>
        /// <param name="typeConfig">The type config.</param>
        /// <param name="table">The table expression.</param>
        /// <param name="uniqueKey">The unique key column.</param>
        /// <returns>The <see cref="SqlTableConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="typeConfig"/>, <paramref name="table"/> or <paramref name="uniqueKey"/> is <c>null</c>.</exception>
        public static SqlTableConfigBuilder SqlTable(this TypeConfig typeConfig, TableExpressionDelegate table, string uniqueKey) =>
            SqlTable(typeConfig, table, new[] {uniqueKey});

        /// <summary>
        /// Configure the SQL table for the graph type.
        /// </summary>
        /// <param name="typeConfig">The type config.</param>
        /// <param name="table">The table expression.</param>
        /// <param name="uniqueKey">The unique key column.</param>
        /// <returns>The <see cref="SqlTableConfigBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="typeConfig"/>, <paramref name="table"/> or <paramref name="uniqueKey"/> is <c>null</c>.</exception>
        public static SqlTableConfigBuilder SqlTable(this TypeConfig typeConfig, TableExpressionDelegate table, string[] uniqueKey)
        {
            if (typeConfig == null) throw new ArgumentNullException(nameof(typeConfig));
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (uniqueKey == null) throw new ArgumentNullException(nameof(uniqueKey));

            var builder = SqlTableConfigBuilder.Create(table, uniqueKey);
            typeConfig.WithMetadata(nameof(SqlTableConfig) ,builder.SqlTableConfig);
            return builder;
        }
    }
}

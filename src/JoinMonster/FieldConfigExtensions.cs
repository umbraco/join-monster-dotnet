using GraphQL;
using GraphQL.Utilities;
using JoinMonster.Builders;

namespace JoinMonster
{
    public static class FieldConfigExtensions
    {
        public static SqlColumnConfigBuilder SqlColumn(this FieldConfig fieldConfig, string? columnName = null)
        {
            var builder = SqlColumnConfigBuilder.Create(columnName ?? fieldConfig.Name);
            fieldConfig.WithMetadata(nameof(SqlColumnConfig), builder.SqlColumnConfig);
            return builder;
        }

        /// <summary>
        /// Set a method that resolves the where clause.
        /// </summary>
        /// <param name="fieldConfig">The field config.</param>
        /// <param name="where">The where clause resolver.</param>
        /// <returns><see cref="FieldConfig"/>.</returns>
        public static FieldConfig SqlWhere(this FieldConfig fieldConfig, WhereDelegate where) =>
            fieldConfig.WithMetadata(nameof(WhereDelegate), where);
    }
}

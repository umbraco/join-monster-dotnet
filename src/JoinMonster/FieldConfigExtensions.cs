using System;
using GraphQL;
using GraphQL.Utilities;
using JoinMonster.Builders;
using JoinMonster.Configs;

namespace JoinMonster
{
    public static class FieldConfigExtensions
    {
        /// <summary>
        /// Configure the SQL column for the field.
        /// </summary>
        /// <param name="fieldConfig">The <see cref="FieldConfig"/>.</param>
        /// <param name="columnName">The column name, if null the field name is used.</param>
        /// <returns>The <see cref="SqlColumnConfigBuilder"/></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldConfig"/> is <c>null</c>.</exception>
        public static SqlColumnConfigBuilder SqlColumn(this FieldConfig fieldConfig, string? columnName = null)
        {
            if (fieldConfig == null) throw new ArgumentNullException(nameof(fieldConfig));

            var builder = SqlColumnConfigBuilder.Create(columnName ?? fieldConfig.Name);
            fieldConfig.WithMetadata(nameof(SqlColumnConfig), builder.SqlColumnConfig);
            return builder;
        }

        /// <summary>
        /// Set a method that resolves the WHERE condition.
        /// </summary>
        /// <param name="fieldConfig">The field config.</param>
        /// <param name="where">The WHERE condition resolver.</param>
        /// <returns>The <see cref="FieldConfig"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldConfig"/> or <see cref="where"/> is <c>null</c>.</exception>
        public static FieldConfig SqlWhere(this FieldConfig fieldConfig, WhereDelegate where)
        {
            if (fieldConfig == null) throw new ArgumentNullException(nameof(fieldConfig));
            if (where == null) throw new ArgumentNullException(nameof(where));

            return fieldConfig.WithMetadata(nameof(WhereDelegate), where);
        }

        /// <summary>
        /// Set a method that resolves the LEFT JOIN condition.
        /// </summary>
        /// <param name="fieldConfig">The field config.</param>
        /// <param name="join">The JOIN condition resolver.</param>
        /// <returns>The <see cref="FieldConfig"/></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldConfig"/> or <see cref="join"/> is <c>null</c>.</exception>
        public static FieldConfig SqlJoin(this FieldConfig fieldConfig, JoinDelegate join)
        {
            if (fieldConfig == null) throw new ArgumentNullException(nameof(fieldConfig));
            if (join == null) throw new ArgumentNullException(nameof(join));

            return fieldConfig.WithMetadata(nameof(JoinDelegate), join);
        }

        /// <summary>
        /// Create a new instance of the <see cref="SqlJunctionConfig"/>
        /// </summary>
        /// <param name="fieldConfig">The field config.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="fromParent">The JOIN condition when joining from the parent table to the junction table.</param>
        /// <param name="toChild">The JOIN condition when joining from the junction table to the child table.</param>
        /// <returns>The <see cref="SqlJunctionConfig"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fieldConfig"/> is <c>null</c>.</exception>
        public static SqlJunctionConfigBuilder SqlJunction(this FieldConfig fieldConfig, string tableName, JoinDelegate fromParent, JoinDelegate toChild)
        {
            if (fieldConfig == null) throw new ArgumentNullException(nameof(fieldConfig));

            var builder = SqlJunctionConfigBuilder.Create(tableName, fromParent, toChild);
            fieldConfig.WithMetadata(nameof(SqlJunctionConfig), builder.SqlJunctionConfig);
            return builder;
        }
    }
}

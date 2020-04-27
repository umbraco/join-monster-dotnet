using System;
using GraphQL;
using GraphQL.Utilities;
using JoinMonster.Builders;
using JoinMonster.Configs;

namespace JoinMonster
{
    public static class FieldConfigExtensions
    {
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
        /// <returns><see cref="FieldConfig"/>The <see cref="FieldConfig"/>.</returns>
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
        /// <returns><see cref="FieldConfig"/>See <see cref="FieldConfig"/></returns>
        public static FieldConfig SqlJoin(this FieldConfig fieldConfig, JoinDelegate join)
        {
            if (fieldConfig == null) throw new ArgumentNullException(nameof(fieldConfig));
            if (join == null) throw new ArgumentNullException(nameof(join));

            return fieldConfig.WithMetadata(nameof(JoinDelegate), join);
        }
    }
}

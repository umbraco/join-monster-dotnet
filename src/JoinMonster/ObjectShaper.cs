using System;
using JoinMonster.Language.AST;
using NestHydration;

namespace JoinMonster
{
    internal class ObjectShaper
    {
        private readonly SqlAstValidator _sqlAstValidator;

        public ObjectShaper(SqlAstValidator sqlAstValidator)
        {
            _sqlAstValidator = sqlAstValidator ?? throw new ArgumentNullException(nameof(sqlAstValidator));
        }

        public Definition DefineObjectShape(SqlTable node)
        {
            _sqlAstValidator.Validate(node);

            return new Definition
            {
                Properties = DefineObjectShape(null, null, node)
            };
        }

        private Properties DefineObjectShape(SqlTable? parent, string? prefix, SqlTable node)
        {
            prefix = parent == null ? prefix : prefix + node.As + "__";

            var properties = new Properties();

            foreach (var child in node.Children)
            {
                switch (child)
                {
                    case SqlColumnBase sqlColumn:
                        properties.Add(new Property(sqlColumn.FieldName)
                        {
                            Column = $"{prefix}{sqlColumn.As}",
                            IsId = sqlColumn.IsId
                        });
                        break;
                    case SqlTable sqlTable:
                        var childProperties = DefineObjectShape(node, prefix, sqlTable);
                        IProperty property;
                        if (sqlTable.GrabMany)
                            property = new PropertyArray(sqlTable.FieldName, childProperties);
                        else
                            property = new PropertyObject(sqlTable.FieldName, childProperties);

                        properties.Add(property);
                        break;
                    case SqlJunction _:
                    case SqlNoop _:
                    case ValueNode _:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(child), $"Unknown type {child.GetType()}");
                }
            }

            return properties;
        }
    }
}

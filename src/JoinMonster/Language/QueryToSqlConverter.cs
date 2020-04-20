using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Language.AST;
using GraphQL.Types;
using JoinMonster.Language.AST;

namespace JoinMonster.Language
{
    public class QueryToSqlConverter
    {
        public Node Convert(IResolveFieldContext context)
        {
            var fieldAst = context.FieldAst;
            var parentType = context.ReturnType;

            return Convert(fieldAst, parentType, context.UserContext);
        }

        private Node Convert(Field fieldAst, IGraphType parentType, IDictionary<string, object> userContext)
        {
            if (parentType is IObjectGraphType objectGraphType)
            {
                var config = parentType.GetSqlTableConfig();
                if (config != null)
                    return HandleTable(fieldAst, objectGraphType, config, userContext);
            }

            return new SqlNoop();
        }

        private Node HandleTable(Field fieldAst, IObjectGraphType graphType, SqlTableConfig config, IDictionary<string, object> userContext)
        {
            var tableName = config.Table;
            var tableAs = fieldAst.Name;

            var sqlColumns = new SqlColumns();

            HandleSelections(sqlColumns, graphType, fieldAst.SelectionSet.Selections);

            return new SqlTable(tableName, tableAs, sqlColumns).WithLocation(fieldAst.SourceLocation);
        }

        private void HandleSelections(SqlColumns sqlColumns, IComplexGraphType graphType, IEnumerable<ISelection> selections)
        {
            foreach (var selection in selections)
            {
                switch (selection)
                {
                    case Field field:

                        var fieldType = graphType.GetField(field.Name);
                        var columnConfig = fieldType.GetSqlColumnConfig();

                        if (columnConfig?.Ignored == true)
                            continue;

                        var fieldName = field.Name;
                        var columnName = columnConfig?.Column ?? fieldName;
                        var columnAs = fieldName;

                        var sqlColumn = new SqlColumn(columnName, fieldName, columnAs)
                            .WithLocation(field.SourceLocation);

                        sqlColumns.Add(sqlColumn);
                        break;
                    case InlineFragment _:
                        break;
                    case FragmentSpread _:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unknown selection kind: {selection.GetType()}");
                }
            }
        }
    }
}

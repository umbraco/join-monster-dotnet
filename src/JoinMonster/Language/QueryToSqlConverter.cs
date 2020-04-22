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
            var field = context.FieldDefinition;
            var parentType = context.ReturnType.GetNamedType();

            return Convert(fieldAst, field, parentType, context.UserContext);
        }

        private Node Convert(Field fieldAst, FieldType field, IGraphType parentType,
            IDictionary<string, object> userContext)
        {
            if (parentType is IComplexGraphType complexGraphType)
            {
                var config = parentType.GetSqlTableConfig();
                if (config != null)
                    return HandleTable(fieldAst, field, complexGraphType, config, userContext);
            }

            return new SqlNoop();
        }

        private Node HandleTable(Field fieldAst, FieldType field, IComplexGraphType graphType,
            SqlTableConfig config, IDictionary<string, object> userContext)
        {
            var tableName = config.Table;
            var tableAs = fieldAst.Name;

            var columns = config.UniqueKey.Union(config.AlwaysFetch ?? Enumerable.Empty<string>())
                .ToDictionary(
                    x => x,
                    column => new SqlColumn(column, null, column)
                );

            HandleSelections(columns, graphType, fieldAst.SelectionSet.Selections);

            var sqlTable = new SqlTable(tableName, tableAs, new SqlColumns(columns.Values)).WithLocation(fieldAst.SourceLocation);
            sqlTable.Where = field.GetSqlWhere();
            return sqlTable;
        }

        private void HandleSelections(IDictionary<string, SqlColumn> sqlColumns, IComplexGraphType graphType, IEnumerable<ISelection> selections)
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

                        sqlColumns[sqlColumn.As] = sqlColumn;
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

using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Language.AST;
using GraphQL.Types;
using JoinMonster.Builders;
using JoinMonster.Configs;
using JoinMonster.Language.AST;

namespace JoinMonster.Language
{
    /// <summary>
    /// The <see cref="QueryToSqlConverter"/> is responsible for converting GraphQL Query Ast to SQL Ast.
    /// </summary>
    public class QueryToSqlConverter
    {
        /// <summary>
        /// Convert the GraphQL Query Ast to SQL Ast.
        /// </summary>
        /// <param name="context">The <see cref="IResolveFieldContext"/>.</param>
        /// <returns>A <see cref="Node"/> representing the SQL Ast.</returns>
        /// <exception cref="ArgumentNullException">If <c>context</c> is null.</exception>
        public virtual SqlTable Convert(IResolveFieldContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var fieldAst = context.FieldAst;
            var field = context.FieldDefinition;
            var parentType = context.ParentType.GetNamedType();

            var node = Convert(fieldAst, field, parentType, 0, context.UserContext);
            if (node is SqlTable sqlTable)
                return sqlTable;

            throw new JoinMonsterException($"Expected node to be of type '{typeof(SqlTable)}' but was '{node.GetType()}'.");
        }

        private Node Convert(Field fieldAst, FieldType field, IGraphType parentType, int depth,
            IDictionary<string, object> userContext)
        {
            var sqlColumnConfig = field.GetSqlColumnConfig();
            if (sqlColumnConfig?.Ignored == true)
                return new SqlNoop();

            var gqlType = field.ResolvedType.GetNamedType();

            if (gqlType is IComplexGraphType complexGraphType)
            {
                if (complexGraphType.IsConnectionType())
                    (complexGraphType, fieldAst) = StripRelayConnection(complexGraphType, fieldAst);

                var sqlTableConfig = complexGraphType.GetSqlTableConfig();

                if(sqlTableConfig == null)
                    return new SqlNoop();

                if (depth >= 1)
                {
                    //TODO: Validate that either join, batch or junction is set on the field
                }

                return HandleTable(fieldAst, field, complexGraphType, sqlTableConfig, depth, userContext);
            }

            // TODO: Looks like we can't check if field.Resolver is null because FieldMiddleware is registered as a resolver
            // if (sqlColumnConfig != null || field.Resolver == null)
                return HandleColumn(fieldAst, field, gqlType, sqlColumnConfig, depth, userContext);

            // return new SqlNoop();
        }

        private Node HandleTable(Field fieldAst, FieldType field, IComplexGraphType graphType,
            SqlTableConfig config, int depth, IDictionary<string, object> userContext)
        {
            var fieldName = fieldAst.Name;
            var tableName = config.Table;
            var tableAs = fieldAst.Name;

            var columns = new List<SqlColumnBase>();

            if (config.UniqueKey.Length == 1)
            {
                columns.Add(new SqlColumn(config.UniqueKey[0], config.UniqueKey[0], config.UniqueKey[0], true));
            }
            else
            {
                var clumsyName = string.Join("#", config.UniqueKey);
                columns.Add(new SqlComposite(config.UniqueKey, clumsyName, clumsyName, true));
            }

            if (config.AlwaysFetch != null)
            {
                foreach (var column in config.AlwaysFetch)
                    columns.Add(new SqlColumn(column, column, column));
            }

            var tables = new List<SqlTable>();

            var arguments = HandleArguments(fieldAst);
            var grabMany = field.ResolvedType.IsListType();
            var where = field.GetSqlWhere();
            var join = field.GetSqlJoin();
            var junction = field.GetSqlJunction();
            var orderBy = field.GetSqlOrder();
            var sortKey = field.GetSqlSortKey();
            var paginate = false;

            if (!grabMany && field.ResolvedType.IsConnectionType())
            {
                grabMany = true;
                paginate = field.GetSqlPaginate().GetValueOrDefault(false);
            }

            HandleSelections(columns, tables, graphType, fieldAst.SelectionSet.Selections, depth, userContext);

            var sqlTable = new SqlTable(tableName, fieldName, tableAs, columns.AsReadOnly(), tables.AsReadOnly(),
                    arguments, grabMany).WithLocation(fieldAst.SourceLocation);

            if (where != null)
            {
                sqlTable.Where = where;
            }

            if (orderBy != null)
            {
                var builder = new OrderByBuilder();
                orderBy(builder, arguments, userContext);
                sqlTable.OrderBy = builder.OrderBy;
            }

            if (sortKey != null)
            {
                var builder = new SortKeyBuilder();
                sortKey(builder, arguments, userContext);
                sqlTable.SortKey = builder.SortKey;
            }

            sqlTable.Paginate = paginate;

            if (join != null)
            {
                sqlTable.Join = join;
            }
            else if (junction != null)
            {
                sqlTable.Junction =
                    new SqlJunction(junction.Table, junction.Table, junction.FromParent, junction.ToChild);

                if (junction.Where != null)
                {
                    sqlTable.Junction.Where = junction.Where;
                }

                if (junction.OrderBy != null)
                {
                    var builder = new OrderByBuilder();
                    junction.OrderBy(builder, arguments, userContext);
                    sqlTable.Junction.OrderBy = builder.OrderBy;
                }

                if (junction.SortKey != null)
                {
                    var builder = new SortKeyBuilder();
                    junction.SortKey(builder, arguments, userContext);
                    sqlTable.Junction.SortKey = builder.SortKey;
                }
            }

            if (paginate)
            {
                HandleColumnsRequiredForPagination(sqlTable, columns);
            }

            return sqlTable;
        }

        private void HandleColumnsRequiredForPagination(SqlTable sqlTable, ICollection<SqlColumnBase> columns)
        {
            if (sqlTable.SortKey != null || sqlTable.Junction?.SortKey != null)
            {
                var sortKey = sqlTable.SortKey ?? sqlTable.Junction?.SortKey;
                if (sortKey == null) return;

                foreach (var column in sortKey.Key)
                {
                    var newChild = new SqlColumn(column, column, column);
                    if (sqlTable.SortKey == null && sqlTable.Junction != null)
                        newChild.FromOtherTable = sqlTable.Junction.As;

                    columns.Add(newChild);
                }
            }
            else if (sqlTable.OrderBy != null || sqlTable.Junction?.OrderBy != null)
            {
                var newChild = new SqlColumn("$total", "$total", "$total");
                if (sqlTable.SortKey == null && sqlTable.Junction != null)
                    newChild.FromOtherTable = sqlTable.Junction.As;

                columns.Add(newChild);
            }
        }

        private Node HandleColumn(Field fieldAst, FieldType field, IGraphType graphType,
            SqlColumnConfig? config, int depth, IDictionary<string, object> userContext)
        {
            var fieldName = fieldAst.Name;
            var columnName = config?.Column ?? fieldName;
            var columnAs = fieldName;

            return new SqlColumn(columnName, fieldName, columnAs).WithLocation(fieldAst.SourceLocation);
        }

        private IReadOnlyDictionary<string, object> HandleArguments(Field fieldAst)
        {
            var arguments = new Dictionary<string, object>();
            if (fieldAst.Arguments != null)
            {
                foreach (var arg in fieldAst.Arguments)
                {
                    arguments.Add(arg.Name, arg.Value.Value);
                }
            }
            return arguments;
        }

        private void HandleSelections(List<SqlColumnBase> sqlColumns, ICollection<SqlTable> tables,
            IComplexGraphType graphType, IEnumerable<ISelection> selections, int depth, IDictionary<string, object> userContext)
        {
            foreach (var selection in selections)
            {
                switch (selection)
                {
                    case Field fieldAst:
                        var field = graphType.GetField(fieldAst.Name);
                        var node = Convert(fieldAst, field, graphType, ++depth, userContext);

                        switch (node)
                        {
                            case SqlColumnBase sqlColumn:
                                var existing = sqlColumns.Find(x => x.FieldName == fieldAst.Name);
                                if (existing != null)
                                    continue;

                                sqlColumns.Add(sqlColumn);
                                break;
                            case SqlTable sqlTable:
                                tables.Add(sqlTable);
                                break;
                            case SqlNoop _:
                                continue;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(node), $"Unknown node type ${node.GetType()}");
                        }

                        break;
                    case InlineFragment _:
                        break;
                    case FragmentSpread _:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(selection), $"Unknown selection kind: {selection.GetType()}");
                }
            }
        }

        private (IComplexGraphType edgeType, Field queryAstNode) StripRelayConnection(IComplexGraphType graphType, Field fieldAst)
        {
            var edgesType = (IComplexGraphType) graphType.GetField("edges").ResolvedType.GetNamedType();
            var edgeType = (IComplexGraphType) edgesType.GetField("node").ResolvedType.GetNamedType();

            var field = fieldAst.SelectionSet.Selections
                           .OfType<Field>()
                           .FirstOrDefault(x => x.Name == "edges")
                           ?.SelectionSet.Selections.OfType<Field>()
                           .FirstOrDefault(x => x.Name == "node")
                       ?? new Field();

            fieldAst = new Field(fieldAst.AliasNode, fieldAst.NameNode)
            {
                Arguments = fieldAst.Arguments,
                Directives = fieldAst.Directives,
                SelectionSet = field.SelectionSet,
                SourceLocation = fieldAst.SourceLocation
            };

            return (edgeType, fieldAst);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Types;
using GraphQLParser;
using GraphQLParser.AST;
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
        private readonly IAliasGenerator _aliasGenerator;

        /// <summary>
        /// Creates a new instance of <see cref="QueryToSqlConverter"/>.
        /// </summary>
        /// <param name="aliasGenerator">The <see cref="IAliasGenerator"/>.</param>
        public QueryToSqlConverter(IAliasGenerator aliasGenerator)
        {
            _aliasGenerator = aliasGenerator ?? throw new ArgumentNullException(nameof(aliasGenerator));
        }

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

            var node = Convert(null, fieldAst, field, parentType, 0, context);
            if (node is SqlTable sqlTable)
                return sqlTable;

            throw new JoinMonsterException($"Expected node to be of type '{typeof(SqlTable)}' but was '{node.GetType()}'.");
        }

        private Node Convert(SqlTable? sqlTable, GraphQLField fieldAst, FieldType field, IGraphType parentType, int depth,
            IResolveFieldContext context)
        {
            var sqlColumnConfig = field.GetSqlColumnConfig();
            if (sqlColumnConfig?.Ignored == true)
                return SqlNoop.Instance;

            var gqlType = field.ResolvedType?.GetNamedType();

            if (gqlType is IComplexGraphType complexGraphType)
            {
                if (complexGraphType.IsConnectionType())
                    (complexGraphType, fieldAst) = StripRelayConnection(complexGraphType, fieldAst);

                var sqlTableConfig = complexGraphType.GetSqlTableConfig();

                if (sqlTableConfig != null)
                {
                    if (depth >= 1)
                    {
                        //TODO: Validate that either join, batch or junction is set on the field
                    }

                    return HandleTable(sqlTable, fieldAst, field, complexGraphType, parentType, sqlTableConfig, depth, context);
                }

                if (sqlColumnConfig == null)
                    return SqlNoop.Instance;
            }

            if(sqlTable == null)
                throw new InvalidOperationException($"Expected {nameof(sqlTable)} not to be null.");

            // TODO: Looks like we can't check if field.Resolver is null because FieldMiddleware is registered as a resolver
            // if (sqlColumnConfig != null || field.Resolver == null)
                return HandleColumn(sqlTable, fieldAst, field, gqlType, sqlColumnConfig, depth, context);

            // return new SqlNoop();
        }

        private Node HandleTable(Node? parent, GraphQLField fieldAst, FieldType field, IComplexGraphType graphType,
            IGraphType parentGraphType, SqlTableConfig config, int depth, IResolveFieldContext context)
        {
            var arguments = HandleArguments(field, fieldAst, context);

            var fieldName = fieldAst.Alias?.Name.StringValue ?? field.Name;
            var tableName = config.Table(arguments, context);
            var tableAs = _aliasGenerator.GenerateTableAlias(fieldName);

            var grabMany = field.ResolvedType.IsListType();
            var where = field.GetSqlWhere();
            var join = field.GetSqlJoin();
            var junction = field.GetSqlJunction();
            var batch = field.GetSqlBatch();
            var orderBy = field.GetSqlOrder();
            var sortKey = field.GetSqlSortKey();
            var paginate = false;

            if (!grabMany && field.ResolvedType.GetNamedType().IsConnectionType())
            {
                paginate = field.GetSqlPaginate().GetValueOrDefault(false);
                grabMany = true;
            }

            var sqlTable = new SqlTable(parent, config, tableName, fieldName, tableAs, arguments, grabMany)
                {
                    ParentGraphType = parentGraphType,
                }
                .WithLocation(fieldAst.Location);

            if (config.UniqueKey.Length == 1)
            {
                var alias = _aliasGenerator.GenerateColumnAlias(config.UniqueKey[0]);
                sqlTable.Columns.Add(new SqlColumn(sqlTable, config.UniqueKey[0], config.UniqueKey[0], alias, true));
            }
            else
            {
                var clumsyName = string.Join("#", config.UniqueKey);
                var alias = _aliasGenerator.GenerateColumnAlias(clumsyName);
                sqlTable.Columns.Add(new SqlComposite(sqlTable, config.UniqueKey, clumsyName, alias, true));
            }

            if (config.AlwaysFetch != null)
            {
                foreach (var column in config.AlwaysFetch)
                {
                    var alias = _aliasGenerator.GenerateColumnAlias(column);
                    sqlTable.Columns.Add(new SqlColumn(sqlTable, column, column, alias));
                }
            }

            if (fieldAst.SelectionSet != null)
                HandleSelections(sqlTable, graphType, fieldAst.SelectionSet.Selections, depth, context);

            sqlTable.ColumnExpression = config.ColumnExpression;

            if (where != null)
            {
                sqlTable.Where = where;
            }

            if (orderBy != null)
            {
                var builder = new OrderByBuilder(sqlTable.As);
                orderBy(builder, arguments, context, sqlTable);
                sqlTable.OrderBy = builder.OrderBy;
            }

            if (sortKey != null)
            {
                var builder = new SortKeyBuilder(sqlTable.As, _aliasGenerator);
                sortKey(builder, arguments, context,sqlTable);
                sqlTable.SortKey = builder.SortKey;
            }

            sqlTable.Paginate = paginate;

            if (join != null)
            {
                sqlTable.Join = join;
            }
            else if (junction != null)
            {
                sqlTable.Junction = new SqlJunction(sqlTable, junction.Table, _aliasGenerator.GenerateTableAlias(junction.Table));

                if (junction.Where != null)
                {
                    sqlTable.Junction.Where = junction.Where;
                }

                if (junction.OrderBy != null)
                {
                    var builder = new OrderByBuilder(tableAs);
                    junction.OrderBy(builder, arguments, context, sqlTable);
                    sqlTable.Junction.OrderBy = builder.OrderBy;
                }

                if (junction.SortKey != null)
                {
                    var builder = new SortKeyBuilder(tableAs, _aliasGenerator);
                    junction.SortKey(builder, arguments, context, sqlTable);
                    sqlTable.Junction.SortKey = builder.SortKey;
                }
                var batchConfig = junction.BatchConfig;

                if (junction.FromParent != null && junction.ToChild != null)
                {
                    sqlTable.Junction.FromParent = junction.FromParent;
                    sqlTable.Junction.ToChild = junction.ToChild;
                }
                else if (batchConfig != null)
                {
                    sqlTable.Junction.Batch = new SqlBatch(
                        new SqlColumn(sqlTable, batchConfig.ThisKey, batchConfig.ThisKey,
                            _aliasGenerator.GenerateColumnAlias(batchConfig.ThisKey))
                        {
                            FromOtherTable = sqlTable.Junction.As,
                            Expression = batchConfig.ThisKeyExpression
                        },
                        new SqlColumn(sqlTable, batchConfig.ParentKey, fieldName,
                            _aliasGenerator.GenerateColumnAlias(batchConfig.ParentKey))
                        {
                            Expression = batchConfig.ParentKeyExpression
                        }
                    )
                    {
                        Join = batchConfig.Join,
                        Where = batchConfig.Where
                    };
                }
            }
            else if (batch != null)
            {
                sqlTable.Batch = new SqlBatch(
                    new SqlColumn(sqlTable, batch.ThisKey, batch.ThisKey, _aliasGenerator.GenerateColumnAlias(batch.ThisKey))
                    {
                        Expression = batch.ThisKeyExpression
                    },
                    new SqlColumn(sqlTable, batch.ParentKey, batch.ParentKey, _aliasGenerator.GenerateColumnAlias(batch.ParentKey))
                    {
                        Expression = batch.ParentKeyExpression
                    }
                )
                {
                    Where = batch.Where,
                    Join = batch.Join
                };
            }

            if (paginate)
                HandleColumnsRequiredForPagination(sqlTable);

            return sqlTable;
        }

        private void HandleColumnsRequiredForPagination(SqlTable sqlTable)
        {
            if (sqlTable.SortKey != null || sqlTable.Junction?.SortKey != null)
            {
                var sortKey = sqlTable.SortKey ?? sqlTable.Junction?.SortKey;
                if (sortKey == null) return;

                do
                {
                    var newChild = new SqlColumn(sqlTable, sortKey.Column, sortKey.Column, sortKey.As);
                    if (sqlTable.SortKey == null && sqlTable.Junction != null)
                        newChild.FromOtherTable = sqlTable.Junction.As;

                    sqlTable.Columns.Add(newChild);
                } while ((sortKey = sortKey.ThenBy) != null);
            }
            else if (sqlTable.OrderBy != null || sqlTable.Junction?.OrderBy != null)
            {
                var newChild = new SqlColumn(sqlTable, "$total", "$total", "$total");
                if (sqlTable.SortKey == null && sqlTable.Junction != null)
                    newChild.FromOtherTable = sqlTable.Junction.As;

                sqlTable.Columns.Add(newChild);
            }
        }

        private Node HandleColumn(SqlTable sqlTable, GraphQLField fieldAst, FieldType field, IGraphType graphType,
            SqlColumnConfig? config, int depth, IResolveFieldContext context)
        {
            var fieldName = fieldAst.Alias?.Name.StringValue ?? field.Name;
            var columnName = config?.Column ?? field.Name;
            var columnAs = _aliasGenerator.GenerateColumnAlias(fieldName);

            var column = new SqlColumn(sqlTable, columnName, fieldName, columnAs).WithLocation(fieldAst.Location);

            column.Arguments = HandleArguments(field, fieldAst, context);
            column.Expression = config?.Expression;

            return column;
        }

        private IReadOnlyDictionary<string, ArgumentValue> HandleArguments(FieldType fieldType, GraphQLField fieldAst, IResolveFieldContext context)
        {
            return ExecutionHelper.GetArguments(fieldType.Arguments, fieldAst.Arguments, context.Variables) ??
                   new Dictionary<string, ArgumentValue>();
        }

        private void HandleSelections(SqlTable parent, IComplexGraphType graphType, IEnumerable<ASTNode> selections,
            int depth, IResolveFieldContext context)
        {
            foreach (var selection in MergeSelections(selections))
            {
                switch (selection)
                {
                    case GraphQLField fieldAst:
                        if (fieldAst.Name.StringValue.StartsWith("__"))
                            continue;

                        var field = graphType.GetField(fieldAst.Name);
                        var node = Convert(parent, fieldAst, field, graphType, ++depth, context);

                        switch (node)
                        {
                            case SqlColumnBase sqlColumn:
                                var fieldName = fieldAst.Alias?.Name.StringValue ?? field.Name;
                                var existing = parent.Columns.Any(x => x.FieldName == fieldName);

                                if (existing)
                                    continue;

                                parent.Columns.Add(sqlColumn);
                                break;
                            case SqlTable sqlTable:
                                parent.Tables.Add(sqlTable);
                                break;
                            case SqlNoop _:
                                continue;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(node), $"Unknown node type ${node.GetType()}");
                        }

                        break;
                    case GraphQLInlineFragment inlineFragment:
                    {
                        var selectionNameOfType = inlineFragment.TypeCondition.Type.Name;
                        var deferredType = context.Schema.AllTypes.Single(x => x.Name == selectionNameOfType);

                        if (deferredType is IComplexGraphType complexGraphType)
                        {
                            HandleSelections(parent, complexGraphType,
                                inlineFragment.SelectionSet.Selections, depth, context);
                        }

                        break;
                    }
                    case GraphQLFragmentSpread fragmentSpread:
                    {
                        var fragmentName = fragmentSpread.FragmentName.Name;
                        var definition = context.Document.FindFragmentDefinition(fragmentName);
                        var selectionNameOfType = definition!.TypeCondition.Type.Name;
                        var deferredType = context.Schema.AllTypes.FirstOrDefault(x => x.Name == selectionNameOfType);

                        if (deferredType is IComplexGraphType complexGraphType)
                        {
                            HandleSelections(parent, complexGraphType,
                                definition.SelectionSet.Selections, depth, context);
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(selection), $"Unknown selection kind: {selection.GetType()}");
                }
            }
        }

        private (IComplexGraphType edgeType, GraphQLField queryAstNode) StripRelayConnection(IComplexGraphType graphType, GraphQLField fieldAst)
        {
            var edgesType = (IComplexGraphType) graphType.GetField("edges")?.ResolvedType.GetNamedType();
            var edgeType = (IComplexGraphType) edgesType?.GetField("node")?.ResolvedType.GetNamedType();

            var nodeField = fieldAst.SelectionSet.Selections
                            .OfType<GraphQLField>()
                            .FirstOrDefault(x => x.Name == "edges")
                            ?.SelectionSet.Selections.OfType<GraphQLField>()
                            .FirstOrDefault(x => x.Name == "node");

            var itemsField = fieldAst.SelectionSet.Selections
                            .OfType<GraphQLField>()
                            .FirstOrDefault(x => x.Name == "items");

            GraphQLSelectionSet? selectionSet = null;
            if (nodeField?.SelectionSet != null && itemsField?.SelectionSet != null)
            {
                selectionSet = new GraphQLSelectionSet
                {
                    Selections = new List<ASTNode>()
                };
                selectionSet.Selections.AddRange(nodeField.SelectionSet.Selections);
                selectionSet.Selections.AddRange(itemsField.SelectionSet.Selections);
            }
            else if (nodeField?.SelectionSet != null)
            {
                selectionSet = nodeField.SelectionSet;
            }
            else if (itemsField?.SelectionSet != null)
            {
                selectionSet = itemsField.SelectionSet;
            }


            fieldAst = new GraphQLField
            {
                Alias = fieldAst.Alias,
                Name = fieldAst.Name,
                Arguments = fieldAst.Arguments,
                Directives = fieldAst.Directives,
                SelectionSet = selectionSet,
                Location = fieldAst.Location
            };

            return (edgeType, fieldAst);
        }

        private IEnumerable<ASTNode> MergeSelections(IEnumerable<ASTNode> selections)
        {
            var merged = new List<ASTNode>();
            var fields = new Dictionary<string, GraphQLField>();
            foreach (var selection in selections)
            {
                if (selection is GraphQLField field)
                {
                    var fieldName = field.Alias?.Name.StringValue ?? field.Name.StringValue;
                    if (fields.TryGetValue(fieldName, out var existing))
                    {
                        if (existing.SelectionSet != null && field.SelectionSet != null)
                        {
                            existing.SelectionSet.Selections = MergeSelections(existing.SelectionSet.Selections
                                .Concat(field.SelectionSet.Selections))
                                .ToList();

                            continue;
                        }
                    }
                    else
                    {
                        fields.Add(fieldName, field);
                    }
                }

                merged.Add(selection);
            }

            return merged;
        }
    }
}

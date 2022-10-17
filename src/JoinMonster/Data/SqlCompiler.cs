using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphQL;
using JoinMonster.Builders;
using JoinMonster.Builders.Clauses;
using JoinMonster.Language.AST;

namespace JoinMonster.Data
{
    /// <summary>
    /// The <see cref="SqlCompiler"/> is responsible for converting SQL Ast to SQL.
    /// </summary>
    public class SqlCompiler : ISqlCompiler
    {
        private readonly ISqlDialect _dialect;

        /// <summary>
        /// Create a new instance of the <see cref="SqlCompiler"/> using a specific <see cref="ISqlDialect"/>.
        /// </summary>
        /// <param name="sqlDialect">The <see cref="ISqlDialect"/> to use.</param>
        public SqlCompiler(ISqlDialect sqlDialect)
        {
            _dialect = sqlDialect ?? throw new ArgumentNullException(nameof(sqlDialect));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <param name="batchScope"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual SqlResult Compile(Node node, IResolveFieldContext context, IEnumerable? batchScope = null)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (context == null) throw new ArgumentNullException(nameof(context));

            // TODO: Should we validate the node?

            var selections = new List<string>();
            var tables = new List<string>();
            var wheres = new List<WhereCondition>();
            var orders = new List<string>();
            var sqlCompilerContext = new SqlCompilerContext(this);

            StringifySqlAst(null, node, Array.Empty<string>(), context, selections, tables, wheres, orders, batchScope ?? Enumerable.Empty<object>(),
                sqlCompilerContext);

            var sb = new StringBuilder();
            sb.AppendLine("SELECT");
            sb.Append("  ");
            sb.AppendLine(string.Join(",\n  ", selections));
            sb.AppendLine(string.Join("\n", tables));

            if (wheres.Count > 0)
            {
                sb.Append("WHERE ");
                sb.AppendLine(_dialect.CompileConditions(wheres, sqlCompilerContext));
            }

            if (orders.Count > 0)
            {
                sb.Append("ORDER BY ");
                sb.AppendLine(string.Join(", ", orders));
            }

            return new SqlResult(sb.ToString().Trim(), sqlCompilerContext.Parameters);
        }

        private void StringifySqlAst(Node? parent, Node node, IReadOnlyCollection<string> prefix,
            IResolveFieldContext context, ICollection<string> selections, ICollection<string> tables,
            ICollection<WhereCondition> wheres, ICollection<string> orders, IEnumerable batchScope,
            SqlCompilerContext compilerContext)
        {
            switch (node)
            {
                case SqlTable sqlTable:
                    HandleTable(parent, sqlTable, prefix, context, selections, tables, wheres, orders, batchScope, compilerContext);

                    if (ThisIsNotTheEndOfThisBatch(parent, sqlTable))
                    {
                        foreach (var child in sqlTable.Children)
                        {
                            StringifySqlAst(node, child, new List<string>(prefix) {sqlTable.As}, context, selections,
                                tables, wheres, orders, batchScope, compilerContext);
                        }
                    }

                    break;
                case SqlColumn sqlColumn:
                {
                    if (parent is not SqlTable table)
                        throw new ArgumentException($"'{nameof(parent)}' must be of type {typeof(SqlTable)}", nameof(parent));

                    var parentTable = table.As;
                    string columnName;

                    if (sqlColumn.Expression != null)
                    {
                        columnName = sqlColumn.Expression(_dialect.Quote(parentTable), sqlColumn.Arguments, context, table);
                    }
                    else if (table.ColumnExpression != null)
                    {
                        columnName = table.ColumnExpression(_dialect.Quote(parentTable), sqlColumn.Name, sqlColumn.Arguments, context);
                    }
                    else
                    {
                        columnName = $"{_dialect.Quote(parentTable)}.{_dialect.Quote(sqlColumn.Name)}";
                    }

                    selections.Add($"{columnName} AS {_dialect.Quote(JoinPrefix(prefix) + sqlColumn.As)}");
                    break;
                }
                case SqlComposite sqlComposite:
                {
                    if (parent is not SqlTable table)
                        throw new ArgumentException($"'{nameof(parent)}' must be of type {typeof(SqlTable)}", nameof(parent));

                    var parentTable = table.As;
                    selections.Add($"{_dialect.CompositeKey(parentTable, sqlComposite.Name)} AS {_dialect.Quote(JoinPrefix(prefix) + sqlComposite.As)}");
                    break;
                }
                case SqlJunction _:
                case SqlNoop _:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(node), $"Don't know how to handle {node.GetType()}.");
            }
        }

        private void HandleTable(Node? parent, SqlTable node, IEnumerable<string> prefix,
            IResolveFieldContext context, ICollection<string> selections, ICollection<string> tables,
            ICollection<WhereCondition> wheres, ICollection<string> orders, IEnumerable batchScope,
            SqlCompilerContext compilerContext)
        {
            var arguments = node.Arguments;

            // also check for batching
            if (node.Paginate == false && (node.Batch == null || node.Junction?.Batch == null || parent == null))
            {
                if (node.Junction?.Where != null)
                {
                    var whereBuilder = new WhereBuilder(_dialect.Quote(node.Junction.As), wheres);
                    node.Junction.Where.Invoke(whereBuilder, arguments, context, node);
                }

                // only add the where clause if there's no join or the join is not paginated
                if (node.Where != null)
                {
                    var whereBuilder = new WhereBuilder(_dialect.Quote(node.As), wheres);
                    node.Where.Invoke(whereBuilder, arguments, context, node);
                }
            }

            if (ThisIsNotTheEndOfThisBatch(parent, node))
            {
                if (node.Junction?.OrderBy != null)
                {
                    var junctionOrderBy = _dialect.CompileOrderBy(node.Junction.OrderBy);
                    orders.Add(junctionOrderBy);
                }

                if (node.OrderBy != null)
                {
                    var orderBy = _dialect.CompileOrderBy(node.OrderBy);
                    orders.Add(orderBy);
                }

                // if (node.Junction?.SortKey != null)
                // {
                //     var junctionOrderBy =  _dialect.CompileOrderBy(node.Junction.SortKey);
                //     orders.Add(junctionOrderBy);
                // }
                //
                if (node.SortKey != null)
                {
                    var builder = new OrderByBuilder(node.SortKey.Table);
                    ThenOrderByBuilder? thenBy = null;

                    var sort = node.SortKey;
                    do
                    {
                        var descending = sort.Direction == SortDirection.Descending;

                        if (thenBy == null)
                        {
                            thenBy = descending ? builder.ByDescending(sort.Column) : builder.By(sort.Column);
                        }
                        else
                        {
                            thenBy = descending ? thenBy.ThenByDescending(sort.Column) : thenBy.ThenBy(sort.Column);
                        }
                    } while ((sort = sort.ThenBy) != null);

                    var order = builder.OrderBy == null ? "" : _dialect.CompileOrderBy(builder.OrderBy);
                    orders.Add(order);
                }
            }

            if (node.Join != null)
            {
                if (parent is SqlTable parentTable)
                {
                    var join = new JoinBuilder(_dialect.Quote(parentTable.Name), _dialect.Quote(parentTable.As),
                        _dialect.Quote(node.Name), _dialect.Quote(node.As));
                    node.Join(join, arguments, context, node);

                    if (node.Paginate)
                    {
                        _dialect.HandleJoinedOneToManyPaginated(parentTable, node, arguments, context, tables, compilerContext,
                            join.Condition == null ? null : _dialect.CompileConditions(new[] {join.Condition}, compilerContext));
                    }
                    else if (join.Condition is RawCondition)
                    {
                        tables.Add($"{join.From} ON {_dialect.CompileConditions(new[] {join.Condition}, compilerContext)}");
                    }
                    else if(join.Condition != null)
                    {
                        tables.Add($"LEFT JOIN {_dialect.Quote(node.Name)} {_dialect.Quote(node.As)} ON {_dialect.CompileConditions(new[] {join.Condition}, compilerContext)}");
                    }

                    return;
                }
            }
            else if (node.Junction?.Batch != null)
            {
                if (parent is SqlTable parentTable)
                {
                    var columnName = node.Junction.Batch.ParentKey.Expression != null
                        ? node.Junction.Batch.ParentKey.Expression(_dialect.Quote(parentTable.As), node.Junction.Batch.ParentKey.Arguments, context, node)
                        : $"{_dialect.Quote(parentTable.As)}.{_dialect.Quote(node.Junction.Batch.ParentKey.Name)}";

                    selections.Add($"{columnName} AS ${_dialect.Quote(JoinPrefix(prefix) + node.Junction.Batch.ParentKey.As)}");
                }
                else
                {
                    var join = new JoinBuilder(_dialect.Quote(node.Name), _dialect.Quote(node.As), _dialect.Quote(node.Junction.Table), _dialect.Quote(node.Junction.As));

                    if (node.Junction.Batch.Join != null)
                        node.Junction.Batch.Join(join, arguments, context, node);

                    if (join.Condition == null)
                        throw new JoinMonsterException($"The 'batch' join condition on table '{node.Name}' for junction '{node.Junction.Table}' cannot be null.");


                    var joinCondition = _dialect.CompileConditions(new[] {join.Condition}, compilerContext);

                    if (node.Paginate)
                    {
                        _dialect.HandleBatchedManyToManyPaginated(parent, node, arguments, context, tables, batchScope.Cast<object>(),
                            compilerContext, joinCondition);
                    }
                    else
                    {
                        tables.Add($"FROM {_dialect.Quote(node.Junction.Table)} {_dialect.Quote(node.Junction.As)}");
                        tables.Add($"LEFT JOIN {_dialect.Quote(node.Name)} {_dialect.Quote(node.As)} ON {joinCondition}");

                        var column = _dialect.Quote(node.Junction.Batch.ThisKey.Name);
                        var whereBuilder = new WhereBuilder(_dialect.Quote(node.Junction.As), wheres);
                        if (node.Junction.Batch.Where != null)
                        {
                            node.Junction.Batch.Where(whereBuilder, column, batchScope, arguments, context, node);
                        }
                        else
                        {
                            whereBuilder.In(column, batchScope);
                        }
                    }
                }
            }
            else if (node.Junction?.FromParent != null && node.Junction?.ToChild != null)
            {
                if (parent is SqlTable parentTable)
                {
                    // TODO: Handle batching and paging
                    var fromParentJoin = new JoinBuilder(_dialect.Quote(parentTable.Name), _dialect.Quote(parentTable.As), _dialect.Quote(node.Junction.Table), _dialect.Quote(node.Junction.As));
                    var toChildJoin = new JoinBuilder(_dialect.Quote(parentTable.Name), _dialect.Quote(node.Junction.As), _dialect.Quote(node.Name), _dialect.Quote(node.As));
                    node.Junction.FromParent(fromParentJoin, arguments, context, node);
                    node.Junction.ToChild(toChildJoin, arguments, context, node);

                    if (fromParentJoin.Condition == null)
                        throw new JoinMonsterException($"The 'fromParent' join condition on table '{node.Name}' for junction '{node.Junction.Table}' cannot be null.");
                    if (toChildJoin.Condition == null)
                        throw new JoinMonsterException($"The 'toChild' join condition on table '{node.Name}' for junction '{node.Junction.Table}' cannot be null.");

                    var compiledFromParentJoinCondition = _dialect.CompileConditions(new[] {fromParentJoin.Condition}, compilerContext);
                    var compiledToChildJoinCondition = _dialect.CompileConditions(new[] {toChildJoin.Condition}, compilerContext);

                    if (node.Paginate)
                    {
                        // _dialect.HandleJoinedManyToManyPaginated();
                    }
                    else
                    {
                        tables.Add(fromParentJoin.Condition is RawCondition
                            ? $"{fromParentJoin.From} ON {compiledFromParentJoinCondition}"
                            : $"LEFT JOIN {_dialect.Quote(node.Junction.Table)} {_dialect.Quote(node.Junction.As)} ON {compiledFromParentJoinCondition}");

                        tables.Add(toChildJoin.Condition is RawCondition
                            ? $"{toChildJoin.From} ON {compiledToChildJoinCondition}"
                            : $"LEFT JOIN {_dialect.Quote(node.Name)} {_dialect.Quote(node.As)} ON {compiledToChildJoinCondition}");
                    }
                }

                return;
            }
            else if (node.Batch != null)
            {
                if (parent is SqlTable parentTable)
                {
                    var columnName = node.Batch.ParentKey.Expression != null
                        ? node.Batch.ParentKey.Expression(_dialect.Quote(parentTable.As), node.Batch.ParentKey.Arguments, context, node)
                        : $"{_dialect.Quote(parentTable.As)}.{_dialect.Quote(node.Batch.ParentKey.Name)}";

                    selections.Add($"{columnName} AS {_dialect.Quote(JoinPrefix(prefix) + node.Batch.ParentKey.As)}");
                }
                else if (node.Paginate)
                {
                    _dialect.HandleBatchedOneToManyPaginated(parent, node, arguments, context, tables, selections, batchScope.Cast<object>(), compilerContext);
                }
                else
                {
                    tables.Add($"FROM {_dialect.Quote(node.Name)} AS {_dialect.Quote(node.As)}");

                    var column = _dialect.Quote(node.Batch.ThisKey.Name);
                    var whereBuilder = new WhereBuilder(_dialect.Quote(node.As), wheres);
                    //if (node.Batch.Where != null)
                    //{
                    //    node.Batch.Where(whereBuilder, column, batchScope, arguments, context, node);
                    //}
                    //else
                    //{
                        whereBuilder.In(column, batchScope);
                    //}
                }

                return;
            }
            else if (node.Paginate)
            {
                _dialect.HandlePaginationAtRoot(parent, node, arguments, context, tables, compilerContext);
                return;
            }

            tables.Add($"FROM {_dialect.Quote(node.Name)} AS {_dialect.Quote(node.As)}");
        }

        private static string JoinPrefix(IEnumerable<string> prefix) =>
            prefix.Skip(1).Aggregate("", (prev, name) => $"{prev}{name}__");

        private static bool ThisIsNotTheEndOfThisBatch(Node? parent, SqlTable sqlTable) =>
            sqlTable.Batch == null && sqlTable.Junction?.Batch == null || parent == null;

    }
}

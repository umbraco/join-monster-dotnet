using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphQL;
using JoinMonster.Builders;
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
        /// Compiles the SQL Ast to SQL.
        /// </summary>
        /// <param name="node">The <see cref="Node"/>.</param>
        /// <param name="context">The <see cref="IResolveFieldContext"/>.</param>
        /// <returns>The compiled SQL.</returns>
        /// <exception cref="ArgumentNullException">If <c>node</c> or <c>context</c> is null.</exception>
        public virtual SqlResult Compile(Node node, IResolveFieldContext context)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (context == null) throw new ArgumentNullException(nameof(context));

            // TODO: Should we validate the node?

            var selections = new List<string>();
            var tables = new List<string>();
            var wheres = new List<string>();
            var orders = new List<string>();
            var parameters = new Dictionary<string, object>();

            StringifySqlAst(null, node, Array.Empty<string>(), context, selections, tables, wheres, orders, parameters);

            var sb = new StringBuilder();
            sb.AppendLine("SELECT");
            sb.Append("  ");
            sb.AppendLine(string.Join(",\n  ", selections));
            sb.AppendLine(string.Join("\n", tables));

            if (wheres.Count > 0)
            {
                sb.Append("WHERE ");
                sb.AppendLine(string.Join(" AND ", wheres));
            }

            if (orders.Count > 0)
            {
                sb.Append("ORDER BY ");
                sb.AppendLine(string.Join(", ", orders));
            }

            return new SqlResult(sb.ToString().Trim(), parameters);
        }

        private void StringifySqlAst(Node? parent, Node node, IReadOnlyCollection<string> prefix,
            IResolveFieldContext context, ICollection<string> selections, ICollection<string> tables,
            ICollection<string> wheres, ICollection<string> orders, Dictionary<string, object> parameters)
        {
            switch (node)
            {
                case SqlTable sqlTable:
                    HandleTable(parent, sqlTable, prefix, context, selections, tables, wheres, orders, parameters);
                    foreach (var child in sqlTable.Children)
                        StringifySqlAst(node, child, new List<string>(prefix) {sqlTable.As}, context, selections, tables, wheres, orders, parameters);
                    break;
                case SqlColumn sqlColumn:
                {
                    if (!(parent is SqlTable table))
                        throw new ArgumentException($"'{nameof(parent)}' must be of type {typeof(SqlTable)}",
                            nameof(parent));

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
                    if (!(parent is SqlTable table))
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
            ICollection<string> wheres, ICollection<string> orders, Dictionary<string, object> parameters)
        {
            var arguments = node.Arguments;

            if (node.Junction?.Where != null)
            {
                var whereBuilder = new WhereBuilder(_dialect, _dialect.Quote(node.Junction.As), wheres, parameters);
                node.Junction?.Where?.Invoke(whereBuilder, arguments, context, node);
            }

            if (node.Where != null)
            {
                var whereBuilder = new WhereBuilder(_dialect, _dialect.Quote(node.As), wheres, parameters);
                node.Where?.Invoke(whereBuilder, arguments, context, node);
            }

            HandleOrderBy(node.Junction?.OrderBy, node.As, orders);
            HandleOrderBy(node.OrderBy, node.As, orders);

            if (parent is SqlTable parentTable)
            {
                if (node.Join != null)
                {
                    var join = new JoinBuilder(_dialect, _dialect.Quote(parentTable.As), _dialect.Quote(node.As));
                    node.Join(join, arguments, context, node);

                    if (node.Paginate)
                    {
                        _dialect.HandleJoinedOneToManyPaginated(parentTable, node, arguments, context, tables, parameters, join.Condition);
                    }
                    else if(join.Condition != null)
                    {
                        tables.Add($"LEFT JOIN {_dialect.Quote(node.Name)} {_dialect.Quote(node.As)} ON {join.Condition}");
                    }
                    else if (join.RawCondition != null)
                    {
                        tables.Add(join.RawCondition);
                    }

                    return;
                }

                if (node.Junction != null)
                {
                    // TODO: Handle batching and paging
                    var fromParentJoin = new JoinBuilder(_dialect, _dialect.Quote(parentTable.As), _dialect.Quote(node.Junction.As));
                    var toChildJoin = new JoinBuilder(_dialect, _dialect.Quote(node.Junction.As), _dialect.Quote(node.As));
                    node.Junction.FromParent(fromParentJoin, arguments, context, node);
                    node.Junction.ToChild(toChildJoin, arguments, context, node);

                    if (fromParentJoin.Condition == null)
                        throw new JoinMonsterException($"The 'fromParent' join condition on table '{node.Name}' for junction '{node.Junction.Table}' cannot be null.");
                    if (toChildJoin.Condition == null)
                        throw new JoinMonsterException($"The 'toChild' join condition on table '{node.Name}' for junction '{node.Junction.Table}' cannot be null.");

                    tables.Add($"LEFT JOIN {_dialect.Quote(node.Junction.Table)} {_dialect.Quote(node.Junction.As)} ON {fromParentJoin.Condition}");
                    tables.Add($"LEFT JOIN {_dialect.Quote(node.Name)} {_dialect.Quote(node.As)} ON {toChildJoin.Condition}");

                    return;
                }
            }
            else if (node.Paginate)
            {
                _dialect.HandlePaginationAtRoot(parent, node, arguments, context, tables, parameters);
                return;
            }

            tables.Add($"FROM {_dialect.Quote(node.Name)} AS {_dialect.Quote(node.As)}");
        }

        private void HandleOrderBy(OrderBy? orderBy, string tableAlias, ICollection<string> orders)
        {
            if (orderBy == null) return;

            do
            {
                orders.Add($"{_dialect.Quote(tableAlias)}.{_dialect.Quote(orderBy.Column)} {(orderBy.Direction == SortDirection.Ascending ? "ASC" : "DESC")}");
            } while ((orderBy = orderBy.ThenBy) != null);
        }

        private static string JoinPrefix(IEnumerable<string> prefix) =>
            prefix.Skip(1).Aggregate("", (prev, name) => $"{prev}{name}__");
    }
}

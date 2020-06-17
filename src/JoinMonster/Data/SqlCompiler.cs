using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphQL;
using JoinMonster.Language.AST;

namespace JoinMonster.Data
{
    /// <summary>
    /// The <see cref="SqlCompiler"/> is responsible for converting SQL Ast to SQL.
    /// </summary>
    public class SqlCompiler
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
        public virtual string Compile(Node node, IResolveFieldContext context)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (context == null) throw new ArgumentNullException(nameof(context));

            // TODO: Should we validate the node?

            var selections = new List<string>();
            var tables = new List<string>();
            var wheres = new List<string>();
            var orders = new List<string>();

            StringifySqlAst(null, node, Array.Empty<string>(), context, selections, tables, wheres, orders);

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

            return sb.ToString().Trim();
        }

        private void StringifySqlAst(Node? parent, Node node, IReadOnlyCollection<string> prefix,
            IResolveFieldContext context, ICollection<string> selections, ICollection<string> tables,
            ICollection<string> wheres, ICollection<string> orders)
        {
            switch (node)
            {
                case SqlTable sqlTable:
                    HandleTable(parent, sqlTable, prefix, context, selections, tables, wheres, orders);
                    foreach (var child in sqlTable.Children)
                        StringifySqlAst(node, child, new List<string>(prefix) {sqlTable.As}, context, selections, tables, wheres, orders);
                    break;
                case SqlColumn sqlColumn:
                {
                    if (!(parent is SqlTable table))
                        throw new ArgumentException($"'{nameof(parent)}' must be of type {typeof(SqlTable)}", nameof(parent));

                    var parentTable = table.As;
                    if (sqlColumn.Expression != null)
                    {
                        selections.Add($"{sqlColumn.Expression(_dialect.Quote(parentTable), sqlColumn.Arguments, context)} AS {_dialect.Quote(JoinPrefix(prefix) + sqlColumn.As)}");
                    }
                    else
                    {
                        selections.Add($"{_dialect.Quote(parentTable)}.{_dialect.Quote(sqlColumn.Name)} AS {_dialect.Quote(JoinPrefix(prefix) + sqlColumn.As)}");
                    }

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
            ICollection<string> wheres, ICollection<string> orders)
        {
            var arguments = node.Arguments;

            var where = node.Junction?.Where?.Invoke(_dialect.Quote(node.Junction.As), arguments, context);
            if (where != null)
                wheres.Add(where);

            where = node.Where?.Invoke(_dialect.Quote(node.As), arguments, context);
            if (where != null)
                wheres.Add(where);

            if (node.Junction?.OrderBy != null)
                HandleOrderBy(node.Junction.OrderBy, node.As, orders);

            if (node.OrderBy != null)
                HandleOrderBy(node.OrderBy, node.As, orders);

            if (parent is SqlTable parentTable)
            {
                if (node.Join != null)
                {
                    var join = node.Join(_dialect.Quote(parentTable.As), _dialect.Quote(node.As), arguments, context);

                    if (node.Paginate)
                    {
                        _dialect.HandleJoinedOneToManyPaginated(parentTable, node, arguments, context, tables, join);
                    }
                    else
                    {
                        tables.Add($"LEFT JOIN {_dialect.Quote(node.Name)} {_dialect.Quote(node.As)} ON {join}");
                    }

                    return;
                }

                if (node.Junction != null)
                {
                    // TODO: Handle batching and paging
                    var joinCondition1 = node.Junction.FromParent(
                        _dialect.Quote(parentTable.As),
                        _dialect.Quote(node.Junction.As),
                        arguments, context);
                    var joinCondition2 = node.Junction.ToChild(
                        _dialect.Quote(node.Junction.As),
                        _dialect.Quote(node.As),
                        arguments, context);

                    tables.Add($"LEFT JOIN {_dialect.Quote(node.Junction.Table)} {_dialect.Quote(node.Junction.As)} ON {joinCondition1}");
                    tables.Add($"LEFT JOIN {_dialect.Quote(node.Name)} {_dialect.Quote(node.As)} ON {joinCondition2}");

                    return;
                }
            }
            else if (node.Paginate)
            {
                _dialect.HandlePaginationAtRoot(parent, node, arguments, context, tables);
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

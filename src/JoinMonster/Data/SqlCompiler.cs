using System;
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
            var wheres = new List<WhereCondition>();
            var orders = new List<string>();
            var sqlCompilerContext = new SqlCompilerContext(this);

            StringifySqlAst(null, node, Array.Empty<string>(), context, selections, tables, wheres, orders, sqlCompilerContext);

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
            ICollection<WhereCondition> wheres, ICollection<string> orders, SqlCompilerContext compilerContext)
        {
            switch (node)
            {
                case SqlTable sqlTable:
                    HandleTable(parent, sqlTable, prefix, context, selections, tables, wheres, orders, compilerContext);
                    foreach (var child in sqlTable.Children)
                        StringifySqlAst(node, child, new List<string>(prefix) {sqlTable.As}, context, selections, tables, wheres, orders, compilerContext);
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
            ICollection<WhereCondition> wheres, ICollection<string> orders, SqlCompilerContext compilerContext)
        {
            var arguments = node.Arguments;

            // also check for batching
            if (node.Paginate == false && parent == null)
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

            if (parent is SqlTable parentTable)
            {
                if (node.Join != null)
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

                if (node.Junction != null)
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

                    if (fromParentJoin.Condition is RawCondition)
                    {
                        tables.Add($"{fromParentJoin.From} ON {_dialect.CompileConditions(new[] {fromParentJoin.Condition}, compilerContext)}");
                    }
                    else
                    {
                        tables.Add($"LEFT JOIN {_dialect.Quote(node.Junction.Table)} {_dialect.Quote(node.Junction.As)} ON {_dialect.CompileConditions(new[] {fromParentJoin.Condition}, compilerContext)}");
                    }

                    if (toChildJoin.Condition is RawCondition)
                    {
                        tables.Add($"{toChildJoin.From} ON {_dialect.CompileConditions(new[] {toChildJoin.Condition}, compilerContext)}");
                    }
                    else
                    {
                        tables.Add($"LEFT JOIN {_dialect.Quote(node.Name)} {_dialect.Quote(node.As)} ON {_dialect.CompileConditions(new[] {toChildJoin.Condition}, compilerContext)}");
                    }

                    return;
                }
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
    }
}

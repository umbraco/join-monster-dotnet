using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Types;
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
        public virtual async Task<string> Compile(Node node, IResolveFieldContext context)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (context == null) throw new ArgumentNullException(nameof(context));

            // TODO: Should we validate the node?

            var selections = new List<string>();
            var tables = new List<string>();
            var wheres = new List<string>();

            await StringifySqlAst(null, node, new string[0], context, selections, tables, wheres).ConfigureAwait(false);

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

            return sb.ToString().Trim();
        }

        private async Task StringifySqlAst(Node? parent, Node node, IReadOnlyCollection<string> prefix,
            IResolveFieldContext context, ICollection<string> selections, ICollection<string> tables,
            ICollection<string> wheres)
        {
            switch (node)
            {
                case SqlTables sqlTables:
                    foreach (var child in sqlTables)
                        await StringifySqlAst(parent, child, prefix, context, selections, tables, wheres).ConfigureAwait(false);
                    break;
                case SqlTable sqlTable:
                    await HandleTable(parent, sqlTable, prefix, context, selections, tables, wheres).ConfigureAwait(false);
                    foreach (var child in sqlTable.Children)
                        await StringifySqlAst(node, child, new List<string>(prefix) {sqlTable.As}, context, selections, tables, wheres).ConfigureAwait(false);
                    break;
                case SqlColumns sqlColumns:
                    foreach (var child in sqlColumns)
                        await StringifySqlAst(parent, child, prefix, context, selections, tables, wheres).ConfigureAwait(false);
                    break;
                case SqlColumn sqlColumn:
                {
                    if (!(parent is SqlTable table))
                        throw new ArgumentException($"'{nameof(parent)}' must be of type {typeof(SqlTable)}", nameof(parent));

                    var parentTable = table.As;
                    selections.Add($"{_dialect.Quote(parentTable)}.{_dialect.Quote(sqlColumn.Name)} AS {_dialect.Quote(JoinPrefix(prefix) + sqlColumn.As)}");
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
                case Arguments _:
                case SqlNoop _:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(node), $"Don't know how to handle {node.GetType()}.");
            }
        }

        private async Task HandleTable(Node? parent, SqlTable node, IReadOnlyCollection<string> prefix,
            IResolveFieldContext context, ICollection<string> selections, ICollection<string> tables,
            ICollection<string> wheres)
        {
            var arguments = node.Arguments.ToDictionary(x => x.Name, x => x.Value.Value);


            if (node.Where != null)
            {
                var where = await node.Where(_dialect.Quote(node.As), arguments, context.UserContext)
                    .ConfigureAwait(false);

                if (where != null)
                    wheres.Add(where);
            }

            if (parent is SqlTable parentTable && node.Join != null)
            {
                var join = await node.Join(_dialect.Quote(parentTable.As), _dialect.Quote(node.As), arguments, context.UserContext)
                    .ConfigureAwait(false);

                tables.Add($"LEFT JOIN {_dialect.Quote(node.Name)} {_dialect.Quote(node.As)} ON {join}");
            }
            else
            {
                tables.Add($"FROM {_dialect.Quote(node.Name)} AS {_dialect.Quote(node.As)}");
            }
        }

        private static string JoinPrefix(IEnumerable<string> prefix) =>
            prefix.Skip(1).Aggregate("", (prev, name) => $"{prev}{name}__");
    }
}

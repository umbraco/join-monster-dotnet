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
        /// <exception cref="ArgumentNullException">If <see cref="node"/> or <see cref="context"/> is null.</exception>
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
                    if (!(parent is SqlTable table))
                        throw new ArgumentException($"'{nameof(parent)}' must be of type {typeof(SqlTable)}", nameof(parent));

                    var parentTable = table.As;
                    selections.Add($"{_dialect.Quote(parentTable)}.{_dialect.Quote(sqlColumn.Name)} AS {_dialect.Quote(JoinPrefix(prefix) + sqlColumn.As)}");
                    break;
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
            tables.Add($"FROM {_dialect.Quote(node.Name)} AS {_dialect.Quote(node.As)}");

            var arguments = node.Arguments.ToDictionary(x => x.Name, x => x.Value.Value);

            var whereTask = node.Where?.Invoke(_dialect.Quote(node.As), arguments, context.UserContext);
            if (whereTask != null)
            {
                var where = await whereTask.ConfigureAwait(false);
                if (where != null)
                    wheres.Add(where);
            }
        }

        private static string JoinPrefix(IEnumerable<string> prefix) =>
            prefix.Skip(1).Aggregate("", (prev, name) => $"{prev}{name}__");
    }
}

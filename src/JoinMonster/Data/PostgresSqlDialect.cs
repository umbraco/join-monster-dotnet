using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using JoinMonster.Language.AST;

namespace JoinMonster.Data
{
    /// <summary>
    /// PostgreSQL Dialect
    /// </summary>
    public class PostgresSqlDialect : SqlDialect
    {
        /// <inheritdoc />
        protected override string MaxLimit { get; } = "ALL";

        /// <inheritdoc />
        public override string Quote(string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));

            var jsonSelector = str.IndexOf("->", StringComparison.OrdinalIgnoreCase);
            if (jsonSelector == -1) return $@"""{str}""";

            var identifier = str.Substring(0, jsonSelector);
            return $@"""{identifier}""{str.Substring(jsonSelector)}";
        }

        /// <inheritdoc />
        public override string CompositeKey(string parentTable, IEnumerable<string> keys)
        {
            if (parentTable == null) throw new ArgumentNullException(nameof(parentTable));
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            var result = keys.Select(key => $@"{Quote(parentTable)}.{Quote(key)}");
            return $"NULLIF(CONCAT({string.Join(", ", result)}), '')";
        }

        /// <inheritdoc />
        public override void HandleJoinedOneToManyPaginated(SqlTable parent, SqlTable node,
            IDictionary<string, object> arguments, IResolveFieldContext context, ICollection<string> tables,
            string? joinCondition)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (tables == null) throw new ArgumentNullException(nameof(tables));

            if (node.Join == null)
                throw new JoinMonsterException($"{nameof(node)}.{nameof(node.Join)} cannot be null.");

            var pagingWhereConditions = new List<string>
            {
                node.Join(Quote(parent.As), Quote(node.As), arguments, context.UserContext)
            };

            var where = node.Where?.Invoke(Quote(node.As), arguments, context.UserContext);
            if (where != null)
                pagingWhereConditions.Add(where);

            if (node.OrderBy != null)
            {
                var (limit, offset, order) = InterpretForOffsetPaging(node, arguments, context);
                tables.Add(OffsetPagingSelect(node.Name, pagingWhereConditions, order, limit, offset,
                    node.As, joinCondition, "LEFT"));
            }
            else
            {
                throw new JoinMonsterException("Cannot paginate without an OrderBy clause.");
            }
        }
    }
}

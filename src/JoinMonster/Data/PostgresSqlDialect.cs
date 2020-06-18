using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using JoinMonster.Builders;
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
            IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context, ICollection<string> tables,
            IDictionary<string, object> parameters, string? joinCondition)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (tables == null) throw new ArgumentNullException(nameof(tables));

            if (node.Join == null)
                throw new JoinMonsterException($"{nameof(node)}.{nameof(node.Join)} on table '{node.Name}' cannot be null.");

            var join = new JoinBuilder(this, Quote(parent.As), Quote(node.As));
            node.Join(join, arguments, context, node);

            if(join.Condition == null)
                throw new JoinMonsterException($"The join condition on table '{node.Name}' cannot be null.");

            var pagingWhereConditions = new List<string>
            {
                join.Condition
            };

            if (node.Where != null)
            {
                var whereBuilder = new WhereBuilder(this, Quote(node.As), pagingWhereConditions, parameters);
                node.Where?.Invoke(whereBuilder, arguments, context);
            }

            if (node.SortKey != null)
            {
                var (limit, order, whereCondition) = InterpretForKeysetPaging(node, arguments, context);
                if (whereCondition != null)
                    pagingWhereConditions.Add(whereCondition);
                tables.Add(KeysetPagingSelect(node.Name, pagingWhereConditions, order, limit, node.As, joinCondition, "LEFT"));
            }
            else if (node.OrderBy != null)
            {
                var (limit, offset, order) = InterpretForOffsetPaging(node, arguments, context);
                tables.Add(OffsetPagingSelect(node.Name, pagingWhereConditions, order, limit, offset,
                    node.As, joinCondition, "LEFT"));
            }
            else
            {
                throw new JoinMonsterException("Cannot paginate without a SortKey or an OrderBy clause.");
            }
        }

        /// <inheritdoc />
        public override void HandlePaginationAtRoot(Node? parent, SqlTable node, IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context, ICollection<string> tables, IDictionary<string, object> parameters)
        {
            var pagingWhereConditions = new List<string>();
            if (node.SortKey != null)
            {
                var (limit, order, whereCondition) = InterpretForKeysetPaging(node, arguments, context);
                if (whereCondition != null)
                    pagingWhereConditions.Add(whereCondition);

                if (node.Where != null)
                {
                    var whereBuilder = new WhereBuilder(this, Quote(node.As), pagingWhereConditions, parameters);
                    node.Where.Invoke(whereBuilder, arguments, context);
                }

                tables.Add(KeysetPagingSelect(node.Name, pagingWhereConditions, order, limit, node.As, null, null));

            }
            else if (node.OrderBy != null)
            {
                var (limit, offset, order) = InterpretForOffsetPaging(node, arguments, context);

                if (node.Where != null)
                {
                    var whereBuilder = new WhereBuilder(this, Quote(node.As), pagingWhereConditions, parameters);
                    node.Where.Invoke(whereBuilder, arguments, context);
                }

                tables.Add(OffsetPagingSelect(node.Name, pagingWhereConditions, order, limit, offset, node.As, null,
                    null));
            }
        }
    }
}

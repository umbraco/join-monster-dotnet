using System;
using System.Collections.Generic;
using GraphQL;
using JoinMonster.Builders;
using JoinMonster.Configs;
using JoinMonster.Language.AST;

namespace JoinMonster.Data
{
    public abstract class SqlDialect : ISqlDialect
    {
        /// <summary>
        /// The max LIMIT supported by the database.
        /// </summary>
        protected virtual string MaxLimit { get; } = "-1";

        /// <inheritdoc />
        public virtual string Quote(string str) => $"\"{str}\"";

        /// <inheritdoc />
        public abstract string CompositeKey(string parentTable, IEnumerable<string> keys);

        /// <inheritdoc />
        public abstract void HandleJoinedOneToManyPaginated(SqlTable parent, SqlTable node,
            IDictionary<string, object> arguments, IResolveFieldContext context, ICollection<string> tables,
            string? joinCondition);

        protected virtual string OffsetPagingSelect(string table, IEnumerable<string> pagingWhereConditions, string order,
            int limit, int offset, string @as, string? joinCondition, string? joinType, object? extraJoin = null)
        {
            var whereCondition = string.Join(" AND ", pagingWhereConditions);
            if (string.IsNullOrEmpty(whereCondition)) whereCondition = "TRUE";

            if (joinCondition == null)
            {
                return $@"FROM (
  SELECT {Quote(@as)}.*, COUNT(*) OVER () AS {Quote("$total")}
  FROM {Quote(table)} {Quote(@as)}
  WHERE {whereCondition}
  ORDER BY {order}
  LIMIT {(limit == -1 ? MaxLimit : (object) limit)} OFFSET {offset}
) ${Quote(@as)}";
            }

            return $@"{joinType ?? ""} JOIN LATERAL (
  SELECT {Quote(@as)}.*, COUNT(*) OVER () AS {Quote("$total")}
  FROM {Quote(table)} {Quote(@as)}
  WHERE {whereCondition}
  ORDER BY {order}
  LIMIT {(limit == -1 ? MaxLimit : (object) limit)} OFFSET {offset}
) {Quote(@as)} ON {joinCondition}";

        }

        protected virtual string OrderColumnsToString(OrderByDelegate order, string tableAlias, IDictionary<string, object> arguments, IResolveFieldContext context)
        {
            var orders = new List<string>();
            var orderByBuilder = new OrderByBuilder();
            order(orderByBuilder, arguments, context.UserContext);
            var orderBy = orderByBuilder.OrderBy;

            if (orderBy == null) return "";

            do
            {
                orders.Add($"{Quote(tableAlias)}.{Quote(orderBy.Column)} {(orderBy.Direction == SortDirection.Ascending ? "ASC" : "DESC")}");
            } while ((orderBy = orderBy.ThenBy) != null);

            return string.Join(", ", orders);
        }

        protected virtual (int limit, int offset, string order) InterpretForOffsetPaging(SqlTable node, IDictionary<string, object> arguments, IResolveFieldContext context)
        {
            if (arguments.ContainsKey("last"))
                throw new JoinMonsterException("Backward pagination not supported with offsets. Consider using keyset pagination instead");

            string? orderTable = null;
            OrderByDelegate? orderColumns = null;

            if (node.OrderBy != null)
            {
                orderTable = node.As;
                orderColumns = node.OrderBy;
            }
            else if(node.Junction != null)
            {
                orderTable = node.Junction.As;
                orderColumns = node.Junction.OrderBy;
            }

            if (orderTable == null || orderColumns == null)
                throw new JoinMonsterException("Cannot do offset paging without an OrderBy clause.");

            var limit = -1;
            var offset = 0;

            if (arguments.TryGetValue("first", out var first))
            {
                limit = Convert.ToInt32(first);
                // we'll get one extra item (hence the +1). this is to determine if there is a next page or not
                if (node.Paginate)
                    limit++;
                if (arguments.TryGetValue("after", out var after))
                    offset = ConnectionUtils.CursorToOffset((string) after);
            }

            var order = OrderColumnsToString(orderColumns, orderTable, arguments, context);

            return (limit, offset, order);
        }
    }
}

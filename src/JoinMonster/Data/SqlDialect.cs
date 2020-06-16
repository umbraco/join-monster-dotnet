using System;
using System.Collections.Generic;
using System.Text.Json;
using GraphQL;
using JoinMonster.Builders;
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
            IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context, ICollection<string> tables,
            string? joinCondition);

        /// <inheritdoc />
        public abstract void HandlePaginationAtRoot(Node? parent, SqlTable node, IReadOnlyDictionary<string, object> arguments,
            IResolveFieldContext context, ICollection<string> tables);

        protected virtual string KeysetPagingSelect(string table, IEnumerable<string> pagingWhereCondition, string order,
            int limit, string @as, string? joinCondition, string? joinType) {
            var whereCondition = string.Join(" AND ", pagingWhereCondition);
            if (string.IsNullOrEmpty(whereCondition))
                whereCondition = "TRUE";

            if (joinCondition == null)
                return $@"FROM (
  SELECT {Quote(@as)}.*
  FROM {table} {Quote(@as)}
  WHERE {whereCondition}
  ORDER BY {order}
  LIMIT {(limit == -1 ? MaxLimit : (object) limit)}
) {Quote(@as)}";

            return $@"{joinType ?? ""} JOIN LATERAL (
  SELECT {Quote(@as)}.*
  FROM {Quote(table)} {Quote(@as)}
  WHERE {whereCondition}
  ORDER BY {order}
  LIMIT {(limit == -1 ? MaxLimit : (object) limit)}
) {Quote(@as)} ON {joinCondition}";

        }

        protected virtual string OffsetPagingSelect(string table, IEnumerable<string> pagingWhereConditions, string order,
            int limit, int offset, string @as, string? joinCondition, string? joinType, object? extraJoin = null)
        {
            var whereCondition = string.Join(" AND ", pagingWhereConditions);
            if (string.IsNullOrEmpty(whereCondition))
                whereCondition = "TRUE";

            if (joinCondition == null)
            {
                return $@"FROM (
  SELECT {Quote(@as)}.*, COUNT(*) OVER () AS {Quote("$total")}
  FROM {Quote(table)} {Quote(@as)}
  WHERE {whereCondition}
  ORDER BY {order}
  LIMIT {(limit == -1 ? MaxLimit : (object) limit)} OFFSET {offset}
) {Quote(@as)}";
            }

            return $@"{joinType ?? ""} JOIN LATERAL (
  SELECT {Quote(@as)}.*, COUNT(*) OVER () AS {Quote("$total")}
  FROM {Quote(table)} {Quote(@as)}
  WHERE {whereCondition}
  ORDER BY {order}
  LIMIT {(limit == -1 ? MaxLimit : (object) limit)} OFFSET {offset}
) {Quote(@as)} ON {joinCondition}";
        }

        protected virtual string OrderColumnsToString(OrderBy? orderBy, string tableAlias)
        {
            var orders = new List<string>();

            if (orderBy == null) return "";

            do
            {
                orders.Add($"{Quote(tableAlias)}.{Quote(orderBy.Column)} {(orderBy.Direction == SortDirection.Ascending ? "ASC" : "DESC")}");
            } while ((orderBy = orderBy.ThenBy) != null);

            return string.Join(", ", orders);
        }

        protected virtual (int limit, int offset, string order) InterpretForOffsetPaging(SqlTable node,
            IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context)
        {
            if (arguments.ContainsKey("last"))
                throw new JoinMonsterException(
                    "Backward pagination not supported with offsets. Consider using keyset pagination instead");

            string? orderTable = null;
            OrderBy? orderColumns = null;

            if (node.OrderBy != null)
            {
                orderTable = node.As;
                orderColumns = node.OrderBy;
            }
            else if (node.Junction != null)
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

            var order = OrderColumnsToString(orderColumns, orderTable);

            return (limit, offset, order);
        }

        protected (int limit, string order, string? whereCondition) InterpretForKeysetPaging(SqlTable node,
            IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context)
        {
            string? sortTable = null;
            SortKey? sortKey = null;

            if (node.SortKey != null)
            {
                sortTable = node.As;
                sortKey = node.SortKey;
            }
            else if (node.Junction != null)
            {
                sortTable = node.Junction.As;
                sortKey = node.Junction.SortKey;
            }

            if (sortTable == null || sortKey == null)
                throw new JoinMonsterException("Cannot do keyset paging without an SortKey clause.");

            var descending = sortKey.Direction == SortDirection.Descending;
            if (arguments.ContainsKey("last"))
                descending = !descending;

            var orderBy = new OrderByBuilder();
            ThenOrderByBuilder? thenBy = null;

            foreach (var key in sortKey.Key)
            {
                if (thenBy == null)
                {
                    thenBy = descending ? orderBy.ByDescending(key) : orderBy.By(key);
                }
                else
                {
                    if (descending) thenBy.ThenByDescending(key);
                    else thenBy.ThenBy(key);
                }
            }

            var order = OrderColumnsToString(orderBy.OrderBy, sortTable);
            var limit = -1;
            var offset = 0;
            string? whereCondition = null;

            if (arguments.TryGetValue("first", out var first))
            {
                limit = Convert.ToInt32(first) + 1;
                if (arguments.TryGetValue("after", out var after))
                {
                    var cursorObj = ConnectionUtils.CursorToObject((string) after);
                    ValidateCursor(cursorObj, sortKey.Key);
                    whereCondition = SortKeyToWhereCondition(cursorObj, descending, sortTable);

                    if (arguments.ContainsKey("before"))
                    {
                        throw new JoinMonsterException("Using 'before' with 'first' is nonsensical.");
                    }
                }
            }
            else if (arguments.TryGetValue("last", out var last))
            {
                limit = Convert.ToInt32(last) + 1;
                if (arguments.TryGetValue("before", out var before))
                {
                    var cursorObj = ConnectionUtils.CursorToObject((string) before);
                    ValidateCursor(cursorObj, sortKey.Key);
                    whereCondition = SortKeyToWhereCondition(cursorObj, descending, sortTable);
                }

                if (arguments.ContainsKey("after"))
                {
                    throw new JoinMonsterException("Using 'after' with 'last' is nonsensical.");
                }
            }

            return (limit, order, whereCondition);
        }

        // the cursor contains the sort keys. it needs to match the keys specified in the `sortKey` on this field in the schema
        private void ValidateCursor(IDictionary<string, object> cursorObj, string[] expectedKeys)
        {
            var actualKeys = cursorObj.Keys;
            var expectedKeySet = new HashSet<string>(expectedKeys);
            var actualKeySet = new HashSet<string>(actualKeys);

            foreach (var key in actualKeys)
            {
                if (!expectedKeySet.Contains(key))
                {
                    throw new JoinMonsterException($"Invalid cursor. The column '{key}' is not in the sort key.");
                }
            }

            foreach (var key in expectedKeys)
            {
                if (!actualKeySet.Contains(key))
                {
                    throw new JoinMonsterException($"Invalid cursor. The column '{key}' is not in the cursor.");
                }
            }
        }

        // take the sort key and translate that for the where clause
        private string SortKeyToWhereCondition(IDictionary<string, object> keyObj, bool descending, string sortTable)
        {
            var sortColumns = new List<string>();
            var sortValues = new List<string>();
            foreach (var kvp in keyObj)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                sortColumns.Add($"{Quote(sortTable)}.{Quote(key)}");
                sortValues.Add(MaybeQuote(value));
            }

            var op = descending ? "<" : ">";

            return $"{string.Join(", ", sortColumns)} {op} ({string.Join(", ", sortValues)})";
        }

        protected virtual string MaybeQuote(object value)
        {
            if (value == null)
                return "NULL";

            if (value is JsonElement element && element.ValueKind == JsonValueKind.Number)
                return value.ToString();

            if (value is byte || value is short || value is int || value is long)
                return value.ToString();

            // Ported from https://github.com/join-monster/join-monster/blob/eed0264b3ad53e3a43ee6791f77207f9bb624e24/src/util.js#L81-L104
            // Picked from https://github.com/brianc/node-postgres/blob/876018/lib/client.js#L235..L260
            // Ported from PostgreSQL 9.2.4 source code in src/interfaces/libpq/fe-exec.c
            var hasBackslash = false;
            var escaped = "\'";

            var valueString = value.ToString();

            foreach (var c in valueString)
            {
                switch (c)
                {
                    case '\'':
                        escaped += c + c;
                        break;
                    case '\\':
                        escaped += c + c;
                        hasBackslash = true;
                        break;
                    default:
                        escaped += c;
                        break;
                }
            }

            escaped += "\'";

            if (hasBackslash)
            {
                escaped = " E" + escaped;
            }

            return escaped;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using GraphQL;
using GraphQL.Execution;
using JoinMonster.Builders;
using JoinMonster.Builders.Clauses;
using JoinMonster.Language.AST;
using SortKey = JoinMonster.Language.AST.SortKey;

namespace JoinMonster.Data
{
    /// <inheritdoc />
    public abstract class SqlDialect : ISqlDialect
    {
        /// <summary>
        /// The max LIMIT supported by the database.
        /// </summary>
        protected virtual string MaxLimit { get; } = "-1";

        /// <inheritdoc />
        public virtual string Quote(string str) => str.Contains("\"") ? str : $"\"{str}\"";

        /// <inheritdoc />
        public abstract string CompositeKey(string parentTable, IEnumerable<string> keys);

        /// <inheritdoc />
        public abstract void HandleJoinedOneToManyPaginated(SqlTable parent, SqlTable node,
            IReadOnlyDictionary<string, ArgumentValue> arguments, IResolveFieldContext context, ICollection<string> tables,
            SqlCompilerContext compilerContext, string? joinCondition);

        /// <inheritdoc />
        public abstract void HandlePaginationAtRoot(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments,
            IResolveFieldContext context, ICollection<string> tables, SqlCompilerContext compilerContext);

        /// <inheritdoc />
        public abstract void HandleBatchedOneToManyPaginated(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments,
            IResolveFieldContext context, ICollection<string> tables, ICollection<string> selections, IEnumerable<object> batchScope, SqlCompilerContext compilerContext);

        /// <inheritdoc />
        public abstract void HandleBatchedManyToManyPaginated(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments,
            IResolveFieldContext context, ICollection<string> tables, IEnumerable<object> batchScope, SqlCompilerContext compilerContext, string joinCondition);

        /// <inheritdoc />
        public virtual string CompileConditions(IEnumerable<WhereCondition> conditions, SqlCompilerContext context)
        {
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var result = new StringBuilder();

            using var enumerator = conditions.GetEnumerator();
            var i = 0;
            while (enumerator.MoveNext())
            {
                var condition = enumerator.Current;

                var compiled = CompileCondition(condition, context);

                if (string.IsNullOrEmpty(compiled))
                    continue;

                var boolOperator = i == 0 ? "" : condition.IsOr ? " OR " : " AND ";
                result.AppendFormat("{0}{1}", boolOperator, compiled);
                i++;
            }

            return result.ToString();
        }

        /// <inheritdoc />
        public virtual string CompileOrderBy(OrderBy orderBy)
        {
            if (orderBy == null) throw new ArgumentNullException(nameof(orderBy));

            var orders = new List<string>();

            var order = orderBy;
            do
            {
                orders.Add($"{Quote(order.Table)}.{Quote(order.Column)} {(order.Direction == SortDirection.Ascending ? "ASC" : "DESC")}");
            } while ((order = order.ThenBy) != null);

            return string.Join(", ", orders);
        }

        protected virtual string CompileCondition(WhereCondition condition, SqlCompilerContext context)
        {
            return condition switch
            {
                CompareColumnsCondition compareColumnsCondition => CompileCondition(compareColumnsCondition, context),
                CompareCondition compareCondition => CompileCondition(compareCondition, context),
                InCondition inCondition => CompileCondition(inCondition, context),
                NestedCondition nestedCondition => CompileCondition(nestedCondition, context),
                RawCondition rawCondition => CompileCondition(rawCondition, context),
                RawSubQueryCondition subQueryCondition => CompileCondition(subQueryCondition, context),
                _ => throw new ArgumentOutOfRangeException(nameof(condition))
            };
        }

        protected virtual string CompileCondition(CompareCondition condition, SqlCompilerContext context)
        {
            var table = condition.Table;
            var column = condition.Column;
            var value = condition.Value;
            var @operator = condition.Operator;

            var parameterName = context.AddParameter(value);
            var sql = $"{Quote(table)}.{Quote(column)} {@operator} {parameterName}";
            return condition.IsNot ? $"NOT({sql})" : sql;

        }

        protected virtual string CompileCondition(CompareColumnsCondition condition, SqlCompilerContext context)
        {
            var sql = $"{Quote(condition.Table)}.{Quote(condition.First)} {condition.Operator} {Quote(condition.SecondTable)}.{Quote(condition.Second)}";
            return condition.IsNot ? $"NOT({sql})" : sql;
        }

        protected virtual string CompileCondition(InCondition condition, SqlCompilerContext context)
        {
            var parameterName = context.AddParameter(PrepareValue(condition.Values, null));
            var sql = $"{Quote(condition.Table)}.{Quote(condition.Column)} = ANY({parameterName})";
            return condition.IsNot ? $"NOT({sql})" : sql;
        }

        protected virtual string CompileCondition(NestedCondition condition, SqlCompilerContext context)
        {
            var sql = CompileConditions(condition.Conditions, context);
            return condition.IsNot ? $"(NOT({sql}))" : $"({sql})";
        }

        protected virtual string CompileCondition(RawCondition condition, SqlCompilerContext context)
        {
            var sql = condition.Sql;
            if (condition.Parameters != null)
            {
                foreach (var parameter in condition.Parameters)
                {
                    var parameterName = context.AddParameter(parameter.Value);
                    sql = sql.Replace($"@{parameter.Key}", parameterName);
                }
            }

            return sql;
        }

        protected virtual string CompileCondition(RawSubQueryCondition condition, SqlCompilerContext context)
        {
            var sql = condition.Sql.Replace("?", CompileConditions(condition.Conditions, context));

            return condition.IsNot ? $"(NOT({sql}))" : $"({sql})";
        }

        protected virtual string KeysetPagingSelect(string table, IEnumerable<WhereCondition> pagingWhereConditions, string order,
            int limit, string @as, string? joinCondition, string? joinType, SqlCompilerContext compilerContext) {

            var whereCondition = CompileConditions(pagingWhereConditions, compilerContext);
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

            return $@"{joinType} JOIN LATERAL (
  SELECT {Quote(@as)}.*
  FROM {Quote(table)} {Quote(@as)}
  WHERE {whereCondition}
  ORDER BY {order}
  LIMIT {(limit == -1 ? MaxLimit : (object) limit)}
) {Quote(@as)} ON {joinCondition}";

        }

        protected virtual string OffsetPagingSelect(string table, IEnumerable<WhereCondition> pagingWhereConditions, string order,
            int limit, int offset, string @as, string? joinCondition, string? joinType, SqlCompilerContext compilerContext,
            object? extraJoin = null)
        {
            var whereCondition = CompileConditions(pagingWhereConditions, compilerContext);
            if (string.IsNullOrEmpty(whereCondition))
                whereCondition = "TRUE";

            if (joinCondition == null)
            {
                return $@"FROM (
  SELECT {Quote(@as)}.*, COUNT(*) OVER () AS {Quote("$total")}
  FROM {Quote(table)} {Quote(@as)}{(extraJoin == null ? null : $"\n{extraJoin}")}
  WHERE {whereCondition}
  ORDER BY {order}
  LIMIT {(limit == -1 ? MaxLimit : (object) limit)} OFFSET {offset}
) {Quote(@as)}";
            }

            return $@"{joinType} JOIN LATERAL (
  SELECT {Quote(@as)}.*, COUNT(*) OVER () AS {Quote("$total")}
  FROM {Quote(table)} {Quote(@as)}
  WHERE {whereCondition}
  ORDER BY {order}
  LIMIT {(limit == -1 ? MaxLimit : (object) limit)} OFFSET {offset}
) {Quote(@as)} ON {joinCondition}";
        }

        protected virtual (int limit, int offset, string order) InterpretForOffsetPaging(SqlTable node,
            IReadOnlyDictionary<string, ArgumentValue> arguments, IResolveFieldContext context)
        {
            if (arguments.TryGetValue("last", out var last) && last.Value != null)
                throw new JoinMonsterException(
                    "Backward pagination not supported with offsets. Consider using keyset pagination instead");

            OrderBy? orderColumns = null;

            if (node.OrderBy != null)
            {
                orderColumns = node.OrderBy;
            }
            else if (node.Junction != null)
            {
                orderColumns = node.Junction.OrderBy;
            }

            if (orderColumns == null)
                throw new JoinMonsterException("Cannot do offset paging without an OrderBy clause.");

            var limit = -1;
            var offset = 0;

            if (arguments.TryGetValue("first", out var first) && first.Value is int firstValue)
            {
                limit = firstValue;
                // we'll get one extra item (hence the +1). this is to determine if there is a next page or not
                if (node.Paginate)
                    limit++;
                if (arguments.TryGetValue("after", out var after) && after.Value is string afterValue)
                    offset = ConnectionUtils.CursorToOffset(afterValue);
            }

            var order = CompileOrderBy(orderColumns);

            return (limit, offset, order);
        }

        protected (int limit, string order, WhereCondition? whereCondition) InterpretForKeysetPaging(SqlTable node,
            IReadOnlyDictionary<string, ArgumentValue> arguments, IResolveFieldContext context)
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

            var builder = new OrderByBuilder(sortTable);
            ThenOrderByBuilder? thenBy = null;
            var isBefore = arguments.TryGetValue("last", out var last) && last.Value != null;

            var sort = sortKey;
            do
            {
                var descending = sort.Direction == SortDirection.Descending;
                if (isBefore) descending = !descending;

                if (thenBy == null)
                {
                    thenBy = descending ? builder.ByDescending(sort.Column) : builder.By(sort.Column);
                }
                else
                {
                    thenBy = descending ? thenBy.ThenByDescending(sort.Column) : thenBy.ThenBy(sort.Column);
                }
            } while ((sort = sort.ThenBy) != null);

            var order = builder.OrderBy == null ? "" : CompileOrderBy(builder.OrderBy);
            var limit = -1;
            var offset = 0;
            WhereCondition? whereCondition = null;

            if (arguments.TryGetValue("first", out var first) && first.Value is int firstValue)
            {
                limit = firstValue + 1;
                if (arguments.TryGetValue("after", out var after) && after.Value is string afterValue)
                {
                    var cursorObj = ConnectionUtils.CursorToObject(afterValue);
                    ValidateCursor(cursorObj, GetKeys(sortKey));
                    whereCondition = SortKeyToWhereCondition(sortKey, cursorObj, isBefore, sortTable);
                }

                if (arguments.TryGetValue("before", out var before) && before.Value != null)
                {
                    throw new JoinMonsterException("Using 'before' with 'first' is nonsensical.");
                }
            }
            else if (last.Value is int lastValue)
            {
                limit = lastValue + 1;
                if (arguments.TryGetValue("before", out var before) && before.Value is string beforeValue)
                {
                    var cursorObj = ConnectionUtils.CursorToObject(beforeValue);
                    ValidateCursor(cursorObj, GetKeys(sortKey));
                    whereCondition = SortKeyToWhereCondition(sortKey, cursorObj, isBefore, sortTable);
                }

                if (arguments.TryGetValue("after", out var after) && after.Value != null)
                {
                    throw new JoinMonsterException("Using 'after' with 'last' is nonsensical.");
                }
            }

            return (limit, order, whereCondition);
        }


        // the cursor contains the sort keys. it needs to match the keys specified in the `sortKey` on this field in the schema
        private void ValidateCursor(IDictionary<string, object> cursorObj, IEnumerable<string> expectedKeys)
        {
            var expectedKeySet = new HashSet<string>(expectedKeys);
            var actualKeySet = new HashSet<string>(cursorObj.Keys);

            foreach (var key in actualKeySet)
            {
                if (!expectedKeySet.Contains(key))
                {
                    throw new JoinMonsterException($"Invalid cursor. The column '{key}' is not in the sort key.");
                }
            }

            foreach (var key in expectedKeySet)
            {
                if (!actualKeySet.Contains(key))
                {
                    throw new JoinMonsterException($"Invalid cursor. The column '{key}' is not in the cursor.");
                }
            }
        }

        private IEnumerable<string> GetKeys(SortKey? sortKey)
        {
            if (sortKey == null)
                return Enumerable.Empty<string>();

            var keys = new List<string>();

            do
            {
                keys.Add(sortKey.As);
            } while ((sortKey = sortKey.ThenBy) != null);

            return keys;
        }

        // Returns the SQL implementation of the sort key cursor WHERE conditions
        // Note: This operation compares the first key, then the second key, then the third key, etc, in order and independently.
        // It's not a A > B AND C > D because C and D should only be compared of A and B are equal. If there are many sortKeys,
        // then we need to implement the hierarchical comparison between them.
        // See https://engineering.shopify.com/blogs/engineering/pagination-relative-cursors for an explanation of what this is doing
        private WhereCondition SortKeyToWhereCondition(SortKey sortKey, IDictionary<string, object> keyObj,
            bool isBefore, string sortTable)
        {
            WhereCondition Condition(SortKey ordering, string? op = null)
            {
                var descending = ordering.Direction == SortDirection.Descending;
                if (isBefore) descending = !descending;
                op ??= descending ? "<" : ">";

                var conditions = new List<WhereCondition>();
                var where = new WhereBuilder(Quote(sortTable), conditions);

                var value = keyObj[ordering.As];
                var preparedValue = PrepareValue(value, ordering.Type);

                HandleSortKeyWhere(where, ordering.Column, preparedValue, op, ordering.Type ?? preparedValue.GetType());

                return conditions.Count == 1 ? conditions[0] : new NestedCondition(conditions);
            }

            var sortKeys = new Stack<SortKey>();

            var sort = sortKey;
            do
            {
                sortKeys.Push(sort);
            } while ((sort = sort.ThenBy) != null);

            return sortKeys
                .Aggregate(Condition(sortKeys.Pop()),
                    (agg, ordering) =>
                        new NestedCondition(
                            new[]
                            {
                                Condition(ordering),
                                new NestedCondition(new[] {Condition(ordering, "="), agg})
                                {
                                    IsOr = true
                                }
                            })
                );
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="where">The where builder</param>
        /// <param name="column">The column name.</param>
        /// <param name="value">The value.</param>
        /// <param name="op">The operator.</param>
        /// <param name="type">The value type.</param>
        protected virtual void HandleSortKeyWhere(WhereBuilder where, string column, object value, string op, Type type)
        {
            where.Column(column, value, op);
        }

        internal static object PrepareValue(object value, Type? type)
        {
            if (value is JsonElement element)
            {
                if (type != null)
                {
                    if (type.IsArray && type.HasElementType)
                    {
                        var elementType = type.GetElementType();
                        var list = (IList) Activator.CreateInstance(type, element.GetArrayLength());

                        var i = -1;
                        foreach (var child in element.EnumerateArray())
                            list[++i] = PrepareValue(child, elementType);

                        return list;
                    }

                    if (type == typeof(DateTime))
                        return element.GetDateTime();
                    if (type == typeof(Guid))
                        return element.GetGuid();
                    if (type == typeof(byte))
                        return element.GetInt16();
                    if (type == typeof(int))
                        return element.GetInt32();
                    if (type == typeof(long))
                        return element.GetInt64();
                    if (type == typeof(double))
                        return element.GetDouble();
                    if (type == typeof(decimal) || type == typeof(float))
                        return element.GetDecimal();
                    if (type == typeof(bool))
                        return element.GetBoolean();
                    if (type == typeof(string))
                        return element.GetString();

                    throw new NotSupportedException($"Type '{type}' is not supported.");
                }

                switch (element.ValueKind)
                {
                    case JsonValueKind.Array:
                    {
                        var result = element.EnumerateArray().Select(x => PrepareValue(x, null)).ToList();
                        return CastArray(result);
                    }
                    case JsonValueKind.String:
                    {
                        var result = element.GetString();
                        if (Guid.TryParse(result, out var guid))
                            return guid;
                        if (DateTime.TryParse(result, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dateTime))
                            return dateTime;
                        return result;
                    }

                    case JsonValueKind.Number:
                        if (element.TryGetInt32(out var intValue))
                            return intValue;
                        if (element.TryGetInt64(out var longValue))
                            return longValue;
                        return element.GetDecimal();
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return element.GetBoolean();
                    case JsonValueKind.Undefined:
                    case JsonValueKind.Object:
                    case JsonValueKind.Null:
                        throw new NotSupportedException("Cannot cast 'Undefined', 'Object' or 'Null'.");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return value;
        }

        internal static IEnumerable CastArray(IEnumerable<object> result)
        {
            var values = result.Where(x => x != null).ToList();
            if (values.Count == 0)
                return values;

            var firstValue = values[0];

            return firstValue switch
            {
                byte _ => values.Cast<byte>().ToList(),
                int _ => values.Cast<int>().ToList(),
                long _ => values.Cast<long>().ToList(),
                double _ => values.Cast<double>().ToList(),
                decimal _ => values.Cast<decimal>().ToList(),
                DateTime _ => values.Cast<DateTime>().ToList(),
                bool _ => values.Cast<bool>().ToList(),
                Guid _ => values.Cast<Guid>().ToList(),
                _ => values.Select(x => x.ToString()).ToList()
            };
        }

        protected virtual string MaybeQuote(object? value)
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

using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using JoinMonster.Builders;
using JoinMonster.Builders.Clauses;
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

            if (str.Contains("\"")) return str;

            var jsonSelector = str.IndexOf("->", StringComparison.Ordinal);
            if (jsonSelector == -1) return $"\"{str}\"";

            var identifier = str.Substring(0, jsonSelector);
            return $"\"{identifier}\"{str.Substring(jsonSelector)}";
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
            SqlCompilerContext compilerContext, string? joinCondition)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (tables == null) throw new ArgumentNullException(nameof(tables));

            if (node.Join == null)
                throw new JoinMonsterException($"{nameof(node)}.{nameof(node.Join)} on table '{node.Name}' cannot be null.");

            var join = new JoinBuilder(Quote(parent.Name), Quote(parent.As), Quote(node.Name), Quote(node.As));
            node.Join(join, arguments, context, node);

            if(join.Condition == null)
                throw new JoinMonsterException($"The join condition on table '{node.Name}' cannot be null.");

            var pagingWhereConditions = new List<WhereCondition>
            {
                 join.Condition
            };

            if (node.Where != null)
            {
                var whereBuilder = new WhereBuilder(Quote(node.As), pagingWhereConditions);
                node.Where.Invoke(whereBuilder, arguments, context, node);
            }

            if (node.SortKey != null)
            {
                var (limit, order, whereCondition) = InterpretForKeysetPaging(node, arguments, context);
                if (whereCondition != null)
                    pagingWhereConditions.Add(whereCondition);
                tables.Add(KeysetPagingSelect(node.Name, pagingWhereConditions, order, limit, node.As, joinCondition, "LEFT", compilerContext));
            }
            else if (node.OrderBy != null)
            {
                var (limit, offset, order) = InterpretForOffsetPaging(node, arguments, context);
                tables.Add(OffsetPagingSelect(node.Name, pagingWhereConditions, order, limit, offset,
                    node.As, joinCondition, "LEFT", compilerContext));
            }
            else
            {
                throw new JoinMonsterException("Cannot paginate without a SortKey or an OrderBy clause.");
            }
        }

        /// <inheritdoc />
        public override void HandlePaginationAtRoot(Node? parent, SqlTable node, IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context, ICollection<string> tables, SqlCompilerContext compilerContext)
        {
            var pagingWhereConditions = new List<WhereCondition>();
            if (node.SortKey != null)
            {
                var (limit, order, whereCondition) = InterpretForKeysetPaging(node, arguments, context);
                if (whereCondition != null)
                    pagingWhereConditions.Add(whereCondition);

                if (node.Where != null)
                {
                    var whereBuilder = new WhereBuilder(Quote(node.As), pagingWhereConditions);
                    node.Where.Invoke(whereBuilder, arguments, context, node);
                }

                tables.Add(KeysetPagingSelect(node.Name, pagingWhereConditions, order, limit, node.As, null, null, compilerContext));

            }
            else if (node.OrderBy != null)
            {
                var (limit, offset, order) = InterpretForOffsetPaging(node, arguments, context);

                if (node.Where != null)
                {
                    var whereBuilder = new WhereBuilder(Quote(node.As), pagingWhereConditions);
                    node.Where.Invoke(whereBuilder, arguments, context, node);
                }

                tables.Add(OffsetPagingSelect(node.Name, pagingWhereConditions, order, limit, offset, node.As, null,
                    null, compilerContext));
            }
        }

        /// <inheritdoc />
        public override string CompileOrderBy(OrderBy orderBy)
        {
            if (orderBy == null) throw new ArgumentNullException(nameof(orderBy));

            var orders = new List<string>();

            var order = orderBy;
            do
            {
                orders.Add($"{Quote(order.Table)}.{Quote(order.Column.Replace("->>", "->"))} {(order.Direction == SortDirection.Ascending ? "ASC" : "DESC")}");
            } while ((order = order.ThenBy) != null);

            return string.Join(", ", orders);
        }

        /// <inheritdoc />
        protected override void HandleSortKeyWhere(WhereBuilder where, string column, object value, string op, Type type)
        {
            var col = $"{Quote(where.Table)}.{Quote(column)}";

            // cast column if it's a JSON field
            if (col.Contains("->"))
            {
                var isArray = type.IsArray && type.HasElementType;
                var postFix = isArray ? "[]" : "";
                if (isArray) type = type.GetElementType()!;

                if (type == typeof(string))
                {
                    col = $"CAST({column} AS text{postFix})";
                }
                else if (type == typeof(Guid))
                {
                    col = $"CAST({column} AS uuid{postFix})";
                }
                else if (type == typeof(DateTime))
                {
                    col = $"CAST({column} AS timestamp without time zone{postFix})";
                }
                else if (type == typeof(byte))
                {
                    col = $"CAST({column} AS smallint{postFix})";
                }
                else if (type == typeof(int))
                {
                    col = $"CAST({column} AS integer{postFix})";
                }
                else if (type == typeof(long))
                {
                    col = $"CAST({column} AS bigint{postFix})";
                }
                else if (type == typeof(decimal) || type == typeof(double))
                {
                    col = $"CAST({column} AS numeric{postFix})";
                }
                else if (type == typeof(bool))
                {
                    col = $"CAST({column} AS bool{postFix})";
                }
            }

            where.Raw($"{col} {op} @value", new {value});
        }
    }
}

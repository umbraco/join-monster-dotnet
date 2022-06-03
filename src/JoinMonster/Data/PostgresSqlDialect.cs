using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Execution;
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
            IReadOnlyDictionary<string, ArgumentValue> arguments, IResolveFieldContext context, ICollection<string> tables,
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
        public override void HandlePaginationAtRoot(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments, IResolveFieldContext context, ICollection<string> tables, SqlCompilerContext compilerContext)
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

        public override void HandleBatchedOneToManyPaginated(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments,
            IResolveFieldContext context, ICollection<string> tables, IEnumerable<object> batchScope, SqlCompilerContext compilerContext)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (tables == null) throw new ArgumentNullException(nameof(tables));
            if (batchScope == null) throw new ArgumentNullException(nameof(batchScope));
            if (compilerContext == null) throw new ArgumentNullException(nameof(compilerContext));

            if (node.Batch == null)
                throw new InvalidOperationException("node.Batchcannot be null.");

            var thisKeyOperand = $"${node.As}.{node.Batch.ThisKey.Name}";

            var whereConditions = new List<WhereCondition>
            {
                new RawCondition($"{thisKeyOperand} = temp.\"{node.Batch.ParentKey.Name}\"", null)
            };

            if (node.Where != null)
            {
                var whereBuilder = new WhereBuilder(node.As, whereConditions);

                node.Where(whereBuilder, arguments, context, node);
            }

            var tempTable =
                $"FROM (VALUES {string.Join("", batchScope.Select(val => $"({val})"))}) temp(\"{node.Batch.ParentKey.Name}\")";
            tables.Add(tempTable);

            var lateralJoinCondition = $"{thisKeyOperand} = temp.\"{node.Batch.ParentKey.Name}\"";

            if (node.SortKey != null) {
                var (limit, order, whereCondition) = InterpretForKeysetPaging(node, arguments, context);
                whereConditions.Add(whereCondition);

                tables.Add(KeysetPagingSelect(node.Name, whereConditions, order, limit, node.As, lateralJoinCondition,
                    null, compilerContext));

            }
            else if (node.OrderBy != null)
            {
                var (limit, offset, order) = InterpretForOffsetPaging(node, arguments, context);

                tables.Add(OffsetPagingSelect(node.Name, whereConditions, order, limit, offset,
                    node.As, lateralJoinCondition, null, compilerContext));
            }
        }

        public override void HandleBatchedManyToManyPaginated(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments,
            IResolveFieldContext context, ICollection<string> tables, IEnumerable<object> batchScope, SqlCompilerContext compilerContext,
            string joinCondition)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (tables == null) throw new ArgumentNullException(nameof(tables));
            if (batchScope == null) throw new ArgumentNullException(nameof(batchScope));
            if (compilerContext == null) throw new ArgumentNullException(nameof(compilerContext));
            if (joinCondition == null) throw new ArgumentNullException(nameof(joinCondition));

            if (node.Junction == null)
                throw new InvalidOperationException("node.Junction cannot be null.");

            if (node.Junction.Batch == null)
                throw new InvalidOperationException("node.Junction.Batch cannot be null.");

            // var thisKeyOperand = GenerateCastExpressionFromValueType(
            //     $"${node.Junction.As}.{node.Junction.Batch.ThisKey.Name}",
            //     batchScope.ElementAtOrDefault(0)
            // );
            var thisKeyOperand = $"${node.Junction.As}.{node.Junction.Batch.ThisKey.Name}";

            var whereConditions = new List<WhereCondition>
            {
                new RawCondition($"{thisKeyOperand} = temp.\"{node.Junction.Batch.ParentKey.Name}\"", null)
            };

            if (node.Junction.Where != null)
            {
                var whereBuilder = new WhereBuilder(node.Junction.As, whereConditions);
                node.Junction.Where(whereBuilder, arguments, context, node);
            }

            if (node.Where != null)
            {
                var whereBuilder = new WhereBuilder(node.As, whereConditions);

                node.Where(whereBuilder, arguments, context, node);
            }

            var tempTable =
                $"FROM (VALUES {string.Join("", batchScope.Select(val => $"({val})"))}) temp(\"{node.Junction.Batch.ParentKey.Name}\")";

            tables.Add(tempTable);
            var lateralJoinCondition = $"{thisKeyOperand} = temp.\"{node.Junction.Batch.ParentKey.Name}\"";

            string? extraJoin = null;

            if (node.Where != null || node.OrderBy != null)
            {
                extraJoin = $"LEFT JOIN {node.Name} {Quote(node.As)} ON {joinCondition}";
            }

            if (node.SortKey != null || node.Junction.SortKey != null)
            {
                var (limit, order, whereCondition) = InterpretForKeysetPaging(node, arguments, context);
                whereConditions.Add(whereCondition);

                tables.Add(KeysetPagingSelect(node.Junction.Table, whereConditions, order, limit, node.Junction.As,
                    lateralJoinCondition, "LEFT", compilerContext));

            }
            else if (node.OrderBy != null || node.Junction.OrderBy != null)
            {
                var (limit, offset, order) = InterpretForOffsetPaging(node, arguments, context);

                tables.Add(OffsetPagingSelect(node.Junction.Table, whereConditions, order, limit, offset,
                    node.Junction.As, joinCondition, "LEFT", compilerContext, extraJoin));
            }

            tables.Add($"LEFT JOIN {node.Name} AS \"{node.As}\" ON {joinCondition}");
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

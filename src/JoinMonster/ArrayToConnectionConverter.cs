using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Types.Relay.DataObjects;
using JoinMonster.Language.AST;

namespace JoinMonster
{
    internal class ArrayToConnectionConverter
    {
        public object? Convert(IEnumerable<IDictionary<string, object?>> data, SqlTable sqlAst, IResolveFieldContext context)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (sqlAst == null) throw new ArgumentNullException(nameof(sqlAst));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var converted = ConvertInternal(data, sqlAst, context);
            return converted switch
            {
                IEnumerable<IDictionary<string, object?>> enumerable when sqlAst.GrabMany => enumerable,
                IEnumerable<IDictionary<string, object?>> enumerable => enumerable.FirstOrDefault(),
                Connection<object> connection => connection,
                null => throw new JoinMonsterException("Expected result to not be null."),
                _ => throw new JoinMonsterException(
                    $"Expected result to be of type '{typeof(IEnumerable<IDictionary<string, object?>>)}' or '{typeof(Connection<object>)}' but was '{converted.GetType()}'")
            };
        }

        public object Convert(IDictionary<string, object?> data, SqlTable sqlAst, IResolveFieldContext context)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (sqlAst == null) throw new ArgumentNullException(nameof(sqlAst));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var converted = ConvertInternal(data, sqlAst, context);
            return converted switch
            {
                IDictionary<string, object?> enumerable => enumerable.FirstOrDefault(),
                Connection<object> connection => connection,
                null => throw new JoinMonsterException("Expected result to not be null."),
                _ => throw new JoinMonsterException(
                    $"Expected result to be of type '{typeof(IEnumerable<IDictionary<string, object?>>)}' or '{typeof(Connection<object>)}' but was '{converted.GetType()}'")
            };
        }

        private object? ConvertInternal(object? data, Node sqlAst, IResolveFieldContext context)
        {
            var metadata = new Dictionary<string, object?>();

            switch (sqlAst)
            {
                case SqlTable table:
                    metadata.Add("field", table.FieldName);
                    break;
                case SqlColumnBase sqlColumnBase:
                    metadata.Add("field", sqlColumnBase.FieldName);
                    break;
            }

            using var _ = context.Metrics.Subject(Constants.MetricsCategory, "Converting array to connection", metadata);
            foreach (var astChild in sqlAst.Children)
            {
                switch (data)
                {
                    case IEnumerable<Dictionary<string, object?>> array:
                    {
                        foreach (var dataItem in array)
                            RecurseOnObjInData(dataItem, astChild, context);

                        break;
                    }
                    case Dictionary<string, object?> dataItem:
                        RecurseOnObjInData(dataItem, astChild, context);
                        break;
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(data), $"Unknown type {data.GetType()}");
                }
            }

            if (sqlAst is not SqlTable sqlTable || sqlTable.Paginate == false)
                return data;

            switch (data)
            {
                case null when sqlTable.Paginate:
                    return new Connection<object>
                    {
                        PageInfo = new PageInfo(),
                        Edges = new List<Edge<object>>()
                    };
                case null:
                    return null!;
                case List<Dictionary<string, object?>> dataArr when sqlTable.SortKey != null || sqlTable.Junction?.SortKey != null:
                {
                    var arguments = sqlTable.Arguments;

                    // $total was a special column for determining the total number of items
                    var arrayLength = dataArr.Count > 0 && dataArr[0].TryGetValue("$total", out var total) ? System.Convert.ToInt32(total) : (int?) null;

                    var sortKey = sqlTable.SortKey ?? sqlTable.Junction?.SortKey;

                    var hasNextPage = false;
                    var hasPreviousPage = false;
                    if (arguments.TryGetValue("first", out var first) && first.Value is int firstValue)
                    {
                        // we fetched an extra one in order to determine if there is a next page, if there is one, pop off that extra
                        if (dataArr.Count > firstValue) {
                            hasNextPage = true;
                            dataArr.RemoveAt(dataArr.Count - 1);
                        }
                    }
                    else if (arguments.TryGetValue("last", out var last) && last.Value is int lastValue)
                    {
                        // if backward paging, do the same, but also reverse it
                        if (dataArr.Count > lastValue)
                        {
                            hasPreviousPage = true;
                            dataArr.RemoveAt(dataArr.Count - 1);
                        }

                        dataArr.Reverse();
                    }

                    var cursor = new Dictionary<string, object?>();

                    var edges = dataArr.Select(obj =>
                    {
                        var sort = sortKey!;
                        do
                        {
                            var value = obj[sort.Column];
                            if (sort.Type != null)
                            {
                                if (sort.Type == typeof(DateTime) && value is string strVal)
                                {
                                    // TODO: is this needed?
                                    value = DateTime.Parse(strVal, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                                }
                                else
                                {
                                    value = System.Convert.ChangeType(value, sort.Type, CultureInfo.InvariantCulture);
                                }
                            }

                            cursor[sort.As] = value;
                        } while ((sort = sort.ThenBy) != null);

                        return new Edge<object> {Cursor = ConnectionUtils.ObjectToCursor(cursor), Node = obj};
                    }).ToList();

                    var startCursor = edges.FirstOrDefault()?.Cursor;
                    var endCursor = edges.LastOrDefault()?.Cursor;

                    var connection = new Connection<object>
                    {
                        Edges = edges,
                        PageInfo = new PageInfo
                        {
                            HasNextPage = hasNextPage,
                            HasPreviousPage = hasPreviousPage,
                            StartCursor = startCursor,
                            EndCursor = endCursor
                        }
                    };

                    if (arrayLength.HasValue)
                        connection.TotalCount = arrayLength.Value;
                    return connection;
                }
                case List<Dictionary<string, object?>> dataArr when (sqlTable.OrderBy != null || sqlTable.Junction?.OrderBy != null):
                {
                    var arguments = sqlTable.Arguments;
                    var offset = 0;
                    if (arguments.TryGetValue("after", out var after) && after.Value is string afterValue)
                        offset = ConnectionUtils.CursorToOffset(afterValue);

                    // $total was a special column for determining the total number of items
                    var arrayLength = dataArr.Count > 0 && dataArr[0].TryGetValue("$total", out var total) ? System.Convert.ToInt32(total) : (int?) null;

                    var connection = ConnectionFromArraySlice(dataArr, arguments, offset, arrayLength);
                    if (arrayLength.HasValue)
                        connection.TotalCount = arrayLength.Value;
                    return connection;
                }
                default:
                    return data;
            }
        }

        private Connection<object> ConnectionFromArraySlice(IReadOnlyCollection<Dictionary<string, object?>> dataArr,
            IReadOnlyDictionary<string, ArgumentValue> arguments, int offset, int? arrayLength)
        {
            if (arrayLength == 0)
            {
                return new Connection<object>
                {
                    PageInfo = new PageInfo(),
                    Edges = new List<Edge<object>>()
                };
            }
            int? first = null;
            if (arguments.TryGetValue("first", out var firstArgument) && firstArgument.Value is int intValue)
                first = intValue;

            var arr = dataArr.AsEnumerable();
            if (first.HasValue)
                arr = arr.Take(first.Value);

            var edges = arr
                .Select((x, i) => new Edge<object>
                {
                    Node = x,
                    Cursor = ConnectionUtils.OffsetToCursor(offset + i + 1)
                }).ToList();

            var startCursor = edges.FirstOrDefault()?.Cursor;
            var endCursor = edges.LastOrDefault()?.Cursor;

            var connection = new Connection<object>
            {
                Edges = edges,
                PageInfo = new PageInfo
                {
                    HasNextPage = dataArr.Count > edges.Count,
                    HasPreviousPage = offset > 0,
                    StartCursor = startCursor,
                    EndCursor = endCursor
                }
            };
            return connection;
        }

        private void RecurseOnObjInData(IDictionary<string, object?> dataItem, Node astChild, IResolveFieldContext context)
        {
            var fieldName = astChild switch
            {
                SqlColumnBase sqlColumnBase => sqlColumnBase.FieldName,
                SqlTable sqlTable => sqlTable.FieldName,
                _ => null
            };

            if (fieldName == null) return;

            if (dataItem.TryGetValue(fieldName, out _))
                dataItem[fieldName] = ConvertInternal(dataItem[fieldName], astChild, context);
        }
    }
}

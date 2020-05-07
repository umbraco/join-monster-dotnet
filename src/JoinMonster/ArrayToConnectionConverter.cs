using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types.Relay.DataObjects;
using JoinMonster.Language.AST;

namespace JoinMonster
{
    internal class ArrayToConnectionConverter
    {
        public IEnumerable<IDictionary<string, object?>> Convert(IEnumerable<IDictionary<string, object?>> data, Node sqlAst)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (sqlAst == null) throw new ArgumentNullException(nameof(sqlAst));

            var converted = ConvertInternal(data, sqlAst);
            return converted switch
            {
                IEnumerable<IDictionary<string, object?>> dictionary => dictionary,
                null => throw new JoinMonsterException("Expected result to not be null."),
                _ => throw new JoinMonsterException(
                    $"Expected result to be of type '{typeof(IEnumerable<IDictionary<string, object?>>)}' but was '{converted.GetType()}'")
            };
        }

        private object? ConvertInternal(object? data, Node sqlAst)
        {
            foreach (var astChild in sqlAst.Children)
            {
                switch (data)
                {
                    case IEnumerable<Dictionary<string, object?>> array:
                    {
                        foreach (var dataItem in array)
                            RecurseOnObjInData(dataItem, astChild);

                        break;
                    }
                    case Dictionary<string, object?> dataItem:
                        RecurseOnObjInData(dataItem, astChild);
                        break;
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(data), $"Unknown type {data.GetType()}");
                }
            }

            if (!(sqlAst is SqlTable sqlTable))
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
                case List<Dictionary<string, object?>> dataArr when (sqlTable.OrderBy != null || sqlTable.Junction?.OrderBy != null):
                {
                    var arguments = sqlTable.Arguments.ToDictionary(x => x.Name, x => x.Value.Value);
                    var offset = 0;
                    if (arguments.TryGetValue("after", out var after))
                        offset = ConnectionUtils.CursorToOffset((string) after);

                    // $total was a special column for determining the total number of items
                    int? arrayLength = dataArr.Count > 0 && dataArr[0].TryGetValue("$total", out var total) ? System.Convert.ToInt32(total) : (int?) null;

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
            IDictionary<string, object> arguments, int offset, int? arrayLength)
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
            if (arguments.TryGetValue("first", out var firstArgument))
                first = (int) firstArgument;

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

        private void RecurseOnObjInData(IDictionary<string, object?> dataItem, Node astChild)
        {
            var fieldName = astChild switch
            {
                SqlColumnBase sqlColumnBase => sqlColumnBase.FieldName,
                SqlTable sqlTable => sqlTable.FieldName,
                _ => null
            };

            if (fieldName == null) return;

            if (dataItem.TryGetValue(fieldName, out _))
                dataItem[fieldName] = ConvertInternal(dataItem[fieldName], astChild);
        }
    }
}

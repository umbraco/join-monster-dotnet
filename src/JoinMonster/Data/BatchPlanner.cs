using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;
using JoinMonster.Configs;
using JoinMonster.Language.AST;
using NestHydration;

namespace JoinMonster.Data
{
    public class BatchPlanner : IBatchPlanner
    {
        private readonly ISqlCompiler _compiler;
        private readonly Hydrator _hydrator;
        private readonly ObjectShaper _objectShaper;
        private readonly ArrayToConnectionConverter _arrayToConnectionConverter;

        /// <summary>
        /// Creates a new instance of <see cref="BatchPlanner"/>.
        /// </summary>
        /// <param name="compiler">The <see cref="ISqlCompiler"/>.</param>
        /// <param name="hydrator">The <see cref="Hydrator"/>.</param>
        public BatchPlanner(ISqlCompiler compiler, Hydrator hydrator)
        {
            _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            _hydrator = hydrator ?? throw new ArgumentNullException(nameof(hydrator));

            _objectShaper = new ObjectShaper(new SqlAstValidator());
            _arrayToConnectionConverter = new ArrayToConnectionConverter();
        }

        /// <inheritdoc />
        public async Task NextBatch(SqlTable sqlAst, object data, DatabaseCallDelegate databaseCall,
            IResolveFieldContext context, CancellationToken cancellationToken)
        {
            if (sqlAst == null) throw new ArgumentNullException(nameof(sqlAst));
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (databaseCall == null) throw new ArgumentNullException(nameof(databaseCall));
            if (context == null) throw new ArgumentNullException(nameof(context));

            // paginated fields are wrapped in connections. strip those off for the batching
            if (sqlAst.Paginate)
            {
                if (data is Connection<object> connection)
                {
                    data = connection.Edges.Select(x => x.Node).OfType<IDictionary<string, object?>>().ToList();
                }
                else if (data is IEnumerable<object> connections)
                {
                    data = connections.OfType<Connection<object>>().SelectMany(x => x.Items.OfType<IDictionary<string, object?>>()).ToList();
                }
            }

            if (data is IEnumerable<IDictionary<string, object?>> entries && entries.Any() == false)
                return;

            foreach (var table in sqlAst.Tables)
            {
                await NextBatchChild(table, data, databaseCall, context, cancellationToken);
            }
        }

        private async Task NextBatchChild(SqlTable sqlTable, object? data, DatabaseCallDelegate databaseCall,
            IResolveFieldContext context, CancellationToken cancellationToken)
        {
            var fieldName = sqlTable.FieldName;

            data = data switch
            {
                List<object> objList => objList.OfType<IDictionary<string, object?>>().ToList(),
                Connection<object> connection => connection.Items.OfType<IDictionary<string, object?>>().ToList(),
                _ => data
            };

            // see if any begin a new batch
            if (sqlTable.Batch != null || sqlTable.Junction?.Batch != null)
            {
                string thisKeyAlias = null!;
                string parentKey = null!;
                Func<object, bool> isTypeOf = _ => true;

                if (sqlTable.ParentGraphType is IObjectGraphType { IsTypeOf: var typeOf } && typeOf is not null)
                    isTypeOf = typeOf;

                if (sqlTable.Batch != null)
                {
                    // if so, we know we'll need to get the key for matching with the parent key
                    sqlTable.AddColumn(sqlTable.Batch.ThisKey);

                    thisKeyAlias = sqlTable.Batch.ThisKey.As;
                    parentKey = sqlTable.Batch.ParentKey.Name;
                }
                else if (sqlTable.Junction?.Batch != null)
                {
                    sqlTable.AddColumn(sqlTable.Junction.Batch.ThisKey);

                    thisKeyAlias = sqlTable.Junction.Batch.ThisKey.As;
                    parentKey = sqlTable.Junction.Batch.ParentKey.Name;
                }

                if (data is IEnumerable<IDictionary<string, object?>> entries)
                {
                    // the "batch scope" is the set of values to match this key against from the previous batch
                    var batchScope = new HashSet<object>();
                    var entryList = entries.ToList();

                    foreach (var entry in entryList)
                    {
                        if (isTypeOf(entry) is false) continue;
                        var values = PrepareValues(entry, parentKey);
                        foreach (var value in values)
                            batchScope.Add(value);
                    }

                    if (batchScope.Count == 0)
                    {
                        if (sqlTable.Paginate)
                        {
                            foreach (var entry in entries)
                                entry[fieldName] = new Connection<object> { Edges = new List<Edge<object>>() };
                        }
                        return;
                    }

                    // generate the SQL, with the batch scope values incorporated in a WHERE IN clause
                    var sqlResult = _compiler.Compile(sqlTable, context, SqlDialect.CastArray(batchScope));
                    var objectShape = _objectShaper.DefineObjectShape(sqlTable);

                    // grab the data
                    var newData = await HandleDatabaseCall(databaseCall, sqlResult, thisKeyAlias, cancellationToken).ConfigureAwait(false);

                    // group the rows by the key so we can match them with the previous batch
                    var newDataGrouped = newData.GroupBy(x => x["$$temp"])
                            .ToDictionary(x => x.Key, x => x.ToList());

                    // if we they want many rows, give them an array
                    if (sqlTable.GrabMany)
                    {
                        foreach (var entry in entryList)
                        {
                            if (isTypeOf(entry) is false) continue;
                            var values = PrepareValues(entry, parentKey);

                            var res = new List<IDictionary<string, object?>>();

                            if (sqlTable.OrderBy == null && sqlTable.SortKey == null)
                            {
                                foreach (var value in values)
                                {
                                    if (newDataGrouped.TryGetValue(value, out var resultValue))
                                    {
                                        res.AddRange(resultValue);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var value in newDataGrouped)
                                {
                                    if (value.Key == null) continue;
                                    if (values.Contains(value.Key))
                                    {
                                        res.AddRange(value.Value);
                                    }
                                }
                            }

                            var nested = _hydrator.Nest(res, objectShape).ToList();

                            entry[fieldName] = sqlTable.Paginate
                                ? _arrayToConnectionConverter.Convert(nested, sqlTable, context)
                                : nested;
                        }
                    }
                    else
                    {
                        var matchedData = new List<IDictionary<string, object?>>();
                        foreach (var entry in entryList)
                        {
                            if (entry.TryGetValue(parentKey, out var key) == false) continue;

                            if (newDataGrouped.TryGetValue(key, out var list) && list.Count > 0)
                            {
                                var res = _hydrator.Nest(list, objectShape).ToList();
                                entry[fieldName] = _arrayToConnectionConverter.Convert(res[0], sqlTable, context);
                                matchedData.Add(entry);
                            }
                        }

                        data = matchedData;
                    }

                    switch (data)
                    {
                        case IEnumerable<IDictionary<string, object?>> list:
                        {
                            List<object> nextLevelData = new List<object>();
                            foreach (var item in list)
                            {
                                if (item.TryGetValue(fieldName, out var value) && value is IEnumerable<IDictionary<string, object?>> dict)
                                {
                                    nextLevelData.AddRange(dict);
                                }
                                else if (value is Connection<object> connection)
                                {
                                    nextLevelData.Add(value);
                                }
                            }

                            await NextBatch(sqlTable, nextLevelData, databaseCall, context, cancellationToken).ConfigureAwait(false);

                            return;
                        }
                        case List<object> objects:
                        {
                            var nextLevelData = objects.Where(x => x != null);

                            await NextBatch(sqlTable, nextLevelData, databaseCall, context, cancellationToken).ConfigureAwait(false);
                            return;
                        }
                    }
                    return;
                }

                switch (data)
                {
                    case IDictionary<string, object?> dict:
                    {
                        var batchScope = PrepareValues(dict, parentKey);

                        if (batchScope.Count == 0)
                        {

                            if (sqlTable.Paginate)
                            {
                                dict[fieldName] = new Connection<object> { Edges = new List<Edge<object>>() };
                            }
                            return;
                        }


                        var sqlResult = _compiler.Compile(sqlTable, context, SqlDialect.CastArray(batchScope));

                        var objectShape = _objectShaper.DefineObjectShape(sqlTable);
                        var newData = await HandleDatabaseCall(databaseCall, sqlResult, thisKeyAlias, cancellationToken).ConfigureAwait(false);

                        var newDataGrouped = newData
                            .GroupBy(x => x["$$temp"])
                            .ToDictionary(x => x.Key, x => _hydrator.Nest(x, objectShape).ToList());

                        if (sqlTable.GrabMany)
                        {
                            var res = new List<IDictionary<string, object?>>();
                            if (sqlTable.OrderBy == null && sqlTable.SortKey == null)
                            {
                                foreach (var value in batchScope)
                                {
                                    if (newDataGrouped.TryGetValue(value, out var resultValue))
                                    {
                                        res.AddRange(resultValue);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var value in newDataGrouped)
                                {
                                    if (value.Key == null) continue;
                                    if (batchScope.Contains(value.Key))
                                    {
                                        res.AddRange(value.Value);
                                    }
                                }
                            }

                            // ensure that any child connection is resolved
                            dict[fieldName] = _arrayToConnectionConverter.Convert(res, sqlTable, context);
                        }
                        else if (newDataGrouped.TryGetValue(parentKey, out var obj) && obj.Count > 0)
                        {
                            dict[fieldName] = obj[0];
                        }

                        if (dict.TryGetValue(fieldName, out var newDataObj) && newDataObj is not null)
                        {
                            await NextBatch(sqlTable, newDataObj, databaseCall, context, cancellationToken).ConfigureAwait(false);
                        }

                        break;
                    }
                    case IEnumerable<IDictionary<string, object?>> list:
                    {
                        var nextLevelData = list
                            .Where(x => x.Count > 0)
                            .Select(x =>
                            {
                                if (x.TryGetValue(fieldName, out var value)
                                    && value is List<IDictionary<string, object?>> dict)
                                {
                                    return dict;
                                }
                                return null;
                            })
                            .Where(x => x is {Count: > 0})
                            .SelectMany(x => x)
                            .ToList();

                        await NextBatch(sqlTable, nextLevelData, databaseCall, context, cancellationToken).ConfigureAwait(false);
                        break;
                    }
                    default:
                    {
                        if (data is IDictionary<string, object?> obj)
                        {
                            if (obj.TryGetValue(fieldName, out var newData) && newData is not null)
                            {
                                await NextBatch(sqlTable, newData, databaseCall, context, cancellationToken).ConfigureAwait(false);
                            }
                        }

                        break;
                    }
                }
            }
            else switch (data)
            {
                case IEnumerable<IDictionary<string, object?>> entries:
                {
                    var tasks = new List<Task>();
                    foreach (var entry in entries)
                    {
                        if (entry.TryGetValue(sqlTable.FieldName, out var newData))
                        {
                            tasks.AddRange(sqlTable.Tables
                                .Select(child =>
                                    NextBatchChild(child, newData, databaseCall, context, cancellationToken)));
                        }
                    }

                    await Task.WhenAll(tasks).ConfigureAwait(false);
                    break;
                }
                case IDictionary<string, object?> entry when entry.TryGetValue(sqlTable.FieldName, out var newData):
                {
                    var tasks = (sqlTable.Tables
                        .Select(child =>
                            NextBatchChild(child, newData, databaseCall, context, cancellationToken)));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                    break;
                }
            }
        }

        private static List<object> PrepareValues(IDictionary<string, object?> dict, string parentKey)
        {
            var batchScope = new HashSet<object>();

            if (dict.TryGetValue(parentKey, out var obj))
            {
                switch (obj)
                {
                    case Connection<object> connection:
                        if (connection.Items != null)
                        {
                            foreach (var item in connection.Items)
                            {
                                batchScope.Add(item);
                            }
                        }
                        break;
                    case JsonElement element:
                    {
                        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null or JsonValueKind.Object)
                            break;

                        var preparedValue = SqlDialect.PrepareValue(element, null);
                        if (preparedValue is IEnumerable enumerable and not string)
                        {
                            foreach (var value in enumerable.Cast<object>())
                                batchScope.Add(value);
                        }
                        else
                        {
                            batchScope.Add(preparedValue);
                        }

                        break;
                    }
                    case IEnumerable enumerable when enumerable is not string:
                    {
                        foreach (var item in enumerable)
                        {
                            batchScope.Add(item);
                        }
                        break;
                    }
                    case null:
                        break;
                    default:
                        batchScope.Add(obj);
                        break;
                }
            }

            return batchScope.ToList();
        }

        // TODO: Refactor to share code with JoinMonsterExecuter
        private static async Task<List<ConcurrentDictionary<string, object?>>> HandleDatabaseCall(
            DatabaseCallDelegate databaseCall, SqlResult sqlResult,
            string keyColumn, CancellationToken cancellationToken)
        {
            using var reader = await databaseCall(sqlResult.Sql, sqlResult.Parameters).ConfigureAwait(false);

            var data = new List<ConcurrentDictionary<string, object?>>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var item = new ConcurrentDictionary<string, object?>();
                for (var i = 0; i < reader.FieldCount; ++i)
                {
                    item[reader.GetName(i)] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(false)
                        ? null
                        : await reader.GetFieldValueAsync<object>(i, cancellationToken).ConfigureAwait(false);
                }

                if (item.ContainsKey("$$temp") == false)
                    item["$$temp"] = item[keyColumn];

                data.Add(item);
            }

            return data;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
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
                    data = connection.Edges.Select(x => (IDictionary<string, object?>) x.Node).ToList();
            }

            if (data is IEnumerable<IDictionary<string, object?>> entries && entries.Any() == false)
                return;

            var tasks = sqlAst.Tables.Select(child =>
                NextBatchChild(child, data, databaseCall, context, cancellationToken));

            await Task.WhenAll(tasks);
        }

        private async Task NextBatchChild(SqlTable sqlTable, object? data, DatabaseCallDelegate databaseCall,
            IResolveFieldContext context, CancellationToken cancellationToken)
        {
            var fieldName = sqlTable.FieldName;

            if (data is List<object> objList)
                data = objList.Select(x => (IDictionary<string, object?>) x);

            // see if any begin a new batch
            if (sqlTable.Batch != null || sqlTable.Junction?.Batch != null)
            {
                string thisKey = null!;
                string parentKey = null!;

                if (sqlTable.Batch != null)
                {
                    // if so, we know we'll need to get the key for matching with the parent key
                    sqlTable.AddColumn(sqlTable.Batch.ThisKey);

                    thisKey = sqlTable.Batch.ThisKey.FieldName;
                    parentKey = sqlTable.Batch.ParentKey.FieldName;
                }
                else if (sqlTable.Junction?.Batch != null)
                {
                    sqlTable.AddColumn(sqlTable.Junction.Batch.ThisKey);

                    thisKey = sqlTable.Junction.Batch.ThisKey.FieldName;
                    parentKey = sqlTable.Junction.Batch.ParentKey.FieldName;
                }

                if (data is IEnumerable<IDictionary<string, object?>> entries)
                {
                    // the "batch scope" is the set of values to match this key against from the previous batch
                    var batchScope = new List<object>();
                    var entryList = entries.ToList();

                    foreach (var entry in entryList)
                    {
                        var value = entry[parentKey];
                        switch (value)
                        {
                            case JsonElement element:
                                batchScope.AddRange(((IEnumerable) SqlDialect.PrepareValue(element, null))
                                    .Cast<object>());
                                break;
                            case null:
                                break;;
                            default:
                                batchScope.Add(value);
                                break;
                        }

                    }

                    if (batchScope.Count == 0) return;

                    // generate the SQL, with the batch scope values incorporated in a WHERE IN clause
                    var sqlResult = _compiler.Compile(sqlTable, context, SqlDialect.CastArray(batchScope));
                    var objectShape = _objectShaper.DefineObjectShape(sqlTable);

                    // grab the data
                    var newData = await HandleDatabaseCall(databaseCall, sqlResult, objectShape, cancellationToken);
                    // group the rows by the key so we can match them with the previous batch
                    var newDataGrouped = newData.GroupBy(x => x[thisKey])
                        .ToDictionary(x => x.Key, x => x.ToList());

                    // but if we paginate, we must convert to connection type first
                    if (sqlTable.Paginate)
                    {
                        //TODO: implement
                        // foreach (var group in newDataGrouped)
                        //     newDataGrouped[group.Key] =
                        //         (List<Dictionary<string, object?>>)
                        //             _arrayToConnectionConverter.Convert(group.Value, sqlTable, context);
                    }

                    // if we they want many rows, give them an array
                    if (sqlTable.GrabMany)
                    {
                        foreach (var entry in entryList)
                        {
                            var values = new List<object>();
                            var obj = entry[parentKey];
                            switch (obj)
                            {
                                case JsonElement element:
                                    values.AddRange(((IEnumerable) SqlDialect.PrepareValue(element, null)).Cast<object>());
                                    break;
                                case null:
                                    break;
                                default:
                                    values.Add(obj);
                                    break;
                            }

                            var res = new List<Dictionary<string, object?>>();
                            foreach (var value in values)
                                res.AddRange(newDataGrouped[value]);

                            entry[fieldName] = res;
                        }
                    }
                    else
                    {
                        var matchedData = new List<object>();
                        foreach (var entry in entryList)
                        {
                            var ob = newDataGrouped[entry[parentKey]];
                            if (ob != null)
                            {
                                entry[fieldName] =
                                    _arrayToConnectionConverter.Convert(newDataGrouped[entry[parentKey]][0], sqlTable,
                                        context);

                                matchedData.Add(entry);
                            }
                            else
                            {
                                entry[fieldName] = null;
                            }
                        }

                        data = matchedData;
                    }


                    switch (data)
                    {
                        case IEnumerable<IDictionary<string, object?>> list:
                        {
                            var nextLevelData = list
                                .Where(x => x.Count > 0)
                                .Select(x => (List<Dictionary<string, object?>>) x[fieldName])
                                .Where(x => x.Count > 0)
                                .SelectMany(x => x)
                                .Select(x => x.ToDictionary())
                                .AsEnumerable();

                            await NextBatch(sqlTable, nextLevelData, databaseCall, context, cancellationToken);
                            return;
                        }
                        case List<object> objects:
                        {
                            var nextLevelData = objects
                                .Where(x => x != null);

                            await NextBatch(sqlTable, nextLevelData, databaseCall, context, cancellationToken);
                            return;
                        }
                    }
                }

                switch (data)
                {
                    case IDictionary<string, object?> dict:
                    {
                        var batchScope = new List<object>();

                        var o = dict[parentKey];
                        switch (o)
                        {
                            case JsonElement element:
                                batchScope.AddRange(((IEnumerable) SqlDialect.PrepareValue(element, null))
                                    .Cast<object>());
                                break;
                            default:
                                batchScope.Add(o);
                                break;
                        }

                        if (batchScope.Count == 0) return;

                        var sqlResult = _compiler.Compile(sqlTable, context, SqlDialect.CastArray(batchScope));

                        var objectShape = _objectShaper.DefineObjectShape(sqlTable);
                        var newData = await HandleDatabaseCall(databaseCall, sqlResult, objectShape, cancellationToken);

                        var newDataGrouped = newData.GroupBy(x => x[thisKey])
                            .ToDictionary(x => x.Key, x => x.ToList());

                        if (sqlTable.Paginate)
                        {
                            var targets = newDataGrouped[parentKey];
                            dict[fieldName] = _arrayToConnectionConverter.Convert(targets, sqlTable, context);
                        }
                        else if (sqlTable.GrabMany)
                        {
                            var res = new List<object>();
                            foreach (var value in batchScope)
                            {
                                res.AddRange(newDataGrouped[value]);
                            }

                            dict[fieldName] = res;
                        }
                        else
                        {
                            var targets = newDataGrouped[parentKey];
                            dict[fieldName] = targets[0];
                        }

                        await NextBatch(sqlTable, dict[fieldName], databaseCall, context, cancellationToken);
                        break;
                    }
                    case IEnumerable<IDictionary<string, object?>> list:
                    {
                        var nextLevelData = list
                            .Where(x => x.Count > 0)
                            .Select(x => (List<Dictionary<string, object?>>) x[fieldName])
                            .Where(x => x.Count > 0)
                            .SelectMany(x => x)
                            .Select(x => x.ToDictionary())
                            .AsEnumerable();

                        await NextBatch(sqlTable, nextLevelData, databaseCall, context, cancellationToken);
                        break;
                    }
                    default:
                    {
                        if (data is IDictionary<string, object?> obj)
                        {
                            await NextBatch(sqlTable, obj[fieldName], databaseCall, context, cancellationToken);
                        }

                        break;
                    }
                }
            }
        }

        // TODO: Refactor to share code with JoinMonsterExecuter
        private async Task<List<Dictionary<string, object?>>> HandleDatabaseCall(
            DatabaseCallDelegate databaseCall, SqlResult sqlResult, Definition objectShape,
            CancellationToken cancellationToken)
        {
            using var reader = await databaseCall(sqlResult.Sql, sqlResult.Parameters);

            var data = new List<Dictionary<string, object?>>();
            while (await reader.ReadAsync(cancellationToken))
            {
                var item = new Dictionary<string, object?>();
                for (var i = 0; i < reader.FieldCount; ++i)
                {
                    item[reader.GetName(i)] = await reader.IsDBNullAsync(i, cancellationToken)
                        ? null
                        : await reader.GetFieldValueAsync<object>(i, cancellationToken);
                }

                data.Add(item);
            }

#pragma warning disable 8620
#pragma warning disable 8619
            return _hydrator.Nest(data, objectShape);
#pragma warning restore 8619
#pragma warning restore 8620
        }
    }
}

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using GraphQL;
using GraphQL.StarWars.Types;
using GraphQL.Types;
using JoinMonster;
using StarWars.Types;

namespace StarWars
{
    public class StarWarsQuery : ObjectGraphType
    {
        public StarWarsQuery(JoinMonsterExecuter joinMonster)
        {
            Name = "Query";

            object Resolve(IResolveFieldContext<object> context) =>
                joinMonster.ExecuteAsync(context, async (sql, parameters) =>
                {
                    Console.WriteLine(sql);
                    Console.WriteLine();

                    var dbConnection = (IDbConnection) context.UserContext[nameof(IDbConnection)];
                    await using var command = (DbCommand) dbConnection.CreateCommand();
                    command.CommandText = sql;
                    foreach (var (key, value) in parameters)
                    {
                        var sqlParameter = command.CreateParameter();
                        sqlParameter.ParameterName = key;
                        sqlParameter.Value = value;
                        command.Parameters.Add(sqlParameter);
                    }

                    return await command.ExecuteReaderAsync();
                }, CancellationToken.None);

            Field<CharacterInterface>("hero", resolve: Resolve)
                .SqlWhere((where, _, __, ___) => where.Column("id", 3));

            Field<HumanType>(
                "human",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the human" }
                ),
                resolve: Resolve
            ).SqlWhere((where, args, _, __) => where.Column("id", args["id"]));

            Field<ListGraphType<HumanType>>(
                    "humans",
                    resolve: Resolve
                )
                .SqlWhere((where, args, _, __) => where.Column("type", "Human"))
                .SqlOrder((order, _, __, ___) => order.By("name"));

            Field<DroidType>(
                "droid",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the droid" }
                ),
                resolve: Resolve
            ).SqlWhere((where, args, _, __) => where.Column("id", args["id"]));

            Field<ListGraphType<PlanetType>>(
                "planets",
                resolve: Resolve
            );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using JoinMonster;
using JoinMonster.Data;
using JoinMonster.Language;
using JoinMonster.Resolvers;
using NestHydration;

namespace StarWars
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SQLiteConnection.CreateFile("starwars.db");

            await using var connection = new SQLiteConnection("Data Source=starwars.db");

            await connection.OpenAsync();

            await PopulateDatabase(connection);

            var compiler = new SqlCompiler(new SQLiteDialect());
            var hydrator = new Hydrator();
            var joinMonster = new JoinMonsterExecuter(
                new QueryToSqlConverter(new DefaultAliasGenerator()),
                compiler,
                new BatchPlanner(compiler, hydrator),
                hydrator
            );

            var serviceProvider = new FuncServiceProvider(type =>
            {
                if (type == typeof(StarWarsQuery))
                    return new StarWarsQuery(joinMonster);

                return Activator.CreateInstance(type);
            });

            var schema = new StarWarsSchema(serviceProvider);

            var query = @"
{
  human(id: ""1"") {
    name
    id
    homePlanet {
      id
      name
    }
    friends {
      name
    }
  }

  humans {
    name
  }
}";

            var start = DateTime.UtcNow;

            var documentExecuter = new DocumentExecuter();
            var data = await documentExecuter.ExecuteAsync(options =>
            {
                options.Schema = schema;
                options.FieldMiddleware.Use<InstrumentFieldsMiddleware>();
                options.EnableMetrics = true;
                options.Query = query;
                options.UserContext = new Dictionary<string, object>
                {
                    {nameof(IDbConnection), connection}
                };
            });

            // data.EnrichWithApolloTracing(start);

            var writer = new DocumentWriter(true);

            Console.WriteLine(await writer.WriteToStringAsync(data));
        }

        private static async Task PopulateDatabase(SQLiteConnection connection)
        {
            await using var transaction = await connection.BeginTransactionAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = @"
CREATE TABLE characters (id INTEGER NOT NULL, name NVARCHAR(20) NOT NULL, type NVARCHAR(10) NOT NULL, homePlanet INTEGER NULL, primaryFunction NVARCHAR(20) NULL);
CREATE TABLE friends (characterId INTEGER NOT NULL, friendId INTEGER NOT NULL);
CREATE TABLE planets (id INTEGER NOT NULL, name NVARCHAR(20) NOT NULL);
INSERT INTO planets (id, name) VALUES (1, 'Tatooine');
INSERT INTO characters (id, name, type, homePlanet) VALUES (1, 'Luke', 'Human', 1);
INSERT INTO characters (id, name, type, homePlanet) VALUES (2, 'Vader', 'Human', 1);
INSERT INTO characters (id, name, type, primaryFunction) VALUES (3, 'R2-D2', 'Droid', 'Astromech');
INSERT INTO characters (id, name, type, primaryFunction) VALUES (4, 'C-3PO', 'Droid', 'Protocol');
INSERT INTO friends (characterId, friendId) VALUES (1, 3);
INSERT INTO friends (characterId, friendId) VALUES (1, 4);
INSERT INTO friends (characterId, friendId) VALUES (3, 1);
INSERT INTO friends (characterId, friendId) VALUES (3, 4);
";
            await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
    }
}

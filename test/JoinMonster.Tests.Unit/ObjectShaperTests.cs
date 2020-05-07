using System.Linq;
using FluentAssertions;
using JoinMonster.Language.AST;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace JoinMonster.Tests.Unit
{
    public class ObjectShaperTests
    {
        [Fact]
        public void DefineObjectShape_WhenCalledWithObject_ReturnsObjectShapeDefinition()
        {
            var node = new SqlTable("products", "products", "products",
                new[] {new SqlColumn("id", "id", "id", true), new SqlColumn("name", "name", "name")},
                new[]
                {
                    new SqlTable("variants", "variants", "variants",
                        new[] {new SqlColumn("id", "id", "id", true), new SqlColumn("name", "name", "name")},
                        new[]
                        {
                            new SqlTable("colors", "color", "color",
                                new[] {new SqlColumn("id", "id", "id", true), new SqlColumn("color", "color", "color")},
                                Enumerable.Empty<SqlTable>(), Enumerable.Empty<Argument>(), false),
                        }, Enumerable.Empty<Argument>(), true)
                }, Enumerable.Empty<Argument>(), true);

            var objectShaper = new ObjectShaper(new SqlAstValidator());

            var definition = objectShaper.DefineObjectShape(node);

            var json = JsonConvert.SerializeObject(definition, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            json.Should()
                .Be("{\"properties\":[{\"name\":\"id\",\"column\":\"id\",\"isId\":true},{\"name\":\"name\",\"column\":\"name\",\"isId\":false},{\"name\":\"variants\",\"properties\":[{\"name\":\"id\",\"column\":\"variants__id\",\"isId\":true},{\"name\":\"name\",\"column\":\"variants__name\",\"isId\":false},{\"name\":\"color\",\"properties\":[{\"name\":\"id\",\"column\":\"variants__color__id\",\"isId\":true},{\"name\":\"color\",\"column\":\"variants__color__color\",\"isId\":false}]}]}]}");
        }
    }
}

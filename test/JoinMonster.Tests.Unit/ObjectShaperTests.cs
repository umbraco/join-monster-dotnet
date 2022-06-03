using System.Collections.Generic;
using FluentAssertions;
using GraphQL.Execution;
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
            var node = new SqlTable(null, null, "products", "products", "products", new Dictionary<string, ArgumentValue>(),
                true);
            node.AddColumn("id", "id", "id", true);
            node.AddColumn("name", "name", "name");
            var variantsTable = node.AddTable(null, "variants", "variants", "variants", new Dictionary<string, ArgumentValue>(), true);
            variantsTable.AddColumn("id", "id", "id", true);
            variantsTable.AddColumn("name", "name", "name");
            variantsTable.SortKey = new SortKey("products", "sortOrder", "sortOrder", typeof(int), SortDirection.Ascending);
            var colorsTable = variantsTable.AddTable(null, "colors", "color", "color", new Dictionary<string, ArgumentValue>(), true);
            colorsTable.AddColumn("id", "id", "id", true);
            colorsTable.AddColumn("color", "color", "color");

            var objectShaper = new ObjectShaper(new SqlAstValidator());

            var definition = objectShaper.DefineObjectShape(node);

            var json = JsonConvert.SerializeObject(definition, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            json.Should()
                .Be("{\"properties\":[{\"name\":\"id\",\"column\":\"id\",\"isId\":true},{\"name\":\"name\",\"column\":\"name\",\"isId\":false},{\"name\":\"variants\",\"properties\":[{\"name\":\"sortOrder\",\"column\":\"variants__sortOrder\",\"isId\":false},{\"name\":\"id\",\"column\":\"variants__id\",\"isId\":true},{\"name\":\"name\",\"column\":\"variants__name\",\"isId\":false},{\"name\":\"color\",\"properties\":[{\"name\":\"id\",\"column\":\"variants__color__id\",\"isId\":true},{\"name\":\"color\",\"column\":\"variants__color__color\",\"isId\":false}]}]}]}");
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GraphQL;
using JoinMonster.Data;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit.Data
{
    public class PostgreSqlDialectTests
    {
        [Fact]
        public void Quote_WithString_QuotesString()
        {
            var dialect = new PostgresSqlDialect();

            var quoted = dialect.Quote("product");

            quoted.Should().Be("\"product\"");
        }

        [Fact]
        public void Quote_WithJsonField_QuotesString()
        {
            var dialect = new PostgresSqlDialect();

            var quoted = dialect.Quote("product->>'id'");

            quoted.Should().Be("\"product\"->>'id'");
        }

        [Fact]
        public void Quote_WithJsonFieldAsString_QuotesString()
        {
            var dialect = new PostgresSqlDialect();

            var quoted = dialect.Quote("product->'id'");

            quoted.Should().Be("\"product\"->'id'");
        }

        [Fact]
        public void CompositeKey_WithParentTableAndKeys_ReturnsCombinedString()
        {
            var dialect = new PostgresSqlDialect();

            var sql = dialect.CompositeKey("people", new[] {"id", "firstName", "lastName"});

            sql.Should().Be("NULLIF(CONCAT(\"people\".\"id\", \"people\".\"firstName\", \"people\".\"lastName\"), '')");
        }

        [Fact]
        public void HandleJoinedOneToManyPaginated_WhenCalled_ReturnsPagedJoinString()
        {
            var dialect = new PostgresSqlDialect();
            var parent = new SqlTable("products", "products", "products", new[] {new SqlColumn("id", "id", "id", true)},
                new SqlTable[0], Enumerable.Empty<Argument>(), true);
            var node = new SqlTable("variants", "variants", "variants", new[] {new SqlColumn("id", "id", "id", true)},
                new SqlTable[0], Enumerable.Empty<Argument>(), true);

            node.Join = (products, variants, _, __) => $"{products}.\"id\" = {variants}.\"productId\"";
            node.OrderBy = (order, _, __) => order.By("id");
            node.Where = (variants, _, __) => $"{variants}.\"id\" <> 1";

            var arguments = new Dictionary<string, object>();
            var context = new ResolveFieldContext();
            var tables = new List<string>();

            dialect.HandleJoinedOneToManyPaginated(parent, node, arguments, context, tables,
                "\"products\".\"id\" = \"variants\".\"productId\"");

            tables.Should()
                .Contain(@"LEFT JOIN LATERAL (
  SELECT ""variants"".*, COUNT(*) OVER () AS ""$total""
  FROM ""variants"" ""variants""
  WHERE ""products"".""id"" = ""variants"".""productId"" AND ""variants"".""id"" <> 1
  ORDER BY ""variants"".""id"" ASC
  LIMIT ALL OFFSET 0
) ""variants"" ON ""products"".""id"" = ""variants"".""productId""");
        }
    }
}

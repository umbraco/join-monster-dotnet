using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        public void HandleJoinedOneToManyPaginated_WhenCalledWithOffsetPagination_ReturnsPagedJoinString()
        {
            var dialect = new PostgresSqlDialect();
            var parent = new SqlTable("products", "products", "products", new[] {new SqlColumn("id", "id", "id", true)},
                new SqlTable[0], new Dictionary<string, object>(), true);
            var node = new SqlTable("variants", "variants", "variants", new[] {new SqlColumn("id", "id", "id", true)},
                new SqlTable[0], new Dictionary<string, object>(), true)
            {
                Join = (products, variants, _, __) => $"{products}.\"id\" = {variants}.\"productId\"",
                OrderBy = new OrderBy("id", SortDirection.Ascending),
                Where = (variants, _, __) => $"{variants}.\"id\" <> 1"
            };


            var arguments = new Dictionary<string, object>();
            var context = new ResolveFieldContext();
            var tables = new List<string>();

            dialect.HandleJoinedOneToManyPaginated(parent, node, arguments, context, tables,
                "\"products\".\"id\" = \"variants\".\"productId\"");

            tables.Should()
                .Contain("LEFT JOIN LATERAL (\n  SELECT \"variants\".*, COUNT(*) OVER () AS \"$total\"\n  FROM \"variants\" \"variants\"\n  WHERE \"products\".\"id\" = \"variants\".\"productId\" AND \"variants\".\"id\" <> 1\n  ORDER BY \"variants\".\"id\" ASC\n  LIMIT ALL OFFSET 0\n) \"variants\" ON \"products\".\"id\" = \"variants\".\"productId\"");
        }

        [Fact]
        public void HandleJoinedOneToManyPaginated_WhenCalledWithKeysetPagination_ReturnsPagedJoinString()
        {
            var dialect = new PostgresSqlDialect();
            var parent = new SqlTable("products", "products", "products", new[] {new SqlColumn("id", "id", "id", true)},
                new SqlTable[0], new Dictionary<string, object>(), true);
            var node = new SqlTable("variants", "variants", "variants", new[] {new SqlColumn("id", "id", "id", true)},
                new SqlTable[0], new Dictionary<string, object>(), true)
            {
                Join = (products, variants, _, __) => $"{products}.\"id\" = {variants}.\"productId\"",
                OrderBy = new OrderBy("id", SortDirection.Ascending),
                SortKey = new SortKey(new[] {"id"}, SortDirection.Ascending),
                Where = (variants, _, __) => $"{variants}.\"id\" <> 1"
            };


            var arguments = new Dictionary<string, object>();
            var context = new ResolveFieldContext();
            var tables = new List<string>();

            dialect.HandleJoinedOneToManyPaginated(parent, node, arguments, context, tables,
                "\"products\".\"id\" = \"variants\".\"productId\"");

            tables.Should()
                .Contain(@"LEFT JOIN LATERAL (
  SELECT ""variants"".*
  FROM ""variants"" ""variants""
  WHERE ""products"".""id"" = ""variants"".""productId"" AND ""variants"".""id"" <> 1
  ORDER BY ""variants"".""id"" ASC
  LIMIT ALL
) ""variants"" ON ""products"".""id"" = ""variants"".""productId""");
        }

        [Fact]
        public void HandleJoinedOneToManyPaginated_WhenCalledWithKeysetPaginationAndFirstAndAfter_ReturnsPagedJoinString()
        {
            var dialect = new PostgresSqlDialect();
            var parent = new SqlTable("products", "products", "products", new[] {new SqlColumn("id", "id", "id", true)},
                new SqlTable[0], new Dictionary<string, object>(), true);
            var node = new SqlTable("variants", "variants", "variants", new[] {new SqlColumn("id", "id", "id", true)},
                new SqlTable[0], new Dictionary<string, object>(), true)
            {
                Join = (products, variants, _, __) => $"{products}.\"id\" = {variants}.\"productId\"",
                SortKey = new SortKey(new[] {"id"}, SortDirection.Descending),
                Where = (variants, _, __) => $"{variants}.\"id\" <> 1"
            };

            var arguments = new Dictionary<string, object>
            {
                {"first", 2},
                {"after", Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { id = 1 })))}
            };
            var context = new ResolveFieldContext();
            var tables = new List<string>();

            dialect.HandleJoinedOneToManyPaginated(parent, node, arguments, context, tables,
                "\"products\".\"id\" = \"variants\".\"productId\"");

            tables.Should()
                .Contain(@"LEFT JOIN LATERAL (
  SELECT ""variants"".*
  FROM ""variants"" ""variants""
  WHERE ""products"".""id"" = ""variants"".""productId"" AND ""variants"".""id"" <> 1 AND ""variants"".""id"" < (1)
  ORDER BY ""variants"".""id"" DESC
  LIMIT 3
) ""variants"" ON ""products"".""id"" = ""variants"".""productId""");
        }

        [Fact]
        public void HandleJoinedOneToManyPaginated_WhenCalledWithKeysetPaginationAndLastAndBefore_ReturnsPagedJoinString()
        {
            var dialect = new PostgresSqlDialect();
            var parent = new SqlTable("products", "products", "products", new[] {new SqlColumn("id", "id", "id", true)},
                new SqlTable[0], new Dictionary<string, object>(), true);
            var node = new SqlTable("variants", "variants", "variants", new[] {new SqlColumn("id", "id", "id", true)},
                new SqlTable[0], new Dictionary<string, object>(), true)
            {
                Join = (products, variants, _, __) => $"{products}.\"id\" = {variants}.\"productId\"",
                OrderBy = new OrderBy("id", SortDirection.Ascending),
                SortKey = new SortKey(new[] {"id"}, SortDirection.Ascending),
                Where = (variants, _, __) => $"{variants}.\"id\" <> 1"
            };


            var arguments = new Dictionary<string, object>
            {
                {"last", 5},
                {"before", Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { id = 1 })))}
            };
            var context = new ResolveFieldContext();
            var tables = new List<string>();

            dialect.HandleJoinedOneToManyPaginated(parent, node, arguments, context, tables,
                "\"products\".\"id\" = \"variants\".\"productId\"");

            tables.Should()
                .Contain(@"LEFT JOIN LATERAL (
  SELECT ""variants"".*
  FROM ""variants"" ""variants""
  WHERE ""products"".""id"" = ""variants"".""productId"" AND ""variants"".""id"" <> 1 AND ""variants"".""id"" < (1)
  ORDER BY ""variants"".""id"" DESC
  LIMIT 6
) ""variants"" ON ""products"".""id"" = ""variants"".""productId""");
        }
    }
}
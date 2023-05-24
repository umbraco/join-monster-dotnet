using System;
using System.Collections.Generic;
using FluentAssertions;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;
using JoinMonster.Data;
using JoinMonster.Language;
using JoinMonster.Language.AST;
using JoinMonster.Tests.Unit.Stubs;
using Xunit;

namespace JoinMonster.Tests.Unit.Data
{
    public class SqlCompilerTests
    {
        [Fact]
        public void Ctor_WithOutSqlDialect_ThrowsException()
        {
            Action action = () => new SqlCompiler(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("sqlDialect");
        }

        [Fact]
        public void Compile_WhenNodeIsNull_ThrowsException()
        {
            var compiler = new SqlCompiler(new SqlDialectStub());

            Func<SqlResult> action = () => compiler.Compile(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .And.ParamName.Should()
                .Be("node");
        }

        [Fact]
        public void Compile_WhenContextIsNull_ThrowsException()
        {
            var compiler = new SqlCompiler(new SqlDialectStub());

            Func<SqlResult> action = () => compiler.Compile(new SqlNoop(), null);

            action.Should()
                .Throw<ArgumentNullException>()
                .And.ParamName.Should()
                .Be("context");
        }

        [Fact]
        public void Compile_WithSimpleQuery_ReturnsSql()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter(new DefaultAliasGenerator());
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"");
        }

        [Fact]
        public void Compile_WhenQueryIncludesUniqueKey_ColumnIsOnlySelectedOnce()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");
            });

            var query = "{ product { id, name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter(new DefaultAliasGenerator());
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"");
        }

        [Fact]
        public void Compile_WhenTableConfigHasCompositeUniqueKey_KeysAreIncludedInGeneratedSql()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", new [] { "id", "firstName", "lastName" });
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter(new DefaultAliasGenerator());
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Sql.Should().Be("SELECT\n  CONCAT(\"product\".\"id\", \"product\".\"firstName\", \"product\".\"lastName\") AS \"id#firstName#lastName\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"");
        }

        [Fact]
        public void Compile_WithWhereQuery_ReturnsSqlIncludingWhereCondition()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Query")
                    .FieldFor("product")
                    .SqlWhere((where, _, __, ___) => where.Column("id", 1));
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter(new DefaultAliasGenerator());
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            var expectedSql = "SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"\nWHERE \"product\".\"id\" = @p0";
            var expectedParameters = new Dictionary<string, object>{{"@p0", 1}};

            sql.Should().BeEquivalentTo(new SqlResult(expectedSql, expectedParameters));

        }

        [Fact]
        public void Compile_WithWhereQueryAndArguments_PassesArgumentsToWhereDelegate()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Query")
                    .FieldFor("product")
                    .SqlWhere((where, arguments, _, __) => where.Column("id", arguments["id"]));
            });

            var query = "{ product(id: \"3\") { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter(new DefaultAliasGenerator());
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            var expectedSql = "SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"\nWHERE \"product\".\"id\" = @p0";
            var expectedParameters = new Dictionary<string, object>{{"@p0", "3"}};

            sql.Should().BeEquivalentTo(new SqlResult(expectedSql, expectedParameters));
        }

        [Fact]
        public void Compile_WithJoinCondition_SqlShouldIncludeJoinCondition()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Query")
                    .FieldFor("product");

                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Product")
                    .FieldFor("variants")
                    .SqlJoin((join, _, __, ___) => join.On("id", "productId"));

                builder.Types.For("ProductVariant")
                    .SqlTable("productVariants", "id");
            });

            var query = "{ product { name, variants { edges { node { name } } } } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter(new DefaultAliasGenerator());
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\",\n  \"variants\".\"id\" AS \"variants__id\",\n  \"variants\".\"name\" AS \"variants__name\"\nFROM \"products\" AS \"product\"\nLEFT JOIN \"productVariants\" \"variants\" ON \"product\".\"id\" = \"variants\".\"productId\"");
        }

        [Fact]
        public void Compile_WithRawJoinCondition_SqlShouldIncludeJoinCondition()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Query")
                    .FieldFor("product");

                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Product")
                    .FieldFor("variants")
                    .SqlJoin((join, _, __, ___) => join.Raw($"{join.ParentTableAlias}.\"id\" = {@join.ChildTableAlias}.\"productId\"", @from: $"LEFT JOIN {join.ChildTableName} {join.ChildTableAlias}"));

                builder.Types.For("ProductVariant")
                    .SqlTable("productVariants", "id");
            });

            var query = "{ product { name, variants { edges { node { name } } } } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter(new DefaultAliasGenerator());
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\",\n  \"variants\".\"id\" AS \"variants__id\",\n  \"variants\".\"name\" AS \"variants__name\"\nFROM \"products\" AS \"product\"\nLEFT JOIN \"productVariants\" \"variants\" ON \"product\".\"id\" = \"variants\".\"productId\"");
        }

        [Fact]
        public void Compile_WithJunction_SqlShouldIncludeJoinCondition()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Product")
                    .FieldFor("relatedProducts")
                    .SqlJunction("productRelations",
                        (join, _, __, ___) => join.On("id", "productId"),
                        (join, _, __, ___) => join.On("relatedProductId", "id"));
            });

            var query = "{ product { name, relatedProducts { name } } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter(new DefaultAliasGenerator());
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\",\n  \"relatedProducts\".\"id\" AS \"relatedProducts__id\",\n  \"relatedProducts\".\"name\" AS \"relatedProducts__name\"\nFROM \"products\" AS \"product\"\nLEFT JOIN \"productRelations\" \"productRelations\" ON \"product\".\"id\" = \"productRelations\".\"productId\"\nLEFT JOIN \"products\" \"relatedProducts\" ON \"productRelations\".\"relatedProductId\" = \"relatedProducts\".\"id\"");
        }

        [Fact(Skip = "Not sure if this is should be possible")]
        public void Compile_WithJunctionAndWhere_SqlShouldIncludeJoinAndWhereCondition()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Product")
                    .FieldFor("relatedProducts")
                    .SqlJunction("productRelations",
                        (join, _, __, ___) => join.On("id", "productId"),
                        (join, _, __, ___) => join.On("relatedProductId", "id"))
                    .Where((where, _, __, ___) => where.Columns("productId", "relatedProductId", "<>"));
            });

            var query = "{ product { name, relatedProducts { name } } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter(new DefaultAliasGenerator());
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\",\n  \"relatedProducts\".\"id\" AS \"relatedProducts__id\",\n  \"relatedProducts\".\"name\" AS \"relatedProducts__name\"\nFROM \"products\" AS \"product\"\nLEFT JOIN \"productRelations\" \"productRelations\" ON \"product\".\"id\" = \"productRelations\".\"productId\"\nLEFT JOIN \"products\" \"relatedProducts\" ON \"productRelations\".\"relatedProductId\" = \"relatedProducts\".\"id\"\nWHERE \"productRelations\".\"productId\" <> \"productRelations\".\"relatedProductId\"");
        }

        [Fact]
        public void Compile_WithOrderBy_SqlShouldIncludeOrderByClause()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Query")
                    .FieldFor("products")
                    .SqlOrder((order, _, __, ___) => order.By("name").ThenByDescending("price"));
            });

            var query = "{ products { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter(new DefaultAliasGenerator());
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Sql.Should().Be("SELECT\n  \"products\".\"id\" AS \"id\",\n  \"products\".\"name\" AS \"name\"\nFROM \"products\" AS \"products\"\nORDER BY \"products\".\"name\" ASC, \"products\".\"price\" DESC");
        }

        [Fact]
        public void Compile_WithWhereAndOrderBy_GeneratesValidSQL()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Query")
                    .FieldFor("products")
                    .SqlWhere((where, _, __, ___) => where.Column("id", 0, "<>"))
                    .SqlOrder((order, _, __, ___) => order.By("name").ThenByDescending("price"));
            });

            var query = "{ products { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter(new DefaultAliasGenerator());
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            var expectedSql =
                "SELECT\n  \"products\".\"id\" AS \"id\",\n  \"products\".\"name\" AS \"name\"\nFROM \"products\" AS \"products\"\nWHERE \"products\".\"id\" <> @p0\nORDER BY \"products\".\"name\" ASC, \"products\".\"price\" DESC";
            var expectedParameters = new Dictionary<string, object>{{"@p0", 0}};

            sql.Should()
                .BeEquivalentTo(new SqlResult(expectedSql, expectedParameters));
        }

        [Fact]
        public void Compile_WithJunctionOrderBy_SqlShouldIncludeJoinAndOrderByClause()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Product")
                    .FieldFor("relatedProducts")
                    .SqlJunction("productRelations",
                        (join, _, __, ___) => join.On("id", "productId"),
                        (join, _, __, ___) => join.On("relatedProductId", "id"))
                    .OrderBy((order, _, __, ___) => order.By("productId"));
            });

            var query = "{ product { name, relatedProducts { name } } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter(new DefaultAliasGenerator());
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\",\n  \"relatedProducts\".\"id\" AS \"relatedProducts__id\",\n  \"relatedProducts\".\"name\" AS \"relatedProducts__name\"\nFROM \"products\" AS \"product\"\nLEFT JOIN \"productRelations\" \"productRelations\" ON \"product\".\"id\" = \"productRelations\".\"productId\"\nLEFT JOIN \"products\" \"relatedProducts\" ON \"productRelations\".\"relatedProductId\" = \"relatedProducts\".\"id\"\nORDER BY \"relatedProducts\".\"productId\" ASC");
        }

        [Fact]
        public void Compile_WithJoinAndPaginate_SqlShouldIncludePaginationClause()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("ProductVariant")
                    .SqlTable("productVariants", "id");

                builder.Types.For("Product")
                    .FieldFor("variants")
                    .SqlJoin((join, _, __, ___) => join.On("id", "productId"))
                    .SqlPaginate(true)
                    .SqlOrder((order, _, __, ___) => order.By("id"));
            });

            var query = "{ product { name, variants { edges { node { name } } } } }";
            var context = CreateResolveFieldContext(schema, query);

            var joinedOneToManyPaginatedSql = "LEFT JOIN LATERAL (\n  SELECT \"variants\".*, COUNT(*) OVER () AS \"$total\"\n  FROM \"productVariants\" \"variants\"\n  WHERE \"products\".\"id\" = \"variants\".\"productId\"\n  ORDER BY \"variants\".\"id\" ASC\n  LIMIT ALL OFFSET 0\n) \"variants\" ON \"products\".\"id\" = \"variants\".\"productId\"";

            var converter = new QueryToSqlConverter(new DefaultAliasGenerator());
            var dialect = new SqlDialectStub(joinedOneToManyPaginatedSql: joinedOneToManyPaginatedSql);
            var compiler = new SqlCompiler(dialect);

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Sql.Should().Be($"SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\",\n  \"variants\".\"id\" AS \"variants__id\",\n  \"variants\".\"name\" AS \"variants__name\",\n  \"variants\".\"$total\" AS \"variants__$total\"\nFROM \"products\" AS \"product\"\n{joinedOneToManyPaginatedSql}\nORDER BY \"variants\".\"id\" ASC");
        }

        [Fact]
        public void Compile_WithMinifyAliasGenerator_SqlHasMinifiedTableAndColumnNames()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");
            });

            var query = "{ product { id, name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter(new MinifyAliasGenerator());
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Sql.Should().Be("SELECT\n  \"a\".\"id\" AS \"b\",\n  \"a\".\"name\" AS \"c\"\nFROM \"products\" AS \"a\"");
        }

        private static ISchema CreateSimpleSchema(Action<SchemaBuilder> configure = null)
        {
            return Schema.For(@"
type ProductVariant {
  id: ID!
  name: String
}

type ProductVariantConnection {
    edges: [ProductVariantEdge]
}

type ProductVariantEdge {
    cursor: String!
    node: ProductVariant
}

type Product {
  id: ID!
  name: String
  variants: ProductVariantConnection
  relatedProducts: [Product]
}

type Query {
  product(id: ID): Product
  products: [Product]
}
", builder => { configure?.Invoke(builder); });
        }

        private static IResolveFieldContext CreateResolveFieldContext(ISchema schema, string query) =>
            StubExecutionStrategy.Instance.GetResolveFieldContext(schema, query);
    }
}

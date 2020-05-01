using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL.Execution;
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

            Func<string> action = () => compiler.Compile(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .And.ParamName.Should()
                .Be("node");
        }

        [Fact]
        public void Compile_WhenContextIsNull_ThrowsException()
        {
            var compiler = new SqlCompiler(new SqlDialectStub());

            Func<string> action = () => compiler.Compile(new SqlNoop(), null);

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

            var converter = new QueryToSqlConverter();
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"");
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

            var converter = new QueryToSqlConverter();
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"");
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

            var converter = new QueryToSqlConverter();
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Should().Be("SELECT\n  CONCAT(\"product\".\"id\", \"product\".\"firstName\", \"product\".\"lastName\") AS \"id#firstName#lastName\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"");
        }

        [Fact]
        public void Compile_WithWhereQuery_ReturnsSqlIncludingWhereCondition()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Query")
                    .FieldFor("product", null)
                    .SqlWhere((tableAlias, _, __) => $"{tableAlias}.\"id\" = 1");
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"\nWHERE \"product\".\"id\" = 1");
        }

        [Fact]
        public void Compile_WithWhereQueryAndArguments_PassesArgumentsToWhereDelegate()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Query")
                    .FieldFor("product", null)
                    .SqlWhere((tableAlias, arguments, __) =>
                        $"{tableAlias}.\"id\" = \"{arguments["id"]}\"");
            });

            var query = "{ product(id: \"3\") { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Should()
                .Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"\nWHERE \"product\".\"id\" = \"3\"");
        }

        [Fact]
        public void Compile_WithJoinCondition_SqlIncludingJoinCondition()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Query")
                    .FieldFor("product", null);

                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Product")
                    .FieldFor("variants", null)
                    .SqlJoin((parentTable, childTable, _, __) =>
                        $"{parentTable}.\"id\" = {childTable}.\"productId\"");

                builder.Types.For("ProductVariant")
                    .SqlTable("productVariants", "id");
            });

            var query = "{ product { name, variants { name } } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\",\n  \"variants\".\"id\" AS \"variants__id\",\n  \"variants\".\"name\" AS \"variants__name\"\nFROM \"products\" AS \"product\"\nLEFT JOIN \"productVariants\" \"variants\" ON \"product\".\"id\" = \"variants\".\"productId\"");
        }

        [Fact]
        public void Compile_WithJunction_SqlIncludingJoinCondition()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Product")
                    .FieldFor("relatedProducts", null)
                    .SqlJunction("productRelations",
                        (products, productRelations, _, __) => $"{products}.\"id\" = {productRelations}.\"productId\"",
                        (productRelations, products, _, __) => $"{productRelations}.\"relatedProductId\" = {products}.\"id\"");
            });

            var query = "{ product { name, relatedProducts { name } } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\",\n  \"relatedProducts\".\"id\" AS \"relatedProducts__id\",\n  \"relatedProducts\".\"name\" AS \"relatedProducts__name\"\nFROM \"products\" AS \"product\"\nLEFT JOIN \"productRelations\" \"productRelations\" ON \"product\".\"id\" = \"productRelations\".\"productId\"\nLEFT JOIN \"products\" \"relatedProducts\" ON \"productRelations\".\"relatedProductId\" = \"relatedProducts\".\"id\"");
        }

        [Fact]
        public void Compile_WithJunctionAndWhere_SqlIncludingJoinCondition()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Product")
                    .FieldFor("relatedProducts", null)
                    .SqlJunction("productRelations",
                        (products, productRelations, _, __) => $"{products}.\"id\" = {productRelations}.\"productId\"",
                        (productRelations, products, _, __) => $"{productRelations}.\"relatedProductId\" = {products}.\"id\"")
                    .Where((productRelations, _, __) => $"{productRelations}.\"productId\" <> {productRelations}.\"relatedProductId\"");
            });

            var query = "{ product { name, relatedProducts { name } } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = compiler.Compile(node, context);

            sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\",\n  \"relatedProducts\".\"id\" AS \"relatedProducts__id\",\n  \"relatedProducts\".\"name\" AS \"relatedProducts__name\"\nFROM \"products\" AS \"product\"\nLEFT JOIN \"productRelations\" \"productRelations\" ON \"product\".\"id\" = \"productRelations\".\"productId\"\nLEFT JOIN \"products\" \"relatedProducts\" ON \"productRelations\".\"relatedProductId\" = \"relatedProducts\".\"id\"\nWHERE \"productRelations\".\"productId\" <> \"productRelations\".\"relatedProductId\"");
        }

        private static ISchema CreateSimpleSchema(Action<SchemaBuilder> configure = null)
        {
            return Schema.For(@"
type ProductVariant {
  id: ID!
  name: String
}

type Product {
  id: ID!
  name: String
  variants: [ProductVariant]
  relatedProducts: [Product]
}
type Query {
  product(id: ID): Product
}
", builder => { configure?.Invoke(builder); });
        }

        private static IResolveFieldContext CreateResolveFieldContext(ISchema schema, string query)
        {
            var documentBuilder = new GraphQLDocumentBuilder();
            var document = documentBuilder.Build(query);
            schema.Initialize();

            var executionContext = new ExecutionContext
            {
                Document = document,
                Schema = schema,
                Fragments = document.Fragments,
                Operation = document.Operations.First()
            };

            var root = new RootExecutionNode(schema.Query)
            {
                Result = executionContext.RootValue
            };

            var fields = ExecutionHelper.CollectFields(executionContext, schema.Query, executionContext.Operation.SelectionSet);
            ExecutionStrategy.SetSubFieldNodes(executionContext, root, fields);

            var subNode = root.SubFields.FirstOrDefault();

            return new ReadonlyResolveFieldContext(subNode.Value, executionContext);
        }
    }
}

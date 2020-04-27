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

            Func<Task> action = async () => await compiler.Compile(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .And.ParamName.Should()
                .Be("node");
        }

        [Fact]
        public void Compile_WhenContextIsNull_ThrowsException()
        {
            var compiler = new SqlCompiler(new SqlDialectStub());

            Func<Task> action = async () => await compiler.Compile(new SqlNoop(), null);

            action.Should()
                .Throw<ArgumentNullException>()
                .And.ParamName.Should()
                .Be("context");
        }

        [Fact]
        public async Task Compile_WithSimpleQuery_ReturnsSql()
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
            var sql = await compiler.Compile(node, context);

            sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"");
        }

        [Fact]
        public async Task Compile_WhenQueryIncludesUniqueKey_ColumnIsOnlySelectedOnce()
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
            var sql = await compiler.Compile(node, context);

            sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"");
        }

        [Fact]
        public async Task Compile_WhenTableConfigHasCompositeUniqueKey_KeysAreIncludedInGeneratedSql()
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
            var sql = await compiler.Compile(node, context);

            sql.Should().Be("SELECT\n  CONCAT(\"product\".\"id\", \"product\".\"firstName\", \"product\".\"lastName\") AS \"id#firstName#lastName\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"");
        }

        [Fact]
        public async Task Compile_WithWhereQuery_ReturnsSqlIncludingWhereCondition()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Query")
                    .FieldFor("product", null)
                    .SqlWhere((tableAlias, _, __) => Task.FromResult($"{tableAlias}.\"id\" = 1"));
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = await compiler.Compile(node, context);

            sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"\nWHERE \"product\".\"id\" = 1");
        }

        [Fact]
        public async Task Compile_WithWhereQueryAndArguments_PassesArgumentsToWhereDelegate()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Query")
                    .FieldFor("product", null)
                    .SqlWhere((tableAlias, arguments, __) =>
                        Task.FromResult($"{tableAlias}.\"id\" = \"{arguments["id"]}\""));
            });

            var query = "{ product(id: \"3\") { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = await compiler.Compile(node, context);

            sql.Should()
                .Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\"\nFROM \"products\" AS \"product\"\nWHERE \"product\".\"id\" = \"3\"");
        }

        [Fact]
        public async Task Compile_WithJoinCondition_SqlIncludingJoinCondition()
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
                        Task.FromResult($"{parentTable}.\"id\" = {childTable}.\"productId\""));

                builder.Types.For("ProductVariant")
                    .SqlTable("productVariants", "id");
            });

            var query = "{ product { name, variants { name } } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var compiler = new SqlCompiler(new SqlDialectStub());

            var node = converter.Convert(context);
            var sql = await compiler.Compile(node, context);

            sql.Should().Be("SELECT\n  \"product\".\"id\" AS \"id\",\n  \"product\".\"name\" AS \"name\",\n  \"variants\".\"id\" AS \"variants__id\",\n  \"variants\".\"name\" AS \"variants__name\"\nFROM \"products\" AS \"product\"\nLEFT JOIN \"productVariants\" \"variants\" ON \"product\".\"id\" = \"variants\".\"productId\"");
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

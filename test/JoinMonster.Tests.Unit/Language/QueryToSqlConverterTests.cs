using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Resolvers;
using GraphQL.Types;
using GraphQL.Utilities;
using JoinMonster.Builders;
using JoinMonster.Configs;
using JoinMonster.Language;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit.Language
{
    public class QueryToSqlConverterTests
    {
        [Fact]
        public void Convert_WithSimpleQuery_ReturnsSqlTableNode()
        {
            var schema = CreateSimpleSchema(builder => { builder.Types.For("Product").SqlTable("products", "id"); });

            var query = "{ product { id, name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should().BeOfType<SqlTable>();
        }

        [Fact]
        public void Convert_WithSimpleQuery_SetsTableName()
        {
            var schema = CreateSimpleSchema(builder => { builder.Types.For("Product").SqlTable("products", "id"); });

            var query = "{ product { id, name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Name.Should()
                .Be("products");
        }

        [Fact]
        public void Convert_WithSimpleQuery_SetsTableAsToTableName()
        {
            var schema = CreateSimpleSchema(builder => { builder.Types.For("Product").SqlTable("products", "id"); });

            var query = "{ product { id, name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.As.Should()
                .Be("product");
        }

        [Fact]
        public void Convert_WithSimpleQuery_SetsSourceLocation()
        {
            var schema = CreateSimpleSchema(builder => { builder.Types.For("Product").SqlTable("products", "id"); });

            var query = "{ product { id, name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.SourceLocation.Should()
                .NotBeNull();
        }

        [Fact]
        public void Convert_WithSimpleQuery_SetsFieldsAsColumnNodes()
        {
            var schema = CreateSimpleSchema(builder => { builder.Types.For("Product").SqlTable("products", "id"); });

            var query = "{ product { id, name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Columns.Should()
                .ContainEquivalentOf(new SqlColumn(node, "id", "id", "id", true),
                    config => config.Excluding(x => x.SourceLocation))
                .And.ContainEquivalentOf(new SqlColumn(node, "name", "name", "name"),
                    config => config.Excluding(x => x.SourceLocation));
        }

        [Fact]
        public void Convert_WhenColumnNameIsConfigured_UsesConfiguredColumnName()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                var product = builder.Types.For("Product");
                product.SqlTable("products", "id");
                product.FieldFor("name", null).SqlColumn("productName");
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Columns.Should()
                .ContainEquivalentOf(new SqlColumn(node, "productName", "name", "name"),
                    config => config.Excluding(x => x.SourceLocation));
        }

        [Fact]
        public void Convert_WithIgnoredField_ColumnsDoesNotContainField()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");
                builder.Types.For("Product")
                    .FieldFor("name", null)
                    .SqlColumn()
                    .Ignore();
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Columns.Should()
                .NotContain(column => column.FieldName == "name");
        }

        [Fact]
        public void Convert_WithUniqueKeyColumn_ColumnsDoesContainsField()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Columns.Should()
                .ContainEquivalentOf(new SqlColumn(node, "id", "id", "id", true));
        }

        [Fact]
        public void Convert_WithMultipleUniqueKeyColumns_ColumnsDoesContainFields()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", new [] { "id", "key" });
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Columns.Should()
                .ContainEquivalentOf(new SqlComposite(node, new[] {"id", "key"}, "id#key", "id#key", true));
        }

        [Fact]
        public void Convert_WhenQueryContainsUniqueKeyColumn_ColumnsOnlyContainsFieldOnce()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");
            });

            var query = "{ product { id, name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Columns.Should()
                .ContainSingle(x => x.FieldName == "id");
        }

        [Fact]
        public void Convert_WithAlwaysFetchColumns_ColumnsDoesContainFields()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id")
                    .AlwaysFetch("key", "type");
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Columns.Should()
                .ContainEquivalentOf(new SqlColumn(node, "key", "key", "key"))
                .And.ContainEquivalentOf(new SqlColumn(node, "type", "type", "type"));
        }

        [Fact]
        public void Convert_WhenFieldHasWhereClause_SetsWhereOnSqlTable()
        {
            void Where(WhereBuilder where, IReadOnlyDictionary<string, object> arguments,
                IResolveFieldContext context) => where.Column("id", 3);

            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Query")
                    .FieldFor("product", null)
                    .SqlWhere(Where);

                builder.Types.For("Product")
                    .SqlTable("products", "id");
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Where.Should()
                .BeEquivalentTo((WhereDelegate)Where);
        }

        [Fact]
        public void Convert_WhenQueryHasArguments_AddsArgumentsToSqlTable()
        {
            var schema = CreateSimpleSchema(builder =>
            {
               builder.Types.For("Product")
                    .SqlTable("products", "id");
            });

            var query = "{ product(id: \"1\") { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Arguments.Should()
                .SatisfyRespectively(argument =>
                {
                    argument.Key.Should().Be("id");
                    argument.Value.Should().Be("1");
                });
        }

        [Fact]
        public void Convert_WithNoContext_ThrowsArgumentNullException()
        {
            var converter = new QueryToSqlConverter();

            Action action = () => converter.Convert(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("context");
        }

        [Fact]
        public void Convert_WhenTypeDoesNotHaveSqlTableConfig_ThrowsException()
        {
            var schema = CreateSimpleSchema();

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            Action action = () => converter.Convert(context);

            action.Should()
                .Throw<JoinMonsterException>()
                .Which.Message.Should()
                .Be($"Expected node to be of type '{typeof(SqlTable)}' but was '{typeof(SqlNoop)}'.");
        }

        [Fact]
        public void Convert_WhenFieldHasJoinExpression_SetsJoinOnSqlTable()
        {
            void Join(JoinBuilder join, IReadOnlyDictionary<string, object> arguments,
                IResolveFieldContext context, Node sqlAstNode) => join.On("id", "productId");

            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Query")
                    .FieldFor("product", null);

                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("ProductVariant")
                    .SqlTable("productVariants", "id");

                builder.Types.For("Product")
                    .FieldFor("variants", null)
                    .SqlJoin(Join);
            });

            var query = "{ product { name, variants { name } } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Tables
                .Should()
                .ContainSingle(x => x.Name == "productVariants")
                .Which.Should()
                .BeOfType<SqlTable>()
                .Which.Join.Should()
                .Be((JoinDelegate) Join);
        }

        [Fact(Skip = "We cannot rely on field resolver is set or not since FieldMiddleware is a resolver.")]
        public void Convert_WhenFieldHasAResolver_ColumnsDoesNotContainField()
        {
            var schema = CreateSimpleSchema(builder =>
            {
               builder.Types.For("Product")
                    .SqlTable("products", "id");

               builder.Types.For("Product")
                   .FieldFor("name", null)
                   .Resolver = new FuncFieldResolver<object>(x => null);
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Columns.OfType<SqlColumn>().Should()
                .NotContain(x => x.Name == "name");
        }

        [Fact]
        public void Convert_WhenFieldHasArguments_AddsArgumentsToSqlColumn()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Query")
                    .FieldFor("product", null);

                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("ProductVariant")
                    .SqlTable("productVariants", "id");

                builder.Types.For("Product")
                    .FieldFor("variants", null)
                    .SqlJoin((_, __, ___, ____) => {});
            });

            var query = "{ product { variants { price(currency: \"DKK\") } } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Tables.Should()
                .ContainSingle()
                .Which.Columns.OfType<SqlColumn>().Should()
                .Contain(x => x.FieldName == "price")
                .Which.Arguments.Should()
                .SatisfyRespectively(argument =>
                {
                    argument.Key.Should().Be("currency");
                    argument.Value.Should().Be("DKK");
                });
        }

        [Fact]
        public void Convert_WhenFieldHasExpression_SetsExpressionOnSqlColumn()
        {
            string Expression(string tableAlias, IReadOnlyDictionary<string, object> arguments,
                IResolveFieldContext context) => $"{tableAlias}.\"productName\"";

            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Product")
                    .FieldFor("name", null)
                    .SqlColumn()
                    .Expression(Expression);
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Columns.OfType<SqlColumn>().Should().ContainSingle(x => x.FieldName == "name")
                .Which.Expression.Should()
                .BeEquivalentTo((ExpressionDelegate)Expression);
        }

        [Fact]
        public void Convert_WhenQueryHasInlineFragment_AddsSelectionToColumns()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Node")
                    .SqlTable("nodes", "id");

                builder.Types.For("Product")
                    .SqlTable("products", "id");

                builder.Types.For("Product").IsTypeOfFunc = obj => true;
                builder.Types.For("ProductVariant").IsTypeOfFunc = obj => true;
            });

            var query = "{ node(id: 1) { id, ...on Product { name } } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Columns.Should()
                .Contain(x => x.FieldName == "name")
                .And.Contain(x => x.FieldName == "id");
        }

        [Fact]
        public void Convert_WhenQueryHasFragments_AddsSelectionToColumns()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Node")
                    .SqlTable("nodes", "id");

                builder.Types.For("Product")
                    .SqlTable("products", "id");
            });

            var query = "{ node(id: 1) { id, ...ProductName } } fragment ProductName on Product { name }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Columns.Should()
                .Contain(x => x.FieldName == "name")
                .And.Contain(x => x.FieldName == "id");
        }

        private static ISchema CreateSimpleSchema(Action<SchemaBuilder> configure = null)
        {
            return Schema.For(@"
interface Node {
  id: ID!
}

type ProductVariant implements Node {
  id: ID!
  name: String
  price: Decimal
}

type Product implements Node {
  id: ID!
  name: String
  variants: [ProductVariant]
}

type Query {
  product(id: ID): Product
  node(id: ID!): Node
}
", builder =>
            {
                builder.Types.For("Product").IsTypeOfFunc = obj => true;
                builder.Types.For("ProductVariant").IsTypeOfFunc = obj => true;

                configure?.Invoke(builder);
            });
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

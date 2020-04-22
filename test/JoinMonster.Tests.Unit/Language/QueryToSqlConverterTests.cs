using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GraphQL.Execution;
using GraphQL.Types;
using GraphQL.Utilities;
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
                .SatisfyRespectively(column =>
                {
                    column.Name.Should().Be("id");
                    column.FieldName.Should().Be("id");
                    column.As.Should().Be("id");
                    column.SourceLocation.Should().NotBeNull();
                }, column =>
                {
                    column.Name.Should().Be("name");
                    column.FieldName.Should().Be("name");
                    column.As.Should().Be("name");
                    column.SourceLocation.Should().NotBeNull();
                });
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
                .ContainEquivalentOf(new SqlColumn("productName", "name", "name"),
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
                .ContainEquivalentOf(new SqlColumn("id", null, "id"));
        }

        [Fact]
        public void Convert_WithMultipleUniqueKeyColumns_ColumnsDoesContainsFields()
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
                .ContainEquivalentOf(new SqlColumn("id", null, "id"))
                .And.ContainEquivalentOf(new SqlColumn("key", null, "key"));
        }

        [Fact]
        public void Convert_WithAlwaysFetchColumns_ColumnsDoesContainFields()
        {
            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Product")
                    .SqlTable("products", new [] { "id" })
                    .AlwaysFetch("key", "type");
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Columns.Should()
                .ContainEquivalentOf(new SqlColumn("key", null, "key"))
                .And.ContainEquivalentOf(new SqlColumn("type", null, "type"));
        }

        [Fact]
        public void Convert_WhenFieldHasWhereExpression_SetsWhereOnSqlTable()
        {
            string Where(string tableAlias, IDictionary<string, object> arguments,
                IDictionary<string, object> userContext) => $"{tableAlias}.id = 3";

            var schema = CreateSimpleSchema(builder =>
            {
                builder.Types.For("Query")
                    .FieldFor("product", null)
                    .SqlWhere(Where);

                builder.Types.For("Product")
                    .SqlTable("products", new [] { "id" })
                    .AlwaysFetch("key", "type");
            });

            var query = "{ product { name } }";
            var context = CreateResolveFieldContext(schema, query);

            var converter = new QueryToSqlConverter();
            var node = converter.Convert(context);

            node.Should()
                .BeOfType<SqlTable>()
                .Which.Where.Should()
                .BeEquivalentTo((WhereDelegate) Where);
        }

        private static ISchema CreateSimpleSchema(Action<SchemaBuilder> configure = null)
        {
            return Schema.For(@"
type Product {
  id: ID!
  name: String
}
type Query {
  product: Product
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

using System;
using FluentAssertions;
using GraphQL.Types;
using GraphQL.Types.Relay;
using Xunit;

namespace JoinMonster.Tests.Unit
{
    public class GraphTypeExtensionsTests
    {
        [Fact]
        public void IsListType_ObjectGraphType_ReturnsFalse()
        {
            var graphType = new ObjectGraphType();

            var isListType = graphType.IsListType();

            isListType.Should().BeFalse();
        }

        [Fact]
        public void IsListType_NonNullOfObjectGraphType_ReturnsFalse()
        {
           var graphType = new NonNullGraphType(new ObjectGraphType());

           var isListType = graphType.IsListType();

           isListType.Should().BeFalse();
        }

        [Fact]
        public void IsListType_ListGraphType_ReturnsTrue()
        {
            var graphType = new ListGraphType(new ObjectGraphType());

            var isListType = graphType.IsListType();

            isListType.Should().BeTrue();
        }

        [Fact]
        public void IsListType_NonNullOfListGraphType_ReturnsFTrue()
        {
           var graphType = new NonNullGraphType(new ListGraphType(new ObjectGraphType()));

           var isListType = graphType.IsListType();

           isListType.Should().BeTrue();
        }

        [Fact]
        public void IsListType_Null_ThrowsArgumentNullException()
        {
            Action action = () => GraphTypeExtensions.IsListType(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SqlTable_WhenGraphTypeIsNull_ThrowsException()
        {
            Action action = () => GraphTypeExtensions.SqlTable(null, "product", "id");

            action.Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be("graphType");
        }

        [Fact]
        public void SqlTable_WithTableName_SetsTable()
        {
            var graphType = new ObjectGraphType();

            var builder = graphType.SqlTable("product", "id");

            builder.SqlTableConfig.Table(null, null).Should().Be("product");
        }

        [Fact]
        public void SqlTable_WithSingleUniqueKey_SetsUniqueKey()
        {
            var graphType = new ObjectGraphType();

            var builder = graphType.SqlTable("product", "id");

            builder.SqlTableConfig.UniqueKey.Should().Contain("id");
        }

        [Fact]
        public void SqlTable_WithMultipleUniqueKey_SetsUniqueKey()
        {
            var graphType = new ObjectGraphType();

            var builder = graphType.SqlTable("product", new [] { "id", "key" });

            builder.SqlTableConfig.UniqueKey.Should().Contain(new[] {"id", "key"});
        }

        [Fact]
        public void GetSqlTableConfig_WhenGraphTypeIsNull_ThrowsException()
        {
            Action action = () => GraphTypeExtensions.GetSqlTableConfig(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("graphType");
        }

        [Fact]
        public void GetSqlTableConfig_WhenConfigHasNotBeenSet_ReturnsNull()
        {
            var graphType = new ObjectGraphType();

            var config = graphType.GetSqlTableConfig();

            config.Should().BeNull();
        }

        [Fact]
        public void GetSqlTableConfig_WhenConfigHasBeenSet_ReturnsConfig()
        {
            var graphType = new ObjectGraphType();
            var builder = graphType.SqlTable("products", "id");

            var config = graphType.GetSqlTableConfig();

            config.Should().Be(builder.SqlTableConfig);
        }

        [Fact]
        public void IsConnectionType_ConnectionType_ReturnsTrue()
        {
            var graphType = new ConnectionType<ObjectGraphType>();

            var isConnectionType = graphType.IsConnectionType();

            isConnectionType.Should().BeTrue();
        }

        [Fact]
        public void IsConnectionType_ObjectGraphType_ReturnsFalse()
        {
            var graphType = new ObjectGraphType();

            var isConnectionType = graphType.IsConnectionType();

            isConnectionType.Should().BeFalse();
        }
    }
}

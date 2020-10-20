using System;
using FluentAssertions;
using GraphQL.Utilities;
using Xunit;

namespace JoinMonster.Tests.Unit
{
    public class TypeConfigExtensionsTests
    {
        [Fact]
        public void SqlTable_WhenTypeConfigIsNull_ThrowsException()
        {
            Action action = () => TypeConfigExtensions.SqlTable(null, "product", "id");

            action.Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be("typeConfig");
        }

        [Fact]
        public void SqlTable_WithTableName_SetsTable()
        {
            var typeConfig = new TypeConfig("Product");

            var builder = typeConfig.SqlTable("product", "id");

            builder.SqlTableConfig.Table(null, null).Should().Be("product");
        }

        [Fact]
        public void SqlTable_WithSingleUniqueKey_SetsUniqueKey()
        {
            var typeConfig = new TypeConfig("Product");

            var builder = typeConfig.SqlTable("product", "id");

            builder.SqlTableConfig.UniqueKey.Should().Contain("id");
        }

        [Fact]
        public void SqlTable_WithMultipleUniqueKey_SetsUniqueKey()
        {
            var typeConfig = new TypeConfig("Product");

            var builder = typeConfig.SqlTable("product", new [] { "id", "key" });

            builder.SqlTableConfig.UniqueKey.Should().Contain(new[] {"id", "key"});
        }
    }
}

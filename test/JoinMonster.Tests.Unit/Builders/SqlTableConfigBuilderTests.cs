using System;
using FluentAssertions;
using JoinMonster.Builders;
using Xunit;

namespace JoinMonster.Tests.Unit.Builders
{
    public class SqlTableConfigBuilderTests
    {
        [Fact]
        public void Create_WhenTableNameIshNull_ThrowsArgumentNullException()
        {
            Action action = () => SqlTableConfigBuilder.Create(null, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Create_WhenUniqueKeyIshNull_ThrowsArgumentNullException()
        {
            Action action = () => SqlTableConfigBuilder.Create((_, __) => "products", null);

        action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Create_WithTableName_SetsTableName()
        {
            var tableName = "products";

            var builder = SqlTableConfigBuilder.Create((_, __) => tableName, new[] {"id"});

            builder.SqlTableConfig.Table(null, null).Should().Be(tableName);
        }

        [Fact]
        public void Create_WithUniqueKey_SetsUniqueKey()
        {
            var uniqueKey = new[] {"id"};

            var builder = SqlTableConfigBuilder.Create((_, __) => "products", uniqueKey);

            builder.SqlTableConfig.UniqueKey.Should().BeEquivalentTo(uniqueKey);
        }

        [Fact]
        public void AlwaysFetch_WithValue_SetsAlwaysFetch()
        {
            var builder = SqlTableConfigBuilder.Create((_, __) => "products", new[] {"id"})
                .AlwaysFetch("type", "name");

            builder.SqlTableConfig.AlwaysFetch.Should().BeEquivalentTo("type", "name");
        }
    }
}

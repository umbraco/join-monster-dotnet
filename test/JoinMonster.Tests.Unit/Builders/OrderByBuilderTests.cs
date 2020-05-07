using System;
using FluentAssertions;
using JoinMonster.Builders;
using JoinMonster.Configs;
using JoinMonster.Data;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit.Builders
{
    public class OrderByBuilderTests
    {
        [Fact]
        public void By_WhenColumnIsNull_ThrowsException()
        {
            var builder = new OrderByBuilder();

            Action action = () => builder.By(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("column");
        }

        [Fact]
        public void ByDescending_WhenColumnIsNull_ThrowsException()
        {
            var builder = new OrderByBuilder();

            Action action = () => builder.ByDescending(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("column");
        }

        [Fact]
        public void By_WithColumnName_SetsOrderByProperty()
        {
            var builder = new OrderByBuilder();

            builder.By("id");

            builder.OrderBy.Should().BeEquivalentTo(new OrderBy("id", SortDirection.Ascending));
        }

        [Fact]
        public void ByDescending_WithColumnName_SetsOrderByProperty()
        {
            var builder = new OrderByBuilder();

            builder.ByDescending("id");

            builder.OrderBy.Should().BeEquivalentTo(new OrderBy("id", SortDirection.Descending));
        }
    }
}

using System;
using FluentAssertions;
using JoinMonster.Builders;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit.Builders
{
    public class ThenOrderByBuilderTests
    {
        [Fact]
        public void Ctor_WhenOrderByIsNull_ThrowsException()
        {
            Action action = () => new ThenOrderByBuilder("products", null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("orderBy");
        }

        [Fact]
        public void ThenBy_WhenColumnIsNull_ThrowsException()
        {
            var builder = new ThenOrderByBuilder("products", new OrderBy("products", "name", SortDirection.Ascending));

            Action action = () => builder.ThenBy(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("column");
        }

        [Fact]
        public void ThenByDescending_WhenColumnIsNull_ThrowsException()
        {
            var builder = new ThenOrderByBuilder("products", new OrderBy("products", "name", SortDirection.Ascending));

            Action action = () => builder.ThenByDescending(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("column");
        }

        [Fact]
        public void ThenBy_WithColumnName_SetsThenByOnOrderBy()
        {
            var orderBy = new OrderBy("products", "name", SortDirection.Ascending);
            var builder = new ThenOrderByBuilder("products", orderBy);

            builder.ThenBy("id");

            orderBy.ThenBy.Should().BeEquivalentTo(new OrderBy("products", "id", SortDirection.Ascending));
        }

        [Fact]
        public void ThenBy_WithColumnName_SetsOrderByProperty()
        {
            var builder = new ThenOrderByBuilder("products", new OrderBy("products", "id", SortDirection.Ascending));

            builder.ThenBy("id");

            builder.OrderBy.Should().BeEquivalentTo(new OrderBy("products", "id", SortDirection.Ascending));
        }

        [Fact]
        public void ThenByDescending_WithColumnName_SetsThenByOnOrderBy()
        {
            var orderBy = new OrderBy("products", "name", SortDirection.Ascending);
            var builder = new ThenOrderByBuilder("products", orderBy);

            builder.ThenByDescending("id");

            orderBy.ThenBy.Should().BeEquivalentTo(new OrderBy("products", "id", SortDirection.Descending));
        }

        [Fact]
        public void ThenByDescending_WithColumnName_SetsOrderByColumn()
        {
            var builder = new ThenOrderByBuilder("products", new OrderBy("products", "id", SortDirection.Ascending));

            builder.ThenByDescending("id");

            builder.OrderBy.Should().BeEquivalentTo(new OrderBy("products", "id", SortDirection.Descending));
        }
    }
}

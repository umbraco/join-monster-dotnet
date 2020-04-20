using System;
using FluentAssertions;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit.Language.AST
{
    public class SqlColumnsTest
    {
        [Fact]
        public void Add_WithNull_ThrowsArgumentNullException()
        {
            var sut = new SqlColumns();

            Action action = () => sut.Add(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Add_WithSqlColumn_ChildrenContainsIt()
        {
            var sut = new SqlColumns();
            var sqlColumn = new SqlColumn("id", "id", "id");

            sut.Add(sqlColumn);

            sut.Children.Should().Contain(sqlColumn);
        }

        [Fact]
        public void Add_WithSqlColumn_EnumeratorContainsIt()
        {
            var sut = new SqlColumns();
            var sqlColumn = new SqlColumn("id", "id", "id");

            sut.Add(sqlColumn);

            sut.Should().Contain(sqlColumn);
        }

        [Fact]
        public void Children_WhenNoColumns_ReturnsEmptyCollection()
        {
            var sut = new SqlColumns();

            sut.Children.Should().BeEmpty();
        }

        [Fact]
        public void GetEnumerator_WhenNoColumns_ReturnsEmptyCollection()
        {
            var sut = new SqlColumns();

            sut.Should().BeEmpty();
        }
    }
}

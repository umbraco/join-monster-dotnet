using System;
using FluentAssertions;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit.Language.AST
{
    public class SqlTablesTest
    {
        [Fact]
        public void Add_WithNull_ThrowsArgumentNullException()
        {
            var sut = new SqlTables();

            Action action = () => sut.Add(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Add_WithSqlTable_ChildrenContainsIt()
        {
            var sut = new SqlTables();
            var sqlTable = new SqlTable("products", "products", new SqlColumns(), new SqlTables(), new Arguments(),
                false, null, null);

            sut.Add(sqlTable);

            sut.Children.Should().Contain(sqlTable);
        }

        [Fact]
        public void Add_WithSqlTable_EnumeratorContainsIt()
        {
            var sut = new SqlTables();
            var sqlTable = new SqlTable("products", "products", new SqlColumns(), new SqlTables(), new Arguments(),
                false, null, null);

            sut.Add(sqlTable);

            sut.Should().Contain(sqlTable);
        }

        [Fact]
        public void Children_WhenNoTables_ReturnsEmptyCollection()
        {
            var sut = new SqlTables();

            sut.Children.Should().BeEmpty();
        }

        [Fact]
        public void GetEnumerator_WhenNoTables_ReturnsEmptyCollection()
        {
            var sut = new SqlTables();

            sut.Should().BeEmpty();
        }
    }
}

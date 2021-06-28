using FluentAssertions;
using Xunit;

namespace JoinMonster.Tests.Unit
{
    public class MinifyAliasGeneratorTests
    {
        protected MinifyAliasGenerator CreateSUT() => new MinifyAliasGenerator();

        [Fact]
        public void GenerateTableAlias_WhenCalled_ReturnsShortAlias()
        {
            var generator = CreateSUT();
            var name = "myTable";

            var alias = generator.GenerateTableAlias(name);

            alias.Should().Be("a");
        }

        [Fact]
        public void GenerateColumnAlias_WhenCalled_ReturnsShortAlias()
        {
            var generator = CreateSUT();
            var name = "id";

            var alias = generator.GenerateColumnAlias(name);

            alias.Should().Be("a");
        }

        [Fact]
        public void GenerateTableAliasThenGenerateColumnAlias_WhenCalled_ReturnsDifferentAliases()
        {
            var generator = CreateSUT();

            var tableAlias = generator.GenerateTableAlias("myTable");
            var columnAlias = generator.GenerateColumnAlias("id");

            tableAlias.Should().Be("a");
            columnAlias.Should().Be("b");
        }

        [Fact]
        public void GenerateTableAlias_WhenCalledMultipleTimes_ReturnsDifferentAliases()
        {
            var generator = CreateSUT();

            var tableAlias1 = generator.GenerateTableAlias("myTable");
            var tableAlias2 = generator.GenerateTableAlias("myTable");
            var tableAlias3 = generator.GenerateTableAlias("anotherTable");
            var tableAlias4 = generator.GenerateTableAlias("table4");

            tableAlias1.Should().Be("a");
            tableAlias2.Should().Be("b");
            tableAlias3.Should().Be("c");
            tableAlias4.Should().Be("d");
        }

        [Fact]
        public void GenerateColumnAlias_WhenCalledMultipleTimesWithSameName_ReturnsSameAlias()
        {
            var generator = CreateSUT();

            var columnAlias1 = generator.GenerateColumnAlias("id");
            var columnAlias2 = generator.GenerateColumnAlias("id");

            columnAlias1.Should().Be("a");
            columnAlias2.Should().Be("a");
        }

        [Fact]
        public void GenerateColumnAlias_WhenCalledMultipleTimesWithDifferentNames_ReturnsDifferentAliases()
        {
            var generator = CreateSUT();

            var names = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ#$";

            foreach (var name in names)
            {
                generator.GenerateColumnAlias(name.ToString());
            }

            var columnAlias1 = generator.GenerateColumnAlias("id");
            var columnAlias2 = generator.GenerateColumnAlias("name");
            var columnAlias3 = generator.GenerateColumnAlias("id");

            columnAlias1.Should().Be("aa");
            columnAlias2.Should().Be("ab");
            columnAlias3.Should().Be("aa");
        }
    }
}

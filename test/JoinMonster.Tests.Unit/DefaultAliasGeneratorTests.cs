using FluentAssertions;
using Xunit;

namespace JoinMonster.Tests.Unit
{
    public class DefaultAliasGeneratorTests
    {
        protected DefaultAliasGenerator CreateSUT() => new DefaultAliasGenerator();

        [Fact]
        public void GenerateTableAlias_WhenCalled_ReturnsName()
        {
            var generator = CreateSUT();
            var name = "myTable";

            var alias = generator.GenerateTableAlias(name);

            alias.Should().Be(name);
        }

        [Fact]
        public void GenerateColumnAlias_WhenCalled_ReturnsName()
        {
            var generator = CreateSUT();
            var name = "id";

            var alias = generator.GenerateColumnAlias(name);

            alias.Should().Be(name);
        }
    }
}

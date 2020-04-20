using FluentAssertions;
using GraphQL.Language.AST;
using JoinMonster.Language;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit.Language.AST
{
    public class NodeTests
    {
        [Fact]
        public void SourceLocation_WhenNotSet_ReturnsNull()
        {
            var node = new TestNode();

            node.SourceLocation.Should().BeNull();
        }

        [Fact]
        public void SourceLocation_WhenSet_ReturnsValue()
        {
            var location = new SourceLocation(1, 2, 3, 4);
            var node = new TestNode()
                .WithLocation(location);

            node.SourceLocation.Should().Be(location);
        }

        [Fact]
        public void Children_ReturnsEmptyEnumerator()
        {
            var node = new TestNode();

            node.Children.Should().BeEmpty();
        }

        private class TestNode : Node
        {
        }
    }
}

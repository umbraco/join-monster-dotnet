using System;
using FluentAssertions;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit.Language.AST
{
    public class ArgumentsTest
    {
        [Fact]
        public void Add_WithNull_ThrowsArgumentNullException()
        {
            var sut = new Arguments();

            Action action = () => sut.Add(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Add_WithArgument_ChildrenContainsIt()
        {
            var sut = new Arguments();
            var argument = new Argument("id", new ValueNode(3));

            sut.Add(argument);

            sut.Children.Should().Contain(argument);
        }

        [Fact]
        public void Add_WithArgument_EnumeratorContainsIt()
        {
            var sut = new Arguments();
            var argument = new Argument("id", new ValueNode(3));

            sut.Add(argument);

            sut.Should().Contain(argument);
        }

        [Fact]
        public void Children_WhenNoArguments_ReturnsEmptyCollection()
        {
            var sut = new Arguments();

            sut.Children.Should().BeEmpty();
        }

        [Fact]
        public void GetEnumerator_WhenNoArguments_ReturnsEmptyCollection()
        {
            var sut = new Arguments();

            sut.Should().BeEmpty();
        }
    }
}

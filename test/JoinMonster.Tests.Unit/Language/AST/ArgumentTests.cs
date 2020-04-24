using System;
using FluentAssertions;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit.Language.AST
{
    public class ArgumentTests
    {
        [Fact]
        public void Ctor_WithNoName_ThrowsArgumentNullException()
        {
            Action action = () => new Argument(null, new ValueNode(1));

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("name");
        }

        [Fact]
        public void Ctor_WithNoValue_ThrowsArgumentNullException()
        {
            Action action = () => new Argument("id", null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("value");
        }

        [Fact]
        public void Ctor_WithName_SetsNameProperty()
        {
            var sut = new Argument("id", new ValueNode(1));

            sut.Name.Should().Be("id");
        }

        [Fact]
        public void Ctor_WithValue_SetsValueProperty()
        {
            var valueNode = new ValueNode(1);
            var sut = new Argument("id", valueNode);

            sut.Value.Should().Be(valueNode);
        }

        [Fact]
        public void Children_WhenCalled_ReturnsValue()
        {
            var valueNode = new ValueNode(1);
            var sut = new Argument("id", valueNode);

            sut.Children.Should().Contain(valueNode);
        }
    }
}

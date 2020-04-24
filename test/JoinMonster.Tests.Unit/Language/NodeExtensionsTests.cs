using System;
using FluentAssertions;
using GraphQL.Language.AST;
using JoinMonster.Language;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit.Language
{
    public class NodeExtensionsTests
    {
        [Fact]
        public void WithLocation_WhenNodeIsNull_ThrowsException()
        {
            Action action = () => ((Node) null).WithLocation(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("node");
        }

        [Fact]
        public void WithLocation_WhenLocationIsNull_ThrowsException()
        {
            Action action = () => new SqlNoop().WithLocation(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("location");
        }

        [Fact]
        public void WithLocation_WhenLocationIsNotNull_SetsLocationOnNode()
        {
            var location = new SourceLocation(0, 0, 1, 2);

            var sut = new SqlNoop().WithLocation(location);

            sut.SourceLocation.Should().Be(location);
        }
    }
}

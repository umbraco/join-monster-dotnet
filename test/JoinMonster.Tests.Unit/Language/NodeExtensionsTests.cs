using System;
using FluentAssertions;
using GraphQLParser.AST;
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
            Action action = () => ((Node) null).WithLocation(new GraphQLLocation());

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("node");
        }


        [Fact]
        public void WithLocation_WhenLocationIsNotNull_SetsLocationOnNode()
        {
            var location = new GraphQLLocation(1, 2);

            var sut = new SqlNoop().WithLocation(location);

            sut.Location.Should().Be(location);
        }
    }
}

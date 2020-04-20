using System;
using FluentAssertions;
using GraphQL.Types;
using Xunit;

namespace JoinMonster.Tests.Unit
{
    public class GraphTypeExtensionsTests
    {
        [Fact]
        public void IsListType_ObjectGraphType_ReturnsFalse()
        {
            var graphType = new ObjectGraphType();

            var isListType = graphType.IsListType();

            isListType.Should().BeFalse();
        }

        [Fact]
        public void IsListType_NonNullOfObjectGraphType_ReturnsFalse()
        {
           var graphType = new NonNullGraphType(new ObjectGraphType());

           var isListType = graphType.IsListType();

           isListType.Should().BeFalse();
        }

        [Fact]
        public void IsListType_ListGraphType_ReturnsTrue()
        {
            var graphType = new ListGraphType(new ObjectGraphType());

            var isListType = graphType.IsListType();

            isListType.Should().BeTrue();
        }

        [Fact]
        public void IsListType_NonNullOfListGraphType_ReturnsFTrue()
        {
           var graphType = new NonNullGraphType(new ListGraphType(new ObjectGraphType()));

           var isListType = graphType.IsListType();

           isListType.Should().BeTrue();
        }

        [Fact]
        public void IsListType_Null_ThrowsArgumentNullException()
        {
            IGraphType graphType = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            Action action = () => graphType.IsListType();

            action.Should().Throw<ArgumentNullException>();
        }
    }
}

using System.Collections.Generic;
using FluentAssertions;
using GraphQL;
using JoinMonster.Resolvers;
using Xunit;

namespace JoinMonster.Tests.Unit.Resolvers
{
    public class DictionaryFieldResolverTests
    {
        [Fact]
        public void Resolve_WhenSourceIsDictionaryAndFieldExists_ReturnsValue()
        {
            var source = new Dictionary<string, object>
            {
                {"name", "Jacket"}
            };

            var context = new ResolveFieldContext
            {
                Source = source,
                FieldName = "name"
            };

            var result = DictionaryFieldResolver.Instance.Resolve(context);

            result.Should().Be("Jacket");
        }

        [Fact]
        public void Resolve_WhenSourceIsNotADictionary_ReturnsNull()
        {
            var context = new ResolveFieldContext
            {
                Source = new object(),
                FieldName = "name"
            };

            var result = DictionaryFieldResolver.Instance.Resolve(context);

            result.Should().BeNull();
        }
    }
}

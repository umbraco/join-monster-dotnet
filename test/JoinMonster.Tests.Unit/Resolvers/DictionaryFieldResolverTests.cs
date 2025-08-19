using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using GraphQL.Types;
using GraphQLParser.AST;
using JoinMonster.Resolvers;
using Xunit;

namespace JoinMonster.Tests.Unit.Resolvers
{
    public class DictionaryFieldResolverTests
    {
        [Fact]
        public async Task Resolve_WhenSourceIsDictionaryAndFieldExists_ReturnsValue()
        {
            var source = new Dictionary<string, object>
            {
                {"name", "Jacket"}
            };

            var context = new ResolveFieldContext
            {
                Source = source,
                FieldAst = new GraphQLField(),
                FieldDefinition = new FieldType
                {
                    Name = "name"
                }
            };

            var result = await DictionaryFieldResolver.Instance.ResolveAsync(context);

            result.Should().Be("Jacket");
        }
        [Fact]
        public async Task Resolve_WhenFieldAstHasAlias_UsesAliasToResolveValue()
        {
            var source = new Dictionary<string, object>
            {
                {"productName#name", "Jacket"}
            };

            var context = new ResolveFieldContext
            {
                Source = source,
                FieldDefinition = new FieldType
                {
                    Name = "name"
                },
                FieldAst = new GraphQLField
                {
                    Alias = new GraphQLAlias
                    {
                        Name = new GraphQLName("productName")
                    },
                    Name = new GraphQLName("name")
                }
            };

            var result = await DictionaryFieldResolver.Instance.ResolveAsync(context);

            result.Should().Be("Jacket");
        }

        [Fact]
        public async Task Resolve_WhenSourceIsNotADictionary_ReturnsNull()
        {
            var context = new ResolveFieldContext
            {
                Source = new object(),
                FieldDefinition = new FieldType
                {
                    Name = "name"
                }
            };

            var result = await DictionaryFieldResolver.Instance.ResolveAsync(context);

            result.Should().BeNull();
        }
    }
}

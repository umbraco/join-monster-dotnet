using System.Collections.Generic;
using FluentAssertions;
using JoinMonster.Builders;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit.Builders
{
    public class SortKeyBuilderTests
    {
        [Fact]
        public void By_WithSingleKey_SetsColumn()
        {
            var aliasGenerator = new DefaultAliasGenerator();
            var builder = new SortKeyBuilder("products", aliasGenerator);

            builder.By("id");

            builder.SortKey.Should().BeEquivalentTo(new SortKey("products", new Dictionary<string, string> {{ "id", "id"}}, SortDirection.Ascending));
        }

        [Fact]
        public void By_WithMultipleKeys_SetsColumn()
        {
            var aliasGenerator = new DefaultAliasGenerator();
            var builder = new SortKeyBuilder("products", aliasGenerator);

            builder.By(new [] {"sortOrder", "id"});

            builder.SortKey.Should().BeEquivalentTo(new SortKey("products", new Dictionary<string, string> {{"sortOrder", "sortOrder"},{ "id", "id"}}, SortDirection.Ascending));
        }

        [Fact]
        public void ByDescending_WithSingleKey_SetsColumn()
        {
            var aliasGenerator = new DefaultAliasGenerator();
            var builder = new SortKeyBuilder("products", aliasGenerator);

            builder.ByDescending("id");

            builder.SortKey.Should().BeEquivalentTo(new SortKey("products", new Dictionary<string, string> {{ "id", "id"}}, SortDirection.Descending));
        }

        [Fact]
        public void ByDescending_WithMultipleKeys_SetsColumn()
        {
            var aliasGenerator = new DefaultAliasGenerator();
            var builder = new SortKeyBuilder("products", aliasGenerator);

            builder.ByDescending(new [] {"sortOrder", "id"});

            builder.SortKey.Should().BeEquivalentTo(new SortKey("products", new Dictionary<string, string> {{"sortOrder", "sortOrder"},{ "id", "id"}}, SortDirection.Descending));
        }
    }
}

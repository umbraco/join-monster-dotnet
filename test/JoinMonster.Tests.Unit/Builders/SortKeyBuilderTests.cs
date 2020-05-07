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
            var builder = new SortKeyBuilder();
            builder.By("id");
            builder.SortKey.Should().BeEquivalentTo(new SortKey(new [] {"id"}, SortDirection.Ascending));
        }

        [Fact]
        public void By_WithMultipleKeys_SetsColumn()
        {
            var builder = new SortKeyBuilder();
            builder.By(new [] {"sortOrder", "id"});
            builder.SortKey.Should().BeEquivalentTo(new SortKey(new [] {"sortOrder", "id"}, SortDirection.Ascending));
        }

        [Fact]
        public void ByDescending_WithSingleKey_SetsColumn()
        {
            var builder = new SortKeyBuilder();
            builder.ByDescending("id");
            builder.SortKey.Should().BeEquivalentTo(new SortKey(new [] {"id"}, SortDirection.Descending));
        }

        [Fact]
        public void ByDescending_WithMultipleKeys_SetsColumn()
        {
            var builder = new SortKeyBuilder();
            builder.ByDescending(new [] {"sortOrder", "id"});
            builder.SortKey.Should().BeEquivalentTo(new SortKey(new [] {"sortOrder", "id"}, SortDirection.Descending));
        }
    }
}

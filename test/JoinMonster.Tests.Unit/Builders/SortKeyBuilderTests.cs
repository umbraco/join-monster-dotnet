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

            builder.SortKey.Should().BeEquivalentTo(new SortKey("products", "id", "id", SortDirection.Ascending));
        }

        [Fact]
        public void By_WithThenByDescending_SetsThenBy()
        {
            var aliasGenerator = new DefaultAliasGenerator();
            var builder = new SortKeyBuilder("products", aliasGenerator);

            builder.By("sortOrder").ThenByDescending("id");

            builder.SortKey.Should()
                .BeEquivalentTo(
                    new SortKey("products", "sortOrder", "sortOrder", SortDirection.Ascending)
                    {
                        ThenBy = new SortKey("products", "id", "id", SortDirection.Descending)

                    });
        }

        [Fact]
        public void ByDescending_WithSingleKey_SetsColumn()
        {
            var aliasGenerator = new DefaultAliasGenerator();
            var builder = new SortKeyBuilder("products", aliasGenerator);

            builder.ByDescending("id");

            builder.SortKey.Should().BeEquivalentTo(new SortKey("products", "id", "id", SortDirection.Descending));
        }

        [Fact]
        public void ByDescending_WithThenBy_SetsColumn()
        {
            var aliasGenerator = new DefaultAliasGenerator();
            var builder = new SortKeyBuilder("products", aliasGenerator);

            builder.ByDescending("sortOrder").ThenBy("id");

            builder.SortKey.Should().BeEquivalentTo(new SortKey("products", "sortOrder", "sortOrder", SortDirection.Descending)
            {
                ThenBy = new SortKey("products", "id", "id", SortDirection.Ascending)
            });
        }
    }
}

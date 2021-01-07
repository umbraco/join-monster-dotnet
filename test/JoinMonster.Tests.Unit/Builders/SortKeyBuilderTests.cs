using System;
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

            builder.By("id", typeof(Guid));

            builder.SortKey.Should().BeEquivalentTo(new SortKey("products", "id", "id", typeof(Guid), SortDirection.Ascending));
        }

        [Fact]
        public void By_WithThenByDescending_SetsThenBy()
        {
            var aliasGenerator = new DefaultAliasGenerator();
            var builder = new SortKeyBuilder("products", aliasGenerator);

            builder.By("sortOrder", typeof(int)).ThenByDescending("id", typeof(Guid));

            builder.SortKey.Should()
                .BeEquivalentTo(
                    new SortKey("products", "sortOrder", "sortOrder", typeof(int), SortDirection.Ascending)
                    {
                        ThenBy = new SortKey("products", "id", "id", typeof(Guid), SortDirection.Descending)
                    });
        }

        [Fact]
        public void ByDescending_WithSingleKey_SetsColumn()
        {
            var aliasGenerator = new DefaultAliasGenerator();
            var builder = new SortKeyBuilder("products", aliasGenerator);

            builder.ByDescending("id", typeof(Guid));

            builder.SortKey.Should().BeEquivalentTo(new SortKey("products", "id", "id", typeof(Guid), SortDirection.Descending));
        }

        [Fact]
        public void ByDescending_WithThenBy_SetsColumn()
        {
            var aliasGenerator = new DefaultAliasGenerator();
            var builder = new SortKeyBuilder("products", aliasGenerator);

            builder.ByDescending("sortOrder", typeof(int)).ThenBy("id", typeof(Guid));

            builder.SortKey.Should().BeEquivalentTo(new SortKey("products", "sortOrder", "sortOrder", typeof(int), SortDirection.Descending)
            {
                ThenBy = new SortKey("products", "id", "id", typeof(Guid), SortDirection.Ascending)
            });
        }
    }
}

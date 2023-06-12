using System;
using FluentAssertions;
using JoinMonster.Builders;
using Xunit;

namespace JoinMonster.Tests.Unit.Builders
{
    public class SqlBatchConfigBuilderTests
    {
        [Fact]
        public void Create_WhenThisKeyIsNull_ThrowsException()
        {
            Action action = () => SqlBatchConfigBuilder.Create(null, null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("thisKey");
        }

        [Fact]
        public void Create_WhenParentKeyIsNull_ThrowsException()
        {
            Action action = () => SqlBatchConfigBuilder.Create("friend_id", null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("parentKey");
        }

        [Fact]
        public void Create_WithThisKey_SetsThisKey()
        {
            var thisKey = "friend_id";
            var builder = SqlBatchConfigBuilder.Create(thisKey, "id", typeof(Guid));

            builder.SqlBatchConfig.ThisKey.Should().Be(thisKey);
        }

        [Fact]
        public void Create_WithParentKey_SetsParentKey()
        {
            var parentKey = "id";
            var builder = SqlBatchConfigBuilder.Create("friend_id", parentKey, typeof(Guid));

            builder.SqlBatchConfig.ParentKey.Should().Be(parentKey);
        }
    }
}

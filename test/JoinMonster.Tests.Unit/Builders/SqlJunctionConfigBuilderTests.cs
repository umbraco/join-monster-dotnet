using System;
using System.Collections.Generic;
using FluentAssertions;
using GraphQL;
using GraphQL.Execution;
using JoinMonster.Builders;
using JoinMonster.Configs;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit.Builders
{
    public class SqlJunctionConfigBuilderTests
    {
        [Fact]
        public void Create_WhenTableNameIsNull_ThrowsException()
        {
            Action action = () => SqlJunctionConfigBuilder.Create(null, null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("tableName");
        }

        [Fact]
        public void Create_WhenFromParentIsNull_ThrowsException()
        {
            Action action = () => SqlJunctionConfigBuilder.Create("friends", null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("fromParent");
        }

        [Fact]
        public void Create_WhenToChildIsNull_ThrowsException()
        {
            Action action = () => SqlJunctionConfigBuilder.Create("friends", (_, __, ___, ____) => { }, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("toChild");
        }

        [Fact]
        public void Create_WithTableName_SetsTableName()
        {
            var tableName = "friends";
            var builder =
                SqlJunctionConfigBuilder.Create(tableName, (_, __, ___, ____) => { }, (_, __, ___, ____) => { });

            builder.SqlJunctionConfig.Table.Should().Be(tableName);
        }

        [Fact]
        public void Create_WithFromParent_SetsFromParent()
        {
            void FromParent(JoinBuilder join, IReadOnlyDictionary<string, ArgumentValue> arguments,
                IResolveFieldContext context, Node sqlAstNode)
            {
            }

            var builder =
                SqlJunctionConfigBuilder.Create("friends", FromParent, (_, __, ___, ____) => { });

            builder.SqlJunctionConfig.FromParent.Should().Be((JoinDelegate) FromParent);
        }

        [Fact]
        public void Create_WithToChild_SetsToChild()
        {
            void ToChild(JoinBuilder join, IReadOnlyDictionary<string, ArgumentValue> arguments,
                IResolveFieldContext context, Node sqlAstNode)
            {
            }

            var builder =
                SqlJunctionConfigBuilder.Create("friends", (_, __, ___, ____) => { }, ToChild);

            builder.SqlJunctionConfig.ToChild.Should().Be((JoinDelegate) ToChild);
        }

        [Fact]
        public void Where_WithWhereCondition_SetsWhere()
        {
            void Where(WhereBuilder where, IReadOnlyDictionary<string, ArgumentValue> arguments,
                IResolveFieldContext context, SqlTable sqlAStNode)
            {
            }

            var builder = SqlJunctionConfigBuilder.Create("friends", (_, __, ___, ____) => { }, (_, __, ___, ____) => { });

            builder.Where(Where);

            builder.SqlJunctionConfig.Where.Should().Be((WhereDelegate) Where);
        }

        [Fact]
        public void Create_WithBatchConfiguration_SetsBatchConfig()
        {
            void Join(JoinBuilder join, IReadOnlyDictionary<string, ArgumentValue> _, IResolveFieldContext __, SqlTable ___)
            {
            }

            var builder = SqlJunctionConfigBuilder.Create("friends", new[] {"characterId", "friendId"}, "friendId", "id", Join);

            builder.SqlJunctionConfig.BatchConfig.Should()
                .BeEquivalentTo(new SqlBatchConfig("friendId", "id")
                {
                    Join = Join
                });
        }
    }
}

using System;
using System.Collections.Generic;
using FluentAssertions;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Execution;
using GraphQL.Types;
using JoinMonster.Builders;
using JoinMonster.Configs;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit
{
    public class ConnectionBuilderExtensionsTests
    {
        [Fact]
        public void SqlBatch_WhenBuilderIsNull_ThrowsException()
        {
            Action action = () => ConnectionBuilderExtensions.SqlBatch<ObjectGraphType>(null, null, null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("connectionBuilder");
        }

        [Fact]
        public void SqlBatch_WithThisKeyAndParentKey_SetsBatchOnFieldType()
        {
            var builder = ConnectionBuilder.Create<ObjectGraphType, object>().SqlBatch("parentId", "id", typeof(Guid));

            builder.FieldType.GetSqlBatch().Should().NotBeNull();
        }

        [Fact]
        public void SqlWhere_WhenBuilderIsNull_ThrowsException()
        {
            Action action = () => ConnectionBuilderExtensions.SqlWhere<ObjectGraphType>(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("connectionBuilder");
        }

        [Fact]
        public void SqlWhere_WhenQueryIsNull_ThrowsException()
        {
            Action action = () => ConnectionBuilder.Create<ObjectGraphType, object>().SqlWhere(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("where");
        }

        [Fact]
        public void SqlWhere_WithQuery_SetsQueryOnFieldType()
        {
            void Where(WhereBuilder where, IReadOnlyDictionary<string, ArgumentValue> arguments,
                IResolveFieldContext context, SqlTable sqlAStNode)
            {
            }

            var builder = ConnectionBuilder.Create<ObjectGraphType, object>().SqlWhere(Where);

            builder.FieldType.GetSqlWhere().Should().Be((WhereDelegate)Where);
        }

        [Fact]
        public void SqlOrder_WhenBuilderIsNull_ThrowsException()
        {
            Action action = () => ConnectionBuilderExtensions.SqlOrder<ObjectGraphType>(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("connectionBuilder");
        }

        [Fact]
        public void SqlOrder_WhenOrderIsNull_ThrowsException()
        {
            Action action = () => ConnectionBuilder.Create<ObjectGraphType, object>().SqlOrder(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("orderBy");
        }

        [Fact]
        public void SqlOrder_WithOrder_SetsOrderOnFieldType()
        {
            void Order(OrderByBuilder order, IReadOnlyDictionary<string, ArgumentValue> arguments,
                IResolveFieldContext context, SqlTable sqlTable) => order.By("id");

            var builder = ConnectionBuilder.Create<ObjectGraphType, object>().SqlOrder(Order);

            builder.FieldType.GetSqlOrder().Should().Be((OrderByDelegate)Order);
        }

        [Fact]
        public void SqlJoin_WhenBuilderIsNull_ThrowsException()
        {
            Action action = () => ConnectionBuilderExtensions.SqlJoin<ObjectGraphType>(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("connectionBuilder");
        }

        [Fact]
        public void SqlJoin_WhenJoinIsNull_ThrowsException()
        {
            Action action = () => ConnectionBuilder.Create<ObjectGraphType, object>().SqlJoin(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("join");
        }

        [Fact]
        public void SqlJoin_WithJoin_SetsJoinOnFieldType()
        {
            void Join(JoinBuilder join, IReadOnlyDictionary<string, ArgumentValue> arguments,
                IResolveFieldContext context, Node sqlAstNode)
            {
            }

            var builder = ConnectionBuilder.Create<ObjectGraphType, object>().SqlJoin(Join);

            builder.FieldType.GetSqlJoin().Should().Be((JoinDelegate)Join);
        }

        [Fact]
        public void SqlPaginate_WhenBuilderIsNull_ThrowsException()
        {
            Action action = () => ConnectionBuilderExtensions.SqlPaginate<ObjectGraphType>(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("connectionBuilder");
        }

        [Fact]
        public void SqlPaginate_WhenCalled_SetsPaginateOnFieldType()
        {
            var builder = ConnectionBuilder.Create<ObjectGraphType, object>().SqlPaginate(true);

            builder.FieldType.GetSqlPaginate().Should().BeTrue();
        }
    }
}

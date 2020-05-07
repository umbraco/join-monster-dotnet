using System;
using System.Collections.Generic;
using FluentAssertions;
using GraphQL.Builders;
using GraphQL.Types;
using JoinMonster.Builders;
using JoinMonster.Configs;
using Xunit;

namespace JoinMonster.Tests.Unit
{
    public class ConnectionExtensionsTests
    {
        [Fact]
        public void SqlWhere_WhenBuilderIsNull_ThrowsException()
        {
            Action action = () => ConnectionExtensions.SqlWhere<ObjectGraphType>(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("builder");
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

            string Where(string tableAlias, IReadOnlyDictionary<string, object> arguments,
                IDictionary<string, object> userContext) => "";

            var builder = ConnectionBuilder.Create<ObjectGraphType, object>().SqlWhere(Where);

            builder.FieldType.GetSqlWhere().Should().Be((WhereDelegate) Where);
        }

        [Fact]
        public void SqlOrder_WhenBuilderIsNull_ThrowsException()
        {
            Action action = () => ConnectionExtensions.SqlOrder<ObjectGraphType>(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("builder");
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
            void Order(OrderByBuilder order, IReadOnlyDictionary<string, object> arguments,
                IDictionary<string, object> userContext) => order.By("id");

            var builder = ConnectionBuilder.Create<ObjectGraphType, object>().SqlOrder(Order);

            builder.FieldType.GetSqlOrder().Should().Be((OrderByDelegate) Order);
        }

        [Fact]
        public void SqlJoin_WhenBuilderIsNull_ThrowsException()
        {
            Action action = () => ConnectionExtensions.SqlJoin<ObjectGraphType>(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("builder");
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
            string Join(string parentTable, string childTable, IReadOnlyDictionary<string, object> arguments,
                IDictionary<string, object> userContext) => "";

            var builder = ConnectionBuilder.Create<ObjectGraphType, object>().SqlJoin(Join);

            builder.FieldType.GetSqlJoin().Should().Be((JoinDelegate) Join);
        }

        [Fact]
        public void SqlPaginate_WhenBuilderIsNull_ThrowsException()
        {
            Action action = () => ConnectionExtensions.SqlPaginate<ObjectGraphType>(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("builder");
        }

        [Fact]
        public void SqlPaginate_WhenCalled_SetsPaginateOnFieldType()
        {
            var builder = ConnectionBuilder.Create<ObjectGraphType, object>().SqlPaginate(true);

            builder.FieldType.GetSqlPaginate().Should().BeTrue();
        }
    }
}

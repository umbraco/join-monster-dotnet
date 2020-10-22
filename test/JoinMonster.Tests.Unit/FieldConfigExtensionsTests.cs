using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Utilities;
using JoinMonster.Builders;
using JoinMonster.Configs;
using JoinMonster.Language.AST;
using JoinMonster.Resolvers;
using Xunit;

namespace JoinMonster.Tests.Unit
{
    public class FieldConfigExtensionsTests
    {
        [Fact]
        public void SqlColumn_WhenFieldConfigIsNull_ThrowsException()
        {
            Action action = () => FieldConfigExtensions.SqlColumn(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("fieldConfig");
        }

        [Fact]
        public void SqlColumn_WhenColumnIsNull_UsesFieldNameAsColumnName()
        {
            var fieldConfig = new FieldConfig("name");

            var builder = fieldConfig.SqlColumn();

            builder.SqlColumnConfig.Column.Should().Be("name");
        }

        [Fact]
        public void SqlColumn_WithColumnName_SetsColumnName()
        {
            var fieldConfig = new FieldConfig("name");

            var builder = fieldConfig.SqlColumn("productName");

            builder.SqlColumnConfig.Column.Should().Be("productName");
        }

        [Fact]
        public void SqlColumn_WhenFieldResolverIsNull_SetsResolver()
        {
            var fieldConfig = new FieldConfig("name");

            fieldConfig.SqlColumn("productName");

            fieldConfig.Resolver.Should().Be(DictionaryFieldResolver.Instance);
        }

        [Fact]
        public void SqlColumn_WhenFieldResolverIsNotNull_DoesntSetResolver()
        {
            var resolver = new FuncFieldResolver<string>(_ => "");
            var fieldConfig = new FieldConfig("name")
            {
                Resolver = resolver
            };

            fieldConfig.SqlColumn("productName");

            fieldConfig.Resolver.Should().Be(resolver);
        }

        [Fact]
        public void SqlColumn_WhenColumnIsIgnored_DoesntSetResolver()
        {
            var fieldConfig = new FieldConfig("name");

            fieldConfig.SqlColumn(ignore: true);

            fieldConfig.Resolver.Should().BeNull();
        }

        [Fact]
        public void SqlWhere_WhenFieldConfigIsNull_ThrowsException()
        {
            Action action = () => FieldConfigExtensions.SqlWhere(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("fieldConfig");
        }

        [Fact]
        public void SqlWhere_WhenWhereDelegateIsNull_ThrowsException()
        {
            var fieldConfig = new FieldConfig("name");
            Action action = () => fieldConfig.SqlWhere(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("where");
        }

        [Fact]
        public void SqlWhere_WithWhereDelegate_AddsWhereDelegateToMetadata()
        {
            void Where(WhereBuilder where, IReadOnlyDictionary<string, object> arguments,
                IResolveFieldContext context, SqlTable sqlAStNode) => where.Column("id", 3);

            var fieldConfig = new FieldConfig("name");

            fieldConfig.SqlWhere(Where);

            fieldConfig.GetMetadata<WhereDelegate>(nameof(WhereDelegate))
                .Should()
                .Be((WhereDelegate) Where);
        }

        [Fact]
        public void SqlJoin_WhenFieldConfigIsNull_ThrowsException()
        {
            Action action = () => FieldConfigExtensions.SqlJoin(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("fieldConfig");
        }

        [Fact]
        public void SqlJoin_WhenJoinDelegateIsNull_ThrowsException()
        {
            var fieldConfig = new FieldConfig("name");
            Action action = () => fieldConfig.SqlJoin(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("join");
        }

        [Fact]
        public void SqlJoin_WithJoinDelegate_AddsJoinDelegateToMetadata()
        {
            void Join(JoinBuilder join, IReadOnlyDictionary<string, object> arguments,
                IResolveFieldContext context, Node sqlAstNode) => join.On("id", "parentId");

            var fieldConfig = new FieldConfig("name");

            fieldConfig.SqlJoin(Join);

            fieldConfig.GetMetadata<JoinDelegate>(nameof(JoinDelegate))
                .Should()
                .Be((JoinDelegate) Join);
        }

        [Fact]
        public void SqlJoin_WhenFieldResolverIsNull_SetsResolver()
        {
            var fieldConfig = new FieldConfig("name");

            fieldConfig.SqlJoin((join, arguments, context, node) => {});

            fieldConfig.Resolver.Should().Be(DictionaryFieldResolver.Instance);
        }
        [Fact]
        public void SqlJoin_WhenFieldResolverIsNotNull_DoesntSetResolver()
        {
            var resolver = new FuncFieldResolver<string>(_ => "");
            var fieldConfig = new FieldConfig("name")
            {
                Resolver = resolver
            };

            fieldConfig.SqlJoin((join, arguments, context, node) => {});

            fieldConfig.Resolver.Should().Be(resolver);
        }

        [Fact]
        public void SqlOrder_WhenOrderByDelegateIsNull_ThrowsException()
        {
            var fieldConfig = new FieldConfig("name");
            Action action = () => fieldConfig.SqlOrder(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("orderBy");
        }

        [Fact]
        public void SqlOrder_WithOrderByDelegate_AddsOrderByDelegateToMetadata()
        {
            void OrderBy(OrderByBuilder order, IReadOnlyDictionary<string, object> arguments,
                IResolveFieldContext context, SqlTable sqlTable) => order.By("name");

            var fieldConfig = new FieldConfig("name");

            fieldConfig.SqlOrder(OrderBy);

            fieldConfig.GetMetadata<OrderByDelegate>(nameof(OrderByDelegate))
                .Should()
                .Be((OrderByDelegate) OrderBy);
        }

        [Fact]
        public void SqlJunction_WhenFieldResolverIsNull_SetsResolver()
        {
            var fieldConfig = new FieldConfig("name");

            fieldConfig.SqlJunction("", (join, arguments, context, node) => { },
                (join, arguments, context, node) => { });

            fieldConfig.Resolver.Should().Be(DictionaryFieldResolver.Instance);
        }

        [Fact]
        public void SqlJunction_WhenFieldResolverIsNotNull_DoesntSetResolver()
        {
            var resolver = new FuncFieldResolver<string>(_ => "");
            var fieldConfig = new FieldConfig("name")
            {
                Resolver = resolver
            };

            fieldConfig.SqlJunction("", (join, arguments, context, node) => { },
                (join, arguments, context, node) => { });

            fieldConfig.Resolver.Should().Be(resolver);
        }
    }
}

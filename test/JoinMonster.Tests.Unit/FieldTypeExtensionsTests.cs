using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using JoinMonster.Builders;
using JoinMonster.Configs;
using JoinMonster.Language.AST;
using JoinMonster.Resolvers;
using Xunit;

namespace JoinMonster.Tests.Unit
{
    public class FieldTypeExtensionsTests
    {
        [Fact]
        public void SqlColumn_WhenFieldTypeIsNull_ThrowsException()
        {
            Action action = () => FieldTypeExtensions.SqlColumn(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("fieldType");
        }

        [Fact]
        public void SqlColumn_WhenColumnIsNull_UsesFieldNameAsColumnName()
        {
            var fieldType = new FieldType
            {
                Name = "name"
            };

            var builder = fieldType.SqlColumn();

            builder.SqlColumnConfig.Column.Should().Be("name");
        }

        [Fact]
        public void SqlColumn_WithColumnName_SetsColumnName()
        {
            var fieldType = new FieldType
            {
                Name = "name"
            };

            var builder = fieldType.SqlColumn("productName");

            builder.SqlColumnConfig.Column.Should().Be("productName");
        }

        [Fact]
        public void SqlColumn_WhenFieldResolverIsNull_SetsResolver()
        {
            var fieldType = new FieldType
            {
                Name = "name"
            };

            fieldType.SqlColumn("productName");

            fieldType.Resolver.Should().Be(DictionaryFieldResolver.Instance);
        }

        [Fact]
        public void SqlColumn_WhenFieldResolverIsNotNull_DoesntSetResolver()
        {
            var resolver = new FuncFieldResolver<string>(_ => "");
            var fieldType = new FieldType
            {
                Name = "name",
                Resolver = resolver
            };

            fieldType.SqlColumn("productName");

            fieldType.Resolver.Should().Be(resolver);
        }

        [Fact]
        public void SqlColumn_WhenColumnIsIgnored_DoesntSetResolver()
        {
            var fieldType = new FieldType
            {
                Name = "name",
            };

            fieldType.SqlColumn(ignore: true);

            fieldType.Resolver.Should().BeNull();
        }

        [Fact]
        public void GetSqlColumnConfig_WhenFieldTypeIsNull_ThrowsException()
        {
            Action action = () => FieldTypeExtensions.GetSqlColumnConfig(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("fieldType");
        }

        [Fact]
        public void GetSqlColumnConfig_WhenColumnConfigHasNotBeenSet_ReturnsNull()
        {
            var fieldType = new FieldType();

            var config = fieldType.GetSqlColumnConfig();

            config.Should().BeNull();
        }

        [Fact]
        public void GetSqlColumnConfig_WhenColumnConfigHasBeenSet_ReturnsColumnConfig()
        {
            var fieldType = new FieldType();
            var builder = fieldType.SqlColumn("name");

            var config = fieldType.GetSqlColumnConfig();

            config.Should().Be(builder.SqlColumnConfig);
        }

        [Fact]
        public void SqlWhere_WhenFieldTypeIsNull_ThrowsException()
        {
            Action action = () => FieldTypeExtensions.SqlWhere(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("fieldType");
        }

        [Fact]
        public void SqlWhere_WhenWhereDelegateIsNull_ThrowsException()
        {
            var fieldType = new FieldType();
            Action action = () => fieldType.SqlWhere(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("where");
        }

        [Fact]
        public void GetSqlWhere_WhenFieldTypeIsNull_ThrowsException()
        {
            Action action = () => FieldTypeExtensions.GetSqlWhere(null);

            action.Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be("fieldType");
        }

        [Fact]
        public void GetSqlWhere_WhenWhereDelegateHasNotBeenSet_ReturnsNull()
        {
            var fieldType = new FieldType();

            var whereDelegate = fieldType.GetSqlWhere();

            whereDelegate.Should().BeNull();
        }

        [Fact]
        public void GetSqlWhere_WhenWhereDelegateHasBeenSet_ReturnsWhereDelegate()
        {
            void Where(WhereBuilder where, IReadOnlyDictionary<string, object> arguments,
                IResolveFieldContext context, SqlTable sqlAStNode)
            {
            }

            var fieldType = new FieldType();
            fieldType.SqlWhere(Where);

            var whereDelegate = fieldType.GetSqlWhere();

            whereDelegate.Should().Be((WhereDelegate) Where);
        }

        [Fact]
        public void SqlJoin_WhenFieldTypeIsNull_ThrowsException()
        {
            Action action = () => FieldTypeExtensions.SqlJoin(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("fieldType");
        }

        [Fact]
        public void SqlJoin_WhenJoinDelegateIsNull_ThrowsException()
        {
            var fieldType = new FieldType();
            Action action = () => fieldType.SqlJoin(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("join");
        }

        [Fact]
        public void SqlJoin_WhenFieldResolverIsNull_SetsResolver()
        {
            var fieldType = new FieldType
            {
                Name = "name"
            };

            fieldType.SqlJoin((join, arguments, context, node) => {});

            fieldType.Resolver.Should().Be(DictionaryFieldResolver.Instance);
        }

        [Fact]
        public void SqlJoin_WhenFieldResolverIsNotNull_DoesntSetResolver()
        {
            var resolver = new FuncFieldResolver<string>(_ => "");
            var fieldType = new FieldType
            {
                Name = "name",
                Resolver = resolver
            };

            fieldType.SqlJoin((join, arguments, context, node) => {});

            fieldType.Resolver.Should().Be(resolver);
        }

        [Fact]
        public void GetSqlJoin_WhenFieldTypeIsNull_ThrowsException()
        {
            Action action = () => FieldTypeExtensions.GetSqlJoin(null);

            action.Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be("fieldType");
        }

        [Fact]
        public void GetSqlJoin_WhenJoinDelegateHasNotBeenSet_ReturnsNull()
        {
            var fieldType = new FieldType();

            var joinDelegate = fieldType.GetSqlJoin();

            joinDelegate.Should().BeNull();
        }

        [Fact]
        public void GetSqlJoin_WhenJoinDelegateHasBeenSet_ReturnsJoinDelegate()
        {
            void Join(JoinBuilder join, IReadOnlyDictionary<string, object> arguments,
                IResolveFieldContext context, Node sqlAstNode) => join.On("id", "parentId");

            var fieldType = new FieldType();
            fieldType.SqlJoin(Join);

            var joinDelegate = fieldType.GetSqlJoin();

            joinDelegate.Should().Be((JoinDelegate) Join);
        }

        [Fact]
        public void SqlOrder_WhenOrderByDelegateIsNull_ThrowsException()
        {
            var fieldType = new FieldType();
            Action action = () => fieldType.SqlOrder(null);

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

            var fieldType = new FieldType();
            fieldType.SqlOrder(OrderBy);

            var orderBy = fieldType.GetSqlOrder();

            orderBy.Should()
                .Be((OrderByDelegate) OrderBy);
        }

        [Fact]
        public void SqlJunction_WhenFieldResolverIsNull_SetsResolver()
        {
            var fieldType = new FieldType
            {
                Name = "name"
            };

            fieldType.SqlJunction("", (join, arguments, context, node) => { },
                (join, arguments, context, node) => { });

            fieldType.Resolver.Should().Be(DictionaryFieldResolver.Instance);
        }

        [Fact]
        public void SqlJunction_WhenFieldResolverIsNotNull_DoesntSetResolver()
        {
            var resolver = new FuncFieldResolver<string>(_ => "");
            var fieldType = new FieldType
            {
                Name = "name",
                Resolver = resolver
            };

            fieldType.SqlJunction("", (join, arguments, context, node) => { },
                (join, arguments, context, node) => { });

            fieldType.Resolver.Should().Be(resolver);
        }
    }
}

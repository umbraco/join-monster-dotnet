using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL.Utilities;
using JoinMonster.Configs;
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
            string Where(string tableAlias, IDictionary<string, object> arguments,
                IDictionary<string, object> userContext) => $"{tableAlias}.\"id\" = 3";

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
            string Join(string parentTable, string childTable, IDictionary<string, object> arguments,
                IDictionary<string, object> userContext) => $"{parentTable}.\"id\" = ${childTable}.\"parentId\"";

            var fieldConfig = new FieldConfig("name");

            fieldConfig.SqlJoin(Join);

            fieldConfig.GetMetadata<JoinDelegate>(nameof(JoinDelegate))
                .Should()
                .Be((JoinDelegate) Join);
        }
    }
}

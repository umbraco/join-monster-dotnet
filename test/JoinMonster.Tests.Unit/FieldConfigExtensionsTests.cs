using System;
using FluentAssertions;
using GraphQL.Utilities;
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
    }
}

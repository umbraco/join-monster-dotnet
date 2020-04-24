using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL.Types;
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
            Task<string> Where(string tableAlias, IDictionary<string, object> arguments,
                IDictionary<string, object> userContext) => Task.FromResult<string>(null);

            var fieldType = new FieldType();
            fieldType.SqlWhere(Where);

            var whereDelegate = fieldType.GetSqlWhere();

            whereDelegate.Should().Be((WhereDelegate) Where);
        }
    }
}

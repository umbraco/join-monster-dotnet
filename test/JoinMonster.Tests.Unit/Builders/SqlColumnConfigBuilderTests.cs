using System.Collections.Generic;
using FluentAssertions;
using JoinMonster.Builders;
using Xunit;

namespace JoinMonster.Tests.Unit.Builders
{
    public class SqlColumnConfigBuilderTests
    {
        [Fact]
        public void Create_WithNullColumnName_SetsColumnToNull()
        {
            var builder = SqlColumnConfigBuilder.Create();

            builder.SqlColumnConfig.Column.Should().BeNull();
        }

        [Fact]
        public void Create_WithColumnName_SetsColumnToValue()
        {
            var columnName = "myColumn";
            var builder = SqlColumnConfigBuilder.Create(columnName);

            builder.SqlColumnConfig.Column.Should().Be(columnName);
        }

        [Fact]
        public void Name_WithValue_SetsColumnName()
        {
            var columnName = "myColumn";
            var builder = SqlColumnConfigBuilder.Create()
                .Name(columnName);

            builder.SqlColumnConfig.Column.Should().Be(columnName);
        }

        [Fact]
        public void Ignore_WithTrue_SetsIgnoredToTrue()
        {
            var builder = SqlColumnConfigBuilder.Create()
                .Ignore(true);

            builder.SqlColumnConfig.Ignored.Should().BeTrue();
        }

        [Fact]
        public void Ignore_WithFalse_SetsIgnoredToFalse()
        {
            var builder = SqlColumnConfigBuilder.Create()
                .Ignore(false);

            builder.SqlColumnConfig.Ignored.Should().BeFalse();
        }

        [Fact]
        public void Dependencies_WithValues_SetsDependencies()
        {
            var dependencies = new[] {"firstName", "lastName"};
            var builder = SqlColumnConfigBuilder.Create()
                .Dependencies(dependencies);

            builder.SqlColumnConfig.Dependencies.Should().BeEquivalentTo(dependencies);
        }

        [Fact]
        public void Expression_WithExpression_SetsExpression()
        {
            string Expression(string tableAlias, IDictionary<string, object> userContext) => $"{tableAlias}.firstName || ' ' {tableAlias}.lastName";

            var builder = SqlColumnConfigBuilder.Create()
                .Expression(Expression);

            builder.SqlColumnConfig.Expression.Should().BeEquivalentTo((ExpressionDelegate) Expression);
        }
    }
}

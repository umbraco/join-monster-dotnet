using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using JoinMonster.Builders;
using Xunit;

namespace JoinMonster.Tests.Unit.Builders
{
    public class SqlColumnConfigBuilderTests
    {
        [Fact]
        public void Create_WithNullColumnName_ThrowsException()
        {
            Action action = () => SqlColumnConfigBuilder.Create(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("columnName");
        }

        [Fact]
        public void Create_WithColumnName_SetsColumnToValue()
        {
            var columnName = "myColumn";
            var builder = SqlColumnConfigBuilder.Create(columnName);

            builder.SqlColumnConfig.Column.Should().Be(columnName);
        }

        [Fact]
        public void Ignore_WithTrue_SetsIgnoredToTrue()
        {
            var builder = SqlColumnConfigBuilder.Create("myColumn")
                .Ignore(true);

            builder.SqlColumnConfig.Ignored.Should().BeTrue();
        }

        [Fact]
        public void Ignore_WithFalse_SetsIgnoredToFalse()
        {
            var builder = SqlColumnConfigBuilder.Create("myColumn")
                .Ignore(false);

            builder.SqlColumnConfig.Ignored.Should().BeFalse();
        }

        [Fact]
        public void Dependencies_WithValues_SetsDependencies()
        {
            var dependencies = new[] {"firstName", "lastName"};
            var builder = SqlColumnConfigBuilder.Create("myColumn")
                .Dependencies(dependencies);

            builder.SqlColumnConfig.Dependencies.Should().BeEquivalentTo(dependencies);
        }

        [Fact]
        public void Expression_WithExpression_SetsExpression()
        {
            Task<string> Expression(string tableAlias, IDictionary<string, object> userContext) => Task.FromResult($"{tableAlias}.firstName || ' ' {tableAlias}.lastName");

            var builder = SqlColumnConfigBuilder.Create("myColumn")
                .Expression(Expression);

            builder.SqlColumnConfig.Expression.Should().BeEquivalentTo((ExpressionDelegate) Expression);
        }
    }
}

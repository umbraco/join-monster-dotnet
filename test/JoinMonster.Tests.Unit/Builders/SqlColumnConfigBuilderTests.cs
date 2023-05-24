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
            string Expression(string tableAlias, IReadOnlyDictionary<string, ArgumentValue> args,
                IResolveFieldContext context, SqlTable sqlAstNode) => $"{tableAlias}.\"firstName\" || ' ' {tableAlias}.\"lastName\"";

            var builder = SqlColumnConfigBuilder.Create("myColumn")
                .Expression(Expression);

            builder.SqlColumnConfig.Expression.Should().BeEquivalentTo((ExpressionDelegate) Expression);
        }
    }
}

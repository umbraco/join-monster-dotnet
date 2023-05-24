using System;
using FluentAssertions;
using JoinMonster.Exceptions;
using JoinMonster.Language.AST;
using Xunit;

namespace JoinMonster.Tests.Unit
{
    public class SqlAstValidatorTests
    {
        [Fact]
        public void Validate_WhenNodeIsNull_ThrowsException()
        {
            var validator = new SqlAstValidator();

            Action action = () => validator.Validate(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("node");
        }

        [Fact]
        public void Validate_WhenNodeIsSqlTableWithAJoin_ThrowsException()
        {
            var validator = new SqlAstValidator();

            var node = new SqlTable(null, null, null, null, null, null, false)
            {
                Join = (join, arguments, context, sqlAstNode) => {}
            };

            Action action = () => validator.Validate(node);

            action.Should()
                .Throw<JoinMonsterException>()
                .Which.Message.Should()
                .Be("Root level field cannot have 'SqlJoin'.");
        }

        [Fact]
        public void Validate_WhenNodeNotIsSqlTable_ThrowsException()
        {
            var validator = new SqlAstValidator();

            var node = new SqlNoop();

            Action action = () => validator.Validate(node);

            action.Should()
                .Throw<JoinMonsterException>()
                .Which.Message.Should()
                .Be($"Expected node to be of type '{typeof(SqlTable)}' but was '{typeof(SqlNoop)}'.");
        }

        [Fact]
        public void Validate_WhenNodeIsSqlTable_DoesNotThrowException()
        {
            var validator = new SqlAstValidator();

            var node = new SqlTable(null, null, null, null, null, null, false);

            Action action = () => validator.Validate(node);

            action.Should().NotThrow();
        }
    }
}

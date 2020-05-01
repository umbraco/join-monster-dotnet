using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL.Types;
using JoinMonster.Data;
using JoinMonster.Language;
using JoinMonster.Tests.Unit.Data;
using JoinMonster.Tests.Unit.Stubs;
using Xunit;

namespace JoinMonster.Tests.Unit
{
    public class JoinMonsterExecuterTests
    {
        [Fact]
        public void Ctor_WhenConverterIsNull_ThrowsException()
        {
            Action action = () => new JoinMonsterExecutor(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("converter");
        }

        [Fact]
        public void Ctor_WhenCompilerIsNull_ThrowsException()
        {
            Action action = () => new JoinMonsterExecutor(new QueryToSqlConverterStub(), null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("compiler");
        }

        [Fact]
        public void Execute_WhenContextIsNull_ThrowsException()
        {
            var sut = new JoinMonsterExecutor(new QueryToSqlConverterStub(), new SqlCompilerStub());

            Func<Task> action = () => sut.ExecuteAsync(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("context");
        }

        [Fact]
        public void Execute_WhenDatabaseCallIsNull_ThrowsException()
        {
            var sut = new JoinMonsterExecutor(new QueryToSqlConverterStub(), new SqlCompilerStub());

            Func<Task> action = () => sut.ExecuteAsync(new ResolveFieldContext(), null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("databaseCall");
        }
    }
}

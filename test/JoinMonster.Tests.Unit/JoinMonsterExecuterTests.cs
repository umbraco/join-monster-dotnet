using System;
using System.Threading.Tasks;
using FluentAssertions;
using JoinMonster.Data;
using GraphQL;
using JoinMonster.Tests.Unit.Stubs;
using NestHydration;
using Xunit;

namespace JoinMonster.Tests.Unit
{
    public class JoinMonsterExecuterTests
    {
        [Fact]
        public void Ctor_WhenConverterIsNull_ThrowsException()
        {
            Action action = () => new JoinMonsterExecutor(null, null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("converter");
        }

        [Fact]
        public void Ctor_WhenCompilerIsNull_ThrowsException()
        {
            Action action = () => new JoinMonsterExecutor(new QueryToSqlConverterStub(), null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("compiler");
        }

        [Fact]
        public void Ctor_WhenHydratorIsNull_ThrowsException()
        {
            Action action = () => new JoinMonsterExecutor(new QueryToSqlConverterStub(), new SqlCompilerStub(), null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("hydrator");
        }

        [Fact]
        public void Execute_WhenContextIsNull_ThrowsException()
        {
            var sut = new JoinMonsterExecutor(new QueryToSqlConverterStub(), new SqlCompilerStub(), new Hydrator());

            Func<Task> action = () => sut.ExecuteAsync(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("context");
        }

        [Fact]
        public void Execute_WhenDatabaseCallIsNull_ThrowsException()
        {
            var sut = new JoinMonsterExecutor(new QueryToSqlConverterStub(), new SqlCompilerStub(), new Hydrator());

            Func<Task> action = () => sut.ExecuteAsync(new ResolveFieldContext(), null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("databaseCall");
        }
    }
}

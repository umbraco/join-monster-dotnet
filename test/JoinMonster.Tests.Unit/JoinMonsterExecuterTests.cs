using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
            Action action = () => new JoinMonsterExecuter(null, null, null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("converter");
        }

        [Fact]
        public void Ctor_WhenCompilerIsNull_ThrowsException()
        {
            Action action = () => new JoinMonsterExecuter(new QueryToSqlConverterStub(), null, null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("compiler");
        }

        [Fact]
        public void Ctor_WhenBatchPlannerIsNull_ThrowsException()
        {
            Action action = () => new JoinMonsterExecuter(new QueryToSqlConverterStub(), new SqlCompilerStub(), null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("batchPlanner");
        }

        [Fact]
        public void Ctor_WhenHydratorIsNull_ThrowsException()
        {
            Action action = () => new JoinMonsterExecuter(new QueryToSqlConverterStub(), new SqlCompilerStub(), new BatchPlannerStub(), null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("hydrator");
        }

        [Fact]
        public void Execute_WhenContextIsNull_ThrowsException()
        {
            var sut = new JoinMonsterExecuter(new QueryToSqlConverterStub(), new SqlCompilerStub(), new BatchPlannerStub(), new Hydrator());

            Func<Task> action = () => sut.ExecuteAsync(null, null, CancellationToken.None);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("context");
        }

        [Fact]
        public void Execute_WhenDatabaseCallIsNull_ThrowsException()
        {
            var sut = new JoinMonsterExecuter(new QueryToSqlConverterStub(), new SqlCompilerStub(), new BatchPlannerStub(), new Hydrator());

            Func<Task> action = () => sut.ExecuteAsync(new ResolveFieldContext(), null, CancellationToken.None);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("databaseCall");
        }
    }
}

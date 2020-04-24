using System;
using System.Collections.Generic;
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
            Action action = () => new JoinMonsterExecuter(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("converter");
        }

        [Fact]
        public void Ctor_WhenCompilerIsNull_ThrowsException()
        {
            Action action = () => new JoinMonsterExecuter(new QueryToSqlConverterStub(), null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("compiler");
        }

        [Fact]
        public void Execute_WhenContextIsNull_ThrowsException()
        {
            var sut = new JoinMonsterExecuter(new QueryToSqlConverterStub(), new SqlCompilerStub());

            Func<Task> action = () => sut.Execute(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("context");
        }

        [Fact]
        public void Execute_WhenDatabaseCallIsNull_ThrowsException()
        {
            var sut = new JoinMonsterExecuter(new QueryToSqlConverterStub(), new SqlCompilerStub());

            Func<Task> action = () => sut.Execute(new ResolveFieldContext(), null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("databaseCall");
        }

        [Fact]
        public async Task Execute_WithContextAndDatabaseCall_CallsDatabaseCallDelegateWithGeneratedSql()
        {
            var sqlValue = "SELECT \"id\", \"name\" FROM \"products\"";
            var sut = new JoinMonsterExecuter(new QueryToSqlConverterStub(), new SqlCompilerStub(sqlValue));

            string sqlFromCallDatabase = null;

            Task<object> CallDatabase(string sql)
            {
                sqlFromCallDatabase = sql;
                return Task.FromResult((object) null);
            }

            await sut.Execute(new ResolveFieldContext(), CallDatabase);

            sqlFromCallDatabase.Should().Be(sqlValue);
        }

        [Fact]
        public async Task Execute_WithContextAndDatabaseCall_ReturnsObjectFromDatabaseCall()
        {
            var sut = new JoinMonsterExecuter(new QueryToSqlConverterStub(), new SqlCompilerStub());
            var product = new ProductStub {Id = "1", Name = "Jacket"};

            Task<object> CallDatabase(string sql) =>
                Task.FromResult((object) product);

            var result = await sut.Execute(new ResolveFieldContext(), CallDatabase);

            result.Should().Be(product);
        }
    }
}

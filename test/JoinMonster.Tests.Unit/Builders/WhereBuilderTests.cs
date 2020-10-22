using System;
using System.Collections.Generic;
using FluentAssertions;
using JoinMonster.Builders;
using JoinMonster.Builders.Clauses;
using Xunit;

namespace JoinMonster.Tests.Unit.Builders
{
    public class WhereBuilderTests
    {
        [Fact]
        public void Column_WhenColumnIsNull_ThrowsException()
        {
            var builder = new WhereBuilder("table", new List<WhereCondition>());

            Action action = () => builder.Column(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("column");
        }

        [Fact]
        public void Column_WhenValueIsNull_ThrowsException()
        {
            var builder = new WhereBuilder("table", new List<WhereCondition>());

            Action action = () => builder.Column("id", null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("value");
        }

        [Fact]
        public void Column_WhenOperatorIsNull_ThrowsException()
        {
            var builder = new WhereBuilder("table", new List<WhereCondition>());

            Action action = () => builder.Column("id", 1, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("operator");
        }

        [Fact]
        public void Column_WithColumnAndValue_AddsConditionToList()
        {
            var whereConditions = new List<WhereCondition>();
            var builder = new WhereBuilder("table", whereConditions);

            builder.Column("id", 1);

            whereConditions.Should()
                .ContainSingle()
                .Which.Should()
                .BeEquivalentTo(new CompareCondition("table", "id", "=", 1));
        }

        [Fact]
        public void Column_WithOrCondition_AddsConditionToList()
        {
            var whereConditions = new List<WhereCondition>();
            var builder = new WhereBuilder("table", whereConditions);

            builder.Or().Column("id", 1);

            whereConditions.Should()
                .ContainSingle()
                .Which.Should()
                .BeEquivalentTo(new CompareCondition("table", "id", "=", 1)
                {
                    IsOr = true
                });
        }

        [Fact]
        public void Column_WithNotCondition_AddsConditionToList()
        {
            var whereConditions = new List<WhereCondition>();
            var builder = new WhereBuilder("table", whereConditions);

            builder.Not().Column("id", 1);

            whereConditions.Should()
                .ContainSingle()
                .Which.Should()
                .BeEquivalentTo(new CompareCondition("table", "id", "=", 1)
                {
                    IsNot = true
                });
        }

        [Fact]
        public void Columns_WhenFirstIsNull_ThrowsException()
        {
            var builder = new WhereBuilder("table", new List<WhereCondition>());

            Action action = () => builder.Columns(null, null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("first");
        }

        [Fact]
        public void Columns_WhenSecondIsNull_ThrowsException()
        {
            var builder = new WhereBuilder("table", new List<WhereCondition>());

            Action action = () => builder.Columns("id", null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("second");
        }

        [Fact]
        public void Columns_WhenOperatorIsNull_ThrowsException()
        {
            var builder = new WhereBuilder("table", new List<WhereCondition>());

            Action action = () => builder.Columns("id", "name", null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("operator");
        }

        [Fact]
        public void Columns_WithFirstAndSecond_AddsConditionToList()
        {
            var whereConditions = new List<WhereCondition>();
            var builder = new WhereBuilder("table", whereConditions);

            builder.Columns("id", "name");

            whereConditions.Should()
                .ContainSingle()
                .Which.Should()
                .BeEquivalentTo(new CompareColumnsCondition("table", "id", "=", "table", "name"));
        }

        [Fact]
        public void Columns_WithOrCondition_AddsConditionToList()
        {
            var whereConditions = new List<WhereCondition>();
            var builder = new WhereBuilder("table", whereConditions);

            builder.Or().Columns("id", "name");

            whereConditions.Should()
                .ContainSingle()
                .Which.Should()
                .BeEquivalentTo(new CompareColumnsCondition("table", "id", "=", "table", "name")
                {
                    IsOr = true
                });
        }

        [Fact]
        public void Columns_WithNotCondition_AddsConditionToList()
        {
            var whereConditions = new List<WhereCondition>();
            var builder = new WhereBuilder("table", whereConditions);


            builder.Not().Columns("id", "name");

            whereConditions.Should()
                .ContainSingle()
                .Which.Should()
                .BeEquivalentTo(new CompareColumnsCondition("table", "id", "=", "table", "name")
                {
                    IsNot = true
                });
        }

        [Fact]
        public void Grouped_WhenWhereIsNull_ThrowsException()
        {
            var builder = new WhereBuilder("table", new List<WhereCondition>());

            Action action = () => builder.Grouped(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("where");
        }

        [Fact]
        public void Grouped_WithOrCondition_AddsConditionToList()
        {
            var whereConditions = new List<WhereCondition>();
            var builder = new WhereBuilder("table", whereConditions);

            builder.Or().Grouped(where => { });

            whereConditions.Should()
                .ContainSingle()
                .Which.Should()
                .BeEquivalentTo(new NestedCondition(new List<WhereCondition>())
                {
                    IsOr = true
                });
        }

        [Fact]
        public void Grouped_WithNotCondition_AddsConditionToList()
        {
            var whereConditions = new List<WhereCondition>();
            var builder = new WhereBuilder("table", whereConditions);

            builder.Not().Grouped(where => { });

            whereConditions.Should()
                .ContainSingle()
                .Which.Should()
                .BeEquivalentTo(new NestedCondition(new List<WhereCondition>())
                {
                    IsNot = true
                });
        }

        [Fact]
        public void Grouped_WithNestedConditions_AddsConditionToList()
        {
            var whereConditions = new List<WhereCondition>();
            var builder = new WhereBuilder("table", whereConditions);

            builder.Grouped(where =>
            {
                where.Column("id", 2);
            });

            whereConditions.Should()
                .ContainSingle()
                .Which.Should()
                .BeEquivalentTo(new NestedCondition(new List<WhereCondition>
                {
                    new CompareCondition("table", "id", "=", 2)
                }));
        }
    }
}

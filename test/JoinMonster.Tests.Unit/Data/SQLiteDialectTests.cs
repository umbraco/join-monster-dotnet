using FluentAssertions;
using JoinMonster.Data;
using Xunit;

namespace JoinMonster.Tests.Unit.Data
{
    public class SQLiteDialectTests
    {
        [Fact]
        public void Quote_WithString_QuotesString()
        {
            var dialect = new SQLiteDialect();

            var quoted = dialect.Quote("product");

            quoted.Should().Be("\"product\"");
        }

        [Fact]
        public void CompositeKey_WithParentTableAndKeys_ReturnsCombinedString()
        {
            var dialect = new SQLiteDialect();

            var sql = dialect.CompositeKey("people", new[] {"id", "firstName", "lastName"});

            sql.Should().Be("\"people\".\"id\" || \"people\".\"firstName\" || \"people\".\"lastName\"");
        }
    }
}

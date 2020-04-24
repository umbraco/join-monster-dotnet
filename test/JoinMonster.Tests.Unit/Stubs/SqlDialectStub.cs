using JoinMonster.Data;

namespace JoinMonster.Tests.Unit.Stubs
{
    public class SqlDialectStub : ISqlDialect
    {
        public string Quote(string value) => $@"""{value}""";
    }
}

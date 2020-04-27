using System.Collections.Generic;
using System.Linq;
using JoinMonster.Data;

namespace JoinMonster.Tests.Unit.Stubs
{
    public class SqlDialectStub : ISqlDialect
    {
        public string Quote(string value) => $@"""{value}""";

        public string CompositeKey(string parentTable, IEnumerable<string> keys)
        {
            var result = keys.Select(key => $@"{Quote(parentTable)}.{Quote(key)}");
            return $"CONCAT({string.Join(", ", result)})";
        }
    }
}

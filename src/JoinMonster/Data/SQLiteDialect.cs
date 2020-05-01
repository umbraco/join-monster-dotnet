using System.Collections.Generic;
using System.Linq;

namespace JoinMonster.Data
{
    /// <summary>
    /// SQLite dialect.
    /// </summary>
    public class SQLiteDialect : ISqlDialect
    {
        /// <inheritdoc />
        public virtual string Quote(string str) => $@"""{str}""";

        /// <inheritdoc />
        public string CompositeKey(string parentTable, IEnumerable<string> keys)
        {
            var result = keys.Select(key => $"{Quote(parentTable)}.{Quote(key)}");
            return string.Join(" || ", result);
        }
    }
}

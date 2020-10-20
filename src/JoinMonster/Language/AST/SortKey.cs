using System;
using System.Collections.Generic;

namespace JoinMonster.Language.AST
{
    public class SortKey
    {
        public SortKey(string table, IDictionary<string, string> key, SortDirection direction)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Key = key;
            Direction = direction;
        }

        public string Table { get; }
        public IDictionary<string, string> Key { get; }
        public SortDirection Direction { get; }
    }
}

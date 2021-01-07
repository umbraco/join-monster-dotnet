using System;
using System.Collections.Generic;

namespace JoinMonster.Language.AST
{
    public class SortKey
    {
        public SortKey(string table, string column, string @as, Type? type, SortDirection direction)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Column = column ?? throw new ArgumentNullException(nameof(column));
            As = @as ?? throw new ArgumentNullException(nameof(@as));
            Type = type;
            Direction = direction;
        }

        public string Table { get; }
        public string Column { get; }
        public string As { get; }
        public Type? Type { get; }
        public SortDirection Direction { get; }
        public SortKey? ThenBy { get; set; }
    }
}

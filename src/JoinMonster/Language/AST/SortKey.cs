using System;
using System.Collections.Generic;

namespace JoinMonster.Language.AST
{
    public class SortKey
    {
        public SortKey(string table, string column, string? whereColumn, string @as, SortDirection direction)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Column = column ?? throw new ArgumentNullException(nameof(column));
            WhereColumn = whereColumn ?? column;
            As = @as ?? throw new ArgumentNullException(nameof(@as));
            Direction = direction;
        }

        public string Table { get; }
        public string Column { get; }
        public string WhereColumn { get; }
        public string As { get; }
        public SortDirection Direction { get; }
        public SortKey? ThenBy { get; set; }
    }
}

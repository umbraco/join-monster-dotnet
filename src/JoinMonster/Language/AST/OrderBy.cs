using System;

namespace JoinMonster.Language.AST
{
    public class OrderBy
    {
        public OrderBy(string table, string column, SortDirection direction)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Column = column ?? throw new ArgumentNullException(nameof(column));
            Direction = direction;
        }

        public string Table { get; }
        public string Column { get; }
        public SortDirection Direction { get; }

        public OrderBy? ThenBy { get; set; }
    }
}

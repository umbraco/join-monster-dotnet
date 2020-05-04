using System;

namespace JoinMonster.Configs
{
    internal class OrderBy
    {
        public OrderBy(string column, SortDirection direction)
        {
            Column = column ?? throw new ArgumentNullException(nameof(column));
            Direction = direction;
        }

        public string Column { get; }
        public SortDirection Direction { get; }

        public OrderBy? ThenBy { get; set; }
    }

    internal enum SortDirection
    {
        Ascending,
        Descending
    }
}

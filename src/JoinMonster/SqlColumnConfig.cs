namespace JoinMonster
{
    public class SqlColumnConfig
    {
        /// <summary>
        /// The column name.
        /// </summary>
        public string? Column { get; set; }

        /// <summary>
        /// Whether or not the field be excluded from the SQL query.
        /// </summary>
        public bool Ignored { get; set; }

        /// <summary>
        /// The dependant columns.
        /// </summary>
        public string[]? Dependencies { get; set; }

        /// <summary>
        /// Custom SQL expression.
        /// </summary>
        public ExpressionDelegate? Expression { get; set; }
    }
}

using System;

namespace JoinMonster.Configs
{
    /// <summary>
    /// SQL column configuration.
    /// </summary>
    public class SqlColumnConfig
    {
        /// <summary>
        /// Creates a new instance of <see cref="SqlColumnConfig"/>.
        /// </summary>
        /// <param name="column">The column name.</param>
        public SqlColumnConfig(string column)
        {
            Column = column ?? throw new ArgumentNullException(nameof(column));
        }

        /// <summary>
        /// The column name.
        /// </summary>
        public string Column { get; }

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

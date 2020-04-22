using System.Collections.Generic;

namespace JoinMonster
{
    /// <summary>
    /// Generates a raw SQL expression.
    /// </summary>
    /// <param name="tableAlias">An auto-generated table alias.</param>
    /// <param name="userContext">The user context.</param>
    /// <returns>A SQL expression.</returns>
    public delegate string ExpressionDelegate(string tableAlias, IDictionary<string, object> userContext);

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

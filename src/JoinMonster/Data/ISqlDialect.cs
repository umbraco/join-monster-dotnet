using System.Collections.Generic;

namespace JoinMonster.Data
{
    /// <summary>
    /// An interface representing a SQL Dialect.
    /// </summary>
    public interface ISqlDialect
    {
        /// <summary>
        /// Quotes a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to quote.</param>
        /// <returns>The quoted <see cref="string"/>.</returns>
        string Quote(string str);

        /// <summary>
        /// Generates a SQL string containing the columns to select.
        /// </summary>
        /// <param name="parentTable">An auto-generated alias for the parent table.</param>
        /// <param name="keys">The keys to select.</param>
        /// <returns>A SQL string containing the columns to select.</returns>
        string CompositeKey(string parentTable, IEnumerable<string> keys);
    }
}

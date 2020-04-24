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
    }
}

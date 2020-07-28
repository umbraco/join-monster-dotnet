namespace JoinMonster
{
    /// <summary>
    /// Generates the aliases that appear in each SQL query
    /// /// </summary>
    public interface IAliasGenerator
    {
        /// <summary>
        /// Generates an alias for a table.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>An alias.</returns>
        string GenerateTableAlias(string name);

        /// <summary>
        /// Generates an alias for a column.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>An alias.</returns>
        string GenerateColumnAlias(string name);
    }
}

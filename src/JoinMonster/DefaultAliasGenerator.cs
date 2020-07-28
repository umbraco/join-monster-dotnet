namespace JoinMonster
{
    /// <summary>
    /// The default alias generator.
    /// </summary>
    public class DefaultAliasGenerator : IAliasGenerator
    {
        /// <inheritdoc />
        public string GenerateTableAlias(string name) => name;

        /// <inheritdoc />
        public string GenerateColumnAlias(string name) => name;
    }
}

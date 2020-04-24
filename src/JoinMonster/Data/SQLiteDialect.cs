namespace JoinMonster.Data
{
    /// <summary>
    /// SQLite dialect.
    /// </summary>
    public class SQLiteDialect : ISqlDialect
    {
        /// <inheritdoc />
        public virtual string Quote(string value) => $@"""{value}""";
    }
}

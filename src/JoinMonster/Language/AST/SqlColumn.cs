namespace JoinMonster.Language.AST
{
    public class SqlColumn : SqlColumnBase
    {
        public SqlColumn(string name, string fieldName, string @as) : base(fieldName, @as)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

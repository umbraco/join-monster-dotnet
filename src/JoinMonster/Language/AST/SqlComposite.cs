namespace JoinMonster.Language.AST
{
    public class SqlComposite : SqlColumnBase
    {
        public SqlComposite(string[] name, string fieldName, string @as) : base(fieldName, @as)
        {
            Name = name;
        }

        public string[] Name { get; }
    }
}

namespace JoinMonster.Language.AST
{
    public class SqlComposite : SqlColumnBase
    {
        public SqlComposite(string[] name, string fieldName, string @as, bool isId = false) : base(fieldName, @as, isId)
        {
            Name = name;
        }

        public string[] Name { get; }
    }
}

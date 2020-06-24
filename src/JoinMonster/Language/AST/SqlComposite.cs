namespace JoinMonster.Language.AST
{
    public class SqlComposite : SqlColumnBase
    {
        public SqlComposite(Node parent, string[] name, string fieldName, string @as, bool isId = false)
            : base(parent, fieldName, @as, isId)
        {
            Name = name;
        }

        public string[] Name { get; }
    }
}

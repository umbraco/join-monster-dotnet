namespace JoinMonster.Language.AST
{
    public class SqlTable : Node
    {
        public SqlTable(string name, string @as, SqlColumns columns)
        {
            Name = name;
            As = @as;
            Columns = columns;
        }

        public string As { get; }
        public string Name { get; }
        public SqlColumns Columns { get; }
    }
}

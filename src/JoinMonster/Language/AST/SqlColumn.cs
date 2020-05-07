namespace JoinMonster.Language.AST
{
    public class SqlColumn : SqlColumnBase
    {
        public SqlColumn(string name, string fieldName, string @as, bool isId = false) : base(fieldName, @as, isId)
        {
            Name = name;
        }

        public string Name { get; }
        public string? FromOtherTable { get; set; }
    }
}

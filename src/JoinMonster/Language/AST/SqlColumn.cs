namespace JoinMonster.Language.AST
{
    public class SqlColumn : Node
    {
        public SqlColumn(string name, string? fieldName, string @as)
        {
            Name = name;
            FieldName = fieldName;
            As = @as;
        }

        public string As { get; }
        public string? FieldName { get; }
        public string Name { get; }
    }
}

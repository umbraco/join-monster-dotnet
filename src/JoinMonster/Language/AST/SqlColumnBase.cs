namespace JoinMonster.Language.AST
{
    public abstract class SqlColumnBase : Node
    {
        protected SqlColumnBase(string fieldName, string @as)
        {
            FieldName = fieldName;
            As = @as;
        }

        public string As { get; }
        public string FieldName { get; }
    }
}

namespace JoinMonster.Language.AST
{
    public abstract class SqlColumnBase : Node
    {
        protected SqlColumnBase(string fieldName, string @as, bool isId)
        {
            FieldName = fieldName;
            As = @as;
            IsId = isId;
        }

        public string As { get; }
        public string FieldName { get; }
        public bool IsId { get; }
    }
}

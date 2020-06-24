using System;

namespace JoinMonster.Language.AST
{
    public abstract class SqlColumnBase : Node
    {
        protected SqlColumnBase(Node parent, string fieldName, string @as, bool isId) : base(parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            FieldName = fieldName;
            As = @as;
            IsId = isId;
        }

        public string As { get; }
        public string FieldName { get; }
        public bool IsId { get; }
    }
}

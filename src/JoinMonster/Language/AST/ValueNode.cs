namespace JoinMonster.Language.AST
{
    public class ValueNode : Node
    {
        public ValueNode(Node parent, object value) : base(parent)
        {
            Value = value;
        }

        public object Value { get; }
    }
}

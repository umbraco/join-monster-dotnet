namespace JoinMonster.Language.AST
{
    public class ValueNode : Node
    {
        public ValueNode(object value)
        {
            Value = value;
        }

        public object Value { get; }
    }
}

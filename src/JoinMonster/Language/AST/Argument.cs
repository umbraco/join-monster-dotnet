using System;
using System.Collections.Generic;

namespace JoinMonster.Language.AST
{
    public class Argument : Node
    {
        public Argument(string name, ValueNode value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Name { get; }
        public ValueNode Value { get; }

        public override IEnumerable<Node> Children
        {
            get { yield return Value; }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace JoinMonster.Language.AST
{
    public abstract class Node
    {
        protected Node() {}

        protected Node(Node? parent)
        {
            Parent = parent;
        }

        public virtual IEnumerable<Node> Children => Enumerable.Empty<Node>();
        public Node? Parent { get; }
        public GraphQLLocation? Location { get; set; }
    }
}

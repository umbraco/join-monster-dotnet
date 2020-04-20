using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;

namespace JoinMonster.Language.AST
{
    public abstract class Node
    {
        public virtual IEnumerable<Node> Children => Enumerable.Empty<Node>();
        public SourceLocation? SourceLocation { get; set; }
    }
}

using System;
using GraphQL.Language.AST;
using JoinMonster.Language.AST;

namespace JoinMonster.Language
{
    public static class NodeExtensions
    {
        public static T WithLocation<T>(this T node, SourceLocation location) where T : Node
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (location == null) throw new ArgumentNullException(nameof(location));

            node.SourceLocation = new SourceLocation(location.Line, location.Column, location.Start, location.End);
            return node;
        }
    }
}

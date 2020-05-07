using System;
using GraphQL.Language.AST;
using JoinMonster.Language.AST;

namespace JoinMonster.Language
{
    /// <summary>
    /// Extension methods for <see cref="Node"/>.
    /// </summary>
    public static class NodeExtensions
    {
        /// <summary>
        /// Set <c>node.SourceLocation</c> to the <paramref name="location"/> value.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="location">The source location.</param>
        /// <typeparam name="T">The node type.</typeparam>
        /// <returns>The <paramref name="node"/> passed in to the method.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="node"/> or <paramref name="location"/> is null.</exception>
        public static T WithLocation<T>(this T node, SourceLocation location) where T : Node
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (location == null) throw new ArgumentNullException(nameof(location));

            node.SourceLocation = new SourceLocation(location.Line, location.Column, location.Start, location.End);
            return node;
        }
    }
}

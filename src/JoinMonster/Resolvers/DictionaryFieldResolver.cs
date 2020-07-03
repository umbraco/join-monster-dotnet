using System.Collections.Generic;
using GraphQL;
using GraphQL.Resolvers;

namespace JoinMonster.Resolvers
{
    /// <summary>
    /// Attempts to return a value for a field from the source object (if it's an <c>IDictionary&lt;string, object&gt;</c>)
    /// matching the key with the field name.
    /// </summary>
    public class DictionaryFieldResolver : IFieldResolver
    {
        /// <summary>
        /// Returns the static instance of the <see cref="DictionaryFieldResolver"/> class.
        /// </summary>
        public static DictionaryFieldResolver Instance { get; } = new DictionaryFieldResolver();

        /// <inheritdoc />
        public object? Resolve(IResolveFieldContext context)
        {
            if (context.Source is IDictionary<string, object> dict
                && dict.TryGetValue(context.FieldAst.Alias ?? context.FieldName, out var value))
            {
                return value;
            }

            return null;
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using JoinMonster.Language;

namespace JoinMonster.Resolvers
{
    /// <summary>
    /// Attempts to return a value for a field from the source object (if it's an <c>IDictionary&lt;string, object&gt;</c>)
    /// matching the key with the field name.
    /// </summary>
    public class DictionaryFieldResolver : IFieldResolver
    {
        private DictionaryFieldResolver()
        {

        }

        /// <summary>
        /// Returns the static instance of the <see cref="DictionaryFieldResolver"/> class.
        /// </summary>
        public static DictionaryFieldResolver Instance { get; } = new();

        /// <inheritdoc />
        public ValueTask<object?> ResolveAsync(IResolveFieldContext context)
        {
            if (context.Source is IDictionary<string, object> dict
                && dict.TryGetValue(context.FieldAlias(), out var value))
            {
                return new ValueTask<object?>(value);
            }

            return new ValueTask<object?>();
        }
    }
}

using GraphQL;
using GraphQLParser.AST;

namespace JoinMonster
{
    public static class FieldAliasExtensions
    {
        public static string FieldAlias(this IResolveFieldContext context)
        {
            return FieldAlias(context.FieldAst, context.FieldAst.Name?.StringValue ?? context.FieldDefinition.Name);
        }

        public static string FieldAlias(this GraphQLField field, string fallbackName)
        {
            var fieldName = field.Name?.StringValue ?? fallbackName;
            if (fieldName.StartsWith("__")) return fieldName;

            var fieldAlias = (field.Alias?.Name ?? field.Name)?.StringValue ?? fallbackName;
            return fieldAlias == fieldName ? fieldName : $"{fieldAlias}#{fieldName}";
        }
    }
}

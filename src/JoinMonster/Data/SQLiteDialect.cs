using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Execution;
using JoinMonster.Language.AST;

namespace JoinMonster.Data
{
    /// <summary>
    /// SQLite dialect.
    /// </summary>
    public class SQLiteDialect : SqlDialect
    {
        /// <inheritdoc />
        public override string Quote(string str)
        {
            return str.Contains("\"") ? str : $@"""{str}""";
        }

        /// <inheritdoc />
        public override string CompositeKey(string parentTable, IEnumerable<string> keys)
        {
            var result = keys.Select(key => $"{Quote(parentTable)}.{Quote(key)}");
            return string.Join(" || ", result);
        }

        /// <inheritdoc />
        public override void HandleJoinedOneToManyPaginated(SqlTable parent, SqlTable node,
            IReadOnlyDictionary<string, ArgumentValue> arguments, IResolveFieldContext context, ICollection<string> tables,
            SqlCompilerContext compilerContext, string? joinCondition) => throw new NotSupportedException();

        /// <inheritdoc />
        public override void HandlePaginationAtRoot(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments, IResolveFieldContext context,
            ICollection<string> tables, SqlCompilerContext compilerContext) => throw new NotSupportedException();

        public override void HandleBatchedOneToManyPaginated(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments,
            IResolveFieldContext context, ICollection<string> tables, IEnumerable<object> batchScope, SqlCompilerContext compilerContext) =>
            throw new NotSupportedException();

        public override void HandleBatchedManyToManyPaginated(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments,
            IResolveFieldContext context, ICollection<string> tables, IEnumerable<object> batchScope, SqlCompilerContext compilerContext,
            string joinCondition) => throw new NotSupportedException();
    }
}

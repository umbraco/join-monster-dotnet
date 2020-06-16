using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using JoinMonster.Language.AST;

namespace JoinMonster.Data
{
    /// <summary>
    /// SQLite dialect.
    /// </summary>
    public class SQLiteDialect : SqlDialect
    {
        /// <inheritdoc />
        public override string Quote(string str) => $@"""{str}""";

        /// <inheritdoc />
        public override string CompositeKey(string parentTable, IEnumerable<string> keys)
        {
            var result = keys.Select(key => $"{Quote(parentTable)}.{Quote(key)}");
            return string.Join(" || ", result);
        }

        /// <inheritdoc />
        public override void HandleJoinedOneToManyPaginated(SqlTable parent, SqlTable node,
            IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context, ICollection<string> tables,
            string? joinCondition) => throw new NotSupportedException();

        /// <inheritdoc />
        public override void HandlePaginationAtRoot(Node? parent, SqlTable node,
            IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context, ICollection<string> tables) =>
            throw new NotSupportedException();
    }
}

using System.Collections.Generic;
using System.Linq;
using GraphQL;
using JoinMonster.Data;
using JoinMonster.Language.AST;

namespace JoinMonster.Tests.Unit.Stubs
{
    public class SqlDialectStub : SqlDialect
    {
        private readonly string _joinedOneToManyPaginatedSql;
        private readonly string _paginatedAtRootSql;

        public SqlDialectStub(string joinedOneToManyPaginatedSql = null, string paginatedAtRootSql = null)
        {
            _joinedOneToManyPaginatedSql = joinedOneToManyPaginatedSql;
            _paginatedAtRootSql = paginatedAtRootSql;
        }

        public override string CompositeKey(string parentTable, IEnumerable<string> keys)
        {
            var result = keys.Select(key => $@"{Quote(parentTable)}.{Quote(key)}");
            return $"CONCAT({string.Join(", ", result)})";
        }

        public override void HandleJoinedOneToManyPaginated(SqlTable parent, SqlTable node,
            IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context, ICollection<string> tables,
            SqlCompilerContext compilerContext, string joinCondition)
        {
            tables.Add(_joinedOneToManyPaginatedSql);
        }

        public override void HandlePaginationAtRoot(Node parent, SqlTable node,
            IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context, ICollection<string> tables,
            SqlCompilerContext compilerContext)
        {
            tables.Add(_paginatedAtRootSql);
        }
    }
}

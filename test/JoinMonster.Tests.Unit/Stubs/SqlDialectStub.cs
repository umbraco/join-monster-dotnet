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

        public SqlDialectStub(string joinedOneToManyPaginatedSql = null)
        {
            _joinedOneToManyPaginatedSql = joinedOneToManyPaginatedSql;
        }

        public override string CompositeKey(string parentTable, IEnumerable<string> keys)
        {
            var result = keys.Select(key => $@"{Quote(parentTable)}.{Quote(key)}");
            return $"CONCAT({string.Join(", ", result)})";
        }

        public override void HandleJoinedOneToManyPaginated(SqlTable parent, SqlTable node, IDictionary<string, object> arguments, IResolveFieldContext context,
            ICollection<string> tables, string joinCondition)
        {
            tables.Add(_joinedOneToManyPaginatedSql);
        }
    }
}

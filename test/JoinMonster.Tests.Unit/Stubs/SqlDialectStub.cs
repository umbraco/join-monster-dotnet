using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Execution;
using JoinMonster.Data;
using JoinMonster.Language.AST;

namespace JoinMonster.Tests.Unit.Stubs
{
    public class SqlDialectStub : SqlDialect
    {
        private readonly string _joinedOneToManyPaginatedSql;
        private readonly string _paginatedAtRootSql;
        private readonly string _batchedOneToManyPaginatedSql;
        private readonly string _batchedManyToManyPaginatedSql;

        public SqlDialectStub(string joinedOneToManyPaginatedSql = null, string paginatedAtRootSql = null,
            string batchedOneToManyPaginatedSql = null, string batchedManyToManyPaginatedSql = null)
        {
            _joinedOneToManyPaginatedSql = joinedOneToManyPaginatedSql;
            _paginatedAtRootSql = paginatedAtRootSql;
            _batchedOneToManyPaginatedSql = batchedOneToManyPaginatedSql;
            _batchedManyToManyPaginatedSql = batchedManyToManyPaginatedSql;
        }

        public override string CompositeKey(string parentTable, IEnumerable<string> keys)
        {
            var result = keys.Select(key => $@"{Quote(parentTable)}.{Quote(key)}");
            return $"CONCAT({string.Join(", ", result)})";
        }

        public override void HandleJoinedOneToManyPaginated(SqlTable parent, SqlTable node,
            IReadOnlyDictionary<string, ArgumentValue> arguments, IResolveFieldContext context, ICollection<string> tables,
            SqlCompilerContext compilerContext, string joinCondition)
        {
            tables.Add(_joinedOneToManyPaginatedSql);
        }

        public override void HandlePaginationAtRoot(Node parent, SqlTable node,
            IReadOnlyDictionary<string, ArgumentValue> arguments, IResolveFieldContext context, ICollection<string> tables,
            SqlCompilerContext compilerContext)
        {
            tables.Add(_paginatedAtRootSql);
        }

        public override void HandleBatchedOneToManyPaginated(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments,
            IResolveFieldContext resolveFieldContext, ICollection<string> tables, ICollection<string> selections, IEnumerable<object> batchScope, SqlCompilerContext compilerContext)
        {
            tables.Add(_batchedOneToManyPaginatedSql);
        }

        public override void HandleBatchedManyToManyPaginated(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments,
            IResolveFieldContext context, ICollection<string> tables, IEnumerable<object> enumerable, SqlCompilerContext compilerContext,
            string joinCondition)
        {
            tables.Add(_batchedManyToManyPaginatedSql);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using GraphQL;
using JoinMonster.Data;
using JoinMonster.Language.AST;

namespace JoinMonster.Tests.Unit.Stubs
{
    public class SqlCompilerStub : ISqlCompiler
    {
        private readonly string _sql;

        public SqlCompilerStub(string sql = null)
        {
            _sql = sql;
        }

        public SqlResult Compile(Node node, IResolveFieldContext context, IEnumerable? batchScope = null)
        {
            return new SqlResult(_sql, new Dictionary<string, object>());
        }
    }
}

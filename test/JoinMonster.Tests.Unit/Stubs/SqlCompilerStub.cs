using GraphQL;
using JoinMonster.Data;
using JoinMonster.Language.AST;

namespace JoinMonster.Tests.Unit.Stubs
{
    public class SqlCompilerStub : SqlCompiler
    {
        private readonly string _sql;

        public SqlCompilerStub(string sql = null) : base(new SqlDialectStub())
        {
            _sql = sql;
        }

        public override string Compile(Node node, IResolveFieldContext context)
        {
            return _sql;
        }
    }
}

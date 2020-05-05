using GraphQL.Types;
using JoinMonster.Language;
using JoinMonster.Language.AST;

namespace JoinMonster.Tests.Unit.Stubs
{
    public class QueryToSqlConverterStub : QueryToSqlConverter
    {
        private readonly SqlTable _node;

        public QueryToSqlConverterStub(SqlTable node = null)
        {
            _node = node;
        }

        public override SqlTable Convert(IResolveFieldContext context) => _node;
    }
}

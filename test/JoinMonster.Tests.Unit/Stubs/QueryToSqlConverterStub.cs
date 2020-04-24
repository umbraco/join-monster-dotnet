using GraphQL.Types;
using JoinMonster.Language;
using JoinMonster.Language.AST;

namespace JoinMonster.Tests.Unit.Stubs
{
    public class QueryToSqlConverterStub : QueryToSqlConverter
    {
        private readonly Node _node;

        public QueryToSqlConverterStub(Node node = null)
        {
            _node = node;
        }

        public override Node Convert(IResolveFieldContext context) => _node;
    }
}

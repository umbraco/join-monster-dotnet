using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using JoinMonster.Configs;
using JoinMonster.Data;
using JoinMonster.Language.AST;

namespace JoinMonster.Tests.Unit.Stubs
{
    public class BatchPlannerStub : IBatchPlanner
    {
        public Task NextBatch(SqlTable sqlAst, object data, DatabaseCallDelegate databaseCall, IResolveFieldContext context,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

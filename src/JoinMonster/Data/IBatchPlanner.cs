using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using JoinMonster.Configs;
using JoinMonster.Language.AST;

namespace JoinMonster.Data
{
    public interface IBatchPlanner
    {
        Task NextBatch(SqlTable sqlAst, object data, DatabaseCallDelegate databaseCall, IResolveFieldContext context,
            CancellationToken cancellationToken);
    }
}

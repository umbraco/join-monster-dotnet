using System.Threading.Tasks;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Types;
using GraphQL.Validation;
using GraphQLParser;

namespace JoinMonster.Tests.Unit.Stubs
{
    public class StubExecutionStrategy : ExecutionStrategy
    {
        public static readonly StubExecutionStrategy Instance = new StubExecutionStrategy();

        /// <inheritdoc />
        public override Task ExecuteNodeTreeAsync(ExecutionContext context, ExecutionNode rootNode)
        {
            return Task.CompletedTask;
        }

        public IResolveFieldContext GetResolveFieldContext(ISchema schema, string query, Variables variables = null)
        {
            var documentBuilder = new GraphQLDocumentBuilder();
            var document = documentBuilder.Build(query);
            schema.Initialize();

            var context = new ExecutionContext
            {
                Document = document,
                Schema = schema,
                Operation = document.OperationWithName(string.Empty)!,
                Variables = variables ?? new Variables(),
                Errors = new ExecutionErrors(),
                ExecutionStrategy = this,
            };

            var rootType = GetOperationRootType(context);
            var rootNode = BuildExecutionRootNode(context, rootType);

            return new ReadonlyResolveFieldContext(rootNode.SubFields[0], context);
        }
    }
}

using System.Collections.Generic;
using System.Diagnostics;
using GraphQL.Execution;
using JoinMonster.Configs;

namespace JoinMonster.Language.AST
{
    [DebuggerDisplay("{GetType().Name} ({Name})")]
    public class SqlColumn : SqlColumnBase
    {
        public SqlColumn(Node parent, string name, string fieldName, string @as, bool isId = false)
            : base(parent, fieldName, @as, isId)
        {
            Name = name;
            Arguments = new Dictionary<string, ArgumentValue>();
        }

        public string Name { get; }
        public string? FromOtherTable { get; set; }
        public IReadOnlyDictionary<string, ArgumentValue> Arguments { get; set; }
        public ExpressionDelegate? Expression { get; set; }
    }
}

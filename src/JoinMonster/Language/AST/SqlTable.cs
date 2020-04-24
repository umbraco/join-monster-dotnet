using System.Collections.Generic;

namespace JoinMonster.Language.AST
{
    public class SqlTable : Node
    {
        public SqlTable(string name, string @as, SqlColumns columns, Arguments arguments)
        {
            Name = name;
            As = @as;
            Columns = columns;
            Arguments = arguments;
        }

        public string As { get; }
        public string Name { get; }
        public Arguments Arguments { get; }
        public SqlColumns Columns { get; }
        public WhereDelegate? Where { get; set; }

        public override IEnumerable<Node> Children
        {
            get
            {
                if (Columns != null)
                    yield return Columns;
                if (Arguments != null)
                    yield return Arguments;
            }
        }
    }
}

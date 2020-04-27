using System.Collections.Generic;
using JoinMonster.Configs;

namespace JoinMonster.Language.AST
{
    public class SqlTable : Node
    {
        public SqlTable(string name, string @as, SqlColumns columns, SqlTables tables, Arguments arguments, bool grabMany, WhereDelegate? where, JoinDelegate? join)
        {
            Name = name;
            As = @as;
            Columns = columns;
            Tables = tables;
            Arguments = arguments;
            GrabMany = grabMany;
            Where = where;
            Join = join;
        }

        public string As { get; }
        public string Name { get; }
        public bool GrabMany { get; }
        public Arguments Arguments { get; }
        public SqlColumns Columns { get; }
        public SqlTables Tables { get; }
        public WhereDelegate? Where { get; }
        public JoinDelegate? Join { get; }

        public override IEnumerable<Node> Children
        {
            get
            {
                if (Arguments != null)
                    yield return Arguments;
                if (Columns != null)
                    yield return Columns;
                if (Tables != null)
                    yield return Tables;
            }
        }
    }
}

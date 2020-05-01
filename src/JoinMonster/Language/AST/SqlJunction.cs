using JoinMonster.Configs;

namespace JoinMonster.Language.AST
{
    public class SqlJunction : Node
    {
        public SqlJunction(string table, string @as, JoinDelegate fromParent, JoinDelegate toChild, WhereDelegate? where)
        {
            Table = table;
            As = @as;
            FromParent = fromParent;
            ToChild = toChild;
            Where = @where;
        }

        public string Table { get; }
        public string As { get; }
        public JoinDelegate FromParent { get; }
        public JoinDelegate ToChild { get; }
        public WhereDelegate? Where { get; }
    }
}

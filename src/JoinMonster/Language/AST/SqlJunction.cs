using System;
using JoinMonster.Configs;

namespace JoinMonster.Language.AST
{
    public class SqlJunction : Node
    {
        public SqlJunction(Node parent, string table, string @as) : base(parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            Table = table;
            As = @as;
        }

        public string Table { get; }
        public string As { get; }
        public JoinDelegate? FromParent { get; set; }
        public JoinDelegate? ToChild { get; set; }
        public WhereDelegate? Where { get; set; }
        public OrderBy? OrderBy { get; set; }
        public SortKey? SortKey { get; set; }
        public SqlBatch? Batch { get; set; }
    }
}

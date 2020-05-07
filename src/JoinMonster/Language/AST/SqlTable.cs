using System.Collections.Generic;
using JoinMonster.Configs;
using JoinMonster.Data;

namespace JoinMonster.Language.AST
{
    public class SqlTable : Node
    {
        public SqlTable(string name, string fieldName, string @as, IEnumerable<SqlColumnBase> columns,
            IEnumerable<SqlTable> tables, IReadOnlyDictionary<string, object> arguments, bool grabMany)
        {
            FieldName = fieldName;
            Name = name;
            As = @as;
            Columns = columns;
            Tables = tables;
            Arguments = arguments;
            GrabMany = grabMany;
        }

        public string As { get; }
        public string Name { get; }
        public string FieldName { get; }
        public bool GrabMany { get; }
        public bool Paginate { get; set; }
        public IReadOnlyDictionary<string, object> Arguments { get; }
        public IEnumerable<SqlColumnBase> Columns { get; }
        public IEnumerable<SqlTable> Tables { get; }
        public SqlJunction? Junction { get; set; }
        public WhereDelegate? Where { get; set; }
        public JoinDelegate? Join { get; set; }
        public OrderBy? OrderBy { get; set; }
        public SortKey? SortKey { get; set; }

        public override IEnumerable<Node> Children
        {
            get
            {
                foreach (var column in Columns)
                    yield return column;
                foreach (var table in Tables)
                    yield return table;
                if (Junction != null)
                    yield return Junction;
            }
        }
    }
}

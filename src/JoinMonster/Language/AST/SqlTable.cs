using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL.Execution;
using GraphQL.Types;
using JoinMonster.Configs;

namespace JoinMonster.Language.AST
{
    [DebuggerDisplay("{GetType().Name} ({Name})")]
    public class SqlTable : Node
    {
        public SqlTable(Node? parent, SqlTableConfig? config, string name, string fieldName, string @as,
            IReadOnlyDictionary<string, ArgumentValue> arguments, bool grabMany) : base(parent)
        {
            Config = config;
            FieldName = fieldName;
            Name = name;
            As = @as;
            Columns = new List<SqlColumnBase>();
            Tables = new List<SqlTable>();
            Arguments = arguments;
            GrabMany = grabMany;
        }

        public string As { get; }
        public string Name { get; }
        public SqlTableConfig Config { get; }
        public string FieldName { get; }
        public bool GrabMany { get; }
        public bool Paginate { get; set; }
        public IReadOnlyDictionary<string, ArgumentValue> Arguments { get; }
        public ICollection<SqlColumnBase> Columns { get; }
        public ICollection<SqlTable> Tables { get; }
        public SqlJunction? Junction { get; set; }
        public SqlBatch? Batch { get; set; }
        public WhereDelegate? Where { get; set; }
        public JoinDelegate? Join { get; set; }
        public OrderBy? OrderBy { get; set; }
        public SortKey? SortKey { get; set; }
        public ColumnExpressionDelegate? ColumnExpression { get; set; }
        internal IGraphType? ParentGraphType { get; set; }

        public SqlColumn AddColumn(string name, string fieldName, string @as, bool isId = false)
        {
            var column = new SqlColumn(this, name, fieldName, @as, isId);
            Columns.Add(column);
            return column;
        }

        public SqlColumn AddColumn(SqlColumn column)
        {
            if (column.Parent != this) throw new InvalidOperationError("Cannot change column parent");

            Columns.Add(column);
            return column;
        }

        public SqlTable AddTable(SqlTableConfig? config, string name, string fieldName, string @as,
                                         IReadOnlyDictionary<string, ArgumentValue> arguments, bool grabMany)
        {
            var sqlTable = new SqlTable(this, config, name, fieldName, @as, arguments, grabMany);
            Tables.Add(sqlTable);
            return sqlTable;
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                foreach (var column in Columns.ToList())
                    yield return column;
                foreach (var table in Tables.ToList())
                    yield return table;
                if (Junction != null)
                    yield return Junction;
            }
        }
    }
}

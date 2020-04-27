using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JoinMonster.Language.AST
{
    public class SqlTables : Node, IEnumerable<SqlTable>
    {
        private List<SqlTable>? _tables;

        public void Add(SqlTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (_tables == null)
                _tables = new List<SqlTable>();
            _tables.Add(table);
        }

        public override IEnumerable<Node> Children => _tables ?? Enumerable.Empty<Node>();

        public IEnumerator<SqlTable> GetEnumerator() =>
            (_tables ?? Enumerable.Empty<SqlTable>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}

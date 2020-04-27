using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JoinMonster.Language.AST
{
    public class SqlColumns : Node, IEnumerable<SqlColumnBase>
    {
        private List<SqlColumnBase>? _columns;

        public void Add(SqlColumnBase column)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));
            if(_columns == null)
                _columns = new List<SqlColumnBase>();
            _columns.Add(column);
        }

        public override IEnumerable<Node> Children => _columns ?? Enumerable.Empty<Node>();

        public IEnumerator<SqlColumnBase> GetEnumerator() =>
            (_columns ?? Enumerable.Empty<SqlColumnBase>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}

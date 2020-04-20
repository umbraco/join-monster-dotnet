using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JoinMonster.Language.AST
{
    public class SqlColumns : Node, IEnumerable<SqlColumn>
    {
        private List<SqlColumn>? _columns;

        public void Add(SqlColumn column)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));
            if(_columns == null)
                _columns = new List<SqlColumn>();
            _columns.Add(column);
        }

        public override IEnumerable<Node> Children => _columns ?? Enumerable.Empty<Node>();

        public IEnumerator<SqlColumn> GetEnumerator() =>
            _columns?.GetEnumerator() ?? Enumerable.Empty<SqlColumn>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}

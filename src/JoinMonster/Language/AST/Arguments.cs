using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JoinMonster.Language.AST
{
    public class Arguments : Node, IEnumerable<Argument>
    {
        private List<Argument>? _arguments;

        public override IEnumerable<Node> Children => _arguments ?? Enumerable.Empty<Node>();

        public void Add(Argument argument)
        {
            if (argument == null) throw new ArgumentNullException(nameof(argument));
            if(_arguments == null)
                _arguments = new List<Argument>();
            _arguments.Add(argument);
        }

        public IEnumerator<Argument> GetEnumerator() =>
            (_arguments ?? Enumerable.Empty<Argument>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}

using System;
using JoinMonster.Exceptions;
using JoinMonster.Language.AST;

namespace JoinMonster
{
    internal class SqlAstValidator
    {
        public void Validate(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            if(!(node is SqlTable sqlTable))
                throw new JoinMonsterException($"Expected node to be of type '{typeof(SqlTable)}' but was '{node.GetType()}'.");

            if(sqlTable.Join != null)
                throw new JoinMonsterException("Root level field cannot have 'SqlJoin'.");
        }
    }
}

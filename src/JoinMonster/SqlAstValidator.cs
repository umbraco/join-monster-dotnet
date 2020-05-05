using JoinMonster.Language.AST;

namespace JoinMonster
{
    internal class SqlAstValidator
    {
        public void Validate(Node node)
        {
            if(!(node is SqlTable sqlTable))
                throw new JoinMonsterException($"Root node must be of type '{typeof(SqlTable)}'.");

            if(sqlTable.Join != null)
                throw new JoinMonsterException("Root level field cannot have 'SqlJoin'.");
        }
    }
}

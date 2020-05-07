namespace JoinMonster.Language.AST
{
    public class SortKey
    {
        public SortKey(string[] key, SortDirection direction)
        {
            Key = key;
            Direction = direction;
        }

        public string[] Key { get; }
        public SortDirection Direction { get; }
    }
}

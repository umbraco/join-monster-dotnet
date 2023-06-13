using System;
using JoinMonster.Configs;

namespace JoinMonster.Language.AST;

public class SqlBatch : Node
{
    public SqlBatch(SqlColumn thisKey, SqlColumn parentKey, Type keyType)
    {
        ThisKey = thisKey;
        ParentKey = parentKey;
        KeyType = keyType;
    }

    public SqlColumn ThisKey { get; }
    public SqlColumn ParentKey { get; }
    public Type KeyType { get; }
    public JoinDelegate? Join { get; set; }
    public BatchWhereDelegate? Where { get; set; }
}

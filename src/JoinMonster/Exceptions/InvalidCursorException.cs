namespace JoinMonster.Exceptions;

/// <summary>
/// Thrown when a provided pagination cursor is invalid
/// </summary>
public class InvalidCursorException : JoinMonsterException
{
    /// <summary>
    /// The offending cursor
    /// </summary>
    public string Cursor { get; }

    public InvalidCursorException(string cursor, string message) : base(message)
    {
        Cursor = cursor;
    }
}

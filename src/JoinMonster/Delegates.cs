using System.Collections.Generic;

namespace JoinMonster
{
    /// <summary>
    /// Generates a raw SQL expression.
    /// </summary>
    /// <param name="tableAlias">An auto-generated table alias.</param>
    /// <param name="userContext">The user context.</param>
    /// <returns>A SQL expression.</returns>
    public delegate string ExpressionDelegate(string tableAlias, IDictionary<string, object> userContext);

    /// <summary>
    /// Generates a where SQL expression.
    /// </summary>
    /// <param name="tableAlias">An auto-generated table alias.</param>
    /// <param name="arguments">An the arguments.</param>
    /// <param name="userContext">The user context.</param>
    /// <returns>A SQL expression.</returns>
    public delegate string? WhereDelegate(string tableAlias, IDictionary<string, object> arguments, IDictionary<string, object> userContext);
}

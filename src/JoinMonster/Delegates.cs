using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoinMonster
{
    /// <summary>
    /// Generates a <c>SQL</c> expression.
    /// </summary>
    /// <param name="tableAlias">An auto-generated table alias. Already quoted.</param>
    /// <param name="userContext">The user context.</param>
    /// <returns>A RAW SQL expression.</returns>
    public delegate Task<string> ExpressionDelegate(string tableAlias, IDictionary<string, object> userContext);

    /// <summary>
    /// Generates a <c>WHERE</c> condition.
    /// </summary>
    /// <param name="tableAlias">An auto-generated table alias. Already quoted</param>
    /// <param name="arguments">An the arguments.</param>
    /// <param name="userContext">The user context.</param>
    /// <returns>A WHERE condition or null if there's no condition.</returns>
    // TODO: Maybe pass in a "ParameterNameGenerator" and return an object containing the WHERE condition and a Dictionary of parameters.
    // TODO: This would make it more safe instead of relying on the implementer to correctly escape the values.
    public delegate Task<string?> WhereDelegate(string tableAlias, IDictionary<string, object> arguments, IDictionary<string, object> userContext);

    /// <summary>
    /// Takes the SQL string and sends it to a database.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>The data fetched from the database or null if nothing was fetched.</returns>
    public delegate Task<object?> DatabaseCallDelegate(string sql);
}

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using GraphQL;
using JoinMonster.Builders;
using JoinMonster.Language.AST;

namespace JoinMonster.Configs
{
    /// <summary>
    /// Generates a SQL expression.
    /// </summary>
    /// <param name="tableAlias">An auto-generated table alias. Already quoted.</param>
    /// <param name="column">The column name. Not quoted.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="context">The context.</param>
    /// <returns>A RAW SQL expression.</returns>
    public delegate string ColumnExpressionDelegate(string tableAlias, string column, IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context);

    /// <summary>
    /// Generates a SQL expression.
    /// </summary>
    /// <param name="tableAlias">An auto-generated table alias. Already quoted.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="context">The context.</param>
    /// <param name="sqlAstNode">The SQL AST node.</param>
    /// <returns>A RAW SQL expression.</returns>
    public delegate string ExpressionDelegate(string tableAlias, IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context, SqlTable sqlAstNode);

    /// <summary>
    /// Generates a <c>WHERE</c> condition.
    /// </summary>
    /// <param name="where">The <see cref="WhereBuilder"/>.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="context">The context.</param>
    public delegate void WhereDelegate(WhereBuilder where, IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context);


    /// <summary>
    /// Generates a <c>JOIN</c> condition.
    /// </summary>
    /// <param name="join">The <see cref="JoinBuilder"/>.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="context">The context.</param>
    /// <param name="sqlAstNode">The SQL AST node.</param>
    public delegate void JoinDelegate(JoinBuilder join, IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context, SqlTable sqlAstNode);

    /// <summary>
    /// Generates a <c>ORDER BY</c> clause.
    /// </summary>
    /// <param name="order">The <see cref="OrderByBuilder"/>.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="context">The context.</param>
    public delegate void OrderByDelegate(OrderByBuilder order, IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context);

    /// <summary>
    /// Defines a sort key that is used when doing keyset pagination.
    /// </summary>
    /// <param name="sort">The <see cref="SortKeyBuilder"/>.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="context">The context.</param>
    public delegate void SortKeyDelegate(SortKeyBuilder sort, IReadOnlyDictionary<string, object> arguments, IResolveFieldContext context);

    /// <summary>
    /// Takes the SQL string and the parameters and sends them to a database.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">The SQL parameters.</param>
    /// <returns>A <see cref="IDataReader"/> that is used to fetch the data from the database.</returns>
    /// <remarks>JoinMonster is responsible for closing the <see cref="IDataReader"/>.</remarks>
    public delegate Task<IDataReader> DatabaseCallDelegate(string sql, IReadOnlyDictionary<string, object> parameters);
}

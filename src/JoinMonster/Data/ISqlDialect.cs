using System.Collections.Generic;
using GraphQL;
using GraphQL.Execution;
using JoinMonster.Builders.Clauses;
using JoinMonster.Language.AST;

namespace JoinMonster.Data
{
    /// <summary>
    /// An interface representing a SQL Dialect.
    /// </summary>
    public interface ISqlDialect
    {
        /// <summary>
        /// Quotes a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to quote.</param>
        /// <returns>The quoted <see cref="string"/>.</returns>
        string Quote(string str);

        /// <summary>
        /// Generates a SQL string containing the columns to select.
        /// </summary>
        /// <param name="parentTable">An auto-generated alias for the parent table.</param>
        /// <param name="keys">The keys to select.</param>
        /// <returns>A SQL string containing the columns to select.</returns>
        string CompositeKey(string parentTable, IEnumerable<string> keys);

        /// <summary>
        /// Handles pagination for one-to-many join.
        /// </summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="node">The node.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="context">The context.</param>
        /// <param name="tables">The tables builder.</param>
        /// <param name="compilerContext">The sql compiler context.</param>
        /// <param name="joinCondition">The join condition if any.</param>
        void HandleJoinedOneToManyPaginated(SqlTable parent, SqlTable node,
            IReadOnlyDictionary<string, ArgumentValue> arguments, IResolveFieldContext context, ICollection<string> tables,
            SqlCompilerContext compilerContext, string? joinCondition);

        /// <summary>
        /// Handles pagination at root.
        /// </summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="node">The node.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="context">The context.</param>
        /// <param name="tables">The tables builder.</param>
        /// <param name="compilerContext">The sql compiler context.</param>
        void HandlePaginationAtRoot(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments,
            IResolveFieldContext context, ICollection<string> tables, SqlCompilerContext compilerContext);

        void HandleBatchedOneToManyPaginated(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments,
            IResolveFieldContext context, ICollection<string> tables, IEnumerable<object> batchScope,
            SqlCompilerContext compilerContext);

        void HandleBatchedManyToManyPaginated(Node? parent, SqlTable node, IReadOnlyDictionary<string, ArgumentValue> arguments,
            IResolveFieldContext context, ICollection<string> tables, IEnumerable<object> batchScope,
            SqlCompilerContext compilerContext, string joinCondition);

        string CompileConditions(IEnumerable<WhereCondition> conditions, SqlCompilerContext context);
        string CompileOrderBy(OrderBy junctionOrderBy);
    }
}

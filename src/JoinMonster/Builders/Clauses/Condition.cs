using System;
using System.Collections.Generic;

namespace JoinMonster.Builders.Clauses
{
    public abstract class WhereCondition
    {
        protected WhereCondition(string? table = null)
        {
            Table = table;
        }

        public bool IsNot { get; set; }
        public bool IsOr { get; set; }
        public string? Table { get; }
    }

    public class CompareCondition : WhereCondition
    {
        public CompareCondition(string table, string column, string @operator, object value) : base(table)
        {
            Column = column ?? throw new ArgumentNullException(nameof(column));
            Operator = @operator ?? throw new ArgumentNullException(nameof(@operator));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Column { get; }
        public string Operator { get; }
        public object Value { get; }
    }

    public class CompareColumnsCondition : WhereCondition
    {
        public CompareColumnsCondition(string firstTable, string first, string @operator, string secondTable, string second) : base(firstTable)
        {
            First = first ?? throw new ArgumentNullException(nameof(first));
            Operator = @operator ?? throw new ArgumentNullException(nameof(@operator));
            SecondTable = secondTable ?? throw new ArgumentNullException(nameof(secondTable));
            Second = second ?? throw new ArgumentNullException(nameof(second));
        }

        public string First { get; }
        public string Operator { get; }
        public string Second { get; }
        public string SecondTable { get; }
    }

    public class RawCondition : WhereCondition
    {
        public RawCondition(string sql, IDictionary<string, object>? parameters)
        {
            Sql = sql ?? throw new ArgumentNullException(nameof(sql));
            Parameters = parameters;
        }

        public string Sql { get; }
        public IDictionary<string, object>? Parameters { get; }
    }

    public class NestedCondition : WhereCondition
    {
        public NestedCondition(IEnumerable<WhereCondition> conditions)
        {
            Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
        }

        public IEnumerable<WhereCondition> Conditions { get; }
    }
}

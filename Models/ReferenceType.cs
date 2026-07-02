namespace KofCWSC.DBObjectAnalyzer.Models;

public enum ReferenceType
{
    Unknown = 0,

    ExecuteSqlRaw,

    ExecuteSqlInterpolated,

    FromSqlRaw,

    FromSqlInterpolated,

    SqlQuery,

    SqlCommand,

    SqlText,

    SqlDependency,

    Comment
}
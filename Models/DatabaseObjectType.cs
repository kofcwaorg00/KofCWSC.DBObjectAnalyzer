namespace KofCWSC.DBObjectAnalyzer.Models;

public enum DatabaseObjectType
{
    Unknown = 0,

    StoredProcedure,

    ScalarFunction,

    InlineTableValuedFunction,

    TableValuedFunction,

    View,

    Trigger
}
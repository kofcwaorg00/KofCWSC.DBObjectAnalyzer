namespace KofCWSC.DBObjectAnalyzer.Models;

public static class DatabaseObjectTypeExtensions
{
    public static DatabaseObjectType ToDatabaseObjectType(this string sqlType)
    {
        return sqlType.ToUpperInvariant() switch
        {
            "P" => DatabaseObjectType.StoredProcedure,
            "FN" => DatabaseObjectType.ScalarFunction,
            "IF" => DatabaseObjectType.InlineTableValuedFunction,
            "TF" => DatabaseObjectType.TableValuedFunction,
            "V" => DatabaseObjectType.View,
            "TR" => DatabaseObjectType.Trigger,
            _ => DatabaseObjectType.Unknown
        };
    }
}
namespace KofCWSC.DBObjectAnalyzer.Models;

/// <summary>
/// Describes the type of dependency discovered while parsing SQL.
/// </summary>
public enum DependencyType
{
    Unknown = 0,

    ProcedureCall,

    FunctionCall,

    ViewReference,

    TableReference
}
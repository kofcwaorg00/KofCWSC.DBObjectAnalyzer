namespace KofCWSC.DBObjectAnalyzer.Models;

/// <summary>
/// Represents a dependency discovered within a SQL definition.
/// </summary>
public sealed record Dependency
(
    string ReferencingSchema,
    string ReferencingObject,

    string ReferencedSchema,
    string ReferencedObject,

    DependencyType DependencyType,

    int LineNumber
)
{
    public string ReferencingFullName =>
        $"{ReferencingSchema}.{ReferencingObject}";

    public string ReferencedFullName =>
        $"{ReferencedSchema}.{ReferencedObject}";
}
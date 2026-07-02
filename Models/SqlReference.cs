namespace KofCWSC.DBObjectAnalyzer.Models;

public class SqlReference
{
    /// <summary>
    /// The object containing the reference.
    /// </summary>
    public DatabaseObject ReferencingObject { get; set; } = null!;

    /// <summary>
    /// The object being referenced.
    /// </summary>
    public DatabaseObject ReferencedObject { get; set; } = null!;

    /// <summary>
    /// Dependency type.
    /// </summary>
    public ReferenceType ReferenceType { get; set; }
        = ReferenceType.SqlDependency;

    /// <summary>
    /// Optional notes.
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{ReferencingObject.FullName} -> {ReferencedObject.FullName}";
    }
}
namespace KofCWSC.DBObjectAnalyzer.Models;

public class DatabaseObject
{
    /// <summary>
    /// SQL Server Object ID.
    /// </summary>
    public int ObjectId { get; set; }

    /// <summary>
    /// Schema name (usually dbo).
    /// </summary>
    public string Schema { get; set; } = string.Empty;

    /// <summary>
    /// Object name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// SQL Server type code.
    /// P, FN, IF, TF, V, TR...
    /// </summary>
    public string SqlType { get; set; } = string.Empty;

    /// <summary>
    /// Strongly typed object type.
    /// </summary>
    public DatabaseObjectType ObjectType { get; set; }
        = DatabaseObjectType.Unknown;

    /// <summary>
    /// Complete CREATE PROCEDURE/FUNCTION/VIEW definition.
    /// </summary>
    public string Definition { get; set; } = string.Empty;

    /// <summary>
    /// All source code references found in the API.
    /// </summary>
    public List<SourceReference> SourceReferences { get; } = new();

    /// <summary>
    /// SQL objects referenced by this object.
    /// (Future phase)
    /// </summary>
    public List<SqlReference> References { get; } = new();

    /// <summary>
    /// SQL objects that reference this object.
    /// (Future phase)
    /// </summary>
    public List<SqlReference> ReferencedBy { get; } = new();

    #region Convenience Properties

    public bool HasSourceReferences =>
        SourceReferences.Count > 0;

    public bool HasSqlReferences =>
        References.Count > 0;

    public bool HasReferencingObjects =>
        ReferencedBy.Count > 0;

    public bool IsReferenced =>
        HasSourceReferences || HasReferencingObjects;

    public bool IsCandidateForDeletion =>
        !IsReferenced;

    public int SourceReferenceCount =>
        SourceReferences.Count;

    public int SqlReferenceCount =>
        References.Count;

    public int ReferencedByCount =>
        ReferencedBy.Count;

    public string FullName =>
        $"{Schema}.{Name}";

    #endregion

    public override string ToString()
    {
        return FullName;
    }
    public string DisplayType =>
    ObjectType switch
    {
        DatabaseObjectType.StoredProcedure => "Stored Procedure",
        DatabaseObjectType.ScalarFunction => "Scalar Function",
        DatabaseObjectType.InlineTableValuedFunction => "Inline TVF",
        DatabaseObjectType.TableValuedFunction => "Table TVF",
        DatabaseObjectType.View => "View",
        DatabaseObjectType.Trigger => "Trigger",
        _ => "Unknown"
    };
}
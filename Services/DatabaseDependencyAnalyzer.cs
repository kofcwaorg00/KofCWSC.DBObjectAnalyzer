using KofCWSC.DBObjectAnalyzer.Models;

namespace KofCWSC.DBObjectAnalyzer.Services;

/// <summary>
/// Builds SQL dependency relationships using the SQL parser.
/// </summary>
public class DatabaseDependencyAnalyzer
{
    private readonly SqlDefinitionCleaner _cleaner = new();
    private readonly SqlTokenizer _tokenizer = new();
    private readonly SqlParser _parser = new();

    public DependencyStatistics Statistics { get; } = new();

    public void Analyze(List<DatabaseObject> databaseObjects)
    {
        ArgumentNullException.ThrowIfNull(databaseObjects);

        //
        // Reset statistics
        //
        Statistics.FunctionCalls = 0;
        Statistics.ProcedureCalls = 0;
        Statistics.ViewReferences = 0;

        //
        // Clear existing relationships
        //
        foreach (var obj in databaseObjects)
        {
            obj.References.Clear();
            obj.ReferencedBy.Clear();
        }

        //
        // Build lookup dictionary
        //
        var lookup = databaseObjects.ToDictionary(
            o => o.FullName,
            StringComparer.OrdinalIgnoreCase);

        //
        // Analyze every SQL object
        //
        foreach (var obj in databaseObjects)
        {
            AnalyzeObject(obj, lookup);
        }
    }

    private void AnalyzeObject(
        DatabaseObject databaseObject,
        IReadOnlyDictionary<string, DatabaseObject> lookup)
    {
        if (string.IsNullOrWhiteSpace(databaseObject.Definition))
            return;

        var cleaned = _cleaner.Clean(databaseObject.Definition);

        var tokens = _tokenizer.Tokenize(cleaned);

        var dependencies = _parser.Parse(
            tokens,
            databaseObject.Schema,
            databaseObject.Name);

        foreach (var dependency in dependencies)
        {
            //
            // Update statistics
            //
            switch (dependency.DependencyType)
            {
                case DependencyType.FunctionCall:
                    Statistics.FunctionCalls++;
                    break;

                case DependencyType.ProcedureCall:
                    Statistics.ProcedureCalls++;
                    break;
                case DependencyType.ViewReference:
                    Statistics.ViewReferences++;
                    break;
            }

            //
            // Locate the referenced database object
            //
            if (!lookup.TryGetValue(
                    dependency.ReferencedFullName,
                    out var referencedObject))
            {
                continue;
            }

            //
            // Prevent duplicate edges
            //
            if (databaseObject.References.Any(r =>
                    ReferenceEquals(r.ReferencedObject, referencedObject) &&
                    r.DependencyType == dependency.DependencyType))
            {
                continue;
            }

            //
            // Create one edge and attach it to both objects
            //
            var reference = new SqlReference
            {
                ReferencingObject = databaseObject,
                ReferencedObject = referencedObject,
                ReferenceType = ReferenceType.SqlDependency,
                DependencyType = dependency.DependencyType,
                LineNumber = dependency.LineNumber
            };

            databaseObject.References.Add(reference);

            referencedObject.ReferencedBy.Add(reference);
        }
    }
}
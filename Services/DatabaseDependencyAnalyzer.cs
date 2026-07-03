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

    public void Analyze(List<DatabaseObject> databaseObjects)
    {
        ArgumentNullException.ThrowIfNull(databaseObjects);

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

        //
        // Build reverse relationships
        //
        BuildReferencedBy(databaseObjects);
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
            var key = dependency.ReferencedFullName;

            if (!lookup.TryGetValue(key, out var referencedObject))
                continue;

            //
            // Prevent duplicates
            //
            if (databaseObject.References.Any(r =>
                    ReferenceEquals(r.ReferencedObject, referencedObject)))
            {
                continue;
            }

            databaseObject.References.Add(new SqlReference
            {
                ReferencingObject = databaseObject,
                ReferencedObject = referencedObject,
                ReferenceType = ReferenceType.SqlDependency,
                Notes = dependency.DependencyType.ToString()
            });
        }
    }

    private static void BuildReferencedBy(
        IEnumerable<DatabaseObject> databaseObjects)
    {
        foreach (var obj in databaseObjects)
        {
            foreach (var reference in obj.References)
            {
                reference.ReferencedObject.ReferencedBy.Add(reference);
            }
        }
    }
}
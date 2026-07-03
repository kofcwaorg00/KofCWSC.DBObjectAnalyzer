using System.Text.RegularExpressions;
using KofCWSC.DBObjectAnalyzer.Models;

namespace KofCWSC.DBObjectAnalyzer.Services;

public class LegacyDatabaseDependencyAnalyzer
{
    public void Analyze(List<DatabaseObject> databaseObjects)
    {
        ArgumentNullException.ThrowIfNull(databaseObjects);

        //
        // Clear previous analysis
        //
        foreach (var obj in databaseObjects)
        {
            obj.References.Clear();
            obj.ReferencedBy.Clear();
        }

        //
        // Lookup by object name
        //
        var lookup = databaseObjects.ToDictionary(
            o => o.Name,
            StringComparer.OrdinalIgnoreCase);

        //
        // Longest names first.
        // Prevents funSYS_GetName matching funSYS_Get.
        //
        var objectNames =
            databaseObjects
                .Select(o => Regex.Escape(o.Name))
                .OrderByDescending(n => n.Length);

        var pattern =
            $@"(?<![A-Za-z0-9_])({string.Join("|", objectNames)})(?![A-Za-z0-9_])";

        var regex = new Regex(
            pattern,
            RegexOptions.IgnoreCase |
            RegexOptions.Compiled);

        foreach (var databaseObject in databaseObjects)
        {
            if (string.IsNullOrWhiteSpace(databaseObject.Definition))
                continue;

            var matches = regex.Matches(databaseObject.Definition);

            foreach (Match match in matches)
            {
                if (!lookup.TryGetValue(
                        match.Value,
                        out var referencedObject))
                {
                    continue;
                }

                //
                // Ignore self references.
                //
                if (ReferenceEquals(databaseObject, referencedObject))
                    continue;

                //
                // Already recorded?
                //
                if (databaseObject.References.Any(r =>
                    ReferenceEquals(
                        r.ReferencedObject,
                        referencedObject)))
                {
                    continue;
                }

                var reference = new SqlReference
                {
                    ReferencingObject = databaseObject,

                    ReferencedObject = referencedObject,

                    ReferenceType = ReferenceType.SqlDependency,

                    Notes = "Definition Scan"
                };

                databaseObject.References.Add(reference);

                referencedObject.ReferencedBy.Add(reference);
            }
        }
    }
}
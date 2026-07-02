using KofCWSC.DBObjectAnalyzer.Models;

namespace KofCWSC.DBObjectAnalyzer.Services;

public class CandidateAnalyzer
{
    public AnalysisResults Analyze(IEnumerable<DatabaseObject> databaseObjects)
    {
        var objects = databaseObjects
            .OrderBy(o => o.Schema)
            .ThenBy(o => o.Name)
            .ToList();

        var results = new AnalysisResults
        {
            DatabaseObjects = objects,

            Candidates = objects
                .Where(o => o.IsCandidateForDeletion)
                .OrderBy(o => o.ObjectType)
                .ThenBy(o => o.Schema)
                .ThenBy(o => o.Name)
                .ToList(),

            ReferencedObjects = objects
                .Where(o => o.IsReferenced)
                .OrderByDescending(o => o.SourceReferenceCount)
                .ThenBy(o => o.Name)
                .ToList(),

            ObjectTypeCounts = objects
                .GroupBy(o => o.ObjectType)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count())
        };

        return results;
    }
}
using KofCWSC.DBObjectAnalyzer.Models;

namespace KofCWSC.DBObjectAnalyzer.Models;

public class AnalysisResults
{
    public List<DatabaseObject> DatabaseObjects { get; init; } = new();

    public List<DatabaseObject> Candidates { get; init; } = new();

    public List<DatabaseObject> ReferencedObjects { get; init; } = new();

    public Dictionary<DatabaseObjectType, int> ObjectTypeCounts { get; init; } = new();

    public int TotalObjects => DatabaseObjects.Count;

    public int CandidateCount => Candidates.Count;

    public int ReferencedCount => ReferencedObjects.Count;
    public ScanStatistics? ScanStatistics { get; set; }
}
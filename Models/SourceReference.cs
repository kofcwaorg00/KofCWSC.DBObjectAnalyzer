namespace KofCWSC.DBObjectAnalyzer.Models;

public class SourceReference
{
    public string FullPath { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public int LineNumber { get; set; }

    public string LineText { get; set; } = string.Empty;

    public ReferenceType ReferenceType { get; set; } = ReferenceType.Unknown;
}
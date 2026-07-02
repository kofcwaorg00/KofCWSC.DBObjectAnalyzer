namespace KofCWSC.DBObjectAnalyzer.Models;

public class ScanStatistics
{
    public int FilesScanned { get; set; }

    public int LinesScanned { get; set; }

    public int MatchesFound { get; set; }

    public TimeSpan Duration { get; set; }

    public override string ToString()
    {
        return
            $"Files={FilesScanned:N0}, " +
            $"Lines={LinesScanned:N0}, " +
            $"Matches={MatchesFound:N0}, " +
            $"Duration={Duration.TotalSeconds:F2}s";
    }
}
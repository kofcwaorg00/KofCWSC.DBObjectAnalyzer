using System.Diagnostics;
using System.Text.RegularExpressions;
using KofCWSC.DBObjectAnalyzer.Configuration;
using KofCWSC.DBObjectAnalyzer.Models;

namespace KofCWSC.DBObjectAnalyzer.Services;

public class SourceScanner
{
    private readonly AnalyzerSettings _settings;

    public SourceScanner(AnalyzerSettings settings)
    {
        _settings = settings;
    }

    public async Task<ScanStatistics> ScanAsync(
        List<DatabaseObject> databaseObjects,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseObjects);

        var stopwatch = Stopwatch.StartNew();

        var statistics = new ScanStatistics();

        var objectLookup = databaseObjects.ToDictionary(
            o => o.Name,
            StringComparer.OrdinalIgnoreCase);

        //
        // Longest names first so
        // uspMemberSave matches before uspMember
        //
        var pattern =
            @"\b(" +
            string.Join("|",
                databaseObjects
                    .Select(o => Regex.Escape(o.Name))
                    .OrderByDescending(n => n.Length)) +
            @")\b";

        var regex = new Regex(
            pattern,
            RegexOptions.IgnoreCase |
            RegexOptions.Compiled);

        foreach (var file in GetSourceFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();

            statistics.FilesScanned++;

            var lines = await File.ReadAllLinesAsync(file, cancellationToken);

            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                statistics.LinesScanned++;

                var line = lines[lineNumber];

                var matches = regex.Matches(line);

                if (matches.Count == 0)
                    continue;

                foreach (Match match in matches)
                {
                    if (!objectLookup.TryGetValue(
                            match.Value,
                            out var databaseObject))
                    {
                        continue;
                    }

                    databaseObject.SourceReferences.Add(
                        new SourceReference
                        {
                            FullPath = file,
                            FileName = Path.GetFileName(file),
                            LineNumber = lineNumber + 1,
                            LineText = line.Trim(),

                            ReferenceType =
                                DetermineReferenceType(line)
                        });

                    statistics.MatchesFound++;
                }
            }
        }

        stopwatch.Stop();

        statistics.Duration = stopwatch.Elapsed;

        return statistics;
    }

    private IEnumerable<string> GetSourceFiles()
    {
        foreach (var folder in _settings.SourceFolders)
        {
            if (!Directory.Exists(folder))
                continue;

            foreach (var file in Directory.EnumerateFiles(
                         folder,
                         "*.*",
                         SearchOption.AllDirectories))
            {
                var extension = Path.GetExtension(file);

                if (!_settings.FileExtensions.Contains(
                        extension,
                        StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (ShouldIgnore(file))
                    continue;

                yield return file;
            }
        }
    }

    private bool ShouldIgnore(string file)
    {
        foreach (var ignore in _settings.IgnoreFolders)
        {
            if (file.Contains(
                Path.DirectorySeparatorChar + ignore + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static ReferenceType DetermineReferenceType(string line)
    {
        var text = line.Trim();

        if (text.StartsWith("//"))
            return ReferenceType.Comment;

        if (text.StartsWith("--"))
            return ReferenceType.Comment;

        if (line.Contains("ExecuteSqlRaw",
                StringComparison.OrdinalIgnoreCase))
            return ReferenceType.ExecuteSqlRaw;

        if (line.Contains("ExecuteSqlInterpolated",
                StringComparison.OrdinalIgnoreCase))
            return ReferenceType.ExecuteSqlInterpolated;

        if (line.Contains("FromSqlRaw",
                StringComparison.OrdinalIgnoreCase))
            return ReferenceType.FromSqlRaw;

        if (line.Contains("FromSqlInterpolated",
                StringComparison.OrdinalIgnoreCase))
            return ReferenceType.FromSqlInterpolated;

        if (line.Contains("SqlQuery",
                StringComparison.OrdinalIgnoreCase))
            return ReferenceType.SqlQuery;

        if (line.Contains("SqlCommand",
                StringComparison.OrdinalIgnoreCase))
            return ReferenceType.SqlCommand;

        return ReferenceType.Unknown;
    }
}
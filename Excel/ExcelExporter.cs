using ClosedXML.Excel;
using KofCWSC.DBObjectAnalyzer.Models;

namespace KofCWSC.DBObjectAnalyzer.Excel;

public class ExcelExporter
{
    public async Task<string> ExportAsync(
        AnalysisResults analysis,
        string outputFolder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(analysis);

        Directory.CreateDirectory(outputFolder);

        using var workbook = new XLWorkbook();

        CreateSummaryWorksheet(workbook, analysis);

        CreateDatabaseObjectsWorksheet(workbook, analysis);

        CreateDependenciesWorksheet(workbook, analysis);

        CreateReferencedByWorksheet(workbook, analysis);

        CreateCandidatesWorksheet(workbook, analysis);

        CreateSourceReferencesWorksheet(workbook, analysis);

        var fileName =
            $"DatabaseAnalysis_{DateTime.Now:yyyy-MM-dd_HHmmss}.xlsx";

        var fullPath = Path.Combine(outputFolder, fileName);

        await Task.Run(() => workbook.SaveAs(fullPath), cancellationToken);

        return fullPath;
    }

    private static void CreateSummaryWorksheet(
        XLWorkbook workbook,
        AnalysisResults analysis)
    {
        var ws = new ExcelWorksheet(workbook, "Summary");

        ws.Title("Database Object Analysis");

        ws.Headers("Metric", "Value");

        ws.Row("Total Objects", analysis.TotalObjects);
        ws.Row("Referenced Objects", analysis.ReferencedCount);
        ws.Row("Candidates", analysis.CandidateCount);

        foreach (var item in analysis.ObjectTypeCounts)
        {
            ws.Row(item.Key.ToString(), item.Value);
        }

        if (analysis.ScanStatistics != null)
        {
            ws.Row("", "");

            ws.Row("Files Scanned",
                analysis.ScanStatistics.FilesScanned);

            ws.Row("Lines Scanned",
                analysis.ScanStatistics.LinesScanned);

            ws.Row("Matches Found",
                analysis.ScanStatistics.MatchesFound);

            ws.Row("Duration",
                analysis.ScanStatistics.Duration);
        }

        ws.Finish();
    }

    private static void CreateDatabaseObjectsWorksheet(
        XLWorkbook workbook,
        AnalysisResults analysis)
    {
        var ws = new ExcelWorksheet(workbook, "Database Objects");

        ws.Title("Database Objects");

        ws.Headers(
    "Object",
    "Type",
    "API References",
    "SQL Outbound",
    "SQL Inbound",
    "Candidate");

        foreach (var obj in analysis.DatabaseObjects.OrderBy(o => o.FullName))
        {
            ws.Row(
    obj.FullName,
    obj.DisplayType,
    obj.SourceReferenceCount,
    obj.SqlReferenceCount,
    obj.ReferencedByCount,
    obj.IsCandidateForDeletion ? "Yes" : "No");

            ws.HighlightStatus(
                obj.IsCandidateForDeletion,
                6);
        }

        ws.Finish();
    }

    private static void CreateCandidatesWorksheet(
        XLWorkbook workbook,
        AnalysisResults analysis)
    {
        var ws = new ExcelWorksheet(workbook, "Candidates");

        ws.Title("Deletion Candidates");

        ws.Headers(
    "Object",
    "Type",
    "API References",
    "SQL Outbound",
    "SQL Inbound");

        foreach (var obj in analysis.Candidates.OrderBy(o => o.FullName))
        {
            ws.Row(
    obj.FullName,
    obj.DisplayType,
    obj.SourceReferenceCount,
    obj.SqlReferenceCount,
    obj.ReferencedByCount);
        }

        ws.Finish();
    }

    private static void CreateSourceReferencesWorksheet(
        XLWorkbook workbook,
        AnalysisResults analysis)
    {
        var ws = new ExcelWorksheet(workbook, "Source References");

        ws.Title("Source Code References");

        ws.Headers(
            "Object",
            "File",
            "Line",
            "Reference Type",
            "Source");

        foreach (var obj in analysis.ReferencedObjects)
        {
            foreach (var reference in obj.SourceReferences)
            {
                ws.Row(
                    obj.Name,
                    reference.FileName,
                    reference.LineNumber,
                    reference.ReferenceType,
                    reference.LineText);
            }
        }

        ws.Finish();
    }
    private static void CreateDependenciesWorksheet(
    XLWorkbook workbook,
    AnalysisResults analysis)
    {
        var ws = new ExcelWorksheet(workbook, "Dependencies");

        ws.Title("SQL Dependencies");

        ws.Headers(
    "Referencing Object",
    "Referencing Type",
    "Referenced Object",
    "Referenced Type",
    "Dependency Type",
    "Line");

        foreach (var obj in analysis.DatabaseObjects
            .OrderBy(o => o.FullName))
        {
            foreach (var reference in obj.References
                .OrderBy(r => r.ReferencedObject.FullName))
            {
                ws.Row(
    obj.FullName,
    obj.DisplayType,
    reference.ReferencedObject.FullName,
    reference.ReferencedObject.DisplayType,
    reference.DependencyType,
    reference.LineNumber);
            }
        }

        ws.Finish();
    }
    private static void CreateReferencedByWorksheet(
    XLWorkbook workbook,
    AnalysisResults analysis)
    {
        var ws = new ExcelWorksheet(workbook, "Referenced By");

        ws.Title("Objects Referenced By");

        ws.Headers(
    "Object",
    "Object Type",
    "Referenced By",
    "Caller Type",
    "Dependency Type",
    "Line");

        foreach (var obj in analysis.DatabaseObjects
            .OrderBy(o => o.FullName))
        {
            foreach (var reference in obj.ReferencedBy
                .OrderBy(r => r.ReferencingObject.FullName))
            {
                ws.Row(
    obj.FullName,
    obj.DisplayType,
    reference.ReferencingObject.FullName,
    reference.ReferencingObject.DisplayType,
    reference.DependencyType,
    reference.LineNumber);
            }
        }

        ws.Finish();
    }
}
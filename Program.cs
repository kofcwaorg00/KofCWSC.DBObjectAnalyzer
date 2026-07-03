using KofCWSC.DBObjectAnalyzer.Configuration;
using KofCWSC.DBObjectAnalyzer.Excel;
using KofCWSC.DBObjectAnalyzer.Models;
using KofCWSC.DBObjectAnalyzer.Services;
using Microsoft.Extensions.Configuration;

Console.WriteLine("KofCWSC Database Object Analyzer");
Console.WriteLine("================================");
Console.WriteLine();

//
// Load Configuration
//
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var settings = configuration
    .GetSection("Analyzer")
    .Get<AnalyzerSettings>()
    ?? throw new InvalidOperationException("Analyzer settings not found.");

var connectionString = configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("Database connection string not found.");

Console.WriteLine($"Output Folder : {settings.OutputFolder}");
Console.WriteLine();

foreach (var folder in settings.SourceFolders)
{
    Console.WriteLine($"Source Folder : {folder}");
}

Console.WriteLine();

//
// Read Database Objects
//
Console.WriteLine("Reading database objects...");

var reader = new DatabaseObjectReader(connectionString);

List<DatabaseObject> databaseObjects =
    await reader.ReadObjectsAsync();

Console.WriteLine($"Objects Found : {databaseObjects.Count:N0}");
Console.WriteLine();

//
// Scan C# Source
//
Console.WriteLine("Scanning C# source...");

var sourceScanner = new SourceScanner(settings);

var scanStatistics =
    await sourceScanner.ScanAsync(databaseObjects);

Console.WriteLine(scanStatistics);
Console.WriteLine();

//
// Analyze SQL Dependencies
//
Console.WriteLine("Analyzing SQL dependencies...");

var dependencyAnalyzer = new DatabaseDependencyAnalyzer();

dependencyAnalyzer.Analyze(databaseObjects);

Console.WriteLine("Dependency analysis complete.");
Console.WriteLine();

//
// Analyze Candidates
//
Console.WriteLine("Analyzing candidates...");

var analyzer = new CandidateAnalyzer();

var analysis = analyzer.Analyze(databaseObjects);

analysis.ScanStatistics = scanStatistics;

Console.WriteLine("Analysis complete.");
Console.WriteLine();

//
// Parser Demo
//
if (settings.RunParserDemo)
{
    Console.WriteLine("Running parser demo...");
    Console.WriteLine();

    var cleaner = new SqlDefinitionCleaner();
    var tokenizer = new SqlTokenizer();
    var parser = new SqlParser();

    foreach (var obj in databaseObjects
        .OrderBy(o => o.FullName))
    {
        if (string.IsNullOrWhiteSpace(obj.Definition))
            continue;
        
        var cleanSql = cleaner.Clean(obj.Definition);
        ////////if (obj.FullName == "dbo.funSYS_GetCouncilForUserID")
        ////////{
        ////////    Console.WriteLine(cleanSql);
        ////////}
        var tokens = tokenizer.Tokenize(cleanSql);

        var dependencies = parser.Parse(
                tokens,
                obj.Schema,
                obj.Name)
                .ToList();

        if (dependencies.Count == 0)
            continue;

        Console.WriteLine(new string('-', 70));
        Console.WriteLine(obj.FullName);
        Console.WriteLine(new string('-', 70));

        foreach (var dependency in dependencies.Distinct())
        {
            Console.WriteLine(
                $"{dependency.DependencyType,-18} {dependency.ReferencedFullName}");
        }

        Console.WriteLine();
    }
}





//
// Console Summary
//
Console.WriteLine("Summary");
Console.WriteLine("-------");

Console.WriteLine($"Total Objects            : {analysis.TotalObjects,5}");
Console.WriteLine($"API Referenced           : {databaseObjects.Count(o => o.HasSourceReferences),5}");
Console.WriteLine($"SQL Referenced           : {databaseObjects.Count(o => o.HasReferencingObjects),5}");
Console.WriteLine($"Possible Candidates      : {analysis.CandidateCount,5}");

Console.WriteLine();

Console.WriteLine("Objects by Type");
Console.WriteLine("---------------");

foreach (var item in analysis.ObjectTypeCounts.OrderBy(o => o.Key))
{
    Console.WriteLine($"{item.Key,-35} {item.Value,5}");
}

Console.WriteLine();

Console.WriteLine("Top API Referenced Objects");
Console.WriteLine("--------------------------");

foreach (var obj in analysis.ReferencedObjects
    .OrderByDescending(o => o.SourceReferenceCount)
    .Take(15))
{
    Console.WriteLine(
        $"{obj.FullName,-50} {obj.SourceReferenceCount,4}");
}

Console.WriteLine();

Console.WriteLine("Top SQL Referenced Objects");
Console.WriteLine("--------------------------");

foreach (var obj in databaseObjects
    .Where(o => o.HasReferencingObjects)
    .OrderByDescending(o => o.ReferencedByCount)
    .Take(15))
{
    Console.WriteLine(
        $"{obj.FullName,-50} {obj.ReferencedByCount,4}");
}

Console.WriteLine();

Console.WriteLine("Top SQL Consumers");
Console.WriteLine("-----------------");

foreach (var obj in databaseObjects
    .Where(o => o.HasSqlReferences)
    .OrderByDescending(o => o.SqlReferenceCount)
    .Take(15))
{
    Console.WriteLine(
        $"{obj.FullName,-50} {obj.SqlReferenceCount,4}");
}

Console.WriteLine();

//
// Export Workbook
//
Console.WriteLine("Creating Excel workbook...");

var exporter = new ExcelExporter();

var workbook = await exporter.ExportAsync(
    analysis,
    settings.OutputFolder);

Console.WriteLine();

Console.WriteLine("Workbook Created");
Console.WriteLine("----------------");
Console.WriteLine(workbook);

Console.WriteLine();
Console.WriteLine("Analysis Complete.");

Console.WriteLine();
Console.Write("Press any key to exit...");
Console.ReadKey();
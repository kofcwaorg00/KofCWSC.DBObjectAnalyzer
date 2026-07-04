namespace KofCWSC.DBObjectAnalyzer.Models;

public class DependencyStatistics
{
    public int FunctionCalls { get; set; }

    public int ProcedureCalls { get; set; }

    public int TotalDependencies =>
        FunctionCalls + ProcedureCalls + ViewReferences;

    public override string ToString()
    {
        return
            $"""
            Dependencies Found
            ------------------
            Function Calls : {FunctionCalls,5}
            Procedure Calls: {ProcedureCalls,5}
            View References: {ViewReferences,5}
            Total          : {TotalDependencies,5}
            """;
    }
    public int ViewReferences { get; set; }
}
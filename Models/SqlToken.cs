namespace KofCWSC.DBObjectAnalyzer.Models;

public class SqlToken
{
    public SqlTokenType TokenType { get; init; }

    public string Value { get; init; } = "";

    public int Position { get; init; }

    public override string ToString()
    {
        return $"{TokenType}: {Value}";
    }
}
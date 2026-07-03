namespace KofCWSC.DBObjectAnalyzer.Models;

/// <summary>
/// Represents a single SQL token.
/// </summary>
public sealed record SqlToken
(
    SqlTokenType TokenType,
    string Value,
    int Line,
    int Column,
    int Position
);
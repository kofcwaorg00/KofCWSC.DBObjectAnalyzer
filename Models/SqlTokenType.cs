namespace KofCWSC.DBObjectAnalyzer.Models;

/// <summary>
/// SQL token types produced by SqlTokenizer.
/// </summary>
public enum SqlTokenType
{
    Unknown = 0,

    Keyword,

    Identifier,

    Number,

    Symbol
}
using KofCWSC.DBObjectAnalyzer.Models;

namespace KofCWSC.DBObjectAnalyzer.Services;

/// <summary>
/// Converts cleaned SQL text into a sequence of SQL tokens.
/// </summary>
public class SqlTokenizer
{
    public IEnumerable<SqlToken> Tokenize(string sql)
    {
        ArgumentNullException.ThrowIfNull(sql);

        if (string.IsNullOrWhiteSpace(sql))
            yield break;

        // Tokenization logic will be implemented
        // incrementally during V2.

        yield break;
    }
}
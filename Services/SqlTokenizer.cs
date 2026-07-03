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

        int position = 0;

        while (position < sql.Length)
        {
            char c = sql[position];

            // Skip whitespace
            if (char.IsWhiteSpace(c))
            {
                position++;
                continue;
            }

            // Identifier
            if (char.IsLetter(c) || c == '_')
            {
                int start = position;

                position++;

                while (position < sql.Length)
                {
                    c = sql[position];

                    if (char.IsLetterOrDigit(c) || c == '_')
                    {
                        position++;
                    }
                    else
                    {
                        break;
                    }
                }

                yield return new SqlToken(
                    SqlTokenType.Identifier,
                    sql.Substring(start, position - start),
                    1,          // Line (temporary)
                    start + 1,  // Column (temporary)
                    start);

                continue;
            }

            //
            // Unknown character
            //
            position++;
        }
    }
}
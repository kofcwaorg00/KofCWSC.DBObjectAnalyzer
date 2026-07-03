using KofCWSC.DBObjectAnalyzer.Models;

namespace KofCWSC.DBObjectAnalyzer.Services;

/// <summary>
/// Converts cleaned SQL text into a sequence of SQL tokens.
/// </summary>
public class SqlTokenizer
{
    private static readonly HashSet<string> s_keywords =
    new(StringComparer.OrdinalIgnoreCase)
    {
        "SELECT",
        "FROM",
        "WHERE",
        "JOIN",
        "LEFT",
        "RIGHT",
        "INNER",
        "OUTER",
        "ON",
        "EXEC",
        "EXECUTE",
        "INSERT",
        "UPDATE",
        "DELETE",
        "MERGE",
        "INTO",
        "VALUES",
        "SET",
        "DECLARE",
        "IF",
        "BEGIN",
        "END",
        "AS",
        "RETURN"
    };
    public IEnumerable<SqlToken> Tokenize(string sql)
    {
        ArgumentNullException.ThrowIfNull(sql);

        if (string.IsNullOrWhiteSpace(sql))
            yield break;

        int position = 0;

        while (position < sql.Length)
        {
            if (char.IsWhiteSpace(sql[position]))
            {
                position++;
                continue;
            }

            if (TryReadIdentifier(sql, ref position, out var token))
            {
                yield return token;
                continue;
            }

            if (TryReadSymbol(sql, ref position, out token))
            {
                yield return token;
                continue;
            }

            position++;
        }
    }
    private bool TryReadIdentifier(string sql,ref int position,out SqlToken token)
    {
        token = default!;

        // Must start with a letter or underscore.
        if (position >= sql.Length)
            return false;

        char c = sql[position];

        if (!char.IsLetter(c) && c != '_')
            return false;

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

        var value = sql.Substring(start, position - start);

        var tokenType = s_keywords.Contains(value)
            ? SqlTokenType.Keyword
            : SqlTokenType.Identifier;

        token = new SqlToken(
            tokenType,
            value,
            1,              // Line (we'll improve this later)
            start + 1,      // Column
            start);         // Character position

        return true;
    }
    private static readonly HashSet<char> Symbols =
[
    '.',
    ',',
    '(',
    ')',
    ';',
    '=',
    '*',
    '+',
    '-',
    '/'
];

    private bool TryReadSymbol(
        string sql,
        ref int position,
        out SqlToken token)
    {
        token = default!;

        if (position >= sql.Length)
            return false;

        char c = sql[position];

        if (!Symbols.Contains(c))
            return false;

        token = new SqlToken(
            SqlTokenType.Symbol,
            c.ToString(),
            1,
            position + 1,
            position);

        position++;

        return true;
    }
}
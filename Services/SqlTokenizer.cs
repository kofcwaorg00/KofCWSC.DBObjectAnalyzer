using System.Text;
using KofCWSC.DBObjectAnalyzer.Models;

namespace KofCWSC.DBObjectAnalyzer.Services;

public class SqlTokenizer
{
    private static readonly HashSet<string> Keywords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "SELECT",
            "FROM",
            "JOIN",
            "INNER",
            "LEFT",
            "RIGHT",
            "FULL",
            "OUTER",
            "WHERE",
            "GROUP",
            "ORDER",
            "BY",
            "HAVING",
            "UNION",
            "ALL",
            "INSERT",
            "UPDATE",
            "DELETE",
            "MERGE",
            "EXEC",
            "EXECUTE",
            "AS",
            "ON",
            "INTO",
            "VALUES"
        };

    private static readonly HashSet<char> Symbols =
        new()
        {
            '(',
            ')',
            '.',
            ',',
            ';',
            '=',
            '+',
            '-',
            '*',
            '/',
            '[',
            ']'
        };

    public List<SqlToken> Tokenize(string sql)
    {
        var tokens = new List<SqlToken>();

        if (string.IsNullOrWhiteSpace(sql))
            return tokens;

        int position = 0;

        while (position < sql.Length)
        {
            char c = sql[position];

            if (char.IsWhiteSpace(c))
            {
                position++;
                continue;
            }

            if (Symbols.Contains(c))
            {
                tokens.Add(new SqlToken
                {
                    TokenType = SqlTokenType.Symbol,
                    Value = c.ToString(),
                    Position = position
                });

                position++;
                continue;
            }

            if (char.IsDigit(c))
            {
                int start = position;

                while (position < sql.Length &&
                       (char.IsDigit(sql[position]) ||
                        sql[position] == '.'))
                {
                    position++;
                }

                tokens.Add(new SqlToken
                {
                    TokenType = SqlTokenType.Number,
                    Value = sql[start..position],
                    Position = start
                });

                continue;
            }

            if (char.IsLetter(c) || c == '_' || c == '#')
            {
                int start = position;

                while (position < sql.Length &&
                       (char.IsLetterOrDigit(sql[position]) ||
                        sql[position] == '_' ||
                        sql[position] == '#'))
                {
                    position++;
                }

                var value = sql[start..position];

                tokens.Add(new SqlToken
                {
                    TokenType = Keywords.Contains(value)
                        ? SqlTokenType.Keyword
                        : SqlTokenType.Identifier,

                    Value = value,

                    Position = start
                });

                continue;
            }

            // Ignore everything else for now.
            position++;
        }

        return tokens;
    }
}
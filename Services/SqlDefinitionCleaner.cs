using System.Text;
using System.Text.RegularExpressions;

namespace KofCWSC.DBObjectAnalyzer.Services;

public class SqlDefinitionCleaner
{
    public string Clean(string definition)
    {
        if (string.IsNullOrWhiteSpace(definition))
            return string.Empty;

        var sql = Normalize(definition);

        sql = RemoveStringLiterals(sql);

        sql = RemoveBlockComments(sql);

        sql = RemoveSingleLineComments(sql);

        sql = CollapseBlankLines(sql);

        return sql;
    }

    private static string Normalize(string sql)
    {
        return sql
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Replace("\t", " ");
    }

    private static string RemoveBlockComments(string sql)
    {
        return Regex.Replace(
            sql,
            @"/\*.*?\*/",
            "",
            RegexOptions.Singleline);
    }

    private static string RemoveSingleLineComments(string sql)
    {
        var builder = new StringBuilder();

        using var reader = new StringReader(sql);

        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            var index = line.IndexOf("--");

            if (index >= 0)
                line = line.Substring(0, index);

            builder.AppendLine(line);
        }

        return builder.ToString();
    }

    private static string RemoveStringLiterals(string sql)
    {
        return Regex.Replace(
            sql,
            @"'([^']|'')*'",
            "''");
    }

    private static string CollapseBlankLines(string sql)
    {
        return Regex.Replace(
            sql,
            @"(\n\s*){2,}",
            "\n");
    }
}
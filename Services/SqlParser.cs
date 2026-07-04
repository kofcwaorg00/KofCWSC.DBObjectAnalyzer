using KofCWSC.DBObjectAnalyzer.Models;

namespace KofCWSC.DBObjectAnalyzer.Services;

/// <summary>
/// Parses SQL tokens into database dependencies.
/// </summary>
public class SqlParser
{
    public IEnumerable<Dependency> Parse(
    IEnumerable<SqlToken> tokens,
    string referencingSchema,
    string referencingObject)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        var list = tokens.ToList();

        int start = FindBodyStart(list);

        for (int i = start; i <= list.Count - 4; i++)
        {
            if (IsProcedureCall(list, i))
            {
                string schema;
                string name;

                if (i + 3 < list.Count &&
                    list[i + 2].TokenType == SqlTokenType.Symbol &&
                    list[i + 2].Value == ".")
                {
                    schema = list[i + 1].Value;
                    name = list[i + 3].Value;
                }
                else
                {
                    // Assume current schema when omitted
                    schema = referencingSchema;
                    name = list[i + 1].Value;
                }

                yield return new Dependency(
                    referencingSchema,
                    referencingObject,
                    schema,
                    name,
                    DependencyType.ProcedureCall,
                    list[i].Line);

                continue;
            }

            if (IsFunctionCall(list, i))
            {
                yield return new Dependency(
                    referencingSchema,
                    referencingObject,
                    list[i].Value,
                    list[i + 2].Value,
                    DependencyType.FunctionCall,
                    list[i].Line);

                continue;
            }
            if (IsViewReference(list, i))
            {
                string schema;
                string objectName;

                if (list[i + 2].TokenType == SqlTokenType.Symbol &&
                    list[i + 2].Value == ".")
                {
                    // dbo.vewSomething
                    schema = list[i + 1].Value;
                    objectName = list[i + 3].Value;
                }
                else
                {
                    // vewSomething
                    schema = "dbo";     // Default schema
                    objectName = list[i + 1].Value;
                }

                yield return new Dependency(
                    referencingSchema,
                    referencingObject,
                    schema,
                    objectName,
                    DependencyType.ViewReference,
                    list[i].Line);

                continue;
            }
        }
    }

    
    private static bool IsFunctionCall(
    IReadOnlyList<SqlToken> tokens,
    int index)
    {
        if (index + 3 >= tokens.Count)
            return false;

        return
            tokens[index].TokenType == SqlTokenType.Identifier &&
            tokens[index + 1].TokenType == SqlTokenType.Symbol &&
            tokens[index + 1].Value == "." &&
            tokens[index + 2].TokenType == SqlTokenType.Identifier &&
            tokens[index + 3].TokenType == SqlTokenType.Symbol &&
            tokens[index + 3].Value == "(";
    }
    private static int FindBodyStart(IReadOnlyList<SqlToken> tokens)
    {
        for (int i = 0; i < tokens.Count - 1; i++)
        {
            if (tokens[i].TokenType != SqlTokenType.Keyword)
                continue;

            if (!tokens[i].Value.Equals("CREATE", StringComparison.OrdinalIgnoreCase) &&
                !tokens[i].Value.Equals("ALTER", StringComparison.OrdinalIgnoreCase))
                continue;

            if (tokens[i + 1].TokenType != SqlTokenType.Keyword)
                continue;

            switch (tokens[i + 1].Value.ToUpperInvariant())
            {
                case "FUNCTION":
                case "PROCEDURE":
                case "PROC":
                case "TRIGGER":

                    // Look for BEGIN (preferred) or AS.
                    for (int j = i + 2; j < tokens.Count; j++)
                    {
                        if (tokens[j].TokenType != SqlTokenType.Keyword)
                            continue;

                        if (tokens[j].Value.Equals("BEGIN", StringComparison.OrdinalIgnoreCase))
                            return j + 1;

                        if (tokens[j].Value.Equals("AS", StringComparison.OrdinalIgnoreCase))
                            return j + 1;
                    }

                    break;

                case "VIEW":

                    // Views begin immediately after AS.
                    for (int j = i + 2; j < tokens.Count; j++)
                    {
                        if (tokens[j].TokenType == SqlTokenType.Keyword &&
                            tokens[j].Value.Equals("AS", StringComparison.OrdinalIgnoreCase))
                        {
                            return j + 1;
                        }
                    }

                    break;
            }
        }

        return 0;
    }
    private static bool IsProcedureCall(
    IReadOnlyList<SqlToken> tokens,
    int index)
    {
        if (index + 1 >= tokens.Count)
            return false;

        // Must start with EXEC or EXECUTE
        if (tokens[index].TokenType != SqlTokenType.Keyword)
            return false;

        if (!tokens[index].Value.Equals("EXEC", StringComparison.OrdinalIgnoreCase) &&
            !tokens[index].Value.Equals("EXECUTE", StringComparison.OrdinalIgnoreCase))
            return false;

        // EXEC dbo.uspSomething
        if (index + 3 < tokens.Count &&
            tokens[index + 1].TokenType == SqlTokenType.Identifier &&
            tokens[index + 2].TokenType == SqlTokenType.Symbol &&
            tokens[index + 2].Value == "." &&
            tokens[index + 3].TokenType == SqlTokenType.Identifier)
        {
            return true;
        }

        // EXEC uspSomething
        if (tokens[index + 1].TokenType == SqlTokenType.Identifier)
        {
            return true;
        }

        return false;
    }
    private static bool IsViewReference(
    IReadOnlyList<SqlToken> tokens,
    int index)
    {
        if (tokens[index].TokenType != SqlTokenType.Keyword)
            return false;

        var keyword = tokens[index].Value.ToUpperInvariant();

        if (keyword != "FROM" &&
            keyword != "JOIN")
        {
            return false;
        }

        //
        // Pattern:
        // FROM dbo.vewSomething
        //
        if (index + 3 < tokens.Count)
        {
            if (tokens[index + 1].TokenType == SqlTokenType.Identifier &&
                tokens[index + 2].TokenType == SqlTokenType.Symbol &&
                tokens[index + 2].Value == "." &&
                tokens[index + 3].TokenType == SqlTokenType.Identifier &&
                tokens[index + 3].Value.StartsWith("vew",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        //
        // Pattern:
        // FROM vewSomething
        //
        if (index + 1 < tokens.Count)
        {
            if (tokens[index + 1].TokenType == SqlTokenType.Identifier &&
                tokens[index + 1].Value.StartsWith("vew",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
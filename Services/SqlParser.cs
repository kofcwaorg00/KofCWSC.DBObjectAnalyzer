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

        for (int i = 0; i <= list.Count - 4; i++)
        {
            if (IsExecProcedureCall(list, i))
            {
                yield return new Dependency(
                    referencingSchema,
                    referencingObject,
                    list[i + 1].Value,
                    list[i + 3].Value,
                    DependencyType.ProcedureCall,
                    list[i].Line);
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
            }

        }
    }

    private static bool IsExecProcedureCall(
        IReadOnlyList<SqlToken> tokens,
        int index)
    {
        return
            tokens[index].TokenType == SqlTokenType.Keyword &&
            (tokens[index].Value.Equals("EXEC", StringComparison.OrdinalIgnoreCase) ||
             tokens[index].Value.Equals("EXECUTE", StringComparison.OrdinalIgnoreCase)) &&

            tokens[index + 1].TokenType == SqlTokenType.Identifier &&

            tokens[index + 2].TokenType == SqlTokenType.Symbol &&
            tokens[index + 2].Value == "." &&

            tokens[index + 3].TokenType == SqlTokenType.Identifier;
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
}
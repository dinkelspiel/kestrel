using System.Text.RegularExpressions;

namespace Kestrel.Client.Prefab;

public class Expression { }
public class ExprObject(Dictionary<string, Expression> value) : Expression
{
    public Dictionary<string, Expression> value = value;
}
public class ExprString(string value) : Expression
{
    public string value = value;
}
public class ExprFloat(float value) : Expression
{
    public float value = value;
}
public class ExprInt(int value) : Expression
{
    public int value = value;
}
public class ExprArray(Expression[] value) : Expression
{
    public Expression[] value = value;
}
public class ExprNull : Expression
{
}


public class PrefabConfig
{
    public static void FromFile(string path)
    {
        string contents = File.ReadAllText(path);
        // string[] tokens = [.. Regex.Split(contents, @"(\n)| ")
        // .Where(token => token != string.Empty)];

        string[] tokens = [.. contents.Split(' ', '\n').Where(token => token != string.Empty)];
        // foreach (string token in tokens)
        // {
        //     Console.WriteLine(ToEscapedString(token));
        // }

        var config = ParseObject(tokens, []);
        Print(config.expr);
    }

    public static void Print(Expression expr)
    {
        switch (expr)
        {
            case ExprObject exprObj:
                PrintObject(exprObj);
                break;
            case ExprArray exprArr:
                PrintArray(exprArr);
                break;
            case ExprString exprString:
                Console.WriteLine(exprString.value);
                break;
            case ExprInt exprInt:
                Console.WriteLine(exprInt.value);
                break;
            case ExprFloat exprFloat:
                Console.WriteLine(exprFloat.value);
                break;
            case ExprNull _:
                Console.WriteLine("null");
                break;
        }
    }

    public static void PrintObject(ExprObject expr)
    {
        Console.WriteLine("{");
        foreach (var kvp in expr.value)
        {
            Console.Write($"{kvp.Key} = ");
            Print(kvp.Value);
        }
        Console.WriteLine("}");
    }

    public static void PrintArray(ExprArray expr)
    {
        Console.WriteLine("[");
        foreach (var kvp in expr.value)
        {
            Print(kvp);
        }
        Console.WriteLine("]");
    }

    public static string ToEscapedString(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    public static (string first, string[] tokens) TakeFirst(string[] tokens)
    {
        // if (tokens.Length == 0) return (null, []);
        if (tokens[0] == "\n") return TakeFirst([.. tokens.Skip(1)]);

        return (tokens[0], tokens.Skip(1).ToArray());
    }

    public static (Expression expr, string[] tokens) Parse(string[] tokens)
    {
        (string token, string[] tokens2) = TakeFirst(tokens);

        switch (token.First())
        {
            case '{':
                return ParseObject(tokens2, []);
            case '[':
                return ParseArray(tokens2, []);
            case '\n':
                return Parse(tokens2);
            case '\"':
                (string value, tokens) = ParseString(tokens, []);
                return (new ExprString(value), tokens);
            default:
                if (float.TryParse(token, out float result)) return (new ExprFloat(result), tokens2);
                if (int.TryParse(token, out int res)) return (new ExprInt(res), tokens2);
                throw new NotImplementedException($"{token} was unimlemented for expression");
        }
        throw new NotImplementedException($"{token} was unimlemented for expression");
    }

    public static (ExprObject expr, string[] tokens) ParseObject(string[] tokens, Dictionary<string, Expression> dict)
    {
        (string key, tokens) = TakeFirst(tokens);

        (string op, string[] tokens2) = TakeFirst(tokens);

        if (op != "=")
        {
            dict.Add(key, new ExprNull());

            if (tokens.Length == 0) return (new ExprObject(dict), tokens);
            (string end2, string[] tokens4) = TakeFirst(tokens);
            if (end2.First() == '}') return (new ExprObject(dict), tokens4);
            return ParseObject(tokens, dict);
        }

        (Expression expr, tokens) = Parse(tokens2);
        dict.Add(key, expr);

        if (tokens.Length == 0) return (new ExprObject(dict), tokens);
        (string end, string[] tokens3) = TakeFirst(tokens);
        if (end.First() == '}') return (new ExprObject(dict), tokens3);
        return ParseObject(tokens, dict);
    }

    public static (ExprArray expr, string[] tokens) ParseArray(string[] tokens, List<Expression> arr)
    {
        (Expression expr, tokens) = Parse(tokens);
        arr.Add(expr);

        if (tokens.Length == 0) return (new ExprArray([.. arr]), tokens);
        (string end, string[] tokens2) = TakeFirst(tokens);
        if (end.First() == ']') return (new ExprArray([.. arr]), tokens2);
        return ParseArray(tokens, arr);
    }

    public static (string value, string[] tokens) ParseString(string[] tokens, List<string> acc)
    {
        (string value, tokens) = TakeFirst(tokens);
        if (value.StartsWith('\"') && value.EndsWith('\"')) return (value[1..^1], tokens);
        if (value.EndsWith('\"')) return (string.Join(" ", [.. acc, value[..^1]]), tokens);

        if (value.StartsWith('\"')) return ParseString(tokens, [.. acc, value[1..]]);

        return ParseString(tokens, [.. acc, value]);
    }
}
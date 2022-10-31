using System.Text.RegularExpressions;

namespace Lex_Analyzer.Core.Cpp.Meta;

public static class Dictionaries
{
    public static readonly Dictionary<string, string> Operators = new()
    {
        { "=", "Assign" },
        { "+", "Sum" },
        { "-", "Substract" },
        { "/", "Division" },
        { "*", "Times" },
        { "++", "Increment" },
        { "--", "Decrement" }
    };

    public static readonly Dictionary<Regex, string> Comments = new()
    {
        { new Regex("/\\*"), "Init multiline comment" },
        { new Regex("//"), "Line comment" },
        { new Regex("/\\*\\*/"), "Empty comment" },
        { new Regex("\\*/"), "End multiline comment" }
    };

    public static readonly Dictionary<string, string> Headers = new()
    {
        { ".h", "Header file" },
    };

    public static readonly Dictionary<string, string> SpecialHeaders = new()
    {
        { "<stdio.h>", "Standard Input-Output library" },
        { "<string.h>", "String Manipulation library" }
    };

    public static readonly Dictionary<Regex, string> Macros = new()
    {
        { new Regex("\\w+"), "Macro" }
    };

    public static readonly Dictionary<string, string> DataTypes = new()
    {
        { "int", "Integer" },
        { "float", "Floating point" },
        { "char", "Character" },
        { "long", "Long integer" },
        { "string", "String of characters" },
        { "bool", "Boolean" },
        { "void", "Empty value" }
    };

    public static readonly Dictionary<string, string> Keywords = new()
    {
        { "return", "Ends execution of a function" },
        { "endl", "Newline"}
    };

    public static readonly Dictionary<string, string> Delimiters = new()
    {
        { ";", "End of instruction" }
    };

    public static readonly Dictionary<string, string> Blocks = new()
    {
        { "{", "Starts block of instructions" },
        { "}", "Ends block of instructions" },
    };

    public static readonly Dictionary<string, string> BuiltInFunctions = new()
    {
        { "cout", "Prints sequence in console" }
    };

    public static readonly string[] NonIdentifiers = new[]
    {
        "_", "-", "+", "/", "*", "`", "~", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "=", "|", "\"", ":", ";",
        "{", "}", "[", "]", "<", ">", "?", "/", "<<", ">>"
    };

    public static readonly string[] Numerals = new[]
    {
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"
    };
}
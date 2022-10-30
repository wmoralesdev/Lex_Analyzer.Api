using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Lex_Analyzer.Common.Extensions;
using Lex_Analyzer.Common.Parsers;
using Lex_Analyzer.Common.ViewModels;
using Lex_Analyzer.Core.Cpp.Meta;
using Microsoft.AspNetCore.Http;
using CollectionExtensions = Lex_Analyzer.Common.Extensions.CollectionExtensions;

namespace Lex_Analyzer.Core.Cpp;

public class AnalyzerService
{
    private readonly FileParsingService _fileParser;

    public AnalyzerService()
    {
        _fileParser = new FileParsingService("cpp");
    }

    public TokenizedResponse Analyze(IFormFile file)
    {
        var sourceCode = _fileParser.ParseFile(file);

        var isData = false;
        var isAdded = false;
        var isComment = false;
        var content = new Hashtable();
        var displayContent = new List<TokenMetaVm>();
        var program = sourceCode.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in program.Enumerate())
        {
            var tokens = line.Item.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens.Enumerate())
            {
                var trimmedToken = token.Item.Trim();

                AnalyzeBlocks(trimmedToken, line.Index, token.Index, content, displayContent, ref isAdded);

                AnalyzeOperators(trimmedToken, line.Index, token.Index, content, displayContent, ref isAdded);

                AnalyzeComments(trimmedToken, line.Index, token.Index, content, displayContent, ref isAdded,
                    ref isComment);

                AnalyzeHeaders(trimmedToken, line.Index, token.Index, content, displayContent, ref isAdded);

                AnalyzeParameters(trimmedToken, line.Index, token.Index, content, displayContent, ref isAdded);

                AnalyzeIdentifiers(trimmedToken, line.Index, token.Index, content, displayContent, ref isAdded,
                    isData);

                AnalyzeDatatypes(trimmedToken, line.Index, token.Index, content, displayContent, ref isAdded,
                    ref isData);

                AnalyzeKeywords(trimmedToken, line.Index, token.Index, content, displayContent, ref isAdded);

                AnalyzeBuiltInFunctions(trimmedToken, line.Index, token.Index, content, displayContent, 
                    ref isAdded);

                AnalyzeDelimiters(trimmedToken, line.Index, token.Index, content, displayContent, ref isAdded);

                AnalyzeSpecialHeaders(trimmedToken, line.Index, token.Index, content, displayContent,
                    ref isAdded);
                
                AnalyzeNumerals(trimmedToken, line.Index, token.Index, content, displayContent, ref isAdded,
                    isComment);

                AnalyzeIdentifiers(trimmedToken, line.Index, token.Index, content, displayContent, ref isAdded,
                    true, isComment);
                
                AnalyzeCommentContent(trimmedToken, line.Index, token.Index, content, displayContent, 
                    ref isAdded, isComment);
            }

            displayContent.Add(new TokenMetaVm
            {
                Meta = "End of line",
                Token = "\n",
            });

            isData = false;
            isComment = false;
            isAdded = false;
        }

        return new TokenizedResponse
        {
            Tokenized = content,
            Display = displayContent
        };
    }

    private void AnalyzeBlocks(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded)
    {
        if (Dictionaries.Blocks.ContainsKey(token))
        {
            var tokenMeta = new TokenMetaVm
            {
                Meta = Dictionaries.Blocks[token],
                Token = token,
            };

            content.Add($"L{lineIndex + 1}-T{tokenIndex + 1}:{token}",
                tokenMeta
            );
            displayContent.Add(tokenMeta);
            isAdded = true;
        }
    }

    private void AnalyzeOperators(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded)
    {
        if (Dictionaries.Operators.ContainsKey(token))
        {
            var tokenMeta = new TokenMetaVm
            {
                Meta = Dictionaries.Operators[token],
                Token = token,
            };

            content.Add($"L{lineIndex + 1}-T{tokenIndex + 1}:{token}",
                tokenMeta
            );
            displayContent.Add(tokenMeta);

            isAdded = true;
        }
    }

    private void AnalyzeComments(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded, ref bool isComment)
    {
        var auxComment = 0;
        foreach (var (key, value) in Dictionaries.Comments)
        {
            if (key.IsMatch(token))
            {
                if (auxComment == 0)
                    isComment = true;

                if (auxComment == Dictionaries.Comments.Count - 1)
                    isComment = false;

                var tokenMeta = new TokenMetaVm
                {
                    Meta = value,
                    Token = token,
                };

                content.Add($"Line{lineIndex}, Token{tokenIndex}",
                    tokenMeta
                );
                displayContent.Add(tokenMeta);
                auxComment++;
                isAdded = true;
            }
        }
    }

    private void AnalyzeHeaders(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded)
    {
        if (token.Contains(".h"))
        {
            var headerExists = Dictionaries.SpecialHeaders.ContainsKey(token);
            string message;

            if (!headerExists)
            {
                message = "UNSPECIFIED HEADER FILE";
            }
            else
            {
                message = Dictionaries.SpecialHeaders[token];
            }

            var tokenMeta = new TokenMetaVm
            {
                Meta = message,
                Token = token,
            };

            content.Add($"L{lineIndex + 1}-T{tokenIndex + 1}:{token}",
                tokenMeta
            );
            displayContent.Add(tokenMeta);
            isAdded = true;
        }
    }

    private void AnalyzeParameters(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded)
    {
        if (new Regex("\\(.*\\)").IsMatch(token))
        {
            var tokenMeta = new TokenMetaVm
            {
                Meta = "Parameters",
                Token = token,
            };

            content.Add($"L{lineIndex + 1}-T{tokenIndex + 1}:{token}",
                tokenMeta
            );
            displayContent.Add(tokenMeta);
            isAdded = true;
        }
    }

    private void AnalyzeIdentifiers(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded, bool isData)
    {
        if (isData && !Dictionaries.NonIdentifiers.Contains(token) && !new Regex("\\(.*\\)").IsMatch(token))
        {
            var tokenMeta = new TokenMetaVm
            {
                Meta = "Identifier",
                Token = token,
            };

            content.Add($"L{lineIndex + 1}-T{tokenIndex + 1}:{token}",
                tokenMeta
            );
            displayContent.Add(tokenMeta);
            isAdded = true;
        }
    }
    
    private void AnalyzeIdentifiers(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded, bool _, bool isComment)
    {
        if (!Dictionaries.NonIdentifiers.Contains(token) && !isComment && !isAdded)
        {
            var tokenMeta = new TokenMetaVm
            {
                Meta = "Identifier",
                Token = token,
            };

            content.Add($"L{lineIndex + 1}-T{tokenIndex + 1}:{token}",
                tokenMeta
            );
            displayContent.Add(tokenMeta);
            isAdded = true;
        }
    }

    private void AnalyzeDatatypes(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded, ref bool isData)
    {
        if (Dictionaries.DataTypes.ContainsKey(token))
        {
            var tokenMeta = new TokenMetaVm
            {
                Meta = Dictionaries.DataTypes[token],
                Token = token,
            };

            content.Add($"L{lineIndex + 1}-T{tokenIndex + 1}:{token}",
                tokenMeta
            );
            displayContent.Add(tokenMeta);

            isData = true;
            isAdded = true;
        }
    }
    
    private void AnalyzeKeywords(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded)
    {
        if (Dictionaries.Keywords.ContainsKey(token))
        {
            var tokenMeta = new TokenMetaVm
            {
                Meta = Dictionaries.Keywords[token],
                Token = token,
            };

            content.Add($"L{lineIndex + 1}-T{tokenIndex + 1}:{token}",
                tokenMeta
            );
            displayContent.Add(tokenMeta);
            isAdded = true;
        }
    }
    
    private void AnalyzeBuiltInFunctions(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded)
    {
        if (Dictionaries.BuiltInFunctions.ContainsKey(token))
        {
            var tokenMeta = new TokenMetaVm
            {
                Meta = Dictionaries.BuiltInFunctions[token],
                Token = token,
            };

            content.Add($"L{lineIndex + 1}-T{tokenIndex + 1}:{token}",
                tokenMeta
            );
            displayContent.Add(tokenMeta);
            isAdded = true;
        }
    }

    private void AnalyzeDelimiters(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded)
    {
        if (Dictionaries.Delimiters.ContainsKey(token))
        {
            var tokenMeta = new TokenMetaVm
            {
                Meta = Dictionaries.Delimiters[token],
                Token = token,
            };

            content.Add($"L{lineIndex + 1}-T{tokenIndex + 1}:{token}",
                tokenMeta
            );
            displayContent.Add(tokenMeta);
            isAdded = true;
        }
    }
    
    private void AnalyzeSpecialHeaders(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded)
    {
        if (token.Contains('#'))
        {
            var tokenMeta = new TokenMetaVm
            {
                Meta = "Header",
                Token = token,
            };

            content.Add($"L{lineIndex + 1}-T{tokenIndex + 1}:{token}",
                tokenMeta
            );
            displayContent.Add(tokenMeta);
            isAdded = true;
        }
    }
    
    private void AnalyzeNumerals(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded, bool isComment)
    {
        if (!isComment && (Dictionaries.Numerals.Contains(token) || token.All(char.IsDigit)))
        {
            var tokenMeta = new TokenMetaVm
            {
                Meta = "Numeral",
                Token = token,
            };

            content.Add($"L{lineIndex + 1}-T{tokenIndex + 1}:{token}",
                tokenMeta
            );
            displayContent.Add(tokenMeta);
            isAdded = true;
        }
    }
    
    private void AnalyzeCommentContent(string token, long lineIndex, long tokenIndex, Hashtable content,
        List<TokenMetaVm> displayContent, ref bool isAdded, bool isComment)
    {
        if (isComment && !isAdded)
        {
            var tokenMeta = new TokenMetaVm
            {
                Meta = "Word in comment",
                Token = token,
            };

            content.Add($"L{lineIndex + 1}-T{tokenIndex + 1}:{token}",
                tokenMeta
            );
            displayContent.Add(tokenMeta);
            isAdded = true;
        }
    }
}
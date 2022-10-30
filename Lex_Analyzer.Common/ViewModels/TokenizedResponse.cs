using System.Collections;

namespace Lex_Analyzer.Common.ViewModels;

public class TokenizedResponse
{
    public Hashtable? Tokenized { get; set; }
    
    public List<TokenMetaVm>? Display { get; set; }
}
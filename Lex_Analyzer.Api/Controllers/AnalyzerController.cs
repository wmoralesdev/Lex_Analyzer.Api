using Lex_Analyzer.Api.ViewModels;
using Lex_Analyzer.Core.Cpp;
using Microsoft.AspNetCore.Mvc;

namespace Lex_Analyzer.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AnalyzerController : ControllerBase
{
    private readonly AnalyzerService _cppAnalyzer;

    public AnalyzerController(AnalyzerService cppAnalyzer)
    {
        _cppAnalyzer = cppAnalyzer;
    }

    [HttpPost("cpp")]
    public IActionResult AnalyzeC(IFormFile file)
    {
        return Ok(_cppAnalyzer.Analyze(file));
    }
}
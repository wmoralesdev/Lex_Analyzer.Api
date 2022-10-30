using System.Text;
using Microsoft.AspNetCore.Http;

namespace Lex_Analyzer.Common.Parsers;

public class FileParsingService
{
    public FileParsingService(string acceptedFileExtension)
    {
        AcceptedFileExtension = acceptedFileExtension;
    }

    public string AcceptedFileExtension { get; }
    
    public string ParseFile(IFormFile file)
    {
        if (!file.FileName.EndsWith(AcceptedFileExtension))
            throw new ArgumentException("Invalid file extension");

        var result = new StringBuilder();
        using var reader = new StreamReader(file.OpenReadStream());

        while (reader.Peek() >= 0)
            result.AppendLine(reader.ReadLine()?.Trim());

        return result.ToString();
    }
}
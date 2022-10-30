using Lex_Analyzer.Core.Cpp;
using Microsoft.Extensions.DependencyInjection;

namespace Lex_Analyzer.Core.Dependencies;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalyzerCore(this IServiceCollection services)
    {
        services.AddTransient<AnalyzerService>();

        return services;
    }
}
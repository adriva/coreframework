using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Adriva.CodeAnalysis.CSharp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var vi = MSBuildLocator.RegisterDefaults();
            using (var workspace = Microsoft.CodeAnalysis.MSBuild.MSBuildWorkspace.Create())
            {
                workspace.SkipUnrecognizedProjects = true;
                workspace.LoadMetadataForReferencedProjects = true;

                var project = await workspace.OpenProjectAsync(args[0]);
                var alternateOptions = project.CompilationOptions
                                .WithGeneralDiagnosticOption(Microsoft.CodeAnalysis.ReportDiagnostic.Error)
                                .WithConcurrentBuild(true);
                project = project.WithCompilationOptions(alternateOptions);
                var compilation = (CSharpCompilation)await project.GetCompilationAsync();

                var sm = new SimilarMethodAnalyzer();
                var co = new CompilationWithAnalyzers(compilation, ImmutableArray.Create(new DiagnosticAnalyzer[] {
                    new NamingAnalyzer(),
                    sm,
                 }), null, CancellationToken.None);

                var diags = await co.GetAllDiagnosticsAsync();
                foreach (var diag in diags)
                {
                    if (diag.Severity != Microsoft.CodeAnalysis.DiagnosticSeverity.Hidden)
                        System.Console.WriteLine(diag);
                }
            }
        }
    }
}

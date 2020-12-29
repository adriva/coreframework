#pragma warning disable RS2008
#pragma warning disable RS1026
using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Adriva.CodeAnalysis.CSharp
{

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class NamingAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            new DiagnosticDescriptor[]{
                DiagnosticDescriptors.AsyncSuffixMissing,
                DiagnosticDescriptors.FieldNamesMustBeCamelCase,
                DiagnosticDescriptors.FieldNamesMustBeOnlyLettersAndDigits,
                DiagnosticDescriptors.PropertyNamesMustBeCamelCase,
                DiagnosticDescriptors.PropertyNamesMustBeOnlyLettersAndDigits,
            });



        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterSyntaxNodeAction(this.AnalyzeAsyncMethodNames, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(this.AnalyzeFieldNames, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(this.AnalyzeFieldNames, SyntaxKind.EventFieldDeclaration);
            context.RegisterSyntaxNodeAction(this.AnalyzePropertyNames, SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeFieldNames(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is BaseFieldDeclarationSyntax fieldDeclarationSyntax)
            {
                foreach (var variable in fieldDeclarationSyntax.Declaration.Variables)
                {
                    string name = variable.Identifier.Text;
                    if (!char.IsLetter(name[0]) || !char.IsUpper(name[0]))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.FieldNamesMustBeCamelCase, fieldDeclarationSyntax.GetLocation(), name));
                    }

                    if (name.Any(c => !char.IsLetterOrDigit(c)))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.FieldNamesMustBeOnlyLettersAndDigits, fieldDeclarationSyntax.GetLocation(), name));
                    }
                }
            }
        }

        private void AnalyzePropertyNames(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is PropertyDeclarationSyntax propertyDeclerationSyntax)
            {
                string name = propertyDeclerationSyntax.Identifier.Text;
                if (!char.IsLetter(name[0]) || !char.IsUpper(name[0]))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.FieldNamesMustBeCamelCase, propertyDeclerationSyntax.GetLocation(), name));
                }

                if (name.Any(c => !char.IsLetterOrDigit(c)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.FieldNamesMustBeOnlyLettersAndDigits, propertyDeclerationSyntax.GetLocation(), name));
                }

            }
        }

        private void AnalyzeAsyncMethodNames(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is MethodDeclarationSyntax methodDeclaration)) return;

            if (methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword))
            {
                string name = context.SemanticModel.GetDeclaredSymbol(methodDeclaration).Name;
                if (!name.EndsWith("Async", StringComparison.OrdinalIgnoreCase))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.AsyncSuffixMissing, methodDeclaration.GetLocation(), methodDeclaration.Identifier.Text));
                }
            }
        }
    }
}
#pragma warning restore RS1026
#pragma warning restore RS2008
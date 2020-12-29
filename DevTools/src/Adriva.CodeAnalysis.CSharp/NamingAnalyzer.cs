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
    internal class NamingAnalyzer : DiagnosticAnalyzer
    {
        private readonly DiagnosticDescriptor AsyncSuffixMissing = new DiagnosticDescriptor("ADR00001", "Async method names should end with 'Async'.", "Add the 'Async' suffix to the method '{0}'.", "Naming", DiagnosticSeverity.Warning, true);
        private readonly DiagnosticDescriptor FieldNamesMustBeCamelCase = new DiagnosticDescriptor("ADR00002", "Field names must be proper camel cased.", "Change the field name '{0}' to use camel casing and start with a letter.", "Naming", DiagnosticSeverity.Warning, true);
        private readonly DiagnosticDescriptor FieldNamesMustBeOnlyLettersAndDigits = new DiagnosticDescriptor("ADR00003", "Field names must contain only letters and digits.", "Change the field name '{0}' to use letters and digits only.", "Naming", DiagnosticSeverity.Warning, true);
        private readonly DiagnosticDescriptor PropertyNamesMustBeCamelCase = new DiagnosticDescriptor("ADR00004", "Property names must be proper camel cased.", "Change the property name '{0}' to use camel casing and start with a letter.", "Naming", DiagnosticSeverity.Warning, true);
        private readonly DiagnosticDescriptor PropertyNamesMustBeOnlyLettersAndDigits = new DiagnosticDescriptor("ADR00005", "Property names must contain only letters and digits.", "Change the property name '{0}' to use letters and digits only.", "Naming", DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            new DiagnosticDescriptor[]{
                this.AsyncSuffixMissing,
                this.FieldNamesMustBeCamelCase,
                this.FieldNamesMustBeOnlyLettersAndDigits,
                this.PropertyNamesMustBeCamelCase,
                this.PropertyNamesMustBeOnlyLettersAndDigits,
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
                        context.ReportDiagnostic(Diagnostic.Create(this.FieldNamesMustBeCamelCase, fieldDeclarationSyntax.GetLocation(), name));
                    }

                    if (name.Any(c => !char.IsLetterOrDigit(c)))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(this.FieldNamesMustBeOnlyLettersAndDigits, fieldDeclarationSyntax.GetLocation(), name));
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
                    context.ReportDiagnostic(Diagnostic.Create(this.FieldNamesMustBeCamelCase, propertyDeclerationSyntax.GetLocation(), name));
                }

                if (name.Any(c => !char.IsLetterOrDigit(c)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(this.FieldNamesMustBeOnlyLettersAndDigits, propertyDeclerationSyntax.GetLocation(), name));
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
                    context.ReportDiagnostic(Diagnostic.Create(this.AsyncSuffixMissing, methodDeclaration.GetLocation(), methodDeclaration.Identifier.Text));
                }
            }
        }
    }
}
#pragma warning restore RS1026
#pragma warning restore RS2008
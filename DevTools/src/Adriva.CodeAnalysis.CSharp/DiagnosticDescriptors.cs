#pragma warning disable RS2008
#pragma warning disable RS1026
using Microsoft.CodeAnalysis;

namespace Adriva.CodeAnalysis.CSharp
{
    internal static class DiagnosticDescriptors
    {
        internal static readonly DiagnosticDescriptor UnknownWarning = new DiagnosticDescriptor("ADR00000", "Unknown warning", "Unknown warning.", "General", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor AsyncSuffixMissing = new DiagnosticDescriptor("ADR00001", "Async method names should end with 'Async'.", "Add the 'Async' suffix to the method '{0}'.", "Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor FieldNamesMustBeCamelCase = new DiagnosticDescriptor("ADR00002", "Field names must be proper camel cased.", "Change the field name '{0}' to use camel casing and start with a letter.", "Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor FieldNamesMustBeOnlyLettersAndDigits = new DiagnosticDescriptor("ADR00003", "Field names must contain only letters and digits.", "Change the field name '{0}' to use letters and digits only.", "Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor PropertyNamesMustBeCamelCase = new DiagnosticDescriptor("ADR00004", "Property names must be proper camel cased.", "Change the property name '{0}' to use camel casing and start with a letter.", "Naming", DiagnosticSeverity.Warning, true);
        internal static readonly DiagnosticDescriptor PropertyNamesMustBeOnlyLettersAndDigits = new DiagnosticDescriptor("ADR00005", "Property names must contain only letters and digits.", "Change the property name '{0}' to use letters and digits only.", "Naming", DiagnosticSeverity.Warning, true);

        internal static readonly DiagnosticDescriptor SimilarMethod = new DiagnosticDescriptor("ADR01000", "Similar method decleration.", "Method '{0}' looks similar to method '{1}' in '{2}'. Check if reusing is possible.", "Reusability", DiagnosticSeverity.Warning, true);
    }
}
#pragma warning restore RS1026
#pragma warning restore RS2008
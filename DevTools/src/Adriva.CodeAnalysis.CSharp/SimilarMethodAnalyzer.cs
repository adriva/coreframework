#pragma warning disable RS2008
#pragma warning disable RS1026
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Adriva.CodeAnalysis.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class SimilarMethodAnalyzer : DiagnosticAnalyzer
    {
        private class StateData
        {
            public readonly IList<IMethodSymbol> ClassMethods = new List<IMethodSymbol>(512);
            public readonly IList<IMethodSymbol> VirtualMethods = new List<IMethodSymbol>(512);
            public readonly IList<IMethodSymbol> InterfaceMethods = new List<IMethodSymbol>(512);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(new[] {
            DiagnosticDescriptors.SimilarMethod
        });

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(this.OnStartCompilation);
        }

        private void OnStartCompilation(CompilationStartAnalysisContext context)
        {
            StateData stateData = new StateData();
            context.RegisterSymbolAction((symbolContext) => this.AnalyzeSimilarMethods(symbolContext, stateData), SymbolKind.Method);
            context.RegisterCompilationEndAction((compilationContext) =>
            {
                var t = stateData;
            });
        }

        private void AddToBucket(IMethodSymbol methodSymbol, StateData stateData)
        {

            if (TypeKind.Interface == methodSymbol.ContainingType.TypeKind)
            {
                stateData.InterfaceMethods.Add(methodSymbol);
                return;
            }
            else if (TypeKind.Class == methodSymbol.ContainingType.TypeKind)
            {
                if (methodSymbol.IsVirtual)
                {
                    stateData.VirtualMethods.Add(methodSymbol);
                    return;
                }

                if (null != methodSymbol.OverriddenMethod)
                {
                    var baseMethod = methodSymbol.OverriddenMethod;
                    while (null != baseMethod.OverriddenMethod)
                    {
                        baseMethod = baseMethod.OverriddenMethod;
                    }

                    this.AddToBucket(baseMethod, stateData);
                    return;
                }

                var interfaceMethods = methodSymbol.ContainingType.AllInterfaces.SelectMany(i => i.GetMembers(methodSymbol.Name).OfType<IMethodSymbol>());
                var interfaceMethod = interfaceMethods.FirstOrDefault(im => null != methodSymbol.ContainingType.FindImplementationForInterfaceMember(im));

                if (null != interfaceMethod)
                {
                    this.AddToBucket(interfaceMethod, stateData);
                    return;
                }
            }

            stateData.ClassMethods.Add(methodSymbol);
        }

        private void AnalyzeSimilarMethods(SymbolAnalysisContext context, StateData stateData)
        {
            if (!(context.Symbol is IMethodSymbol methodSymbol)) return;
            if (MethodKind.Ordinary != methodSymbol.MethodKind && MethodKind.DeclareMethod != methodSymbol.MethodKind) return;

            this.AddToBucket(methodSymbol, stateData);
        }
    }
}
#pragma warning restore RS1026
#pragma warning restore RS2008
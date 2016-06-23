using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MissingAwaitAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MissingAwaitAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor AwaitTaskTask = new DiagnosticDescriptor("ASYNC101",
            title:"Double await",
            messageFormat:"Return of this await '{0}' can itself be awaited; consider doing await twice",
            category:"Async",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Dangerous to await a Task<Task>> or Task<Task<...>>.");

        private static DiagnosticDescriptor ImplicitTaskTask = new DiagnosticDescriptor("ASYNC102",
            title: "Implicit conversion of Task<Task> to Task",
            messageFormat: "An implicit cast from Task<Task> to Task will throw away information about when the inner Task has completed; consider using await.",
            category: "Async",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Dangerous to implicitly cast from Task<Task> to Task");


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AwaitTaskTask, ImplicitTaskTask);

        public class WellKnownTypeCache
        {
            private INamedTypeSymbol tTask, tTask1;
            private SemanticModel semanticModel;
            
            public INamedTypeSymbol GetTask(SemanticModel semanticModel)
            {
                if (semanticModel != this.semanticModel) { this.semanticModel = semanticModel; tTask = null; tTask1 = null; }
                if (tTask == null) tTask = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
                return tTask;
            }

            public INamedTypeSymbol GetGenericTask(SemanticModel semanticModel)
            {
                if (semanticModel != this.semanticModel) { this.semanticModel = semanticModel; tTask = null; tTask1 = null; }
                if (tTask1 == null) tTask1 = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
                return tTask1;
            }

        }

        public override void Initialize(AnalysisContext context)
        {
            var cache = new WellKnownTypeCache();
            context.RegisterSyntaxNodeAction(ctx => AnalyzeStatementExpression(cache, ctx), SyntaxKind.ExpressionStatement);
            context.RegisterSyntaxNodeAction(ctx => AnalyzeImplicitConversion(cache, ctx), SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeImplicitConversion(WellKnownTypeCache cache, SyntaxNodeAnalysisContext context)
        {
            var visitor = new ImplicitConversionWalker() { cache=cache, context = context };
            visitor.Visit(context.Node);
        }

        class ImplicitConversionWalker : CSharpSyntaxWalker
        {
            public SyntaxNodeAnalysisContext context;
            public WellKnownTypeCache cache;
            public override void Visit(SyntaxNode node)
            {
                if (node is ExpressionSyntax)
                {
                    var tinfo = context.SemanticModel.GetTypeInfo(node);
                    var tSrc = tinfo.Type as INamedTypeSymbol;
                    var tDst = tinfo.ConvertedType as INamedTypeSymbol;
                    if (tSrc != null && tDst != null
                        && (object)tSrc.ConstructedFrom == cache.GetGenericTask(context.SemanticModel)
                        && (object)tDst == cache.GetTask(context.SemanticModel))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(ImplicitTaskTask, node.GetLocation()));
                        return;
                    }
                }
                base.Visit(node);
            }
        }

        public void AnalyzeStatementExpression(WellKnownTypeCache cache, SyntaxNodeAnalysisContext context)
        {
            // If we encounter "await e;" where the result type of the await is a Task ...
            var expression = (context.Node as ExpressionStatementSyntax).Expression;
            if (expression == null) return;
            if (expression.Kind() != SyntaxKind.AwaitExpression) return;
            var typeInfo = context.SemanticModel.GetTypeInfo(expression);
            var type = typeInfo.Type as INamedTypeSymbol;
            if (type == null) return;
            if ((object)type != cache.GetTask(context.SemanticModel) && (object)type.ConstructedFrom != cache.GetGenericTask(context.SemanticModel)) return;

            // and if "e" didn't have the form "Task.WhenAny(...)" ...
            var awaitOperand = (expression as AwaitExpressionSyntax).Expression;
            if (awaitOperand.Kind() == SyntaxKind.InvocationExpression)
            {
                var targetNode = (awaitOperand as InvocationExpressionSyntax).Expression;
                if (targetNode != null)
                {
                    var targetSymbol = context.SemanticModel.GetSymbolInfo(targetNode).Symbol;
                    if (targetSymbol != null)
                    {
                        if ((object)targetSymbol.ContainingType == cache.GetTask(context.SemanticModel) && targetSymbol.Name == "WhenAny") return;
                    }
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(AwaitTaskTask, context.Node.GetLocation(), type.Name));
        }

    }
}

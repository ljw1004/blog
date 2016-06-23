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

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeStatementExpression, SyntaxKind.ExpressionStatement);
            context.RegisterSyntaxNodeAction(AnalyzeImplicitConversion, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeImplicitConversion(SyntaxNodeAnalysisContext context)
        {
            var visitor = new ImplicitConversionWalker() { context = context };
            visitor.Visit(context.Node);
        }

        class ImplicitConversionWalker : CSharpSyntaxWalker
        {
            public SyntaxNodeAnalysisContext context;
            public override void Visit(SyntaxNode node)
            {
                if (node is ExpressionSyntax)
                {
                    var tinfo = context.SemanticModel.GetTypeInfo(node);
                    if ((object)tinfo.Type != null && tinfo.Type != tinfo.ConvertedType
                        && tinfo.ConvertedType.ToDisplayString() == "System.Threading.Tasks.Task"
                        && tinfo.Type.ToDisplayString().StartsWith("System.Threading.Tasks.Task<System.Threading.Tasks.Task"))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(ImplicitTaskTask, node.GetLocation()));
                        return;
                    }
                }
                base.Visit(node);
            }
        }

        public void AnalyzeStatementExpression(SyntaxNodeAnalysisContext context)
        {
            // If we encounter "await e;" where the result type of the await is a Task ...
            var expression = (context.Node as ExpressionStatementSyntax).Expression;
            if (expression == null) return;
            if (expression.Kind() != SyntaxKind.AwaitExpression) return;
            var typeInfo = context.SemanticModel.GetTypeInfo(expression);
            var type = typeInfo.Type as INamedTypeSymbol;
            if (type == null) return;
            var typeName = type.ToDisplayString();
            if (!typeName.StartsWith("System.Threading.Tasks.Task")) return;

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
                        var targetName = targetSymbol.ToDisplayString();
                        if (targetName.StartsWith("System.Threading.Tasks.Task.WhenAny(")) return;
                    }
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(AwaitTaskTask, context.Node.GetLocation(), type.Name));
        }

    }
}

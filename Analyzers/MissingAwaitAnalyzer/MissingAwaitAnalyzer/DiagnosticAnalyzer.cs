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
        public const string DiagnosticId = "MissingAwaitAnalyzer";

        private static readonly LocalizableString Title = "Double await";
        private static readonly LocalizableString MessageFormat = "Return of this await '{0}' can itself be awaited; consider doing await twice";
        private static readonly LocalizableString Description = "Dangerous to await a Task<Task>> or Task<Task<...>>.";
        private const string Category = "Async";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ExpressionStatement);
        }

        public void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var expression = (context.Node as ExpressionStatementSyntax).Expression;
            if (expression == null) return;
            if (expression.Kind() != SyntaxKind.AwaitExpression) return;

            var typeInfo = context.SemanticModel.GetTypeInfo(expression);
            var type = typeInfo.Type as INamedTypeSymbol;
            if (type == null) return;
            var typeName = type.ToDisplayString();
            if (!typeName.StartsWith("System.Threading.Tasks.Task")) return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), type.Name));
        }

    }
}

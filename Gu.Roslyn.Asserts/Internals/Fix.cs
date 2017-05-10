namespace Gu.Roslyn.Asserts.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class Fix
    {
        internal static async Task<Solution> ApplyAsync(Solution solution, CodeFixProvider codeFix, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var actions = new List<CodeAction>();
            var document = solution.GetDocument(diagnostic.Location.SourceTree);
            actions.Clear();
            var context = new CodeFixContext(
                document,
                diagnostic,
                (a, d) => actions.Add(a),
                CancellationToken.None);
            await codeFix.RegisterCodeFixesAsync(context).ConfigureAwait(false);
            if (actions.Count == 0)
            {
                return solution;
            }

            if (actions.Count > 1)
            {
                throw Fail.CreateException("Expected only one action");
            }

            var operations = await actions[0].GetOperationsAsync(cancellationToken)
                                             .ConfigureAwait(false);
            return operations.OfType<ApplyChangesOperation>()
                             .Single()
                             .ChangedSolution;
        }

        internal static async Task<Solution> ApplyAsync(CodeFixProvider codeFix, FixAllScope scope, TestDiagnosticProvider diagnosticProvider, CancellationToken cancellationToken)
        {
            var context = new FixAllContext(
                diagnosticProvider.Document,
                codeFix,
                scope,
                diagnosticProvider.EquivalenceKey,
                codeFix.FixableDiagnosticIds,
                diagnosticProvider,
                cancellationToken);
            var action = await codeFix.GetFixAllProvider().GetFixAsync(context).ConfigureAwait(false);

            var operations = await action.GetOperationsAsync(cancellationToken)
                                             .ConfigureAwait(false);
            return operations.OfType<ApplyChangesOperation>()
                             .Single()
                             .ChangedSolution;
        }

        internal static async Task<Solution> ApplyAllFixableOneByOneAsync(Solution solution, DiagnosticAnalyzer analyzer, CodeFixProvider codeFix, CancellationToken cancellationToken)
        {
            var fixable = await Analyze.GetFixableDiagnosticsAsync(solution, analyzer, codeFix).ConfigureAwait(false);
            var fixedSolution = solution;
            int count;
            do
            {
                count = fixable.Count;
                if (count == 0)
                {
                    return fixedSolution;
                }

                fixedSolution = await ApplyAsync(fixedSolution, codeFix, fixable[0], cancellationToken).ConfigureAwait(false);
                fixable = await Analyze.GetFixableDiagnosticsAsync(fixedSolution, analyzer, codeFix).ConfigureAwait(false);
            }
            while (fixable.Count < count);
            return fixedSolution;
        }

        internal static async Task<Solution> ApplyAllFixableScopeByScopeAsync(Solution solution, DiagnosticAnalyzer analyzer, CodeFixProvider codeFix, FixAllScope scope, CancellationToken cancellationToken)
        {
            var fixable = await Analyze.GetFixableDiagnosticsAsync(solution, analyzer, codeFix).ConfigureAwait(false);
            var fixedSolution = solution;
            int count;
            do
            {
                count = fixable.Count;
                if (count == 0)
                {
                    return fixedSolution;
                }

                var diagnosticProvider = await TestDiagnosticProvider.CreateAsync(fixedSolution, codeFix, fixable).ConfigureAwait(false);
                fixedSolution = await ApplyAsync(codeFix, scope, diagnosticProvider, cancellationToken).ConfigureAwait(false);
                fixable = await Analyze.GetFixableDiagnosticsAsync(fixedSolution, analyzer, codeFix).ConfigureAwait(false);
            }
            while (fixable.Count < count);
            return fixedSolution;
        }

        internal sealed class TestDiagnosticProvider : FixAllContext.DiagnosticProvider
        {
            private IEnumerable<Diagnostic> diagnostics;

            private TestDiagnosticProvider(IEnumerable<Diagnostic> diagnostics, Document document, string equivalenceKey)
            {
                this.diagnostics = diagnostics;
                this.Document = document;
                this.EquivalenceKey = equivalenceKey;
            }

            public Document Document { get; }

            public string EquivalenceKey { get; }

            public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
            {
                return Task.FromResult(this.diagnostics);
            }

            public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
            {
                return Task.FromResult(this.diagnostics.Where(i => i.Location.GetLineSpan().Path == document.Name));
            }

            public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
            {
                return Task.FromResult(this.diagnostics.Where(i => !i.Location.IsInSource));
            }

            internal static async Task<TestDiagnosticProvider> CreateAsync(Solution solution, CodeFixProvider codeFix, IEnumerable<Diagnostic> diagnostics)
            {
                var actions = new List<CodeAction>();
                var diagnostic = diagnostics.First();
                var context = new CodeFixContext(solution.GetDocument(diagnostic.Location.SourceTree), diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
                await codeFix.RegisterCodeFixesAsync(context).ConfigureAwait(false);
                return new TestDiagnosticProvider(diagnostics, solution.GetDocument(diagnostics.First().Location.SourceTree), actions.First().EquivalenceKey);
            }
        }
    }
}
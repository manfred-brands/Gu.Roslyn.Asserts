namespace Gu.Roslyn.Asserts
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Roslyn.Asserts.Internals;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    public static partial class AnalyzerAssert
    {
        /// <summary>
        /// Verifies that <paramref name="codeWithErrorsIndicated"/> produces the expected diagnostics.
        /// </summary>
        /// <typeparam name="TAnalyzer">The type of the analyzer.</typeparam>
        /// <param name="codeWithErrorsIndicated">The code with error positions indicated.</param>
        public static void Diagnostics<TAnalyzer>(params string[] codeWithErrorsIndicated)
            where TAnalyzer : DiagnosticAnalyzer, new()
        {
            Diagnostics(
                new TAnalyzer(),
                DiagnosticsAndSources.CreateFromCodeWithErrorsIndicated(new TAnalyzer(), codeWithErrorsIndicated));
        }

        /// <summary>
        /// Verifies that <paramref name="codeWithErrorsIndicated"/> produces the expected diagnostics.
        /// </summary>
        /// <param name="analyzerType">The type of the analyzer.</param>
        /// <param name="codeWithErrorsIndicated">The code with error positions indicated.</param>
        public static void Diagnostics(Type analyzerType, params string[] codeWithErrorsIndicated)
        {
            Diagnostics(
                (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType),
                DiagnosticsAndSources.CreateFromCodeWithErrorsIndicated((DiagnosticAnalyzer)Activator.CreateInstance(analyzerType), codeWithErrorsIndicated));
        }

        /// <summary>
        /// Verifies that <paramref name="codeWithErrorsIndicated"/> produces the expected diagnostics.
        /// </summary>
        /// <param name="analyzer">The analyzer to apply.</param>
        /// <param name="codeWithErrorsIndicated">The code with error positions indicated.</param>
        public static void Diagnostics(DiagnosticAnalyzer analyzer, params string[] codeWithErrorsIndicated)
        {
            Diagnostics(
                analyzer,
                DiagnosticsAndSources.CreateFromCodeWithErrorsIndicated(analyzer, codeWithErrorsIndicated));
        }

        /// <summary>
        /// Verifies that <paramref name="code"/> produces the expected diagnostics.
        /// </summary>
        /// <typeparam name="TAnalyzer">The type of the analyzer.</typeparam>
        /// <param name="expectedDiagnostic">The expected diagnostic</param>
        /// <param name="code">The code to analyze.</param>
        public static void Diagnostics<TAnalyzer>(ExpectedDiagnostic expectedDiagnostic, params string[] code)
            where TAnalyzer : DiagnosticAnalyzer, new()
        {
            Diagnostics(
                new TAnalyzer(),
                DiagnosticsAndSources.Create(expectedDiagnostic, code));
        }

        /// <summary>
        /// Verifies that <paramref name="code"/> produces the expected diagnostics.
        /// </summary>
        /// <param name="analyzerType">The type of the analyzer.</param>
        /// <param name="expectedDiagnostic">The expected diagnostic</param>
        /// <param name="code">The code to analyze.</param>
        public static void Diagnostics(Type analyzerType, ExpectedDiagnostic expectedDiagnostic, params string[] code)
        {
            Diagnostics(
                (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType),
                DiagnosticsAndSources.Create(expectedDiagnostic, code));
        }

        /// <summary>
        /// Verifies that <paramref name="code"/> produces the expected diagnostics.
        /// </summary>
        /// <param name="analyzer">The analyzer to apply.</param>
        /// <param name="expectedDiagnostic">The expected diagnostic</param>
        /// <param name="code">The code to analyze.</param>
        public static void Diagnostics(DiagnosticAnalyzer analyzer, ExpectedDiagnostic expectedDiagnostic, params string[] code)
        {
            Diagnostics(
                analyzer,
                DiagnosticsAndSources.Create(expectedDiagnostic, code));
        }

        /// <summary>
        /// Verifies that <paramref name="code"/> produces the expected diagnostics.
        /// </summary>
        /// <typeparam name="TAnalyzer">The type of the analyzer.</typeparam>
        /// <param name="expectedDiagnostics">The expected diagnostics</param>
        /// <param name="code">The code to analyze.</param>
        public static void Diagnostics<TAnalyzer>(IReadOnlyList<ExpectedDiagnostic> expectedDiagnostics, params string[] code)
            where TAnalyzer : DiagnosticAnalyzer, new()
        {
            Diagnostics(
                new TAnalyzer(),
                new DiagnosticsAndSources(expectedDiagnostics, code));
        }

        /// <summary>
        /// Verifies that <paramref name="code"/> produces the expected diagnostics.
        /// </summary>
        /// <param name="analyzerType">The type of the analyzer.</param>
        /// <param name="expectedDiagnostics">The expected diagnostics</param>
        /// <param name="code">The code to analyze.</param>
        public static void Diagnostics(Type analyzerType, IReadOnlyList<ExpectedDiagnostic> expectedDiagnostics, params string[] code)
        {
            Diagnostics(
                (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType),
                new DiagnosticsAndSources(expectedDiagnostics, code));
        }

        /// <summary>
        /// Verifies that <paramref name="code"/> produces the expected diagnostics.
        /// </summary>
        /// <param name="analyzer">The analyzer to apply.</param>
        /// <param name="expectedDiagnostics">The expected diagnostics</param>
        /// <param name="code">The code to analyze.</param>
        public static void Diagnostics(DiagnosticAnalyzer analyzer, IReadOnlyList<ExpectedDiagnostic> expectedDiagnostics, params string[] code)
        {
            Diagnostics(analyzer, new DiagnosticsAndSources(expectedDiagnostics, code));
        }

        /// <summary>
        /// Verifies that <paramref name="diagnosticsAndSources"/> produces the expected diagnostics.
        /// </summary>
        /// <param name="analyzer">The analyzer to apply.</param>
        /// <param name="diagnosticsAndSources">The code to analyze.</param>
        public static void Diagnostics(DiagnosticAnalyzer analyzer, DiagnosticsAndSources diagnosticsAndSources)
        {
            VerifyAnalyzerSupportsDiagnostics(analyzer, diagnosticsAndSources.ExpectedDiagnostics);
            var sln = CodeFactory.CreateSolution(diagnosticsAndSources, analyzer, SuppressedDiagnostics, MetadataReferences);
            var diagnostics = Analyze.GetDiagnostics(analyzer, sln);
            VerifyDiagnostics(diagnosticsAndSources, diagnostics);
        }

        /// <summary>
        /// Verifies that <paramref name="codeWithErrorsIndicated"/> produces the expected diagnostics.
        /// </summary>
        /// <param name="analyzer">The analyzer to apply.</param>
        /// <param name="codeWithErrorsIndicated">The code with error positions indicated.</param>
        /// <param name="compilationOptions">The <see cref="CSharpCompilationOptions"/> to use.</param>
        /// <param name="metadataReferences">The meta data metadataReferences to use when compiling.</param>
        public static void Diagnostics(DiagnosticAnalyzer analyzer, string codeWithErrorsIndicated, CSharpCompilationOptions compilationOptions, IEnumerable<MetadataReference> metadataReferences)
        {
            Diagnostics(analyzer, new[] { codeWithErrorsIndicated }, compilationOptions, metadataReferences);
        }

        /// <summary>
        /// Verifies that <paramref name="codeWithErrorsIndicated"/> produces the expected diagnostics.
        /// </summary>
        /// <param name="analyzer">The analyzer to apply.</param>
        /// <param name="codeWithErrorsIndicated">The code with error positions indicated.</param>
        /// <param name="compilationOptions">The <see cref="CSharpCompilationOptions"/> to use.</param>
        /// <param name="metadataReferences">The meta data metadataReferences to use when compiling.</param>
        public static void Diagnostics(DiagnosticAnalyzer analyzer, IReadOnlyList<string> codeWithErrorsIndicated, CSharpCompilationOptions compilationOptions, IEnumerable<MetadataReference> metadataReferences)
        {
            var diagnosticsAndSources = DiagnosticsAndSources.CreateFromCodeWithErrorsIndicated(analyzer, codeWithErrorsIndicated);
            VerifyAnalyzerSupportsDiagnostics(analyzer, diagnosticsAndSources.ExpectedDiagnostics);
            var sln = CodeFactory.CreateSolution(diagnosticsAndSources.Code, compilationOptions, metadataReferences);
            var diagnostics = Analyze.GetDiagnostics(analyzer, sln);
            VerifyDiagnostics(diagnosticsAndSources, diagnostics);
        }

        /// <summary>
        /// Verifies that <paramref name="sources"/> produces the expected diagnostics.
        /// </summary>
        /// <param name="analyzer">The analyzer to apply.</param>
        /// <param name="sources">The code with error positions indicated.</param>
        /// <param name="compilationOptions">The <see cref="CSharpCompilationOptions"/> to use.</param>
        /// <param name="metadataReferences">The meta data metadataReferences to use when compiling.</param>
        /// <param name="expectedMessage">The expected message in the diagnostic produced by the analyzer.</param>
        /// <returns>The meta data from the run..</returns>
        [Obsolete("To be removed.")]
        public static async Task<DiagnosticsMetadata> DiagnosticsWithMetadataAsync(
            DiagnosticAnalyzer analyzer,
            DiagnosticsAndSources sources,
            CSharpCompilationOptions compilationOptions,
            IEnumerable<MetadataReference> metadataReferences,
            string expectedMessage = null)
        {
            if (sources.ExpectedDiagnostics.Count == 0)
            {
                throw new AssertException("Expected code to have at least one error position indicated with '↓'");
            }

            var data = await Analyze.GetDiagnosticsWithMetadataAsync(
                                        analyzer,
                                        sources.Code,
                                        compilationOptions,
                                        metadataReferences)
                                    .ConfigureAwait(false);

            var expecteds = sources.ExpectedDiagnostics;
            var actuals = data.Diagnostics
                              .SelectMany(x => x)
                              .ToArray();

            if (expecteds.SetEquals(actuals))
            {
                if (expectedMessage != null)
                {
                    foreach (var actual in data.Diagnostics.SelectMany(x => x))
                    {
                        var actualMessage = actual.GetMessage(CultureInfo.InvariantCulture);
                        TextAssert.AreEqual(expectedMessage, actualMessage, $"Expected and actual diagnostic message for the diagnostic {actual} does not match");
                    }
                }

                return new DiagnosticsMetadata(
                    sources.Code,
                    sources.ExpectedDiagnostics,
                    data.Diagnostics,
                    data.Solution);
            }

            var error = StringBuilderPool.Borrow();
            error.AppendLine("Expected and actual diagnostics do not match.");
            var missingExpected = expecteds.Except(actuals);
            for (var i = 0; i < missingExpected.Count; i++)
            {
                if (i == 0)
                {
                    error.Append("Expected:\r\n");
                }

                var expected = missingExpected[i];
                error.AppendLine(expected.ToString(sources.Code));
            }

            if (actuals.Length == 0)
            {
                error.AppendLine("Actual: <no errors>");
            }

            var missingActual = actuals.Except(expecteds);
            if (actuals.Length > 0 && missingActual.Count == 0)
            {
                error.AppendLine("Actual: <missing>");
            }

            for (var i = 0; i < missingActual.Count; i++)
            {
                if (i == 0)
                {
                    error.Append("Actual:\r\n");
                }

                var actual = missingActual[i];
                error.AppendLine(actual.ToErrorString());
            }

            throw new AssertException(StringBuilderPool.Return(error));
        }

        private static void VerifyDiagnostics(DiagnosticsAndSources diagnosticsAndSources, IReadOnlyList<ImmutableArray<Diagnostic>> actuals, string expectedMessage = null)
        {
            VerifyDiagnostics(diagnosticsAndSources, actuals.SelectMany(x => x).ToArray(), expectedMessage);
        }

        private static void VerifyDiagnostics(DiagnosticsAndSources diagnosticsAndSources, IReadOnlyList<Diagnostic> actuals, string expectedMessage = null)
        {
            if (diagnosticsAndSources.ExpectedDiagnostics.Count == 0)
            {
                throw new AssertException("Expected code to have at least one error position indicated with '↓'");
            }

            if (diagnosticsAndSources.ExpectedDiagnostics.SetEquals(actuals))
            {
                if (expectedMessage != null)
                {
                    foreach (var actual in actuals)
                    {
                        var actualMessage = actual.GetMessage(CultureInfo.InvariantCulture);
                        TextAssert.AreEqual(expectedMessage, actualMessage, $"Expected and actual diagnostic message for the diagnostic {actual} does not match");
                    }
                }

                return;
            }

            var error = StringBuilderPool.Borrow();
            if (actuals.Count == 1 &&
                diagnosticsAndSources.ExpectedDiagnostics.Count == 1 &&
                diagnosticsAndSources.ExpectedDiagnostics[0].Id == actuals[0].Id)
            {
                if (diagnosticsAndSources.ExpectedDiagnostics[0].PositionMatches(actuals[0]) &&
                    !diagnosticsAndSources.ExpectedDiagnostics[0].MessageMatches(actuals[0]))
                {
                    CodeAssert.AreEqual(diagnosticsAndSources.ExpectedDiagnostics[0].Message, actuals[0].GetMessage(CultureInfo.InvariantCulture), "Expected and actual messages do not match.");
                }
            }

            error.AppendLine("Expected and actual diagnostics do not match.");
            var missingExpected = diagnosticsAndSources.ExpectedDiagnostics.Except(actuals);
            for (var i = 0; i < missingExpected.Count; i++)
            {
                if (i == 0)
                {
                    error.Append("Expected:\r\n");
                }

                var expected = missingExpected[i];
                error.AppendLine(expected.ToString(diagnosticsAndSources.Code));
            }

            if (actuals.Count == 0)
            {
                error.AppendLine("Actual: <no errors>");
            }

            var missingActual = actuals.Except(diagnosticsAndSources.ExpectedDiagnostics);
            if (actuals.Count > 0 && missingActual.Count == 0)
            {
                error.AppendLine("Actual: <missing>");
            }

            for (var i = 0; i < missingActual.Count; i++)
            {
                if (i == 0)
                {
                    error.Append("Actual:\r\n");
                }

                var actual = missingActual[i];
                error.AppendLine(actual.ToErrorString());
            }

            throw new AssertException(error.Return());
        }

        /// <summary>
        /// Meta data from a call to GetAnalyzerDiagnosticsAsync
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "For debugging.")]
        [Obsolete("To be removed.")]
        public class DiagnosticsMetadata
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DiagnosticsMetadata"/> class.
            /// </summary>
            /// <param name="sources">The code to analyze.</param>
            /// <param name="expectedDiagnostics">Info about the expected diagnostics.</param>
            /// <param name="actualDiagnostics">The diagnostics returned from Roslyn</param>
            /// <param name="solution">The solution the analysis was run on.</param>
            public DiagnosticsMetadata(
                IReadOnlyList<string> sources,
                IReadOnlyList<ExpectedDiagnostic> expectedDiagnostics,
                IReadOnlyList<ImmutableArray<Diagnostic>> actualDiagnostics,
                Solution solution)
            {
                this.Sources = sources;
                this.ExpectedDiagnostics = expectedDiagnostics;
                this.ActualDiagnostics = actualDiagnostics;
                this.Solution = solution;
            }

            /// <summary>
            /// Gets the code that was analyzed.
            /// </summary>
            public IReadOnlyList<string> Sources { get; }

            /// <summary>
            /// Gets the meta data about the expected diagnostics.
            /// </summary>
            public IReadOnlyList<ExpectedDiagnostic> ExpectedDiagnostics { get; }

            /// <summary>
            /// Gets the actual diagnostics returned from Roslyn.
            /// </summary>
            public IReadOnlyList<ImmutableArray<Diagnostic>> ActualDiagnostics { get; }

            /// <summary>
            /// Gets the solution the analysis was performed on.
            /// </summary>
            public Solution Solution { get; }
        }
    }
}

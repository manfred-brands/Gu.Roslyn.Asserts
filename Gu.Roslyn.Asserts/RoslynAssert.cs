﻿namespace Gu.Roslyn.Asserts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.Asserts.Internals;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// The RoslynAssert class contains a collection of static methods used for assertions on the behavior of analyzers and code fixes.
    /// </summary>
    public static partial class RoslynAssert
    {
        /// <summary>
        /// Check that the <paramref name="analyzer"/> exports <paramref name="expectedDiagnostic"/>.
        /// </summary>
        /// <param name="analyzer">The <see cref="DiagnosticAnalyzer"/>.</param>
        /// <param name="expectedDiagnostic">The <see cref="ExpectedDiagnostic"/>.</param>
        public static void VerifyAnalyzerSupportsDiagnostic(DiagnosticAnalyzer analyzer, ExpectedDiagnostic expectedDiagnostic)
        {
            if (analyzer is null)
            {
                throw new ArgumentNullException(nameof(analyzer));
            }

            if (expectedDiagnostic is null)
            {
                throw new ArgumentNullException(nameof(expectedDiagnostic));
            }

            VerifyAnalyzerSupportsDiagnostic(analyzer, expectedDiagnostic.Id);
        }

        /// <summary>
        /// Check that the <paramref name="analyzer"/> exports <paramref name="descriptor"/>.
        /// </summary>
        /// <param name="analyzer">The <see cref="DiagnosticAnalyzer"/>.</param>
        /// <param name="descriptor">The <see cref="DiagnosticDescriptor"/>.</param>
        public static void VerifyAnalyzerSupportsDiagnostic(DiagnosticAnalyzer analyzer, DiagnosticDescriptor descriptor)
        {
            if (analyzer is null)
            {
                throw new ArgumentNullException(nameof(analyzer));
            }

            if (descriptor is null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            VerifyAnalyzerSupportsDiagnostic(analyzer, descriptor.Id);
        }

        /// <summary>
        /// Check that the <paramref name="analyzer"/> exports <paramref name="expectedDiagnostics"/>.
        /// </summary>
        /// <param name="analyzer">The <see cref="DiagnosticAnalyzer"/>.</param>
        /// <param name="expectedDiagnostics">The <see cref="IReadOnlyList{ExpectedDiagnostic}"/>.</param>
        internal static void VerifyAnalyzerSupportsDiagnostics(DiagnosticAnalyzer analyzer, IReadOnlyList<ExpectedDiagnostic> expectedDiagnostics)
        {
            foreach (var expectedDiagnostic in expectedDiagnostics)
            {
                VerifyAnalyzerSupportsDiagnostic(analyzer, expectedDiagnostic.Id);
            }
        }

        /// <summary>
        /// Check that the <paramref name="analyzer"/> exports <paramref name="descriptors"/>.
        /// </summary>
        /// <param name="analyzer">The <see cref="DiagnosticAnalyzer"/>.</param>
        /// <param name="descriptors">The <see cref="IReadOnlyList{DiagnosticDescriptor}"/>.</param>
        internal static void VerifyAnalyzerSupportsDiagnostics(DiagnosticAnalyzer analyzer, IReadOnlyList<DiagnosticDescriptor> descriptors)
        {
            foreach (var expectedDiagnostic in descriptors)
            {
                VerifyAnalyzerSupportsDiagnostic(analyzer, expectedDiagnostic.Id);
            }
        }

        /// <summary>
        /// Check that the analyzer supports a diagnostic with <paramref name="descriptor"/>.
        /// </summary>
        /// <param name="analyzer">The <see cref="DiagnosticAnalyzer"/>.</param>
        /// <param name="descriptor">The descriptor of the supported diagnostic.</param>
        internal static void VerifySingleSupportedDiagnostic(DiagnosticAnalyzer analyzer, out DiagnosticDescriptor descriptor)
        {
            if (analyzer.SupportedDiagnostics.Length == 0)
            {
                var message = $"{analyzer.GetType().Name}.SupportedDiagnostics returns an empty array.";
                throw new AssertException(message);
            }

            if (analyzer.SupportedDiagnostics.Length > 1)
            {
                var message = "This can only be used for analyzers with one SupportedDiagnostics.\r\n" +
                              "Prefer overload with ExpectedDiagnostic.";
                throw new AssertException(message);
            }

            descriptor = analyzer.SupportedDiagnostics[0];
            if (descriptor is null)
            {
                var message = $"{analyzer.GetType().Name}.SupportedDiagnostics[0] returns null.";
                throw new AssertException(message);
            }
        }

        /// <summary>
        /// Check that the analyzer supports a diagnostic with <paramref name="expectedId"/>.
        /// </summary>
        /// <param name="analyzer">The <see cref="DiagnosticAnalyzer"/>.</param>
        /// <param name="expectedId">The descriptor of the supported diagnostic.</param>
        internal static void VerifyAnalyzerSupportsDiagnostic(DiagnosticAnalyzer analyzer, string expectedId)
        {
            if (analyzer.SupportedDiagnostics.Length > 0 &&
                analyzer.SupportedDiagnostics.Length != analyzer.SupportedDiagnostics.Select(x => x.Id).Distinct().Count())
            {
                var message = $"{analyzer.GetType().Name}.SupportedDiagnostics has more than one descriptor with ID '{analyzer.SupportedDiagnostics.ToLookup(x => x.Id).First(x => x.Count() > 1).Key}'.";
                throw new AssertException(message);
            }

            var descriptors = analyzer.SupportedDiagnostics.Count(x => x.Id == expectedId);
            if (descriptors == 0)
            {
                var message = $"{analyzer.GetType().Name} does not produce a diagnostic with ID '{expectedId}'.{Environment.NewLine}" +
                              $"{analyzer.GetType().Name}.{nameof(analyzer.SupportedDiagnostics)}: {Format(analyzer.SupportedDiagnostics)}.{Environment.NewLine}" +
                              $"The expected diagnostic is: '{expectedId}'.";
                throw new AssertException(message);
            }

            if (descriptors > 1)
            {
                var message = $"{analyzer.GetType().Name} supports multiple diagnostics with ID '{expectedId}'.{Environment.NewLine}" +
                              $"{analyzer.GetType().Name}.{nameof(analyzer.SupportedDiagnostics)}: {Format(analyzer.SupportedDiagnostics)}.{Environment.NewLine}" +
                              $"The expected diagnostic is: {expectedId}.";
                throw new AssertException(message);
            }
        }

        private static void VerifyCodeFixSupportsAnalyzer(DiagnosticAnalyzer analyzer, CodeFixProvider fix)
        {
            if (!analyzer.SupportedDiagnostics.Select(d => d.Id).Intersect(fix.FixableDiagnosticIds).Any())
            {
                var message = $"{analyzer.GetType().Name} does not produce diagnostics fixable by {fix.GetType().Name}.{Environment.NewLine}" +
                              $"{analyzer.GetType().Name}.{nameof(analyzer.SupportedDiagnostics)}: {Format(analyzer.SupportedDiagnostics)}.{Environment.NewLine}" +
                              $"{fix.GetType().Name}.{nameof(fix.FixableDiagnosticIds)}: {Format(fix.FixableDiagnosticIds)}.";
                throw new AssertException(message);
            }
        }

        private static string Format(IEnumerable<DiagnosticDescriptor> supportedDiagnostics)
        {
            return Format(supportedDiagnostics.Select(x => x.Id));
        }

        private static string Format(IEnumerable<string> ids)
        {
            // ReSharper disable PossibleMultipleEnumeration
            return ids.TrySingle(out var single)
                ? $"'{single}'"
                : $"{{{string.Join(", ", ids)}}}";
            //// ReSharper restore PossibleMultipleEnumeration
        }

        private static async Task AreEqualAsync(IReadOnlyList<string> expected, Solution actual, string? messageHeader)
        {
            var actualCount = actual.Projects.SelectMany(x => x.Documents).Count();
            if (expected.Count != actualCount)
            {
                throw new AssertException($"Expected {expected.Count} documents the fixed solution has {actualCount} documents.");
            }

            foreach (var project in actual.Projects)
            {
                foreach (var document in project.Documents)
                {
                    var fixedSource = await CodeReader.GetStringFromDocumentAsync(document, CancellationToken.None).ConfigureAwait(false);
                    CodeAssert.AreEqual(FindExpected(fixedSource), fixedSource, messageHeader);
                }
            }

            string FindExpected(string fixedSource)
            {
                var fixedNamespace = CodeReader.Namespace(fixedSource);
                var fixedFileName = CodeReader.FileName(fixedSource);
                var match = expected.FirstOrDefault(x => x == fixedSource);
                if (match != null)
                {
                    return match;
                }

                foreach (var candidate in expected)
                {
                    if (CodeReader.Namespace(candidate) == fixedNamespace &&
                        CodeReader.FileName(candidate) == fixedFileName)
                    {
                        return candidate;
                    }
                }

                throw new AssertException($"The fixed solution contains a document {fixedFileName} in namespace {fixedNamespace} that is not in the expected documents.");
            }
        }
    }
}

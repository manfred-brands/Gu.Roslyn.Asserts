namespace Gu.Roslyn.Asserts.Analyzers.Tests.RenameLocalTests
{
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ArgumentAnalyzer();
        private static readonly CodeFixProvider Fix = new RenameFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.NameShouldMatchParameter);

        [Test]
        public static void WhenLocalAnalyzerWrongName()
        {
            var before = @"
namespace N
{
    using Microsoft.CodeAnalysis.Diagnostics;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        [Test]
        public static void M()
        {
            var code = ""class C { }"";
            var wrong = new PlaceholderAnalyzer();
            RoslynAssert.Valid(↓wrong, code);
        }
    }
}";

            var after = @"
namespace N
{
    using Microsoft.CodeAnalysis.Diagnostics;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        [Test]
        public static void M()
        {
            var code = ""class C { }"";
            var analyzer = new PlaceholderAnalyzer();
            RoslynAssert.Valid(analyzer, code);
        }
    }
}";
            var expectedDiagnostic = ExpectedDiagnostic.WithMessage("Name of 'wrong' should be 'analyzer'.");
            RoslynAssert.CodeFix(Analyzer, Fix, expectedDiagnostic, new[] { Code.PlaceholderAnalyzer, before }, after, fixTitle: "Rename to 'analyzer'.");
        }

        [Explicit("Temp suppress.")]
        [Test]
        public static void WhenFieldAnalyzerWrongName()
        {
            var before = @"
namespace N
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        private static readonly PlaceholderAnalyzer Wrong = new PlaceholderAnalyzer();

        [Test]
        public static void M()
        {
            var code = ""class C { }"";
            RoslynAssert.Valid(↓Wrong, code);
        }
    }
}";

            var after = @"
namespace N
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        private static readonly PlaceholderAnalyzer Analyzer = new PlaceholderAnalyzer();

        [Test]
        public static void M()
        {
            var code = ""class C { }"";
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}";
            var expectedDiagnostic = ExpectedDiagnostic.WithMessage("Name of 'Wrong' should be 'Analyzer'.");
            RoslynAssert.CodeFix(Analyzer, Fix, expectedDiagnostic, new[] { Code.PlaceholderAnalyzer, before }, after, fixTitle: "Rename to 'Analyzer'.");
        }

        [Test]
        public static void WhenOneParam()
        {
            var before = @"
namespace N
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        private static readonly PlaceholderAnalyzer Analyzer = new PlaceholderAnalyzer();

        [Test]
        public static void M()
        {
            var C = ""class C { }"";
            RoslynAssert.Valid(Analyzer, ↓C);
        }
    }
}";

            var after = @"
namespace N
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        private static readonly PlaceholderAnalyzer Analyzer = new PlaceholderAnalyzer();

        [Test]
        public static void M()
        {
            var code = ""class C { }"";
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}";
            var expectedDiagnostic = ExpectedDiagnostic.WithMessage("Name of 'C' should be 'code'.");
            RoslynAssert.CodeFix(Analyzer, Fix, expectedDiagnostic, new[] { Code.PlaceholderAnalyzer, before }, after);
        }

        [Test]
        public static void WhenOnlyOneBeforeHasPosition()
        {
            var before = @"
namespace N
{
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        [Test]
        public static void M()
        {
            var c1 = ""class C1 { }"";
            var code2 = ""class ↓C2 { }"";
            var after = ""class C2 { }"";
            RoslynAssert.CodeFix((DiagnosticAnalyzer)null, (CodeFixProvider)null, new [] { c1, code2 }, after);
        }
    }
}";

            var after = @"
namespace N
{
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        [Test]
        public static void M()
        {
            var c1 = ""class C1 { }"";
            var before = ""class ↓C2 { }"";
            var after = ""class C2 { }"";
            RoslynAssert.CodeFix((DiagnosticAnalyzer)null, (CodeFixProvider)null, new [] { c1, before }, after);
        }
    }
}";
            var expectedDiagnostic = ExpectedDiagnostic.WithMessage("Name of 'code2' should be 'before'.");
            var diagnosticsAndSources = new DiagnosticsAndSources(new[] { expectedDiagnostic }, new[] { before });
            RoslynAssert.CodeFix(Analyzer, Fix, diagnosticsAndSources, new[] { after }, fixTitle: "Rename to 'before'.");
        }

        [Test]
        public static void WhenOnlyOneCodeHasPosition()
        {
            var before = @"
namespace N
{
    using Microsoft.CodeAnalysis.Diagnostics;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        [Test]
        public static void M()
        {
            var code1 = ""class C1 { }"";
            var code2 = ""class ↓C2 { }"";
            var after = ""class C2 { }"";
            RoslynAssert.Diagnostics((DiagnosticAnalyzer)null, new [] { code1, code2 });
        }
    }
}";

            var after = @"
namespace N
{
    using Microsoft.CodeAnalysis.Diagnostics;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        [Test]
        public static void M()
        {
            var code1 = ""class C1 { }"";
            var code = ""class ↓C2 { }"";
            var after = ""class C2 { }"";
            RoslynAssert.Diagnostics((DiagnosticAnalyzer)null, new [] { code1, code });
        }
    }
}";
            var expectedDiagnostic = ExpectedDiagnostic.WithMessage("Name of 'code2' should be 'code'.");
            var diagnosticsAndSources = new DiagnosticsAndSources(new[] { expectedDiagnostic }, new[] { before });
            RoslynAssert.CodeFix(Analyzer, Fix, diagnosticsAndSources, new[] { after }, fixTitle: "Rename to 'code'.");
        }
    }
}

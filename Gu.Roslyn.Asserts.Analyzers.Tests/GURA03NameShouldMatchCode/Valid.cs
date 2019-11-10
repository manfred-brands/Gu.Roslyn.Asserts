namespace Gu.Roslyn.Asserts.Analyzers.Tests.GURA03NameShouldMatchCode
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new InvocationAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.GURA03NameShouldMatchCode;

        [TestCase("class C1 { }", "private const string C1")]
        [TestCase("class C1 { }", "private static readonly string C1")]
        [TestCase("class C1 { }", "const string C1")]
        [TestCase("public class C1 { }", "private const string C1")]
        [TestCase("public partial class C1 { }", "private const string C1")]
        public static void Class(string declaration, string field)
        {
            var code = @"
namespace N
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        private static readonly PlaceholderAnalyzer Analyzer = new PlaceholderAnalyzer();

        private const string C1 = @""
namespace N
{
    public class C1 { }
}"";

        [Test]
        public static void M()
        {
            var c2 = ""class C2 { }"";
            RoslynAssert.Valid(Analyzer, C1, c2);
        }
    }
}".AssertReplace("public class C1 { }", declaration)
  .AssertReplace("private const string C1", field);
            RoslynAssert.Valid(Analyzer, Descriptor, Code.PlaceholderAnalyzer, code);
        }

        [Test]
        public static void GenericClass()
        {
            var code = @"
namespace N
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        private static readonly PlaceholderAnalyzer Analyzer = new PlaceholderAnalyzer();

        private const string COfT = @""
namespace N
{
    public partial class C<T> { }
}"";

        [Test]
        public static void M()
        {
            var c2 = ""class C2 { }"";
            RoslynAssert.Valid(Analyzer, COfT, c2);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, Code.PlaceholderAnalyzer, code);
        }

        [Test]
        public static void Struct()
        {
            var code = @"
namespace N
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        private static readonly PlaceholderAnalyzer Analyzer = new PlaceholderAnalyzer();

        private const string S = @""
namespace N
{
    public struct S { }
}"";

        [Test]
        public static void M()
        {
            var c2 = ""class C2 { }"";
            RoslynAssert.Valid(Analyzer, S, c2);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, Code.PlaceholderAnalyzer, code);
        }

        [Test]
        public static void IgnorePartialClass()
        {
            var code = @"
namespace N
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        private static readonly PlaceholderAnalyzer Analyzer = new PlaceholderAnalyzer();

        private const string C1Part1 = @""
namespace N
{
    public partial class C1 { }
}"";

        private const string C1Part2 = @""
namespace N
{
    public partial class C1 { }
}"";

        [Test]
        public static void M()
        {
            var c2 = ""class C2 { }"";
            RoslynAssert.Valid(Analyzer, C1Part1, C1Part2, c2);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, Code.PlaceholderAnalyzer, code);
        }

        [Test]
        public static void Inline()
        {
            var code = @"
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
            RoslynAssert.Valid(Analyzer, ""class C2 { }"");
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, Code.PlaceholderAnalyzer, code);
        }

        [Test]
        public static void OneParam()
        {
            var code = @"
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
            var c1 = ""class C1 { }"";
            RoslynAssert.Valid(Analyzer, c1);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, Code.PlaceholderAnalyzer, code);
        }

        [Test]
        public static void TwoParams()
        {
            var code = @"
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
            var c1 = ""class C1 { }"";
            var c2 = ""class C2 { }"";
            RoslynAssert.Valid(Analyzer, c1, c2);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, Code.PlaceholderAnalyzer, code);
        }

        [Test]
        public static void TwoParamsExplicitArray()
        {
            var code = @"
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
            var c1 = ""class C1 { }"";
            var c2 = ""class C2 { }"";
            RoslynAssert.Valid(Analyzer, new []{ c1, c2 });
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, Code.PlaceholderAnalyzer, code);
        }

        [Test]
        public static void InlineStringEmptyParam()
        {
            var code = @"
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
            RoslynAssert.Valid(Analyzer, string.Empty);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, Code.PlaceholderAnalyzer, code);
        }

        [TestCase("string.Empty")]
        [TestCase("\"\"")]
        [TestCase("\"SYNTAX_ERROR\"")]
        public static void WhenNoNameInCode(string expression)
        {
            var code = @"
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
            var empty = string.Empty;
            RoslynAssert.Valid(Analyzer, empty);
        }
    }
}".AssertReplace("string.Empty", expression);

            RoslynAssert.Valid(Analyzer, Descriptor, Code.PlaceholderAnalyzer, code);
        }

        [Test]
        public static void WhenLocalNameMatchesParameter()
        {
            var code = @"
namespace N
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class C
    {
        private static readonly PlaceholderAnalyzer Analyzer = new PlaceholderAnalyzer();

        private const string C1 = @""
namespace N
{
    public class C1 { }
}"";

        [Test]
        public static void M()
        {
            var code = ""class C2 { }"";
            RoslynAssert.Valid(Analyzer, C1, code);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptors.GURA03NameShouldMatchCode, new[] { Code.PlaceholderAnalyzer, code });
        }
    }
}

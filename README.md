# Gu.Roslyn.Asserts

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/Gu.Roslyn.Asserts.svg)](https://www.nuget.org/packages/Gu.Roslyn.Asserts/)
[![Build status](https://ci.appveyor.com/api/projects/status/a0976a1dmtcx387r/branch/master?svg=true)](https://ci.appveyor.com/project/JohanLarsson/gu-roslyn-asserts/branch/master)
[![Build Status](https://dev.azure.com/guorg/Gu.Roslyn.Asserts/_apis/build/status/GuOrg.Gu.Roslyn.Asserts?branchName=master)](https://dev.azure.com/guorg/Gu.Roslyn.Asserts/_build/latest?definitionId=8&branchName=master)
[![Gitter](https://badges.gitter.im/GuOrg/Gu.Roslyn.Asserts.svg)](https://gitter.im/GuOrg/Gu.Roslyn.Asserts?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

Microsoft are working on an official package for testing analyzers: [Microsoft.CodeAnalysis.CSharp.CodeFix.Testing](https://dotnet.myget.org/feed/roslyn-analyzers/package/nuget/Microsoft.CodeAnalysis.CSharp.CodeFix.Testing.XUnit).

Hopefully this library will not be needed in the future.

<!--
[![Build status](https://ci.appveyor.com/api/projects/status/a0976a1dmtcx387r/branch/master?svg=true)](https://ci.appveyor.com/project/JohanLarsson/gu-roslyn-asserts/branch/master)
-->

Asserts for testing Roslyn analyzers.

Use 1.x for Microsoft.CodeAnalysis 1.x

- [RoslynAssert.Valid](#roslynassertvalid)
- [RoslynAssert.Diagnostics](#roslynassertdiagnostics)
- [CodeFix](#codefix)
  - [Code fix only](#code-fix-only)
- [FixAll](#fixall)
- [NoFix](#nofix)
- [Refactoring](#refactoring)
- [AST](#ast)
  - [SyntaxFactoryWriter](#syntaxfactorywriter)
- [Settings](#settings)
- [Analyze](#analyze)
  - [GetDiagnosticsAsync](#getdiagnosticsasync)
- [Fix](#fix)
- [CodeFactory](#codefactory)
  - [CreateSolution](#createsolution)
    - [Create a Microsoft.CodeAnalysis.AdhocWorkspace, a Roslyn Solution from code.](#create-a-microsoftcodeanalysisadhocworkspace--a-roslyn-solution-from-code)
    - [Create a Microsoft.CodeAnalysis.AdhocWorkspace, a Roslyn Solution from a file on disk.](#create-a-microsoftcodeanalysisadhocworkspace--a-roslyn-solution-from-a-file-on-disk)
- [Benchmark](#benchmark)
- [SyntaxNodeExt](#syntaxnodeext)
- [AstView](#astview)
- [Usage with different test project types](#usage-with-different-test-project-types)
  - [Net472 new project type.](#net472-new-project-type)
  - [NetCoreApp2.0](#netcoreapp20)


# RoslynAssert.Valid

Use `RoslynAssert.Valid<NopAnalyzer>(code)` to test that an analyzer does not report errors for valid code.
The code is checked so that it does not have any compiler errors either.
A typical test fixture looks like:

```c#
public class Valid
{
    private static readonly DiagnosticAnalyzer Analyzer = new YourAnalyzer();

    [Test]
    public void M()
    {
        var code = @"
namespace N
{
    class C
    {
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }
    ...
}
```

If the analyzer produces many diagnostics you can pass in a descriptor so that only diagnostics matching it are checked.

```c#
public class Valid
{
    private static readonly DiagnosticAnalyzer Analyzer = new YourAnalyzer();
    private static readonly DiagnosticDescriptor Descriptor = YourAnalyzer.SomeDescriptor;

    [Test]
    public void M()
    {
        var code = @"
namespace N
{
    class C
    {
    }
}";
        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }
    ...
}
```

When testing all analyzers something like this can be used:

```c#
public class Valid
{
    private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers =
        typeof(TypeInAnalyzerAssembly)
        .Assembly.GetTypes()
        .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
        .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
        .ToArray();


    private static readonly Solution ValidProjectSln = CodeFactory.CreateSolution(
        ProjectFile.Find("Valid.csproj"),
        AllAnalyzers);

    [TestCaseSource(nameof(AllAnalyzers))]
    public void Valid(DiagnosticAnalyzer analyzer)
    {
        RoslynAssert.Valid(analyzer, ValidProjectSln);
    }
}
```

# RoslynAssert.Diagnostics

Use `RoslynAssert.Diagnostics<FieldNameMustNotBeginWithUnderscore>(code)` to test that the analyzer reports an error or warning at the position indicated by the character `↓`. To type this character, hold down <kbd>Alt</kbd> and use the numpad to type the number <kbd>2</kbd><kbd>5</kbd>.

A typical test fixture looks like:

```c#
public class Diagnostics
{
    private static readonly DiagnosticAnalyzer Analyzer = new YourAnalyzer();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(YourAnalyzer.Descriptor);

    [Test]
    public void M()
    {
        var code = @"
namespace N
{
    class ↓C
    {
    }
}";
        RoslynAssert.Diagnostics(Analyzer, code);
    }

    [Test]
    public void M()
    {
        var code = @"
namespace N
{
    class ↓C
    {
    }
}";
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Don't name it C"), code);
    }
    ...
}
```

If the analyzer produces many diagnostics you can pass in a descriptor so that only diagnostics matching it are checked.

```c#
public class Diagnostics
{
    private static readonly DiagnosticAnalyzer Analyzer = new YourAnalyzer();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(YourAnalyzer.Descriptor);

    [Test]
    public void M()
    {
        var code = @"
namespace N
{
    class ↓Foo
    {
    }
}";
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }
    ...
}
```

If the analyzer supports many diagnostics the overload with `ExpectedDiagnostic` must be used. This suppresses all diagnstics other than the expected.

# CodeFix
Test that the analyzer reports an error or warning at the position indicated by the character `↓` and that the code fix fixes it and produces the expected code.
To type this character, hold down <kbd>Alt</kbd> and use the numpad to type the number <kbd>2</kbd><kbd>5</kbd>.

```c#
public class CodeFix
{
    private static readonly DiagnosticAnalyzer Analyzer = new FieldNameMustNotBeginWithUnderscore();
    private static readonly CodeFixProvider Fix = new SA1309CodeFixProvider();

	[Test]
	public void M()
	{
		var before = @"
namespace N
{
	class C
	{
		private readonly int ↓_value;
	}
}";

		var after = @"
namespace N
{
	class C
	{
		private readonly int value;
	}
}";
		RoslynAssert.CodeFix(Analyzer, Fix, before, after);
	}
}

```

A typical test fixture looks like:

```c#
public class CodeFix
{
    private static readonly DiagnosticAnalyzer Analyzer = new YourAnalyzer();
    private static readonly CodeFixProvider Fix = new YorCodeFixProvider();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(YourAnalyzer.Descriptor);

    [Test]
    public void M1()
    {
        var before = @"
namespace N
{
    class C
    {
        private readonly int ↓_value;
    }
}";

        var after = @"
namespace N
{
    class C
    {
        private readonly int value;
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, before, after);
    }
}
```

With explicit title for the fix to apply. Useful when there are many candidate fixes.

```cs
public class CodeFix
{
    private static readonly DiagnosticAnalyzer Analyzer = new YourAnalyzer();
    private static readonly CodeFixProvider Fix = new YorCodeFixProvider();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(YourAnalyzer.Descriptor);

    [Test]
    public void M1()
    {
        var before = @"
namespace N
{
    class C
    {
        private readonly int ↓_value;
    }
}";

        var after = @"
namespace N
{
    class C
    {
        private readonly int value;
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, before, after, fixTitle: "Don't use underscore prefix");
    }
    ...
}
```

If the analyzer supports many diagnostics the overload with `ExpectedDiagnostic` must be used. This suppresses all diagnostics other than the expected.

## Code fix only

When the code fix is for a warning produced by an analyzer that you do not own, for example a built in analyzer in Visual Studio.
```c#
public class CodeFix
{
    private static readonly CodeFixProvider Fix = new RemoveUnusedFixProvider();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("CS0067");

	[Test]
	public void TestThatCodeFixProducesExpectedCode()
	{
		var before = @"
namespace N
{
	using System;

	public class C
	{
		public event EventHandler ↓Bar;
	}
}";

		var after = @"
namespace N
{
	using System;

	public class C
	{
	}
}";
		RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after);
	}
}
```

# FixAll

When there are many issues that will be fixed:
RoslynAssert.FixAll does:
- Fix all diagnostics one by one
- Fix all diagnostics in all supported scopes.

```c#
public class CodeFix
{
    private static readonly DiagnosticAnalyzer Analyzer = new FieldNameMustNotBeginWithUnderscore();
    private static readonly CodeFixProvider Fix = new SA1309CodeFixProvider();

	[Test]
	public void M()
	{
		var before = @"
namespace N
{
	class C
	{
		private readonly int ↓_value1;
		private readonly int ↓_value2;
	}
}";

		var after = @"
namespace N
{
	class C
	{
		private readonly int value1;
		private readonly int value2;
	}
}";
		RoslynAssert.FixAll(Analyzer, Fix, before, after);
	}
}
```

# NoFix

Test that the analyzer reports an error or warning at the position indicated by the character `↓` and that the code fix does not change the code.
To type this character, hold down <kbd>Alt</kbd> and use the numpad to type the number <kbd>2</kbd><kbd>5</kbd>.
This can happen if for example it is decided to not support rare edge cases with the code fix.

```c#
public class CodeFix
{
    private static readonly DiagnosticAnalyzer Analyzer = new FieldNameMustNotBeginWithUnderscore();
    private static readonly CodeFixProvider Fix = new SA1309CodeFixProvider();

	[Test]
	public void M()
	{
		var code = @"
namespace N
{
	class C
	{
		private readonly int ↓_value;
	}
}";

		RoslynAssert.NoFix(Analyzer, Fix, code);
	}
}
```

# Refactoring
```cs
public class CodeFix
{
	private static readonly CodeRefactoringProvider Refactoring = new ClassNameToUpperCaseRefactoringProvider();

    [Test]
    public void M()
    {
        var before = @"
class ↓c
{
}";

        var after = @"
class C
{
}";
        RoslynAssert.Refactoring(Refactoring, before, after);
		// Or if you want to assert on title also
		RoslynAssert.Refactoring(Refactoring, before, after, title: "Change to uppercase.");
    }
}
```

# AST

For checking every node and token in the tree.

```cs
[Test]
public void CheckAst()
{
    var actual = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName("a"), SyntaxFactory.IdentifierName("b"));
    var expected = CSharpSyntaxTree.ParseText("var c = a + b").FindAssignmentExpression("a + b");
    RoslynAssert.Ast(expected, actual);
}
```

## SyntaxFactoryWriter

Get a string with a call to SyntaxFactory for generating the code passed in.

```cs
var code = @"namespace A.B
{
    public class C
    {
    }
}";
var call = SyntaxFactoryWriter.Serialize(code);
```

# Settings

Settings.Default is meant to be set once and contains information used by asserts and code factory methods.
If specific settings are required for a test there are overloads acceping a settings intance.

### Sample ModuleInitializer.cs (for the test project.)

```c#
internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        Settings.Default = Settings.Default.WithMetadataReferences(
            // This adds all transitive metadata references from containing project.
            Asserts.MetadataReferences.Transitive(typeof(ModuleInitializer)));
    }
}
```

# Analyze

## GetDiagnosticsAsync

Analyze a cs, csproj or sln file on disk.

```c#
[Test]
public async Task GetDiagnosticsFromProjectOnDisk()
{
    var dllFile = new Uri(Assembly.GetExecutingAssembly().CodeBase, UriKind.Absolute).LocalPath;
    Assert.AreEqual(true, CodeFactory.TryFindProjectFile(new FileInfo(dllFile), out FileInfo projectFile));
    var diagnostics = await Analyze.GetDiagnosticsAsync(new FieldNameMustNotBeginWithUnderscore(), projectFile)
                                    .ConfigureAwait(false);
    ...
}
```

# Fix

When dropping down to manual mode `Analyze` & `Fix` can be used like this:

```cs
[Test]
public void SingleClassOneErrorCorrectFix()
{
    var code = @"
namespace N
{
    class Foo
    {
        private readonly int _value;
    }
}";

    var after = @"
namespace N
{
    class Foo
    {
        private readonly int value;
    }
}";
    var analyzer = new FieldNameMustNotBeginWithUnderscore();
    var settings = Settings.Default.WithCompilationOptions(CodeFactory.DefaultCompilationOptions(analyzer))
                                   .WithMetadataReferences(MetadataReference.CreateFromFile(typeof(int).Assembly.Location))
    var sln = CodeFactory.CreateSolution(code, settings: settings);
    var diagnostics = Analyze.GetDiagnostics(sln, analyzer);
    var fixedSln = Fix.Apply(sln, new DoNotUseUnderscoreFix(), diagnostics);
    CodeAssert.AreEqual(after, fixedSln.Projects.Single().Documents.Single());
}
```

# CodeFactory

## CreateSolution

### Create a Microsoft.CodeAnalysis.AdhocWorkspace, a Roslyn Solution from code.

```c#
[Test]
public void CreateSolutionFromSources()
{
    var code = @"
namespace N
{
    class Foo
    {
        private readonly int _value;
    }
}";
    var sln = CodeFactory.CreateSolution(code, new[] { new FieldNameMustNotBeginWithUnderscore() });
    Assert.AreEqual("N", sln.Projects.Single().Name);
    Assert.AreEqual("Foo.cs", sln.Projects.Single().Documents.Single().Name);
}

[Test]
public void CreateSolutionFromSources()
{
    var code1 = @"
namespace Project1
{
    class Foo1
    {
        private readonly int _value;
    }
}";

    var code2 = @"
namespace Project2
{
    class Foo2
    {
        private readonly int _value;
    }
}";
    var sln = CodeFactory.CreateSolution(new[] { code1, code2 }, new[] { new FieldNameMustNotBeginWithUnderscore() });
    CollectionAssert.AreEqual(new[] { "Project1", "Project2" }, sln.Projects.Select(x => x.Name));
    Assert.AreEqual(new[] { "Foo1.cs", "Foo2.cs" }, sln.Projects.Select(x => x.Documents.Single().Name));
}
```

### Create a Microsoft.CodeAnalysis.AdhocWorkspace, a Roslyn Solution from a file on disk.

```c#
[Test]
public void CreateSolutionFromProjectFile()
{
    Assert.AreEqual(
        true,
        CodeFactory.TryFindProjectFile(
            new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase, UriKind.Absolute).LocalPath),
            out FileInfo projectFile));
    var solution = CodeFactory.CreateSolution(projectFile);
}

[Test]
public void CreateSolutionFromSolutionFile()
{
    Assert.AreEqual(
        true,
        CodeFactory.TryFindFileInParentDirectory(
            new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase, UriKind.Absolute).LocalPath).Directory, "Gu.Roslyn.Asserts.sln",
            out FileInfo solutionFile));
    var solution = CodeFactory.CreateSolution(solutionFile);
}
```

# Benchmark

Sample benchmark using BenchmarkDotNet.

```cs
public class FieldNameMustNotBeginWithUnderscoreBenchmark
{
    private static readonly Solution Solution = CodeFactory.CreateSolution(
        CodeFactory.FindSolutionFile("Gu.Roslyn.Asserts.sln"));

    private static readonly Benchmark Benchmark = Benchmark.Create(Solution, new FieldNameMustNotBeginWithUnderscore());

    [BenchmarkDotNet.Attributes.Benchmark]
    public void RunOnGuRoslynAssertsSln()
    {
        Benchmark.Run();
    }
}
```

# SyntaxNodeExt
```cs
[Test]
public void FindAssignmentExpressionDemo()
{
    var syntaxTree = CSharpSyntaxTree.ParseText(
        @"
namespace N
{
    internal class Foo
    {
        internal Foo()
        {
            var temp = 1;
            temp = 2;
        }
    }
}");
    var compilation = CSharpCompilation.Create("test", new[] { syntaxTree });
    var semanticModel = compilation.GetSemanticModel(syntaxTree);
    var assignment = syntaxTree.FindAssignmentExpression("temp = 2");
    Assert.AreEqual("temp = 2", assignment.ToString());
    Assert.AreEqual("int", semanticModel.GetTypeInfo(assignment.Right).Type.ToDisplayString());
}
```

# AstView
![Animation](https://user-images.githubusercontent.com/1640096/60766676-77ba5f80-a0ad-11e9-95c2-1b789d5490be.gif)

# Usage with different test project types
## Net472 new project type.
```xml
<PropertyGroup>
  <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
  <GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>
</PropertyGroup>
```


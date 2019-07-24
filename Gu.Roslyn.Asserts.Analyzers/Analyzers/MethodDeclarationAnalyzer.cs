namespace Gu.Roslyn.Asserts.Analyzers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodDeclarationAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.GURA06TestShouldBeInCorrectClass,
            Descriptors.GURA09UseStandardNames);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.MethodDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is MethodDeclarationSyntax methodDeclaration &&
                context.ContainingSymbol is IMethodSymbol method &&
                InvocationWalker.TryFindRoslynAssert(methodDeclaration, out var invocation))
            {
                if (invocation.TryGetMethodName(out var name) &&
                    name != method.ContainingType.Name)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.GURA06TestShouldBeInCorrectClass,
                        methodDeclaration.Identifier.GetLocation(),
                        ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), name),
                        name));
                }

                using (var walker = StringLiteralWalker.BorrowAndVisit(methodDeclaration))
                {
                    while (walker.TryFindReplacement(out var before, out var location, out var after))
                    {
                        if (after != null)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                Descriptors.GURA09UseStandardNames,
                                location,
                                ImmutableDictionary<string, string>.Empty.Add("before", before)
                                                                         .Add("after", after),
                                $"Use standard name {after} instead of {before}."));
                        }
                        else
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                Descriptors.GURA09UseStandardNames,
                                location,
                                ImmutableDictionary<string, string>.Empty.Add("before", before),
                                $"Use standard name instead of {before}."));
                        }
                    }
                }
            }
        }

        private sealed class StringLiteralWalker : PooledWalker<StringLiteralWalker>
        {
            private static readonly string[] Words =
            {
                "Foo",
                "IFoo",
                "Bar",
                "IBar",
                "Baz",
                "IBaz",
                "Meh",
                "IMeh",
                "Lol",
                "ILol",
                "SomeClass",
                "SomeInterface",
                "ISomeInterface",
                "SomeField",
                "SomeEvent",
                "SomeProperty",
                "SomeMethod",
            };

            private static readonly string[] PropertyWords =
            {
                "A",
                "B",
                "C",
                "D",
                "E",
                "F",
                "G",
            };

            private readonly List<LiteralExpressionSyntax> literals = new List<LiteralExpressionSyntax>();
            private readonly ConcurrentDictionary<LiteralExpressionSyntax, CompilationUnitSyntax> roots = new ConcurrentDictionary<LiteralExpressionSyntax, CompilationUnitSyntax>();
            private readonly HashSet<TextSpan> locations = new HashSet<TextSpan>();

            private StringLiteralWalker()
            {
            }

            public override void VisitLiteralExpression(LiteralExpressionSyntax node)
            {
                if (node.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    this.literals.Add(node);
                }

                base.VisitLiteralExpression(node);
            }

            internal static StringLiteralWalker BorrowAndVisit(SyntaxNode node) => BorrowAndVisit(node, () => new StringLiteralWalker());

            internal bool TryFindReplacement(out string before, out Location location, out string after)
            {
                foreach (var literal in this.literals)
                {
                    foreach (var word in Words)
                    {
                        var index = -1;
                        while (this.TryFindToken(literal, word, index + 1, StringComparison.OrdinalIgnoreCase, out index, out var token))
                        {
                            var candidateLocation = literal.SyntaxTree.GetLocation(new TextSpan(literal.SpanStart + index, token.ValueText.Length));
                            if (token.IsKind(SyntaxKind.IdentifierToken) &&
                                this.locations.Add(candidateLocation.SourceSpan) &&
                                ShouldWarn(token))
                            {
                                before = token.ValueText;
                                location = candidateLocation;
                                after = this.Replace(token);
                                return true;
                            }
                        }
                    }

                    foreach (var word in PropertyWords)
                    {
                        var index = -1;
                        while (this.TryFindToken(literal, word, index + 1, StringComparison.OrdinalIgnoreCase, out index, out var token))
                        {
                            var candidateLocation = literal.SyntaxTree.GetLocation(new TextSpan(literal.SpanStart + index, token.ValueText.Length));
                            if (token.IsKind(SyntaxKind.IdentifierToken) &&
                                this.locations.Add(candidateLocation.SourceSpan) &&
                                token.Parent.IsKind(SyntaxKind.PropertyDeclaration))
                            {
                                before = token.ValueText;
                                location = candidateLocation;
                                after = this.Replace(token);
                                return true;
                            }
                        }
                    }
                }

                before = null;
                location = null;
                after = null;
                return false;

                bool ShouldWarn(SyntaxToken candidate)
                {
                    switch (candidate.Parent.Kind())
                    {
                        case SyntaxKind.StringLiteralExpression:
                        case SyntaxKind.IdentifierName:
                            return false;
                        default:
                            return true;
                    }
                }
            }

            protected override void Clear()
            {
                this.literals.Clear();
                this.roots.Clear();
                this.locations.Clear();
            }

            private static bool TryIndexOf(LiteralExpressionSyntax literal, string text, int startIndex, StringComparison stringComparison, out int index)
            {
                index = literal.Token.Text.IndexOf(text, startIndex, stringComparison);
                return index >= 0;
            }

            private string Replace(SyntaxToken token)
            {
                switch (token.Parent)
                {
                    case EnumDeclarationSyntax declaration:
                        switch (token.ValueText)
                        {
                            case "Foo":
                                return this.ReplaceTypeName(new Names("E", "E1"), declaration);
                            case "Bar":
                                return this.ReplaceTypeName(new Names("E", "E2"), declaration);
                            case "Baz":
                                return this.ReplaceTypeName(new Names("E", "E3"), declaration);
                            default:
                                return this.ReplaceTypeName(new Names("E", null), declaration);
                        }

                    case ClassDeclarationSyntax declaration:
                        switch (token.ValueText)
                        {
                            case "Foo":
                                return this.ReplaceTypeName(new Names("C", "C1"), declaration);
                            case "Bar":
                                return this.ReplaceTypeName(new Names("C", "C2"), declaration);
                            case "Baz":
                                return this.ReplaceTypeName(new Names("C", "C3"), declaration);
                            default:
                                return this.ReplaceTypeName(new Names("C", null), declaration);
                        }

                    case StructDeclarationSyntax declaration:
                        switch (token.ValueText)
                        {
                            case "Foo":
                                return this.ReplaceTypeName(new Names("S", "S1"), declaration);
                            case "Bar":
                                return this.ReplaceTypeName(new Names("S", "S2"), declaration);
                            case "Baz":
                                return this.ReplaceTypeName(new Names("S", "S3"), declaration);
                            default:
                                return this.ReplaceTypeName(new Names("S", null), declaration);
                        }

                    case InterfaceDeclarationSyntax declaration:
                        switch (token.ValueText)
                        {
                            case "IFoo":
                                return this.ReplaceTypeName(new Names("I", "I1"), declaration);
                            case "IBar":
                                return this.ReplaceTypeName(new Names("I", "I2"), declaration);
                            case "IBaz":
                                return this.ReplaceTypeName(new Names("I", "I3"), declaration);
                            default:
                                return this.ReplaceTypeName(new Names("I", null), declaration);
                        }

                    case FieldDeclarationSyntax declaration:
                        return this.ReplaceMemberName("F", declaration);
                    case EventDeclarationSyntax declaration:
                        return this.ReplaceMemberName("E", declaration);
                    case EventFieldDeclarationSyntax declaration:
                        return this.ReplaceMemberName("E", declaration);
                    case PropertyDeclarationSyntax declaration:
                        return this.ReplaceMemberName("P", declaration);
                    case MethodDeclarationSyntax declaration:
                        return this.ReplaceMemberName("M", declaration);
                }

                return null;
            }

            private string ReplaceTypeName(Names candidateNames, BaseTypeDeclarationSyntax declaration)
            {
                if (this.literals.TrySingle(x => this.TryGetRoot(x, out _), out var single))
                {
                    var index = -1;
                    while (this.TryFindToken(single, candidateNames.WhenSingle, index + 1, StringComparison.Ordinal, out index, out var candidateToken))
                    {
                        switch (candidateToken.Parent)
                        {
                            case BaseTypeDeclarationSyntax member when declaration.Contains(member):
                                return null;
                            case MemberDeclarationSyntax member when declaration.Contains(member):
                                return null;
                        }
                    }

                    return candidateNames.WhenSingle;
                }

                if (candidateNames.Else is string name)
                {
                    foreach (var candidateLiteral in this.literals)
                    {
                        var index = -1;
                        while (this.TryFindToken(candidateLiteral, name, index + 1, StringComparison.Ordinal, out index, out var candidateToken))
                        {
                            switch (candidateToken.Parent)
                            {
                                case BaseTypeDeclarationSyntax member when member != declaration:
                                    return null;
                                case MemberDeclarationSyntax member when declaration.Contains(member):
                                    return null;
                            }
                        }
                    }
                }

                return candidateNames.Else;
            }

            private string ReplaceMemberName(string name, MemberDeclarationSyntax declaration)
            {
                switch (declaration.Parent)
                {
                    case TypeDeclarationSyntax typeDeclaration:
                        {
                            if (OnlyOverloads() ||
                                OnlyOneOfKind())
                            {
                                return name;
                            }

                            var i = 1;
                            while (typeDeclaration.Members.TryFirst(x => IsCollision(x), out _))
                            {
                                i++;
                            }

                            return $"{name}{i}";

                            bool OnlyOverloads()
                            {
                                if (declaration is MethodDeclarationSyntax methodDeclaration)
                                {
                                    foreach (var member in typeDeclaration.Members)
                                    {
                                        if (member is MethodDeclarationSyntax method &&
                                            method.Identifier.ValueText != methodDeclaration.Identifier.ValueText)
                                        {
                                            return false;
                                        }
                                    }

                                    return true;
                                }

                                return false;
                            }

                            bool OnlyOneOfKind()
                            {
                                foreach (var member in typeDeclaration.Members)
                                {
                                    if (member.Kind() == declaration.Kind() &&
                                        member != declaration)
                                    {
                                        return false;
                                    }
                                }

                                return true;
                            }

                            bool IsCollision(MemberDeclarationSyntax candidate)
                            {
                                switch (candidate)
                                {
                                    case MethodDeclarationSyntax candidateDeclaration:
                                        if (declaration is MethodDeclarationSyntax method)
                                        {
                                            if (candidateDeclaration.Identifier.ValueText.IsParts(name, i.ToString()))
                                            {
                                                if (method.ParameterList.Parameters.Count ==
                                                    candidateDeclaration.ParameterList.Parameters.Count)
                                                {
                                                    for (var j = 0; j < method.ParameterList.Parameters.Count; j++)
                                                    {
                                                        if (!method.ParameterList.Parameters[j].Type.IsEquivalentTo(candidateDeclaration.ParameterList.Parameters[j].Type))
                                                        {
                                                            return false;
                                                        }
                                                    }

                                                    return true;
                                                }
                                            }

                                            return false;
                                        }

                                        return candidateDeclaration.Identifier.ValueText.IsParts(name, i.ToString());
                                    case TypeDeclarationSyntax candidateDeclaration:
                                        return candidateDeclaration.Identifier.ValueText.IsParts(name, i.ToString());
                                    case EnumDeclarationSyntax candidateDeclaration:
                                        return candidateDeclaration.Identifier.ValueText.IsParts(name, i.ToString());
                                    case PropertyDeclarationSyntax candidateDeclaration:
                                        return candidateDeclaration.Identifier.ValueText.IsParts(name, i.ToString());
                                    case EventDeclarationSyntax candidateDeclaration:
                                        return candidateDeclaration.Identifier.ValueText.IsParts(name, i.ToString());
                                    case BaseFieldDeclarationSyntax candidateDeclaration:
                                        return candidateDeclaration.Declaration.Variables.TrySingle(x => x.Identifier.ValueText.IsParts(name, i.ToString()), out _);
                                    default:
                                        return true;
                                }
                            }
                        }

                    case EnumDeclarationSyntax enumDeclaration:
                        {
                            if (enumDeclaration.Members.TrySingle(out _))
                            {
                                return name;
                            }

                            var i = 1;
                            while (enumDeclaration.Members.TryFirst(x => x.Identifier.ValueText.IsParts(name, i.ToString()), out _))
                            {
                                i++;
                            }

                            return $"{name}{i}";
                        }

                    case BaseTypeDeclarationSyntax _:
                        return name;
                    default:
                        return null;
                }
            }

            private bool TryGetRoot(LiteralExpressionSyntax literal, out CompilationUnitSyntax root)
            {
                root = this.roots.GetOrAdd(literal, x =>
                {
                    if (CSharpSyntaxTree.ParseText(literal.Token.ValueText).TryGetRoot(out var node))
                    {
                        return node as CompilationUnitSyntax;
                    }

                    return null;
                });

                return root != null;
            }

            private bool TryFindToken(LiteralExpressionSyntax literal, string word, int startIndex, StringComparison stringComparison, out int index, out SyntaxToken token)
            {
                if (TryIndexOf(literal, word, startIndex, stringComparison, out index) &&
                    this.TryGetRoot(literal, out var root))
                {
                    var offset = literal.Token.Text.Length - literal.Token.ValueText.Length - 1;
                    token = root.FindToken(index - offset);
                    return true;
                }

                token = default;
                return false;
            }

            private struct Names
            {
                internal readonly string WhenSingle;
                internal readonly string Else;

                public Names(string whenSingle, string @else)
                {
                    this.WhenSingle = whenSingle;
                    this.Else = @else;
                }
            }
        }
    }
}

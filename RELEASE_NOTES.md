#### 4.0.4
* BUGFIX: stop infinite loop in fix all when fix updates the code without fixing the issue.

#### 4.0.3
* Better error message for RoslynAssert.CodeFix
* OBSOLETE: ExpectedDiagnostic.FromMarkup.

#### 4.0.2
* BREAKING: Remove duplicate enum AllowedCompilationDiagnostics.

#### 4.0.1
* BREAKING: Broke everything, all I can say is I'm sorry. See the documentation for new API. Found no way out of the mess I created before.

#### 3.3.1
* BUGFIX: Handle collision in metadata references.

#### 3.2.2
* BUGFIX: infinite loop in analyzer when looking for replacements.
* OBSOLETE: for changing to RoslynAssert.MetadataReferences. (Code fix changing to preferred API)
* Make analyzers default disabled. Most of them are pretty useless.
* BUGFIX: SolutionFile.Find(Assembly) when more than one sln.

#### 3.2
* FEATURE: Copy to local analyzer and fix.
* BUGFIX: Allow CodeFix to have many diagnostics as long as there is only one fix.
* OBSOLETE: Global state, prefer attributes.
* FEATURE: SuppressWarningsAttribute
* BREAKING: Rename parameters suppressWarnings was suppressedDiagnostics (with code fix)
* FEATURE: Handle AdditionalLocations
* USABILITY: Improved error messages.

#### 3.1
* FEATURE: Better error message: include compiler error if no diagnostics.
* FEATURE: Analyzer nuget for fixing breaking changes.
* BREAKING: Synchronized parameter names and positions. Shipping code fixes that fixes breakages.
* BREAKING: More optional parameters for many overloads.
* OBSOLETE: Generic API added a code fix for refactoring to preferred API.
* BREAKING: Require code in RoslynAssert.Diagnostics to compile. This may break existing tests but it is a good thing.

#### 3.0
* BREAKING: Microsoft.CodeAnalysis >= 3.
* BREAKING: Drop net46 support.

#### 2.9.0
* FEATURE: RoslynAssert.NoRefactoring.
* BREAKING: Move title parameter in RoslynAssert.Refactoring.
* BUGFIX: Swap actual and expected in RoslynAssert.Refactoring.

#### 2.8.0
* FEATURE: Make trivia configurable in AstWriter output.
* BREAKING: AstWriterSettings(AstFormat) was AstWriterSettings(bool)
* FEATURE: Setting for ignoring empty trivia.

#### 2.7.1
* BUGFIX: RoslynAssert.CodeFix() when partial classes in multiple documents.

#### 2.7.0
* FEATURE: RoslynAssert.CodeFix() supports fix changing multiple documents.
* FEATURE: RoslynAssert.CodeFix() supports fix changing and or adding multiple documents.
* OBSOLETE: Use RoslynAssert and not AnalyzerAssert. Ctrl + h refactor it.

#### 2.6.3
* FEATURE: MetadataReferences.CreateBinary()
* BREAKING: Refactor overloads and use optional parameters.

#### 2.6.2
* MetadataReferences.Transitive(type) handle generic types.
* Use reference assemblies.

#### 2.6.0
* Compile valid code once.
* Use DiagnosticDescriptor in Valid. Old API made [Obsolete]
* Better error when two descriptors have the same ID.
* Use LanguageVersion.Latest.

#### 2.5.0
* BREAKING: Don't throw test framework exceptions.
* BREAKING: Require no compiler errors in AnalyzerAssert.Valid.
* FEATURE: CodeAssert better message when differ at end.
* FEATURE: Better message when message differs.

#### 2.4.2
* FEATURE: AnalyzerAssert.Ast.
* FEATURE: AstWriter.

#### 2.4.0
* BREAKING: Mark async API obsolete.
* FEATURE: Create sln from github url.
* FEATURE: Support testing refactorings.
* FEATURE: Multitarget net46 & netstandard2.0
* BREAKING: Probably changed some overload.

#### 2.3.1
* BUGFIX: ExpectedDiagnostic.Create without path should nolt throw.

#### 2.3.1
* BUGFIX: FindExpression

#### 2.3.0
* FEATURE: Limited support for resolving references when parsing project & sln files.
* FEATURE: Add more metadata when parsing files.
* FEATURE: Expose fix methods.
* BUGFIX: The project already transitively references the target project. #53
* BUGFIX: Apply fixes one-by-one in document order. #51
* FEATURE: More overloads to CodeFix & FixAll. #50

#### 2.2.9
* BUGFIX: Find with whitespace.

#### 2.2.8
* FEATURE: Allow code to contain node code in Find methods.

#### 2.2.7
* BUGFIXES: TryFindInvocation when argument is invocation.

#### 2.2.6
* BUGFIXES: TryFind methods.

#### 2.2.5
* FEATURE: Make more Analyze methods public.

#### 2.2.4
* BUGFIX NoFix handles expected diagnostic with error indicated.

#### 2.2.3
* BREAKING: NoFix is stricter now, requires no registered code action
* BUGFIX: Handle suppressing one of the diagnostics the analyzer supports.

#### 2.2.2
* FEATURE: Reuse shared workspace when creating solutions.

#### 2.2.1
* BUGFIX: handle many analyzers for same diagnostic.

#### 2.2.0
* BUGFIX: handle expected diagnostic when analyzer supports many.
* BREAKING: Removed obsolete ErrorMessage

#### 2.1.1
* BUGFIX: remove check for single diagnostic.

#### 2.1.0
* FEATURE: handle error indicated in code with expected diagnostic
* FEATURE: AnalyzerAssert.CodeFix<TAnalyzer, TCodeFix>with expected diagnostic

#### 2.0.0
Use this version for Microsoft.CodeAnalysis.CSharp 2.x

#### 1.0.0
Use this version for Microsoft.CodeAnalysis.CSharp 1.x

#### 0.4.0
* BUGFIX: Better heuristics for determining if a csproj is new format
* FEATURE: CodeFactory.CreateSolutionWithOneProject
* FEATURE: CodeComparer
* FEATURE ExpectedDiagnostic.
* BREAKING: Change signature of AnalyzerAssert.DiagnosticsWithMetadataAsync
* BREAKING: Move DiagnosticsAndSources to separate class.

#### 0.3.6
* BUGFIX: Parse filenames with error indicators.
* FEATURE Benchmark API.

#### 0.3.5
* BUGFIX: FixAll when multiple projects.

#### 0.3.4
* FEATURE: Keep format for untouched parts in code fix.

#### 0.3.3
* FEATURE: FixAllInDocument
* FEATURE: FindProjectFile & FindSolutionFile

#### 0.3.2
* FEATURE: Figure out intra project dependencies

#### 0.3.1
* FEATURE: Add transitive dependencies.

#### 0.3.0
* BREAKING: Remove obsolete NoDiagnostics
* FEATURE: AnalyzerAssert.suppressWarnings
* FEATURE: overloads with CSharpCompilationOptions.
* FEATURE: Shallower stacks in exceptions.


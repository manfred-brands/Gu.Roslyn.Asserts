﻿namespace Gu.Roslyn.Asserts.Tests
{
    using System.Runtime.CompilerServices;

    using Microsoft.CodeAnalysis;

    internal static class ModuleInitializer
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            Settings.Default = new Settings(
                CodeFactory.DefaultCompilationOptions(null),
                new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location).WithAliases(new[] { "global", "mscorlib" }),
                    MetadataReference.CreateFromFile(typeof(System.Diagnostics.Debug).Assembly.Location).WithAliases(new[] { "global", "System" }),
                });
        }
    }
}

﻿namespace Gu.Roslyn.Asserts
{
    using System;
    using Gu.Roslyn.Asserts.Internals;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Assert for testing if code equals.
    /// </summary>
    public static class CodeAssert
    {
        /// <summary>
        /// Verify that two strings of code are equal. Agnostic to end of line characters.
        /// </summary>
        /// <param name="expected">The expected code.</param>
        /// <param name="actual">The actual code.</param>
        public static void AreEqual(string expected, string actual)
        {
            if (expected is null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual is null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            AreEqual(expected, actual, null);
        }

        /// <summary>
        /// Verify that two strings of code are equal. Agnostic to end of line characters.
        /// </summary>
        /// <param name="expected">The expected code.</param>
        /// <param name="actual">The actual code.</param>
        public static void AreEqual(string expected, Document actual)
        {
            if (expected is null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual is null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            AreEqual(expected, actual.GetCode(), null);
        }

        /// <summary>
        /// Verify that two strings of code are equal. Agnostic to end of line characters.
        /// </summary>
        /// <param name="expected">The expected code.</param>
        /// <param name="actual">The actual code.</param>
        public static void AreEqual(Document expected, Document actual)
        {
            if (expected is null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual is null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            AreEqual(expected.GetCode(), actual.GetCode(), null);
        }

        /// <summary>
        /// Verify that two strings of code are equal. Agnostic to end of line characters.
        /// </summary>
        /// <param name="expected">The expected code.</param>
        /// <param name="actual">The actual code.</param>
        public static void AreEqual(Document expected, string actual)
        {
            if (expected is null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual is null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            AreEqual(expected.GetCode(), actual, null);
        }

        /// <summary>
        /// Verify that two strings of code are equal. Agnostic to end of line characters.
        /// </summary>
        /// <param name="expected">The expected code.</param>
        /// <param name="actual">The actual code.</param>
        /// <param name="messageHeader">The first line to add to the exception message on error.</param>
        internal static void AreEqual(string expected, string actual, string? messageHeader)
        {
            var expectedPos = 0;
            var actualPos = 0;
            var line = 1;
            while (expectedPos < expected.Length && actualPos < actual.Length)
            {
                var ec = expected[expectedPos];
                var ac = actual[actualPos];
                if (ec == '\r' || ac == '\r')
                {
                    if (ec == '\r')
                    {
                        expectedPos++;
                    }

                    if (ac == '\r')
                    {
                        actualPos++;
                    }

                    continue;
                }

                if (ec != ac)
                {
                    if (IsAt(expected, expectedPos, "\\r") ||
                        IsAt(actual, actualPos, "\\r"))
                    {
                        if (IsAt(expected, expectedPos, "\\r"))
                        {
                            expectedPos += 2;
                        }

                        if (IsAt(actual, actualPos, "\\r"))
                        {
                            actualPos += 2;
                        }

                        continue;
                    }

                    var errorBuilder = StringBuilderPool.Borrow();
                    if (messageHeader != null)
                    {
                        errorBuilder.AppendLine(messageHeader);
                    }

                    if (!IsSingleLine(expected) ||
                        !IsSingleLine(actual))
                    {
                        errorBuilder.AppendLine(
                            CodeReader.TryGetFileName(expected, out var fileName)
                                ? $"Mismatch on line {line} of file {fileName}."
                                : $"Mismatch on line {line}.");
                    }

                    var expectedLine = expected.Split('\n')[line - 1].Trim('\r');
                    var actualLine = actual.Split('\n')[line - 1].Trim('\r');
                    var diffPos = DiffPos(expectedLine, actualLine);

                    errorBuilder.AppendLine($"Expected: {expectedLine}")
                                .AppendLine($"Actual:   {actualLine}")
                                .AppendLine($"          {new string(' ', diffPos)}^");

                    if (!IsSingleLine(expected) ||
                        !IsSingleLine(actual))
                    {
                        errorBuilder.AppendLine("Expected:")
                                    .Append(expected)
                                    .AppendLine()
                                    .AppendLine("Actual:")
                                    .Append(actual)
                                    .AppendLine();
                    }

                    throw new AssertException(errorBuilder.Return());
                }

                if (ec == '\n')
                {
                    line++;
                }

                expectedPos++;
                actualPos++;
            }

            while (expectedPos < expected.Length && expected[expectedPos] == '\r')
            {
                expectedPos++;
            }

            while (actualPos < actual.Length && actual[actualPos] == '\r')
            {
                actualPos++;
            }

            if (expectedPos == expected.Length && actualPos == actual.Length)
            {
                return;
            }

            var messageBuilder = StringBuilderPool.Borrow();
            if (messageHeader != null)
            {
                messageBuilder.AppendLine(messageHeader);
            }

            messageBuilder.AppendLine(CodeReader.TryGetFileName(expected, out var name) ? $"Mismatch at end of file {name}." : "Mismatch at end.")
                          .Append("Expected: ").AppendLine(GetEnd(expected))
                          .Append("Actual:   ").AppendLine(GetEnd(actual))
                          .AppendLine($"          {new string(' ', DiffPos(GetEnd(expected), GetEnd(actual)))}^");

            if (!IsSingleLine(expected) ||
                !IsSingleLine(actual))
            {
                messageBuilder.AppendLine("Expected:")
                              .Append(expected)
                              .AppendLine()
                              .AppendLine("Actual:")
                              .Append(actual)
                              .AppendLine();
            }

            throw new AssertException(messageBuilder.Return());

            static bool IsSingleLine(string text)
            {
                var foundNewLine = false;
                foreach (var c in text)
                {
                    switch (c)
                    {
                        case '\r':
                        case '\n':
                            foundNewLine = true;
                            break;
                        default:
                            if (foundNewLine)
                            {
                                return false;
                            }

                            break;
                    }
                }

                return true;
            }

            static int DiffPos(string expectedLine, string actualLine)
            {
                var diffPos = Math.Min(expectedLine.Length, actualLine.Length);
                for (var i = 0; i < Math.Min(expectedLine.Length, actualLine.Length); i++)
                {
                    if (expectedLine[i] != actualLine[i])
                    {
                        diffPos = i;
                        break;
                    }
                }

                return diffPos;
            }

            static string GetEnd(string text)
            {
                var lastLine = false;
                var builder = StringBuilderPool.Borrow();
                for (var i = text.Length - 1; i >= 0; i--)
                {
                    switch (text[i])
                    {
                        case '\r':
                            if (lastLine)
                            {
                                return builder.Return();
                            }

                            builder.Insert(0, "\\r");
                            break;
                        case '\n':
                            if (lastLine)
                            {
                                return builder.Return();
                            }

                            builder.Insert(0, "\\n");
                            break;
                        default:
                            lastLine = true;
                            builder.Insert(0, text[i]);
                            break;
                    }
                }

                return builder.Return();
            }
        }

        private static bool IsAt(string text, int pos, string toMatch)
        {
            if (text.Length <= pos)
            {
                return false;
            }

            var start = pos - toMatch.Length + 1;
            if (start < 0)
            {
                return false;
            }

            for (var i = toMatch.Length - 1; i >= 0; i--)
            {
                if (text[start + i] != toMatch[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}

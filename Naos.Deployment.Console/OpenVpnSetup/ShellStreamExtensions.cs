﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShellStreamExtensions.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in Naos.Deployment.Recipes.OpenVpnSetup source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Console
{
    using System;
    using Renci.SshNet;

    using static System.FormattableString;

#pragma warning disable CS3001 // Argument type is not CLS-compliant

    /// <summary>
    /// Extension methods on type <see cref="SshClient"/>.
    /// </summary>
#if !NaosDeploymentConsole
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCode("Naos.Deployment.Recipes.OpenVpnSetup", "See package version number")]
#endif
    public static class ShellStreamExtensions
    {
        private static readonly TimeSpan ReadLineTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Reads all lines from the shell and writes to the logger.
        /// </summary>
        /// <param name="shellStream">The shell stream.</param>
        /// <param name="readLineTimeout">Optional time to wait for a line of text from the shell.  DEFAULT is <see cref="ReadLineTimeout"/>.</param>
        /// <param name="logger">Optional logger.  Default is to log nothing.</param>
        public static void ReadAllLines(
            this ShellStream shellStream,
            TimeSpan readLineTimeout = default,
            Action<string> logger = null)
        {
            if (readLineTimeout == default)
            {
                readLineTimeout = ReadLineTimeout;
            }

            string line;

            do
            {
                line = shellStream.ReadLine(readLineTimeout);

                logger?.Invoke(line);
            }
            while (line != null);
        }

        /// <summary>
        /// Reads an expected value from the shell and transmits a response.
        /// </summary>
        /// <param name="shellSteam">The shell stream.</param>
        /// <param name="shellOutputContains">The text read from the shell must contain this string.</param>
        /// <param name="response">The response.</param>
        /// <param name="includeNewlineAfterResponse">Optional value indicating whether to include a newline after response.  DEFAULT is true.</param>
        /// <param name="logger">Optional logger.  Default is to log nothing.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Newline", Justification = "Spelling/name is correct.")]
        public static void ExpectAndRespond(
            this ShellStream shellSteam,
            string shellOutputContains,
            string response,
            bool includeNewlineAfterResponse = true,
            Action<string> logger = null)
        {
            var shellOutput = shellSteam.Read();

            logger?.Invoke(shellOutput);

            if (!shellOutput.Contains(shellOutputContains))
            {
                throw new InvalidOperationException(Invariant($"Expected the shell output to contain '{shellOutputContains}', but it didn't.  Shell output is '{shellOutput}'."));
            }

            if (includeNewlineAfterResponse)
            {
                shellSteam.WriteLine(response);
            }
            else
            {
                shellSteam.Write(response);
            }
        }
    }

#pragma warning restore CS3001 // Argument type is not CLS-compliant
}

﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SshClientExtensions.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in Naos.Deployment.Recipes.OpenVpnSetup source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Console
{
    using System;
    using System.Threading;
    using OBeautifulCode.Assertion.Recipes;

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ssh", Justification = "Spelling/name is correct.")]
    public static class SshClientExtensions
    {
        /// <summary>
        /// Runs the specified command.
        /// </summary>
        /// <param name="client">The SSH client.</param>
        /// <param name="command">The command to run.</param>
        /// <param name="logger">Optional logger.  Default is to log nothing.</param>
        /// <returns>
        /// The result of executing the command.
        /// </returns>
        public static string RunCommandAndThrowOnError(
            this SshClient client,
            string command,
            Action<string> logger = null)
        {
            new { client }.AsArg().Must().NotBeNull();
            new { command }.AsArg().Must().NotBeNullNorWhiteSpace();

            logger?.Invoke(Invariant($"RUNNING COMMAND: {command}"));
            using (var sshCommand = client.RunCommand(command))
            {
                if (!string.IsNullOrWhiteSpace(sshCommand.Result))
                {
                    logger?.Invoke(Invariant($"RESULT: {sshCommand.Result}"));
                }

                if ((sshCommand.ExitStatus != 0) || (!string.IsNullOrWhiteSpace(sshCommand.Error)))
                {
                    var errorMessage = Invariant($"ERROR ({sshCommand.ExitStatus}): {sshCommand.Error}");
                    logger?.Invoke(errorMessage);
                    throw new InvalidOperationException(Invariant($"SSH command failed: {errorMessage}"));
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
                return sshCommand.Result;
            }
        }

        /// <summary>
        /// Determines if a reboot is required.
        /// </summary>
        /// <param name="client">The SSH client.</param>
        /// <param name="logger">Optional logger.  Default is to log nothing.</param>
        /// <returns>
        /// true if a reboot is required; otherwise, false.
        /// </returns>
        // ReSharper disable once UnusedMember.Global
        public static bool IsRebootRequired(
            this SshClient client,
            Action<string> logger = null)
        {
            new { client }.AsArg().Must().NotBeNull();

            var commandOutput = RunCommandAndThrowOnError(client, BashCommands.IsRebootRequired, logger).Trim();
            switch (commandOutput)
            {
                case "yes":
                    return true;
                case "no":
                    return false;
                default:
                    throw new InvalidOperationException(Invariant($"The command to determine if a reboot required had an unexpected result (neither 'yes' nor 'no'): {commandOutput}"));
            }
        }
    }

#pragma warning restore CS3001 // Argument type is not CLS-compliant
}

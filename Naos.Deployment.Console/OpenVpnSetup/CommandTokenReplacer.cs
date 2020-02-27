﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandTokenReplacer.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in Naos.Deployment.Recipes.OpenVpnSetup source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Console
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    using OBeautifulCode.Assertion.Recipes;

    using static System.FormattableString;

    /// <summary>
    /// Replaces tokens in commands.
    /// </summary>
#if !NaosDeploymentConsole
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCode("Naos.Deployment.Recipes.OpenVpnSetup", "See package version number")]
#endif
    public static class CommandTokenReplacer
    {
        private static readonly Regex SubnetRegex = new Regex("^([0-9]{1,3}\\.){3}[0-9]{1,3}(\\/([0-9]|[1-2][0-9]|3[0-2]))$", RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Inserts a token value into a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="token">The token to replace.</param>
        /// <param name="tokenValue">The token value.</param>
        /// <returns>
        /// The original command with the specified token replaced with the specified token value.
        /// </returns>
        public static string Insert(
            this string command,
            string token,
            string tokenValue)
        {
            new { command }.AsArg().Must().NotBeNullNorWhiteSpace();
            new { token }.AsArg().Must().NotBeNullNorWhiteSpace();

            var result = command.Replace(token, tokenValue);

            return result;
        }

        /// <summary>
        /// Inserts a username into a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="username">The username.</param>
        /// <returns>
        /// The original command with the username token replaced with the specified username.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Username")]
        public static string InsertUsername(
            this string command,
            string username)
        {
            new { username }.AsArg().Must().NotBeNullNorWhiteSpace().And().BeAlphanumeric(new[] { '-' });

            var result = command.Insert(CommandTokens.Username, username);

            return result;
        }

        /// <summary>
        /// Inserts a password into a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        /// The original command with the password token replaced with the specified password.
        /// </returns>
        public static string InsertPassword(
            this string command,
            string password)
        {
            new { password }.AsArg().Must().NotBeNullNorWhiteSpace();

            password = password.Replace("'", "\\'");
            var result = command.Insert(CommandTokens.Password, password);

            return result;
        }

        /// <summary>
        /// Inserts a hostname into a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="hostname">The hostname.</param>
        /// <returns>
        /// The original command with the hostname token replaced with the specified hostname.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "hostname", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Hostname", Justification = "Spelling/name is correct.")]
        public static string InsertHostname(
            this string command,
            string hostname)
        {
            Uri.CheckHostName(hostname).AsArg(Invariant($"{nameof(Uri)}.{nameof(Uri.CheckHostName)}({nameof(hostname)})")).Must().NotBeEqualTo(UriHostNameType.Unknown);

            var result = command.Insert(CommandTokens.Hostname, hostname);

            return result;
        }

        /// <summary>
        /// Inserts a subnet into a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="subnet">The subnet.</param>
        /// <returns>
        /// The original command with the subnet token replaced with the specified subnet.
        /// </returns>
        public static string InsertSubnet(
            this string command,
            string subnet)
        {
            new { subnet }.AsArg().Must().NotBeNullNorWhiteSpace();
            SubnetRegex.IsMatch(subnet).AsArg(Invariant($"{nameof(SubnetRegex)}.{nameof(Regex.IsMatch)}")).Must().BeTrue();

            var result = command.Insert(CommandTokens.Subnet, subnet);

            return result;
        }

        /// <summary>
        /// Inserts an 0-based array index into a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="index">The 0-based array index.</param>
        /// <returns>
        /// The original command with the array index token replaced with the specified index.
        /// </returns>
        public static string InsertArrayIndex(
            this string command,
            int index)
        {
            new { index }.AsArg().Must().BeGreaterThanOrEqualTo(0);

            var result = command.Insert(CommandTokens.ArrayIndex, index.ToString(CultureInfo.InvariantCulture));

            return result;
        }

        /// <summary>
        /// Inserts a PEM-encoded cryptographic resource (e.g. a certificate, private key).
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="cryptographicResource">The cryptographic resource.</param>
        /// <returns>
        /// The original command with the cryptographic resource token replaced with the specified cryptographic resource.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pem", Justification = "Spelling/name is correct.")]
        public static string InsertPemEncodedCryptographicResource(
            this string command,
            string cryptographicResource)
        {
            new { cryptographicResource }.AsArg().Must().NotBeNullNorWhiteSpace();

            cryptographicResource = cryptographicResource.Replace("\r\n", "\n");
            var result = command.Insert(CommandTokens.CryptographicResourcePemEncoded, cryptographicResource);

            return result;
        }

        /// <summary>
        /// Inserts an OpenVPN Access Server license key.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="licenseKey">The license key.</param>
        /// <returns>
        /// The original command with the OpenVPN Access Server license key token replaced with the specified license key.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vpn", Justification = "Spelling/name is correct.")]
        public static string InsertOpenVpnAccessServerLicenseKey(
            this string command,
            string licenseKey)
        {
            new { licenseKey }.AsArg().Must().NotBeNullNorWhiteSpace();

            var result = command.Insert(CommandTokens.OpenVpnAccessServerLicenseKey, licenseKey);

            return result;
        }
    }
}

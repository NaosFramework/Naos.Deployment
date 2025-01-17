﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleAbstraction.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    using CLAP;

    using Naos.AWS.Domain;
    using Naos.Bootstrapper;
    using Naos.CodeAnalysis.Recipes;
    using Naos.Configuration.Domain;
    using Naos.Deployment.Core;
    using Naos.Deployment.Core.CertificateManagement;
    using Naos.Deployment.Domain;
    using Naos.Deployment.Persistence;
    using Naos.Deployment.Tracking;
    using Naos.MachineManagement.Local;
    using Naos.Recipes.RunWithRetry;
    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Execution.Recipes;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Security.Recipes;
    using OBeautifulCode.Serialization;
    using OBeautifulCode.Serialization.Json;

    using static System.FormattableString;

    using SysEnvironment = System.Environment;

    /// <summary>
    /// Deployment logic to be invoked from the console harness.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Deployer", Justification = "Spelling/name is correct.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Like it this way.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "Used by CLAP.")]
    public class ConsoleAbstraction : ConsoleAbstractionBase
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<TypeRepresentation> ExceptionTypeRepresentationsToOnlyPrintMessage => new[] { typeof(CredentialsPreRunCheckFailedException).ToRepresentation() };

        /// <inheritdoc />
        protected override bool RequiresElevatedPrivileges => true;

        /// <inheritdoc />
        protected override void CustomPerformEntryPointPreChecks()
        {
            // ENSURE NETWORK IS PRIVATE: Check that the active network is private as this will be required for any WinRM commands.
            using (var localMachineManager = new LocalMachineManager())
            {
                var connectionProfiles = localMachineManager.RunScript("{ Get-NetConnectionProfile }");
                foreach (var connectionProfile in connectionProfiles)
                {
                    var networkCategoryInt = connectionProfile.NetworkCategory;
                    if (networkCategoryInt == 0)
                    {
                        throw new InvalidOperationException(
                            Invariant(
                                $"Network (Name: '{connectionProfile.Name}', InterfaceAlias: '{connectionProfile.InterfaceAlias}', InterfaceIndex: '{connectionProfile.InterfaceIndex}') has a network category of 'Public'.{Environment.NewLine}    WinRM requires connections to be non-public in order to run remote commands on servers.{Environment.NewLine}    In order to deploy new machines you must either ONLY be connected to non-public networks. {Environment.NewLine}{Environment.NewLine}        OR {Environment.NewLine}{Environment.NewLine}    Change the category of the network; example PowerShell command: {Environment.NewLine}        Set-NetConnectionProfile -InterfaceIndex {connectionProfile.InterfaceIndex} -NetworkCategory 'Private' {Environment.NewLine}"));
                    }
                }
            }
        }

        // Replace 'Naos' with yours here.
        private const string CredentialsEnvironmentVaraibleName = "NaosDeploymentCredentialsJson";

        // Replace 'Naos' with yours here.
        private const string DefaultWorkingDirectory = "C:\\DeploymentTemp-Naos";

        // Replace 'Naos' with yours here.
        private const string DefaultTempArcologyDirectory = "C:\\ArcologyTemp-Naos";

        // Replace 'Naos' with yours here.
        private const string PackagePrefixToStrip = "Naos";

        // Replace 'Naos' with yours here.
        private const string DnsSuffix = "naosproject.com";

        private static readonly ObcJsonSerializer DefaultJsonSerializer = new ObcJsonSerializer(
            typeof(NaosDeploymentCoreJsonSerializationConfiguration).ToJsonSerializationConfigurationType());

        /// <summary>
        /// Gets new credentials on the computing platform provider, optionally saves to an environment variable.
        /// </summary>
        /// <param name="userName">Username of the credentials.</param>
        /// <param name="password">Password of the credentials.</param>
        /// <param name="mfaUserName">Username matching the MFA device to use when authenticating.</param>
        /// <param name="mfaValue">Token from the MFA device to use when authenticating.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Optional environment name that will set the <see cref="Naos.Configuration" /> precedence instead of the default which is reading the App.Config value.</param>
        /// <param name="setEnvironmentVariable">A value indicating whether or not to store the result in the user environment variable: NaosDeploymentCredentialsJson.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "mfa", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mfa", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Justification = "Not sure why it's complaining...")]
        [Verb(Aliases = "credentials", Description = "Gets new credentials on the computing platform provider, optionally saves to an environment variable.")]
        public static void GetNewCredentialJson(
            [Aliases("")] [Required] [Description("Username of the credentials.")] string userName,
            [Aliases("")] [Required] [Description("Password of the credentials.")] string password,
            [Aliases("")] [Required] [Description("Username matching the MFA device to use when authenticating.")] string mfaUserName,
            [Aliases("")] [Required] [Description("Token from the MFA device to use when authenticating.")] string mfaValue,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("")] [Required] [Description("A value indicating whether or not to store the result in the user environment variable: NaosDeploymentCredentialsJson.")] bool setEnvironmentVariable)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var location = Computing.Details[environment.ToLowerInvariant()].LocationName;
            var awsAccountId = GetAwsAccountIdFromEnvironment(environment);
            var oneDayTokenLifespan = TimeSpan.FromDays(1);
            var virtualMfaDeviceId = Invariant($"arn:aws:iam::{awsAccountId}:mfa/{mfaUserName}");

            void HandleResult(string result)
            {
                if (setEnvironmentVariable)
                {
                    SysEnvironment.SetEnvironmentVariable(CredentialsEnvironmentVaraibleName, result, EnvironmentVariableTarget.User);
                }

                Console.WriteLine(result);
            }

            NaosDeploymentBootstrapper.GetNewCredentialsJson(
                location,
                oneDayTokenLifespan,
                userName,
                password,
                virtualMfaDeviceId,
                mfaValue,
                false,
                HandleResult);
        }

        /// <summary>
        /// Creates a new environment.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="environmentCertificatePassword">Password of the environment certificate file.</param>
        /// <param name="deploymentCertificatePassword">Password of the deployment certificate file.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "create", Description = "Creates a new environment.")]
        public static void CreateEnvironment(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Required] [Aliases("envPass")] [Description("Password of the environment certificate file.")] string environmentCertificatePassword,
            [Required] [Aliases("depPass")] [Description("Password of the deployment certificate file.")] string deploymentCertificatePassword,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var configFilePath = Path.Combine(DefaultTempArcologyDirectory, Invariant($"{environment}-Configuration.xml"));
            if (File.Exists(configFilePath))
            {
                var backupConfigFilePath = Path.ChangeExtension(
                    configFilePath,
                    Invariant($".BackedUpOn{DateTime.UtcNow.ToString("yyyy-MM-dd--HH-mm-ss", CultureInfo.InvariantCulture)}Z.xml"));
                if (File.Exists(backupConfigFilePath))
                {
                    throw new ArgumentException(Invariant($"Unexpected file present on disk: {backupConfigFilePath}"));
                }

                File.Move(configFilePath, backupConfigFilePath);
            }

            var xmlSerializer = new XmlSerializer(typeof(ConfigEnvironment));
            var configObject = EnvironmentSpec.BuildEnvironmentSpec(environment);
            using (var configFileWriter = new StreamWriter(configFilePath))
            {
                xmlSerializer.Serialize(configFileWriter, configObject);
            }

            var environmentCertificateFilePath = Path.Combine(DefaultTempArcologyDirectory, Invariant($"{environment}.Environment.pfx"));
            var deploymentCertificateFilePath = Path.Combine(DefaultTempArcologyDirectory, Invariant($"{environment}.Deployment.pfx"));

            var rootDomainHostingIdMap = new Dictionary<string, string>
                                             {
                                                 { DnsSuffix, "AwsHostingId" },
                                             };
            var rootDomainHostingIdMapJson = DefaultJsonSerializer.SerializeToString(rootDomainHostingIdMap);

            var windowsSkuSearchPatternMap = new Dictionary<string, string>
                           {
                               { "doesNotMatter", "Windows_Server-2019-English-Full-Base-*" },
                               { "base", "Windows_Server-2019-English-Full-Base-*" },
                               { "sqlWeb", "Windows_Server-2019-English-Full-SQL_2019_Web-*" },
                               { "sqlStandard", "Windows_Server-2019-English-Full-SQL_2019_Standard-*" },
                               { "sqlEnterprise", "Windows_Server-2019-English-Full-SQL_2019_Enterprise-*" },
                           };
            var windowsSkuSearchPatternMapJson = DefaultJsonSerializer.SerializeToString(windowsSkuSearchPatternMap);

            var locationAbbreviation = Computing.Details[environment.ToLowerInvariant()].LocationAbbreviation;
            var computingPlatformKeyFilePath = Path.Combine(DefaultTempArcologyDirectory, Invariant($"{environment}--{locationAbbreviation.ToUpperInvariant()}.pem"));

            var credsJson = string.IsNullOrEmpty(credentialsJson) ? System.Environment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.CreateEnvironment(
                credsJson,
                configFilePath,
                DefaultTempArcologyDirectory,
                computingPlatformKeyFilePath,
                environmentCertificateFilePath,
                environmentCertificatePassword,
                deploymentCertificateFilePath,
                deploymentCertificatePassword,
                windowsSkuSearchPatternMapJson,
                rootDomainHostingIdMapJson,
                environment);
        }

        /// <summary>
        /// Destroy an existing environment.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [Verb(Aliases = "destroy", Description = "Destroy an existing environment.")]
        public static void DestroyEnvironment(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var environmentName = environment.ToLowerInvariant();
            var configFilePath = Path.Combine(DefaultTempArcologyDirectory, Invariant($"{environmentName}-Configuration.Created.xml"));

            var credsJson = string.IsNullOrEmpty(credentialsJson) ? System.Environment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.DestroyEnvironment(
                credsJson,
                configFilePath,
                environmentName);
        }

        /// <summary>
        /// Deploys a new instance of the VPN server.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        /// <param name="workingPath">Optionally sets the working directory; DEFAULT is <see cref="DefaultWorkingDirectory" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vpn", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Like it this way.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "credentialsJson", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "workingPath", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "nuget", Justification = "Not sure why it's complaining...")]
        [Verb(Aliases = "deployvpn", Description = "Deploys a new instance of the VPN server.")]
        public static void DeployVpnServer(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType,
            [Aliases("")] [Description("Optionally sets the working directory; DEFAULT is DefaultWorkingDirectory.")] [DefaultValue(DefaultWorkingDirectory)] string workingPath)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var deployment = ConsolidatedDeploymentFactory.BuildVpnServerDeployment(environment, DnsSuffix);
            var packagesJson = DefaultJsonSerializer.SerializeToString(deployment.Packages);
            var overridesJson = DefaultJsonSerializer.SerializeToString(deployment.DeploymentConfigurationOverride);
            DeployAdvanced(
                credentialsJson,
                deployment.Name,
                packagesJson,
                overridesJson,
                ExistingDeploymentStrategy.Replace,
                debug,
                environment,
                environmentType,
                true,
                workingPath,
                false);
        }

        /// <summary>
        /// Configure a new VPN server.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="vpnAdminPassword">Admin password for VPN.</param>
        /// <param name="license">Optional license to apply.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "System.String.IndexOf(System.String,System.StringComparison)", Justification = "String check is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId     = "vpn",
            Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Globalization",
            "CA1308:NormalizeStringsToUppercase",
            Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId     = "Vpn",
            Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Maintainability",
            "CA1506:AvoidExcessiveClassCoupling",
            Justification = "Like it this way.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Naming",
            "CA2204:Literals should be spelled correctly",
            MessageId     = "credentialsJson",
            Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Naming",
            "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId     = "nuget",
            Justification = "Not sure why it's complaining...")]
        [Verb(Aliases = "configvpn", Description = "Configure a new VPN server.")]
        public static void ConfigureVpnServer(
            [Aliases("")]
            [Description(
                "Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")]
            [DefaultValue(null)]
            string credentialsJson,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)]
            bool debug,
            [Aliases("pass")] [Required] [Description("Admin password for VPN.")]
            string vpnAdminPassword,
            [Aliases("")] [DefaultValue(null)] [Description("License to apply.")]
            string license,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")]
            string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)]
            EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);
            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType, true);

            string privateKey = null;

            void PrivateKeyResultRecorder(
                string s) => privateKey = s;

            var credsJson = string.IsNullOrEmpty(credentialsJson)
                ? System.Environment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User)
                : credentialsJson;
            NaosDeploymentBootstrapper.GetComputingContainerPrivateKey(
                credsJson,
                infrastructureTrackerJson,
                InstanceAccessibility.Tunnel,
                environment,
                environmentType,
                PrivateKeyResultRecorder);

            privateKey.MustForOp(nameof(privateKey)).NotBeNullNorWhiteSpace();

            var instanceName = ConsolidatedDeploymentFactory.BuildVpnServerDeployment(environment, DnsSuffix).Name;

            string instanceDescriptionJson = null;

            void InstanceDescriptionJsonResultRecorder(
                string s) => instanceDescriptionJson = s;

            NaosDeploymentBootstrapper.GetInstanceDetails(
                credsJson,
                infrastructureTrackerJson,
                instanceName,
                environment,
                environmentType,
                InstanceDescriptionJsonResultRecorder);
            instanceDescriptionJson.MustForOp(nameof(instanceDescriptionJson)).NotBeNullNorWhiteSpace();
            var instanceDescription = DefaultJsonSerializer.Deserialize<InstanceDescription>(instanceDescriptionJson);

            string consoleOutput = null;

            void ConsoleOutputRecorder(
                string s) => consoleOutput = s;

            NaosDeploymentBootstrapper.GetConsoleOutput(
                credsJson,
                infrastructureTrackerJson,
                instanceName,
                environment,
                environmentType,
                ConsoleOutputRecorder);
            consoleOutput.MustForOp(nameof(consoleOutput)).NotBeNullNorWhiteSpace();
            var beginSshHostKeyFingerprintsMarker = "-----BEGIN SSH HOST KEY FINGERPRINTS-----";
            var endSshHostKeyFingerprintsMarker   = "-----END SSH HOST KEY FINGERPRINTS-----";
            var indexOfSshFingerprintsStart       = consoleOutput.IndexOf(beginSshHostKeyFingerprintsMarker, StringComparison.InvariantCulture);
            var indexOfSshFingerprintsEnd = consoleOutput.IndexOf(endSshHostKeyFingerprintsMarker, StringComparison.InvariantCulture)
                                          + endSshHostKeyFingerprintsMarker.Length;
            var sshFingerprintsBlock              = consoleOutput.Substring(indexOfSshFingerprintsStart, indexOfSshFingerprintsEnd - indexOfSshFingerprintsStart);
            var beginRsaMarker                    = "2048 SHA256:";
            var indexOfRsaStartBlock              = sshFingerprintsBlock.IndexOf(beginRsaMarker, StringComparison.InvariantCulture) + beginRsaMarker.Length;
            var rsaStartBlock                     = sshFingerprintsBlock.Substring(indexOfRsaStartBlock);
            var endRsaMarker                      = " root@";
            var indexOfRsaEnd                     = rsaStartBlock.IndexOf(endRsaMarker, StringComparison.InvariantCulture);
            var rsaKey                            = rsaStartBlock.Substring(0, indexOfRsaEnd);
            rsaKey.MustForOp(nameof(rsaKey)).NotBeNullNorWhiteSpace();

            var secondCidrBlockValue = Computing.Details[environment.ToLowerInvariant()].SecondCidrComponent;
            var connectionSettings = new SshConnectionSettings
                                     {
                                         Username                 = "openvpnas",
                                         UserPemEncodedPrivateKey = privateKey,
                                         ServerAddress            = instanceDescription.PublicIpAddress,
                                         ServerPublicKeyAlgorithmToBase64Sha256ThumbprintMap = new Dictionary<HostKeyAlgorithm, string>
                                                                                               {
                                                                                                   { HostKeyAlgorithm.Rsa, rsaKey },
                                                                                               },
                                     };

            var certificateRetrieverJson = GetCertificateRetrieverJsonFromEnvironment(environment, true);
            var certificateRetrieverConfiguration =
                DefaultJsonSerializer.Deserialize<CertificateManagementConfigurationBase>(certificateRetrieverJson);
            var certificateRetriever = CertificateManagementFactory.CreateReader(certificateRetrieverConfiguration);
            Func<Task<IReadOnlyCollection<string>>> allCertificateNamesAsyncFunc = () => certificateRetriever.GetAllCertificateNamesAsync();
            var certificateNames = allCertificateNamesAsyncFunc.ExecuteSynchronously();
            var certificateName =
                certificateNames.SingleOrDefault(_ => _.ToUpperInvariant() == Invariant($"vpn.{environment}.{DnsSuffix}").ToUpperInvariant());

            Func<Task<CertificateDescriptionWithClearPfxPayload>> certificateByNameAsyncFunc =
                () => certificateRetriever.GetCertificateByNameAsync(certificateName);
            var certificateDescription = !string.IsNullOrWhiteSpace(certificateName)
                ? certificateByNameAsyncFunc.ExecuteSynchronously()
                : null;
            var certificate = certificateDescription != null
                ? CertHelper.ExtractCryptographicObjectsFromPfxFile(certificateDescription.PfxBytes, certificateDescription.PfxPasswordInClearText)
                : null;

            var openVpnAccessServerSettings = new OpenVpnAccessServerSettings
                                              {
                                                  AdminUsername = "admin-openvpn",
                                                  AdminPassword = vpnAdminPassword,
                                                  Hostname = Invariant($"vpn.{environment.ToLowerInvariant()}.{DnsSuffix}"),
                                                  PrivateSubnetsClientsCanAccess = new[]
                                                                                   {
                                                                                       Invariant($"10.{secondCidrBlockValue}.0.0/16"),
                                                                                   },
                                                  LicenseKey =
                                                      license, // "XXX-LICENSE-KEY [Optional as server can run 2 connections without a license]",
                                                  WebserverCaBundlePemEncoded =
                                                      certificate?.CertificateChain.GetIntermediateChainFromCertChain()
                                                                  .AsPemEncodedString(), // "<SSL Cert Intermediary chain> [Optional as server will fall back on OpenVPN cert but browser will have a warning]",
                                                  WebserverCertificate =
                                                      certificate?.CertificateChain.GetEndUserCertFromCertChain()
                                                                  .AsPemEncodedString(), // @"<SSL Cert> [Optional as server will fall back on OpenVPN cert but browser will have a warning]",
                                                  WebserverPrivateKeyPemEncoded =
                                                      certificate
                                                        ?.PrivateKey
                                                         .AsPemEncodedString(), // @"<SSL Cert Private Key> [Optional as server will fall back on OpenVPN cert but browser will have a warning]",
                                              };

            OpenVpnAccessServerSetupExecutor.SetupVpnServer(connectionSettings, openVpnAccessServerSettings, logger: Console.WriteLine);
        }

        /// <summary>
        /// Export a certificate from local store to pfx file.
        /// </summary>
        /// <param name="thumbprint">Thumbprint of certificate.</param>
        /// <param name="pfxPassword">Password for pfx file.</param>
        /// <param name="exportFilePath">File path to write pfx file to.</param>
        [Verb(Aliases = "exportCert", Description = "Export a certificate from local store to pfx file.")]
        public static void ExportCertificateFromStore(
            [Aliases("")] [Description("Thumbprint of certificate.")] [Required]
            string thumbprint,
            [Aliases("pass")] [Description("Password for pfx file.")] [Required]
            string pfxPassword,
            [Aliases("path")] [Description("File path to write pfx file to.")] [Required]
            string exportFilePath)
        {
            StoreLocation.LocalMachine.ExportPfxFromCertificateStoreToFile(StoreName.My, thumbprint, pfxPassword, exportFilePath, false, true);
        }

        /// <summary>
        /// Deploys a new instance of the Arcology server.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        /// <param name="workingPath">Optionally sets the working directory; DEFAULT is <see cref="DefaultWorkingDirectory" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Arcology", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Like it this way.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "credentialsJson", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "workingPath", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "nuget", Justification = "Not sure why it's complaining...")]
        [Verb(Aliases = "deployarcology", Description = "Deploys a new instance of the Arcology server.")]
        public static void DeployArcologyServer(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType,
            [Aliases("")] [Description("Optionally sets the working directory; DEFAULT is DefaultWorkingDirectory.")] [DefaultValue(DefaultWorkingDirectory)] string workingPath)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var configFileManager = new ConfigFileManager(new[] { Config.CommonPrecedence }, Config.DefaultConfigDirectoryName, DefaultJsonSerializer);

            var deploymentDatabase = Config.Get<DeploymentDatabase>(NaosDeploymentCoreJsonSerializationConfiguration.NaosDeploymentCoreJsonSerializerRepresentation);

            var deployment = ConsolidatedDeploymentFactory.BuildArcologyServerDeployment(deploymentDatabase.ConnectionSettings.Credentials.Password.ToInsecureString(), environment, DnsSuffix);
            var packagesJson = configFileManager.SerializeConfigToFileText(deployment.Packages);
            var overridesJson = configFileManager.SerializeConfigToFileText(deployment.DeploymentConfigurationOverride);
            DeployAdvanced(
                credentialsJson,
                deployment.Name,
                packagesJson,
                overridesJson,
                ExistingDeploymentStrategy.NotPossibleToReplaceOrDuplicate,
                debug,
                environment,
                environmentType,
                true,
                workingPath,
                false);
        }

        /// <summary>
        /// Migrate one arcology to another (used to move from temp files during environment creation to database after deployment of it).
        /// </summary>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="arcologyFilePath">Arcology directory root path on disk.</param>
        /// <param name="direction">Direction to migrate.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "arcology", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Arcology", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "migrate", Description = "Migrate one arcology to another (used to move from temp files during environment creation to database after deployment of it).")]
        public static void MigrateArcology(
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("arcPath")] [DefaultValue(DefaultTempArcologyDirectory)] [Description("Arcology directory root path on disk.")] string arcologyFilePath,
            [Aliases("dir")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] ArcologyMigrationDirection direction,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            new { direction }.AsArg().Must().NotBeEqualTo(ArcologyMigrationDirection.Unknown);

            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            CertificateLocator encryptingCertificateLocator;
            var databaseArcologyConfigurationJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var databaseArcologyConfiguration = (InfrastructureTrackerConfigurationBase)DefaultJsonSerializer.Deserialize(databaseArcologyConfigurationJson, typeof(InfrastructureTrackerConfigurationBase));
            using (var databaseTracker = InfrastructureTrackerFactory.Create(databaseArcologyConfiguration))
            {
                switch (direction)
                {
                    case ArcologyMigrationDirection.DirectoryToDatabase:
                        Arcology directoryArcology;
                        using (var fileArcology = new RootFolderEnvironmentFolderInstanceFileTracker(arcologyFilePath))
                        {
                            directoryArcology = fileArcology.GetArcologyByEnvironmentName(environment.ToLowerInvariant());
                        }

                        if (directoryArcology.ComputingContainers.Select(_ => _.EncryptingCertificateLocator.CertificateThumbprint).Distinct().Count() != 1)
                        {
                            throw new ArgumentException("Different encrypting certificates for different containers is not supported.");
                        }

                        encryptingCertificateLocator = directoryArcology.ComputingContainers.First().EncryptingCertificateLocator;

                        Func<Task> createDatabaseArcologyFunc = () => databaseTracker.Create(
                                                            environment.ToLowerInvariant(),
                                                            directoryArcology,
                                                            directoryArcology.Instances);

                        createDatabaseArcologyFunc.ExecuteSynchronously();
                        break;
                    case ArcologyMigrationDirection.DatabaseToDirectory:

                        if (!Directory.Exists(arcologyFilePath))
                        {
                            Directory.CreateDirectory(arcologyFilePath);
                        }

                        var mongoTracker = (MongoInfrastructureTracker)databaseTracker;
                        Func<Task<Arcology>> arcologyByEnvironmentNameAsyncFunc =
                            () => mongoTracker.GetArcologyByEnvironmentNameAsync(environment.ToLowerInvariant());

                        var databaseArcology = arcologyByEnvironmentNameAsyncFunc.ExecuteSynchronously();
                        using (var fileArcology = new RootFolderEnvironmentFolderInstanceFileTracker(arcologyFilePath))
                        {
                            encryptingCertificateLocator = databaseArcology.ComputingContainers.First().EncryptingCertificateLocator;
                            var databaseArcologyInfo = new ArcologyInfo
                            {
                                Location = databaseArcology.Location,
                                SerializedEnvironmentSpecification = databaseArcology.SerializedEnvironmentSpecification,
                                ComputingContainers = databaseArcology.ComputingContainers,
                                RootDomainHostingIdMap = databaseArcology.RootDomainHostingIdMap,
                                WindowsSkuSearchPatternMap = databaseArcology.WindowsSkuSearchPatternMap,
                            };

                            Func<Task> createFileArcologyFunc = () => fileArcology.Create(
                                                                    environment.ToLowerInvariant(),
                                                                    databaseArcologyInfo,
                                                                    databaseArcology.Instances);

                            createFileArcologyFunc.ExecuteSynchronously();
                        }

                        break;
                    default:
                        throw new NotSupportedException(Invariant($"{nameof(ArcologyMigrationDirection)}: {direction} is not supported."));
                }
            }

            // CERTIFICATES
            var certificatesJsonFilePath = Path.Combine(arcologyFilePath, Invariant($"{environment.ToLowerInvariant()}.Certificates.json"));
            var certificateManagerConfigForDatabaseJson = GetCertificateManagerJsonFromEnvironment(environment);
            var certificateManagerConfigForDatabase = (CertificateManagementConfigurationBase)DefaultJsonSerializer.Deserialize(certificateManagerConfigForDatabaseJson, typeof(CertificateManagementConfigurationBase));
            IGetCertificates certificateReader;
            IPersistCertificates certificateWriter;
            switch (direction)
            {
                case ArcologyMigrationDirection.DirectoryToDatabase:
                    certificateReader = new CertificateRetrieverFromFile(certificatesJsonFilePath);
                    certificateWriter = CertificateManagementFactory.CreateWriter(certificateManagerConfigForDatabase);
                    break;
                case ArcologyMigrationDirection.DatabaseToDirectory:
                    certificateReader = CertificateManagementFactory.CreateReader(certificateManagerConfigForDatabase);
                    CertificateWriterToFile.Create(certificatesJsonFilePath);
                    certificateWriter = new CertificateWriterToFile(certificatesJsonFilePath);
                    break;
                default:
                    throw new NotSupportedException(Invariant($"{nameof(ArcologyMigrationDirection)}: {direction} is not supported."));
            }

            Func<Task<IReadOnlyCollection<string>>> allCertificateNamesAsyncFunc = () => certificateReader.GetAllCertificateNamesAsync();
            var allCertificateNames = allCertificateNamesAsyncFunc.ExecuteSynchronously();
            foreach (var certificateName in allCertificateNames)
            {
                Func<Task<CertificateDescriptionWithClearPfxPayload>> certificateByNameAsyncFunc =
                    () => certificateReader.GetCertificateByNameAsync(certificateName);
                var certificate = certificateByNameAsyncFunc.ExecuteSynchronously();
                Func<Task> persistCertificateAsyncFunc =
                    () => certificateWriter.PersistCertificateAsync(certificate.ToEncryptedVersion(encryptingCertificateLocator));
                persistCertificateAsyncFunc.ExecuteSynchronously();
            }
        }

        /// <summary>
        /// Gets the password of an instance from the provided tracker.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="instanceName">Name of the computer to get password for (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "password", Description = "Gets the password of an instance from the provided tracker.")]
        public static void GetPassword(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("name")] [Required] [Description("Name of the computer to get password for (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').")] string instanceName,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.GetPassword(credsJson, infrastructureTrackerJson, instanceName, environment.ToLowerInvariant(), environmentType, Console.WriteLine);
        }

        /// <summary>
        /// Gets the private key of the computing container with the specified accessibility.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="accessibility">Accessibility of the computing container to get the private key for.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "privatekey", Description = "Gets the private key of the computing container with the specified accessibility.")]
        public static void GetComputingContainerPrivateKey(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("acc")] [Required] [Description("accessibility of the computing container to get the private key for.")] InstanceAccessibility accessibility,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.GetComputingContainerPrivateKey(credsJson, infrastructureTrackerJson, accessibility, environment.ToLowerInvariant(), environmentType, Console.WriteLine);
        }

        /// <summary>
        /// Gets the status of the instance found by name in provided tracker.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="instanceName">Name of the computer to get password for (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "status", Description = "Gets the status of the instance found by name in provided tracker.")]
        public static void GetInstanceStatus(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("name")] [Required] [Description("Name of the instance to start (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').")] string instanceName,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.GetInstanceStatus(credsJson, infrastructureTrackerJson, instanceName, environment.ToLowerInvariant(), environmentType, Console.WriteLine);
        }

        /// <summary>
        /// Gets the instances that are active (not terminated) from the underlying computing provider.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "query", Description = "Gets the instances that are active (not terminated) from the underlying computing provider.")]
        public static void GetActiveInstancesFromProvider(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.GetActiveInstancesFromProvider(credsJson, infrastructureTrackerJson, environment.ToLowerInvariant(), environmentType, Console.WriteLine);
        }

        /// <summary>
        /// Gets the instance names (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a') in provided tracker.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "list", Description = "Gets the instance names (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a') in provided tracker.")]
        public static void GetInstanceNames(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.GetInstanceNames(credsJson, infrastructureTrackerJson, environment.ToLowerInvariant(), environmentType, Console.WriteLine);
        }

        /// <summary>
        /// Gets the instances only in either tracking or computer platform.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "diff", Description = "Gets the instances only in either tracking or computer platform.")]
        public static void GetInstancesInTrackingAndNotProviderOrReverse(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")]
            [DefaultValue(null)] string credentialsJson,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.GetInstancesInTrackingAndNotProviderOrReverse(credsJson, infrastructureTrackerJson, environment.ToLowerInvariant(), environmentType, Console.WriteLine);
        }

        /// <summary>
        /// Removes an instance from tracking.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="instanceName">Name of instance to remove.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "retire", Description = "Gets the instances only in either tracking or computer platform.")]
        public static void RemoveTrackedInstance(
            [DefaultValue(null)] string credentialsJson,
            [Aliases("name")] [Required] [Description("Name of instance to remove.")] string instanceName,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.RemoveTrackedInstance(credsJson, infrastructureTrackerJson, instanceName, environment.ToLowerInvariant(), environmentType, Console.WriteLine);
        }

        /// <summary>
        /// Removes an instance from tracking that is not in the computing platform.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="privateIpAddressOfInstanceToRemove">IP Address of instance to remove (cannot be used with <paramref name="instanceNameOfInstanceToRemove" />).</param>
        /// <param name="instanceNameOfInstanceToRemove">Name of instance to remove (cannot be used with <paramref name="privateIpAddressOfInstanceToRemove" />).</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ip", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ip", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "purge", Description = "Removes an instance from tracking that is not in the computing platform.")]
        public static void RemoveTrackedInstanceNotInComputingPlatform(
            [DefaultValue(null)] string credentialsJson,
            [Aliases("ip")] [Description("IP Address of instance to remove.")] string privateIpAddressOfInstanceToRemove,
            [Aliases("name")] [Description("Name of instance to remove.")] string instanceNameOfInstanceToRemove,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.RemoveTrackedInstanceNotInComputingPlatform(credsJson, infrastructureTrackerJson, privateIpAddressOfInstanceToRemove, instanceNameOfInstanceToRemove, environment.ToLowerInvariant(), environmentType, Console.WriteLine);
        }

        /// <summary>
        /// Removes an instance from the computing platform that is not in tracking.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="systemIdOfInstanceToRemove">ID of instance to remove (ID from the computing platform).</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "kill", Description = "Removes an instance from the computing platform that is not in tracking.")]
        public static void RemoveInstanceInComputingPlatformNotTracked(
            [DefaultValue(null)] string credentialsJson,
            [Aliases("id")] [Required] [Description("ID of instance to remove (ID from the computing platform).")] string systemIdOfInstanceToRemove,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.RemoveInstanceInComputingPlatformNotTracked(credsJson, infrastructureTrackerJson, systemIdOfInstanceToRemove, environment.ToLowerInvariant(), environmentType, Console.WriteLine);
        }

        /// <summary>
        /// Gets the details of the instance found by name in provided tracker.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="instanceName">Name of the computer to get password for (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "details", Description = "Gets the details of the instance found by name in provided tracker.")]
        public static void GetInstanceDetails(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("name")] [Required] [Description("Name of the instance to start (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').")] string instanceName,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.GetInstanceDetails(credsJson, infrastructureTrackerJson, instanceName, environment.ToLowerInvariant(), environmentType, Console.WriteLine);
        }

        /// <summary>
        /// Starts a remote session instance found by name in provided tracker.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="instanceName">Name of the computer to get password for (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').</param>
        /// <param name="shouldConnectInFullScreen">A value indicating whether or not to connect in full screen mode.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "connect", Description = "Starts a remote session instance found by name in provided tracker.")]
        public static void ConnectToInstance(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("name")] [Required] [Description("Name of the instance to start (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').")] string instanceName,
            [Aliases("fullscreen")] [Description("Connect in fullscreen mode.")] [DefaultValue(true)] bool shouldConnectInFullScreen,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.ConnectToInstance(credsJson, infrastructureTrackerJson, instanceName, environment.ToLowerInvariant(), environmentType, shouldConnectInFullScreen);
        }

        /// <summary>
        /// Starts the instance found by name in provided tracker.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="instanceName">Name of the computer to get password for (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "start", Description = "Starts the instance found by name in provided tracker.")]
        public static void StartInstance(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("name")] [Required] [Description("Name of the instance to start (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').")] string instanceName,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.StartInstance(credsJson, infrastructureTrackerJson, instanceName, environment.ToLowerInvariant(), environmentType);
        }

        /// <summary>
        /// Stops the instance found by name in provided tracker.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="instanceName">Name of the computer to get password for (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').</param>
        /// <param name="force">Force the shutdown.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "stop", Description = "Stops the instance found by name in provided tracker.")]
        public static void StopInstance(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("name")] [Required] [Description("Name of the instance to start (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').")] string instanceName,
            [Aliases("")] [Description("Force the shutdown.")] [DefaultValue(false)] bool force,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.StopInstance(credsJson, infrastructureTrackerJson, instanceName, force, environment.ToLowerInvariant(), environmentType);
        }

        /// <summary>
        /// Starts the instance found by name in provided tracker.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="instanceName">Name of the computer to get password for (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').</param>
        /// <param name="force">Force the shutdown.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is fine.")]
        [Verb(Aliases = "bounce", Description = "Stops then starts the instance found by name in provided tracker.")]
        public static void StopThenStartInstance(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("name")] [Required] [Description("Name of the instance to start (short name - i.e. 'Database' NOT 'instance-Development-Database@us-west-1a').")] string instanceName,
            [Aliases("")] [Description("Force the shutdown.")] [DefaultValue(false)] bool force,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType);
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;
            NaosDeploymentBootstrapper.StopThenStartInstance(credsJson, infrastructureTrackerJson, instanceName, force, environment.ToLowerInvariant(), environmentType);
        }

        /// <summary>
        /// Deploys a new instance with specified packages.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="instanceName">Name of the instance.</param>
        /// <param name="instanceType">Optionally set a system specific type of the instance.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        /// <param name="workingPath">Optionally sets the working directory; DEFAULT is <see cref="DefaultWorkingDirectory" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Adhoc", Justification = "Spelling/name is correct.")]
        [Verb(Aliases = "deployadhoc", Description = "Deploys a new instance with specified name and type.")]
        public static void DeployAdhoc(
            [Aliases("")][Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")][DefaultValue(null)] string credentialsJson,
            [Aliases("name")] [Required] [Description("Name of the instance.")] string instanceName,
            [Aliases("type")] [Description("Optionally set a system specific type of the instance.")] string instanceType,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType,
            [Aliases("")] [Description("Optionally sets the working directory; DEFAULT is DefaultWorkingDirectory.")] [DefaultValue(DefaultWorkingDirectory)]
            string workingPath)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var configFileManager = new ConfigFileManager(new[] { Config.CommonPrecedence }, Config.DefaultConfigDirectoryName, DefaultJsonSerializer);

            var deployment = ConsolidatedDeploymentFactory.BuildAdhocDeployment(instanceName, instanceType);
            var packagesJson = configFileManager.SerializeConfigToFileText(deployment.Packages);
            var overridesJson = configFileManager.SerializeConfigToFileText(deployment.DeploymentConfigurationOverride);
            DeployAdvanced(
                credentialsJson,
                instanceName,
                packagesJson,
                overridesJson,
                ExistingDeploymentStrategy.DeploySideBySide,
                debug,
                environment,
                environmentType,
                false,
                workingPath,
                false);
        }

        /// <summary>
        /// Deploys a new mongo instance.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="name">Name of the server.</param>
        /// <param name="database">Name of the database.</param>
        /// <param name="adminPassword">Admin password.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        /// <param name="workingPath">Optionally sets the working directory; DEFAULT is <see cref="DefaultWorkingDirectory" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Adhoc", Justification = "Spelling/name is correct.")]
        [Verb(Aliases = "deployadhocmongo", Description = "Deploys a new instance of the Arcology server.")]
        public static void DeployAdhocMongoServer(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("")] [Required] [Description("Name of server.")] string name,
            [Aliases("")] [Required] [Description("Name of database.")] string database,
            [Aliases("")] [Required] [Description("Admin password.")] string adminPassword,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType,
            [Aliases("")] [Description("Optionally sets the working directory; DEFAULT is DefaultWorkingDirectory.")] [DefaultValue(DefaultWorkingDirectory)] string workingPath)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var configFileManager = new ConfigFileManager(new[] { Config.CommonPrecedence }, Config.DefaultConfigDirectoryName, DefaultJsonSerializer);

            var deployment = ConsolidatedDeploymentFactory.BuildTestMongoServerDeployment(name, database, adminPassword);
            var packagesJson = configFileManager.SerializeConfigToFileText(deployment.Packages);
            var overridesJson = configFileManager.SerializeConfigToFileText(deployment.DeploymentConfigurationOverride);
            DeployAdvanced(
                credentialsJson,
                deployment.Name,
                packagesJson,
                overridesJson,
                ExistingDeploymentStrategy.DeploySideBySide,
                debug,
                environment,
                environmentType,
                false,
                workingPath,
                false);
        }

        /// <summary>
        /// Deploys a new instance with specified packages.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="packageId">Package ID to deploy.</param>
        /// <param name="instanceName">Optional name of instance.</param>
        /// <param name="turnOffAfterDeployment">Optional switch to ensure server OFF after deployment.</param>
        /// <param name="leaveOnAfterDeployment">Optional switch to ensure server ON after deployment.</param>
        /// <param name="instanceSystemType">Optional system specific instance type to use.</param>
        /// <param name="instanceCount">Optional number of instances to deploy.</param>
        /// <param name="existingDeploymentStrategy">Optional strategy for how to handle existing deployments; DEFAULT is Replace.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        /// <param name="workingPath">Optionally sets the working directory; DEFAULT is <see cref="DefaultWorkingDirectory" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "turnOff", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "packagesToDeployJson", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "instanceName", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "overrideDeploymentConfigJson", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "credentialsJson", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "workingPath", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "nuget", Justification = "Not sure why it's complaining...")]
        [Verb(Aliases = "deploy", Description = "Deploys a new instance with specified package.")]
        public static void DeployPackage(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("id")] [Required] [Description("Optional packages descriptions (with overrides) to configure the instance with.")] string packageId,
            [Aliases("name")] [Description("Optional name for instance.")] [DefaultValue(null)] string instanceName,
            [Aliases("off")] [Description("Optional deployment configuration to use as an override in JSON; ESCAPE QUOTES.")] [DefaultValue(null)] bool? turnOffAfterDeployment,
            [Aliases("on")] [Description("Optional deployment configuration to use as an override in JSON; ESCAPE QUOTES.")] [DefaultValue(null)] bool? leaveOnAfterDeployment,
            [Aliases("type")] [Description("Optional system specific instance type to use.")] [DefaultValue(null)] string instanceSystemType,
            [Aliases("count")] [Description("Optional number of instances to deploy.")] [DefaultValue(null)] int? instanceCount,
            [Aliases("existing")] [Description("Optional strategy to handle existing deployment.")] [DefaultValue(ExistingDeploymentStrategy.Replace)] ExistingDeploymentStrategy existingDeploymentStrategy,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType,
            [Aliases("")] [Description("Optionally sets the working directory; DEFAULT is DefaultWorkingDirectory.")] [DefaultValue(DefaultWorkingDirectory)] string workingPath)
        {
            new { packageId }.AsArg().Must().NotBeNullNorWhiteSpace();

            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            if (turnOffAfterDeployment != null && leaveOnAfterDeployment != null
                && turnOffAfterDeployment == true && leaveOnAfterDeployment == true)
            {
                throw new ArgumentException(Invariant($"Cannot have both {nameof(turnOffAfterDeployment)} and {nameof(leaveOnAfterDeployment)} switches on at the same time."));
            }

            var configFileManager = new ConfigFileManager(new[] { Config.CommonPrecedence }, Config.DefaultConfigDirectoryName, DefaultJsonSerializer);

            DeploymentConfiguration overrideObject;
            if (turnOffAfterDeployment != null && turnOffAfterDeployment == true)
            {
                overrideObject = new DeploymentConfiguration { PostDeploymentStrategy = new PostDeploymentStrategy { TurnOffInstance = true }, };
            }
            else if (leaveOnAfterDeployment != null && leaveOnAfterDeployment == true)
            {
                overrideObject = new DeploymentConfiguration { PostDeploymentStrategy = new PostDeploymentStrategy { TurnOffInstance = false }, };
            }
            else
            {
                overrideObject = new DeploymentConfiguration();
            }

            if (!string.IsNullOrWhiteSpace(instanceSystemType))
            {
                overrideObject.InstanceType = new InstanceType { SpecificInstanceTypeSystemId = instanceSystemType, };
            }

            if (instanceCount != null)
            {
                overrideObject.InstanceCount = (int)instanceCount;
            }

            var overrideJson = configFileManager.SerializeConfigToFileText(overrideObject);
            var withoutPackagePrefix = string.IsNullOrWhiteSpace(PackagePrefixToStrip)
                ? packageId
                : Regex.Replace(packageId, "^" + PackagePrefixToStrip + "\\.", string.Empty, RegexOptions.IgnoreCase);

            var instanceNameLocal = string.IsNullOrWhiteSpace(instanceName) ? withoutPackagePrefix.Replace(".", string.Empty) : instanceName;
            var packagesJson = "[{\"packageDescription\": { \"id\" : \"" + packageId + "\"}}]";
            DeployAdvanced(
                credentialsJson,
                instanceNameLocal,
                packagesJson,
                overrideJson,
                existingDeploymentStrategy,
                debug,
                environment,
                environmentType,
                true,
                workingPath,
                false);
        }

        /// <summary>
        /// Deploys a new instance with specified packages.
        /// </summary>
        /// <param name="credentialsJson">Credentials for the computing platform provider to use in JSON.</param>
        /// <param name="instanceName">Optional name of the instance (one will be generated from the package list if not provided).</param>
        /// <param name="packagesToDeployJson">Packages descriptions (with overrides) to configure the instance with.</param>
        /// <param name="overrideDeploymentConfigJson">Optional deployment configuration to use as an override in JSON.</param>
        /// <param name="existingDeploymentStrategy">Optional strategy for how to handle existing deployments; DEFAULT is Replace.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Environment name being deployed to.</param>
        /// <param name="environmentType">Optionally sets the type of environment; DEFAULT is <see cref="EnvironmentType.Aws" />.</param>
        /// <param name="useFileBasedArcology">Optionally sets a value indicating to use the file based arcology; DEFAULT is false.</param>
        /// <param name="workingPath">Optionally sets the working directory; DEFAULT is <see cref="DefaultWorkingDirectory" />.</param>
        /// <param name="runCommonSetup">INTERNAL USE ONLY - run the common setup logic; DEFAULT is true, should be false if chained from another method in <see cref="ConsoleAbstraction" />.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Arcology", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "nuget", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Like it this way.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "packagesToDeployJson", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "instanceName", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "overrideDeploymentConfigJson", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "credentialsJson", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "workingPath", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "nuget", Justification = "Not sure why it's complaining...")]
        [Verb(Aliases = "deployadvanced", Description = "Deploys a new instance with specified packages.")]
        public static void DeployAdvanced(
            [Aliases("")] [Description("Credentials for the computing platform provider to use in JSON; DEFAULT will be environment variable value of NaosDeploymentCredentialsJson.")] [DefaultValue(null)] string credentialsJson,
            [Aliases("name")] [Required] [Description("Name of the instance (one will be generated from the package list if not provided).")] string instanceName,
            [Aliases("packages")] [Required] [Description("Packages descriptions (with overrides) to configure the instance with; ESCAPE QUOTES.")] string packagesToDeployJson,
            [Aliases("override")] [Description("Optional deployment configuration to use as an override in JSON; ESCAPE QUOTES.")] [DefaultValue("{}")] string overrideDeploymentConfigJson,
            [Aliases("existing")] [Description("Optional strategy to handle existing deployment.")] [DefaultValue(ExistingDeploymentStrategy.Replace)] ExistingDeploymentStrategy existingDeploymentStrategy,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment,
            [Aliases("envType")] [Description("Optionally sets the type of environment; DEFAULT is Aws")] [DefaultValue(EnvironmentType.Aws)] EnvironmentType environmentType,
            [Aliases("")] [Description("Optionally sets a value indicating to use the file based arcology; DEFAULT is false.")] [DefaultValue(false)] bool useFileBasedArcology,
            [Aliases("")] [Description("Optionally sets the working directory; DEFAULT is DefaultWorkingDirectory.")] [DefaultValue(DefaultWorkingDirectory)] string workingPath,
            [Aliases("")] [Description("INTERNAL USE ONLY.")] [DefaultValue(true)] bool runCommonSetup)
        {
            new { workingPath }.AsArg().Must().NotBeNullNorWhiteSpace();

            var stopwatch = Stopwatch.StartNew();
            if (runCommonSetup)
            {
                CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);
            }

            var localWorkingPath = Path.Combine(workingPath, DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss", CultureInfo.InvariantCulture));
            var announcementFilePath = Path.Combine(localWorkingPath, "Announcements.log");
            var debugAnnouncementFilePath = Path.Combine(localWorkingPath, "DebugAnnouncements.log");
            var nugetAnnouncementFilePath = Path.Combine(localWorkingPath, "NuGetOutput.log");
            var telemetryFilePath = Path.Combine(localWorkingPath, "Telemetry.json");

            var environmentCertificateName = Invariant($"{environment.ToLowerInvariant()}.Environment");

            var infrastructureTrackerJson = GetInfrastructureTrackerJsonFromEnvironment(environment, environmentType, useFileBasedArcology);
            var certificateRetrieverJson = GetCertificateRetrieverJsonFromEnvironment(environment, useFileBasedArcology);
            var deploymentAdjustmentApplicatorJson = GetDeploymentAdjustmentApplicatorJsonFromEnvironment(environment);
            var nugetPackageRepositoryConfigurationsJson = GetNugetPackageRepositoryConfigurationsJson();
            var credsJson = string.IsNullOrEmpty(credentialsJson) ? SysEnvironment.GetEnvironmentVariable(CredentialsEnvironmentVaraibleName, EnvironmentVariableTarget.User) : credentialsJson;

            NaosDeploymentBootstrapper.Deploy(
                credsJson,
                nugetPackageRepositoryConfigurationsJson,
                certificateRetrieverJson,
                infrastructureTrackerJson,
                overrideDeploymentConfigJson,
                environmentCertificateName,
                announcementFilePath,
                debugAnnouncementFilePath,
                telemetryFilePath,
                nugetAnnouncementFilePath,
                instanceName,
                localWorkingPath,
                packagesToDeployJson,
                existingDeploymentStrategy,
                deploymentAdjustmentApplicatorJson,
                environment.ToLowerInvariant(),
                environmentType);
            stopwatch.Stop();

            Console.WriteLine("Deployment took: " + stopwatch.Elapsed);
        }

        /// <summary>
        /// Upload a certificate to the arcology from a file along with additional information about it as well as encrypting information.
        /// </summary>
        /// <param name="name">Name of the certificate to load.</param>
        /// <param name="pfxFilePath">File path to the certificate to load (in PFX file format).</param>
        /// <param name="clearTextPassword">Clear text password of the certificate to load.</param>
        /// <param name="certificateSigningRequestPemEncodedFilePath">File path to Certificate Signing Request (PEM encoded).</param>
        /// <param name="encryptingCertificateThumbprint">Thumbprint of the encrypting certificate.</param>
        /// <param name="encryptingCertificateIsValid">Value indicating whether or not the encrypting certificate is valid.</param>
        /// <param name="encryptingCertificateStoreName"><see cref="StoreName"/> to find the encrypting certificate.</param>
        /// <param name="encryptingCertificateStoreLocation"><see cref="StoreLocation"/> to find the encrypting certificate.</param>
        /// <param name="debug">A value indicating whether or not to launch the debugger.</param>
        /// <param name="environment">Optional environment name that will set the <see cref="Naos.Configuration" /> precedence instead of the default which is reading the App.Config value.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "pfx", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pem", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Like it this way.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "certificateWriterJson", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "encryptingCertificateStoreLocation", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "encryptingCertificateStoreName", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "encryptingCertificateIsValid", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "encryptingCertificateThumbprint", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "cleanPassword", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "pfxFilePath", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3", Justification = "Is validated with Must.")]
        [Verb(Aliases = "upload", Description = "Deploys a new instance with specified packages.")]
        public static void UploadCertificate(
            [Aliases("")] [Required] [Description("Name of the certificate to load.")] string name,
            [Aliases("")] [Required] [Description("File path to the certificate to load (in PFX file format).")] string pfxFilePath,
            [Aliases("")] [Required] [Description("Clear text password of the certificate to load.")] string clearTextPassword,
            [Aliases("")] [DefaultValue(null)] [Description("File path to Certificate Signing Request (PEM encoded).")] string certificateSigningRequestPemEncodedFilePath,
            [Aliases("")] [Required] [Description("Thumbprint of the encrypting certificate.")] string encryptingCertificateThumbprint,
            [Aliases("")] [Required] [Description("Value indicating whether or not the encrypting certificate is valid.")] bool encryptingCertificateIsValid,
            [Aliases("")] [DefaultValue(null)] [Description("Store name to find the encrypting certificate.")] string encryptingCertificateStoreName,
            [Aliases("")] [DefaultValue(null)] [Description("Store location to find the encrypting certificate.")] string encryptingCertificateStoreLocation,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("env")] [Required] [Description("Sets the Naos.Configuration precedence to use specific settings.")] string environment)
        {
            CommonSetup(debug, environment.ToLowerInvariant(), announcer: NullAnnouncer);

            var certificateWriterJson = GetCertificateManagerJsonFromEnvironment(environment);

            NaosDeploymentBootstrapper.UploadCertificate(
                certificateWriterJson,
                name,
                pfxFilePath,
                clearTextPassword,
                certificateSigningRequestPemEncodedFilePath,
                encryptingCertificateThumbprint,
                encryptingCertificateIsValid,
                encryptingCertificateStoreName,
                encryptingCertificateStoreLocation);
        }

        /// <summary>
        /// Downloads the certificate from arcology and prints the password.
        /// </summary>
        /// <param name="debug">if set to <c>true</c> [debug].</param>
        /// <param name="environment">The environment.</param>
        /// <param name="certificateName">Name of the certificate.</param>
        /// <param name="exportFilePath">The export file path.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Arcology", Justification = NaosSuppressBecause.CA1704_IdentifiersShouldBeSpelledCorrectly_SpellingIsCorrectInContextOfTheDomain)]
        [Verb(Aliases = "downloadCert", Description = "Downloads a certificate from the arcology.")]
        public static void DownloadCertificateFromArcology(
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)]
            bool debug,
            [Aliases("env")] [Required] [Description("Sets the Its.Configuration precedence to use specific settings.")]
            string environment,
            [Aliases("name")] [Description("Name of the certificate in the arcology.")] [Required]
            string certificateName,
            [Aliases("path")] [Description("File path to write pfx file to.")] [Required]
            string exportFilePath)
        {
            CommonSetup(debug, environment);

            var certificateRetrieverJson = GetCertificateRetrieverJsonFromEnvironment(environment);

            var pfxPassword = NaosDeploymentBootstrapper.GetCertificatePasswordAndWritePfxFileFromArcology(certificateName, certificateRetrieverJson, exportFilePath);
            Console.WriteLine(Invariant($"File written to: {exportFilePath}"));
            Console.WriteLine(Invariant($"Password: {pfxPassword}"));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        private static string GetCertificateManagerJsonFromEnvironment(string environment, bool useFileArcology = false)
        {
            if (useFileArcology)
            {
                var config = new CertificateManagementConfigurationFile
                             {
                                 FilePath = Path.Combine(DefaultTempArcologyDirectory, "Certificates.json"),
                             };

                var json = DefaultJsonSerializer.SerializeToString(config);
                return json;
            }
            else
            {
                var deploymentDatabaseJson = ReadConfigFileText(environment.ToLowerInvariant(), nameof(DeploymentDatabase));
                deploymentDatabaseJson.MustForOp(nameof(deploymentDatabaseJson)).NotBeNull();
                var adjustedJson = "{\"database\": " + deploymentDatabaseJson + "}";
                return adjustedJson;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        private static string GetCertificateRetrieverJsonFromEnvironment(string environment, bool useFileArcology = false)
        {
            if (useFileArcology)
            {
                var config = new CertificateManagementConfigurationFile
                             {
                                 FilePath = Path.Combine(DefaultTempArcologyDirectory, Invariant($"{environment}.Certificates.json")),
                             };

                var json = DefaultJsonSerializer.SerializeToString(config);
                return json;
            }
            else
            {
                var deploymentDatabaseJson = ReadConfigFileText(environment.ToLowerInvariant(), nameof(DeploymentDatabase));
                deploymentDatabaseJson.MustForOp(nameof(deploymentDatabaseJson)).NotBeNull();
                var adjustedJson = "{\"database\": " + deploymentDatabaseJson + "}";
                return adjustedJson;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        private static string GetInfrastructureTrackerJsonFromEnvironment(string environment, EnvironmentType environmentType, bool useFileArcology = false)
        {
            if (useFileArcology)
            {
                var config = new InfrastructureTrackerConfigurationFolder { RootFolderPath = DefaultTempArcologyDirectory };
                var json = DefaultJsonSerializer.SerializeToString(config);
                return json;
            }
            else if (environmentType == EnvironmentType.Manual)
            {
                var config = new InfrastructureTrackerConfigurationNull();
                var json = DefaultJsonSerializer.SerializeToString(config);
                return json;
            }
            else if (environmentType == EnvironmentType.Aws)
            {
                var deploymentDatabaseJson = ReadConfigFileText(environment.ToLowerInvariant(), nameof(DeploymentDatabase));
                deploymentDatabaseJson.MustForOp(nameof(deploymentDatabaseJson)).NotBeNull();
                var adjustedJson = "{\"database\": " + deploymentDatabaseJson + "}";
                return adjustedJson;
            }
            else
            {
                throw new NotSupportedException(Invariant($"Unsupported {nameof(EnvironmentType)} - {environmentType}."));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Want lowercase here.")]
        private static string GetDeploymentAdjustmentApplicatorJsonFromEnvironment(string environment)
        {
            var deploymentAdjustmentApplicatorJsonFromEnvironment = ReadConfigFileText(environment.ToLowerInvariant(), nameof(DeploymentAdjustmentStrategiesApplicator));
            if (deploymentAdjustmentApplicatorJsonFromEnvironment == null)
            {
                var nullAdjuster     = new DeploymentAdjustmentStrategiesApplicator(new List<AdjustDeploymentBase>());
                var nullAdjusterJson = DefaultJsonSerializer.SerializeToString(nullAdjuster);
                deploymentAdjustmentApplicatorJsonFromEnvironment = nullAdjusterJson;
            }

            return deploymentAdjustmentApplicatorJsonFromEnvironment;
        }

        private static string GetNugetPackageRepositoryConfigurationsJson()
        {
            var configFileManager = new ConfigFileManager(new[] { Config.CommonPrecedence }, Config.DefaultConfigDirectoryName, DefaultJsonSerializer);

            var nugetPackageRepositoryConfigurationsJson = ReadConfigFileText(Config.CommonPrecedence, nameof(PackageRepositoryConfigurations));
            nugetPackageRepositoryConfigurationsJson.MustForOp(nameof(nugetPackageRepositoryConfigurationsJson)).NotBeNull();

            var configs = configFileManager.DeserializeConfigFileText<PackageRepositoryConfigurations>(nugetPackageRepositoryConfigurationsJson);
            var configsJson = configFileManager.SerializeConfigToFileText(configs.Configurations);

            return configsJson;
        }

        private static string ReadConfigFileText(string folderName, string fileNameWithExtension)
        {
            var rootPath = Path.Combine(Config.SettingsDirectory, folderName);
            var filePath = Path.Combine(rootPath, Invariant($"{fileNameWithExtension}.json"));
            return File.Exists(filePath) ? File.ReadAllText(filePath) : null;
        }

        private static void NullAnnouncer(string obj)
        {
            /* no-op */
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "environment", Justification = "Will eventually be needed when the environments are split out.")]
        private static string GetAwsAccountIdFromEnvironment(string environment)
        {
            // TODO: make these different account for security isolation...
            return "00000000000";
        }
    }

    /// <summary>
    /// Specifies a direction to migrate the arcology.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Arcology", Justification = "Spelling/name is correct.")]
    public enum ArcologyMigrationDirection
    {
        /// <summary>
        /// Direction is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// From a directory arcology to a database arcology.
        /// </summary>
        DirectoryToDatabase,

        /// <summary>
        /// From a database arcology to a directory arcology.
        /// </summary>
        DatabaseToDirectory,
    }
}

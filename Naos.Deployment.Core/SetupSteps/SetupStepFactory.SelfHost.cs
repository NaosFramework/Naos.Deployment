﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetupStepFactory.SelfHost.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using Naos.Cron;
    using Naos.Deployment.Domain;
    using Naos.Logging.Domain;

    /// <summary>
    /// Factory to create a list of setup steps from various situations (abstraction to actual machine setup).
    /// </summary>
    public partial class SetupStepFactory
    {
        private async Task<List<SetupStep>> GetSelfHostSpecificSteps(InitializationStrategySelfHost selfHostStrategy, LogWritingSettings defaultLogWritingSettings, IReadOnlyCollection<ItsConfigOverride> itsConfigOverrides, string packageDirectoryPath, string environment, string adminPassword, Func<string, string> funcToCreateNewDnsWithTokensReplaced)
        {
            var selfHostSteps = new List<SetupStep>();
            var selfHostDnsEntries = selfHostStrategy.SelfHostSupportedDnsEntries.Select(funcToCreateNewDnsWithTokensReplaced).ToList();
            var sslCertificateName = selfHostStrategy.SslCertificateName;
            var scheduledTaskAccount = this.GetAccountToUse(selfHostStrategy);
            var selfHostExeFilePathRelativeToPackageRoot = selfHostStrategy.SelfHostExeFilePathRelativeToPackageRoot;
            var selfHostExeArguments = selfHostStrategy.SelfHostArguments;
            var applicationId = Guid.NewGuid().ToString().ToUpper();
            var runElevated = selfHostStrategy.RunElevated;
            var priority = selfHostStrategy.Priority ?? this.Settings.HarnessSettings.DefaultTaskPriority;

            // specific steps to support self hosting
            var certDetails = await this.certificateRetriever.GetCertificateByNameAsync(sslCertificateName);
            if (certDetails == null)
            {
                throw new DeploymentException("Could not find certificate by name: " + sslCertificateName);
            }

            var configureCertParams = new object[] { StoreLocation.LocalMachine.ToString(), StoreName.My.ToString(), certDetails.GetPowershellPathableThumbprint(), applicationId, selfHostDnsEntries };
            selfHostSteps.Add(
                new SetupStep
                    {
                        Description = $"Configure SSL Certificate {sslCertificateName} for Self Hosting.",
                        SetupFunc =
                            machineManager =>
                            machineManager.RunScript(
                                this.Settings.DeploymentScriptBlocks.ConfigureSslCertificateForHosting.ScriptText,
                                configureCertParams).ToList(),
                    });

            var configureUserParams = new object[] { scheduledTaskAccount, selfHostDnsEntries };
            selfHostSteps.Add(
                new SetupStep
                    {
                        Description = $"Configure user {scheduledTaskAccount} for Self Hosting.",
                        SetupFunc =
                            machineManager =>
                            machineManager.RunScript(this.Settings.DeploymentScriptBlocks.ConfigureUserForHosting.ScriptText, configureUserParams).ToList(),
                    });

            var openPortParams = new[] { "443", "Allow TCP 443 IN for Self Hosting" };
            selfHostSteps.Add(
                new SetupStep
                    {
                        Description = $"Open port 443 for Self Hosting.",
                        SetupFunc =
                            machineManager =>
                            machineManager.RunScript(this.Settings.DeploymentScriptBlocks.OpenPort.ScriptText, openPortParams).ToList(),
                    });

            // task steps to keep the console exe alive
            var schedule = new IntervalSchedule { Interval = TimeSpan.FromMinutes(1) };
            var exeFilePathRelativeToPackageRoot = selfHostExeFilePathRelativeToPackageRoot;
            var arguments = selfHostExeArguments;
            var name = "SelfHostKeepAliveFor" + exeFilePathRelativeToPackageRoot;
            var description = $"Task to ensure that the self host {exeFilePathRelativeToPackageRoot} is always running.";
            var scheduledTaskStesps = this.GetScheduledTaskSpecificStepsParameterizedWithoutStrategy(
                defaultLogWritingSettings,
                itsConfigOverrides,
                packageDirectoryPath,
                environment,
                exeFilePathRelativeToPackageRoot,
                schedule,
                scheduledTaskAccount,
                adminPassword,
                runElevated,
                priority,
                name,
                description,
                arguments);

            selfHostSteps.AddRange(scheduledTaskStesps);

            return selfHostSteps;
        }

        private string GetAccountToUse(InitializationStrategySelfHost selfHostStrategy)
        {
            var scheduledTaskAccount = string.IsNullOrEmpty(selfHostStrategy.ScheduledTaskAccount)
                                           ? this.Settings.HarnessSettings.HarnessAccount
                                           : selfHostStrategy.ScheduledTaskAccount;
            return scheduledTaskAccount;
        }
    }
}

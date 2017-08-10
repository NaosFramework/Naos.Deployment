﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetupStepFactory.CertificateToInstall.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Naos.Deployment.Domain;

    using Spritely.Recipes;

    /// <summary>
    /// Factory to create a list of setup steps from various situations (abstraction to actual machine setup).
    /// </summary>
    internal partial class SetupStepFactory
    {
        private async Task<List<SetupStep>> GetCertificateToInstallSpecificStepsAsync(InitializationStrategyCertificateToInstall certToInstallStrategy, string packageDirectoryPath, string harnessAccount, string iisAccount)
        {
            var usersToGrantPrivateKeyAccess = new[] { certToInstallStrategy.AccountToGrantPrivateKeyAccess };
            var certificateName = certToInstallStrategy.CertificateToInstall;

            return await this.GetCertificateToInstallSpecificStepsParameterizedWithoutStrategyAsync(packageDirectoryPath, harnessAccount, iisAccount, usersToGrantPrivateKeyAccess, certificateName);
        }

        private async Task<List<SetupStep>> GetCertificateToInstallSpecificStepsParameterizedWithoutStrategyAsync(
            string tempPathToStoreFileWhileInstalling,
            string harnessAccount,
            string iisAccount,
            ICollection<string> usersToGrantPrivateKeyAccess,
            string certificateName)
        {
            var certSteps = new List<SetupStep>();

            var tokenAppliedUsers =
                usersToGrantPrivateKeyAccess.Select(_ => TokenSubstitutions.GetSubstitutedStringForAccounts(_, harnessAccount, iisAccount)).ToArray();
            var tokenAppliedUsersString = string.Join(",", tokenAppliedUsers);

            var certDetails = await this.certificateRetriever.GetCertificateByNameAsync(certificateName);
            if (certDetails == null)
            {
                throw new DeploymentException("Could not find certificate by name: " + certificateName);
            }

            var certificateTargetPath = Path.Combine(tempPathToStoreFileWhileInstalling, certDetails.GenerateFileName());
            certSteps.Add(
                new SetupStep
                    {
                        Description = "Send certificate file (removed after installation): " + certDetails.GenerateFileName(),
                        SetupFunc = machineManager =>
                            {
                                machineManager.SendFile(certificateTargetPath, certDetails.PfxBytes);
                                return new dynamic[0];
                            },
                    });

            var installCertificateParams = new object[] { certificateTargetPath, certDetails.PfxPasswordInClearText.ToSecureString(), tokenAppliedUsers };

            certSteps.Add(
                new SetupStep
                    {
                        Description = $"Installing certificate  '{certificateName}' for [{tokenAppliedUsersString}]",
                        SetupFunc =
                            machineManager =>
                            machineManager.RunScript(this.settings.DeploymentScriptBlocks.InstallCertificate.ScriptText, installCertificateParams),
                    });

            return certSteps;
        }
    }
}

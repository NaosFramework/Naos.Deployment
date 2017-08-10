﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetupStepFactory.InstanceLevel.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Naos.Deployment.Domain;
    using Naos.Packaging.Domain;

    /// <summary>
    /// Factory to create a list of setup steps from various situations (abstraction to actual machine setup).
    /// </summary>
    internal partial class SetupStepFactory
    {
        /// <summary>
        /// Gets the instance level setup steps.
        /// </summary>
        /// <param name="computerName">Computer name to use in windows for the instance.</param>
        /// <param name="windowsSku">The windows SKU used to create the instance.</param>
        /// <param name="environment">Environment the instance is in.</param>
        /// <param name="chocolateyPackages">Chocolatey packages to install.</param>
        /// <param name="allInitializationStrategies">All initialization strategies to be setup.</param>
        /// <returns>List of setup steps </returns>
        public async Task<ICollection<SetupStep>> GetInstanceLevelSetupSteps(string computerName, WindowsSku windowsSku, string environment, IReadOnlyCollection<PackageDescription> chocolateyPackages, IReadOnlyCollection<InitializationStrategyBase> allInitializationStrategies)
        {
            var ret = new List<SetupStep>();

            var setupWinRm = new SetupStep
                                 {
                                     Description = "Setup WinRM",
                                     SetupFunc =
                                         machineManager =>
                                         machineManager.RunScript(
                                             this.settings.DeploymentScriptBlocks.SetupWinRmScriptBlock
                                             .ScriptText),
                                 };

            ret.Add(setupWinRm);

            var setupUpdates = new SetupStep
                                   {
                                       Description = "Setup Windows Updates",
                                       SetupFunc =
                                           machineManager =>
                                           machineManager.RunScript(
                                               this.settings.DeploymentScriptBlocks
                                               .SetupWindowsUpdatesScriptBlock.ScriptText),
                                   };

            ret.Add(setupUpdates);

            var setupTime = new SetupStep
                                {
                                    Description = "Setup Windows Time",
                                    SetupFunc =
                                        machineManager =>
                                        machineManager.RunScript(
                                            this.settings.DeploymentScriptBlocks.SetupWindowsTimeScriptBlock
                                            .ScriptText),
                                };

            ret.Add(setupTime);

            var execScripts = new SetupStep
                                  {
                                      Description = "Enable Script Execution",
                                      SetupFunc =
                                          machineManager =>
                                          machineManager.RunScript(
                                              this.settings.DeploymentScriptBlocks
                                              .EnableScriptExecutionScriptBlock.ScriptText),
                                  };

            ret.Add(execScripts);

            var windowsSkuEnvironmentVariable = "WindowsSku";
            var addEnvironmentVariables = new SetupStep
                                              {
                                                  Description = "Add Machine Level Environment Variables",
                                                  SetupFunc = machineManager =>
                                                      {
                                                          var environmentVariablesToAdd = new[]
                                                                                                  {
                                                                                                      new
                                                                                                          {
                                                                                                              Name = this.settings.EnvironmentEnvironmentVariableName,
                                                                                                              Value = environment,
                                                                                                          },
                                                                                                      new
                                                                                                          {
                                                                                                              Name = windowsSkuEnvironmentVariable,
                                                                                                              Value = windowsSku.ToString(),
                                                                                                          },
                                                                                                  };
                                                          return
                                                              machineManager.RunScript(
                                                                  this.settings.DeploymentScriptBlocks.AddMachineLevelEnvironmentVariables.ScriptText,
                                                                  new[] { environmentVariablesToAdd });
                                                      },
                                              };

            ret.Add(addEnvironmentVariables);

            var wallpaperUpdate = new SetupStep
                                      {
                                          Description = "Customize Instance Wallpaper",
                                          SetupFunc = machineManager =>
                                              {
                                                  var environmentVariablesToAddToWallpaper = new[] { this.settings.EnvironmentEnvironmentVariableName, windowsSkuEnvironmentVariable };
                                                  return
                                                      machineManager.RunScript(
                                                          this.settings.DeploymentScriptBlocks.UpdateInstanceWallpaper.ScriptText,
                                                          new[] { environmentVariablesToAddToWallpaper });
                                              },
                                      };

            ret.Add(wallpaperUpdate);

            var registryKeysToUpdateExplorer = new[]
                                                   {
                                                       new
                                                           {
                                                               Path = "Registry::HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced",
                                                               Name = "Hidden",
                                                               Value = "1",
                                                               Type = "DWord",
                                                           },
                                                       new
                                                           {
                                                               Path = "Registry::HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced",
                                                               Name = "ShowSuperHidden",
                                                               Value = "1",
                                                               Type = "DWord",
                                                           },
                                                       new
                                                           {
                                                               // http://superuser.com/questions/666891/script-to-set-hide-file-extensions
                                                               Path = "Registry::HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced",
                                                               Name = "HideFileExt",
                                                               Value = "0",
                                                               Type = "DWord",
                                                           },
                                                   };

            var explorerShowHidden = new SetupStep
                                         {
                                             Description = "Set Explorer to show all hidden files with extensions",
                                             SetupFunc = machineManager =>
                                                 {
                                                     var fileExplorerParams = new[] { registryKeysToUpdateExplorer };
                                                     return
                                                         machineManager.RunScript(
                                                             this.settings.DeploymentScriptBlocks.UpdateWindowsRegistryEntries.ScriptText,
                                                             fileExplorerParams);
                                                 },
                                         };

            ret.Add(explorerShowHidden);

            var installChocoSteps = this.GetChocolateySetupSteps(chocolateyPackages);
            ret.AddRange(installChocoSteps);

            if (!string.IsNullOrEmpty(this.environmentCertificateName))
            {
                var distinctInitializationStrategyTypes = allInitializationStrategies.Select(_ => _.GetType()).ToList();
                if (distinctInitializationStrategyTypes.Any(_ => this.InitializationStrategyTypesThatNeedEnvironmentCertificate.Contains(_)))
                {
                    var usersToGrantAccessToKey = allInitializationStrategies.Select(this.GetAccountToUse).Where(_ => _ != null).Distinct().ToArray();

                    var environmentCertSteps =
                        await
                        this.GetCertificateToInstallSpecificStepsParameterizedWithoutStrategyAsync(
                            this.RootDeploymentPath,
                            this.settings.HarnessSettings.HarnessAccount,
                            this.settings.WebServerSettings.IisAccount,
                            usersToGrantAccessToKey,
                            this.environmentCertificateName);
                    ret.AddRange(environmentCertSteps);
                }
            }

            var rename = new SetupStep
                             {
                                 Description = "Rename Computer",
                                 SetupFunc = machineManager =>
                                     {
                                         var renameParams = new[] { computerName };
                                         return machineManager.RunScript(
                                             this.settings.DeploymentScriptBlocks.RenameComputerScriptBlock.ScriptText,
                                             renameParams);
                                     },
                             };

            ret.Add(rename);

            return ret;
        }

        private ICollection<SetupStep> GetChocolateySetupSteps(IReadOnlyCollection<PackageDescription> chocolateyPackages)
        {
            var installChocoSteps = new List<SetupStep>();
            if (chocolateyPackages != null && chocolateyPackages.Any())
            {
                var installChocoClientStep = new SetupStep
                                                 {
                                                     Description = "Install Chocolatey Client",
                                                     SetupFunc =
                                                         machineManager =>
                                                         machineManager.RunScript(this.settings.DeploymentScriptBlocks.InstallChocolatey.ScriptText),
                                                 };

                installChocoSteps.Add(installChocoClientStep);

                foreach (var chocoPackage in chocolateyPackages)
                {
                    var installChocoPackagesStep = new SetupStep
                                                       {
                                                           Description = "Install Chocolatey Package: " + chocoPackage.GetIdDotVersionString(),
                                                           SetupFunc = machineManager =>
                                                               {
                                                                   var installChocoPackageParams = new object[] { chocoPackage };
                                                                   return
                                                                       machineManager.RunScript(
                                                                           this.settings.DeploymentScriptBlocks.InstallChocolateyPackages.ScriptText,
                                                                           installChocoPackageParams);
                                                               },
                                                       };

                    installChocoSteps.Add(installChocoPackagesStep);
                }
            }

            return installChocoSteps;
        }
    }
}

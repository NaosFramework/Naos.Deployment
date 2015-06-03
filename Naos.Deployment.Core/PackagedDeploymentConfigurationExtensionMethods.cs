﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackagedDeploymentConfigurationExtensionMethods.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core
{
    using System.Collections.Generic;
    using System.Linq;

    using Naos.Deployment.Contract;

    /// <summary>
    /// Additional behavior to add the initialization strategies.
    /// </summary>
    public static class PackagedDeploymentConfigurationExtensionMethods
    {
        public static ICollection<PackagedDeploymentConfiguration> ApplyDefaults(
            this ICollection<PackagedDeploymentConfiguration> packagedConfigs,
            DeploymentConfiguration defaultDeploymentConfig)
        {
            if (packagedConfigs.Count == 0)
            {
                return
                    new[]
                        {
                            new PackagedDeploymentConfiguration
                                {
                                    Package = null,
                                    DeploymentConfiguration = defaultDeploymentConfig
                                }
                        }
                        .ToList();
            }
            else
            {
                return
                    packagedConfigs.Select(
                        _ =>
                        new PackagedDeploymentConfiguration
                            {
                                Package = _.Package,
                                DeploymentConfiguration =
                                    _.DeploymentConfiguration.ApplyDefaults(
                                        defaultDeploymentConfig)
                            }).ToList();
            }
        }

        /// <summary>
        /// Retrieves the initialization strategies matching the specified type.
        /// </summary>
        /// <typeparam name="T">Type of initialization strategy to look for.</typeparam>
        /// <param name="baseCollection">Base collection of packaged configurations to operate on.</param>
        /// <returns>Collection of initialization strategies matching the type specified.</returns>
        public static ICollection<T> GetInitializationStrategiesOf<T>(
            this ICollection<PackagedDeploymentConfiguration> baseCollection) where T : InitializationStrategyBase
        {
            var ret =
                baseCollection.SelectMany(_ => _.InitializationStrategies.Select(strat => strat as T))
                    .Where(_ => _ != null)
                    .ToList();

            return ret;
        }

        /// <summary>
        /// Retrieves the initialization strategies matching the specified type.
        /// </summary>
        /// <typeparam name="T">Type of initialization strategy to look for.</typeparam>
        /// <param name="baseObject">Base packaged configuration to operate on.</param>
        /// <returns>Collection of initialization strategies matching the type specified.</returns>
        public static ICollection<T> GetInitializationStrategiesOf<T>(
            this PackagedDeploymentConfiguration baseObject) where T : InitializationStrategyBase
        {
            var ret = baseObject.InitializationStrategies.Select(_ => _ as T).Where(_ => _ != null).ToList();
            return ret;
        }

        /// <summary>
        /// Overrides the deployment config in a collection of packaged configurations.
        /// </summary>
        /// <param name="baseCollection">Base collection of packaged configurations to operate on.</param>
        /// <param name="overrideConfig">Configuration to apply as an override.</param>
        /// <returns>New collection of packaged configurations with overrides applied.</returns>
        public static ICollection<PackagedDeploymentConfiguration>
            OverrideDeploymentConfig(
            this ICollection<PackagedDeploymentConfiguration> baseCollection,
            DeploymentConfiguration overrideConfig)
        {
            var ret =
                baseCollection.Select(
                    _ =>
                    new PackagedDeploymentConfiguration
                        {
                            DeploymentConfiguration = overrideConfig,
                            Package = _.Package,
                            ItsConfigOverrides = _.ItsConfigOverrides,
                            InitializationStrategies = _.InitializationStrategies,
                        }).ToList();
            return ret;
        }
    }
}

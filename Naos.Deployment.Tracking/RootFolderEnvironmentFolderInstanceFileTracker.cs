﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RootFolderEnvironmentFolderInstanceFileTracker.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Naos.Deployment.Domain;
    using Naos.Packaging.Domain;

    /// <summary>
    /// Tracking system/certificate manager that will use a root folder and will have a folder per environment with a config file and store a file per machine.
    /// </summary>
    public class RootFolderEnvironmentFolderInstanceFileTracker : ITrackComputingInfrastructure
    {
        private const string InstancePrefix = "Instance--";
        private const string IpInfix = "ip--";

        // should maybe break out a lock provider and lock by environment...
        private readonly object fileSync = new object();

        private readonly string rootFolderPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootFolderEnvironmentFolderInstanceFileTracker"/> class.
        /// </summary>
        /// <param name="rootFolderPath">Path to save all state to.</param>
        public RootFolderEnvironmentFolderInstanceFileTracker(string rootFolderPath)
        {
            this.rootFolderPath = rootFolderPath;
        }

        /// <inheritdoc />
        public ICollection<InstanceDescription> GetInstancesByDeployedPackages(string environment, ICollection<PackageDescription> packages)
        {
            lock (this.fileSync)
            {
                var arcology = this.GetArcologyByEnvironmentName(environment);
                return arcology.GetInstancesByDeployedPackages(packages);
            }
        }

        /// <inheritdoc />
        public void ProcessInstanceTermination(string environment, string systemId)
        {
            lock (this.fileSync)
            {
                var arcology = this.GetArcologyByEnvironmentName(environment);
                var matchingInstance =
                    arcology.Instances.SingleOrDefault(_ => _.InstanceDescription.Id == systemId);

                // once we have found the file we just need to delete the file
                if (matchingInstance != null)
                {
                    var arcologyFolderPath = this.GetArcologyFolderPath(arcology.Environment);
                    var instanceFilePathNamed = GetInstanceFilePathNamed(arcologyFolderPath, matchingInstance);
                    var instanceFilePathIp = GetInstanceFilePathIp(arcologyFolderPath, matchingInstance);

                    if (File.Exists(instanceFilePathNamed))
                    {
                        File.Delete(instanceFilePathNamed);
                    }

                    // clean up the file before it had a name (if applicable)
                    if (File.Exists(instanceFilePathIp))
                    {
                        File.Delete(instanceFilePathIp);
                    }
                }
            }
        }

        /// <inheritdoc />
        public InstanceCreationDetails GetNewInstanceCreationDetails(
            string environment,
            DeploymentConfiguration deploymentConfiguration,
            ICollection<PackageDescription> intendedPackages)
        {
            lock (this.fileSync)
            {
                var arcology = this.GetArcologyByEnvironmentName(environment);
                var newDeployedInstance = arcology.CreateNewDeployedInstance(deploymentConfiguration, intendedPackages);

                // write
                arcology.MutateInstancesAdd(newDeployedInstance);
                this.SaveArcology(arcology);

                return newDeployedInstance.InstanceCreationDetails;
            }
        }

        /// <inheritdoc />
        public void ProcessInstanceCreation(InstanceDescription instanceDescription)
        {
            lock (this.fileSync)
            {
                var arcology = this.GetArcologyByEnvironmentName(instanceDescription.Environment);

                var toUpdate =
                    arcology.Instances.SingleOrDefault(
                        _ => _.InstanceCreationDetails.PrivateIpAddress == instanceDescription.PrivateIpAddress);

                if (toUpdate == null)
                {
                    throw new DeploymentException(
                        "Expected to find a tracked instance (pre-creation) with private IP: "
                        + instanceDescription.PrivateIpAddress);
                }

                // write
                toUpdate.InstanceDescription = instanceDescription;
                this.SaveArcology(arcology);
            }
        }

        /// <inheritdoc />
        public void ProcessDeployedPackage(string environment, string systemId, PackageDescription package)
        {
            lock (this.fileSync)
            {
                var arcology = this.GetArcologyByEnvironmentName(environment);
                var instanceToUpdate = arcology.Instances.SingleOrDefault(_ => _.InstanceDescription.Id == systemId);
                if (instanceToUpdate == null)
                {
                    throw new DeploymentException(
                        "Expected to find a tracked instance (post-creation) with system ID: "
                        + systemId);
                }

                // write
                Arcology.UpdatePackageVerificationInInstanceDeploymentList(instanceToUpdate, package);
                this.SaveArcology(arcology);
            }
        }

        /// <inheritdoc />
        public InstanceDescription GetInstanceDescriptionById(string environment, string systemId)
        {
            lock (this.fileSync)
            {
                var arcology = this.GetArcologyByEnvironmentName(environment);
                return arcology.GetInstanceDescriptionById(systemId);
            }
        }

        /// <inheritdoc />
        public string GetInstanceIdByName(string environment, string name)
        {
            lock (this.fileSync)
            {
                var arcology = this.GetArcologyByEnvironmentName(environment);
                return arcology.GetInstanceIdByName(name);
            }
        }

        /// <inheritdoc />
        public string GetPrivateKeyOfInstanceById(string environment, string systemId)
        {
            lock (this.fileSync)
            {
                var arcology = this.GetArcologyByEnvironmentName(environment);
                return arcology.GetPrivateKeyOfInstanceById(systemId);
            }
        }

        /// <inheritdoc />
        public string GetDomainZoneId(string environment, string domain)
        {
            lock (this.fileSync)
            {
                var arcology = this.GetArcologyByEnvironmentName(environment);
                return arcology.GetDomainZoneId(domain);
            }
        }

        /// <summary>
        /// Gets a copy of the arcology by environment.
        /// </summary>
        /// <param name="environment">The environment to get an arcology for.</param>
        /// <returns>The arcology for the provided environment.</returns>
        public Arcology GetArcologyByEnvironmentName(string environment)
        {
            environment = environment ?? "[NULL VALUE PASSED]";
            environment = string.IsNullOrEmpty(environment) ? "[EMPTY STRING PASSED]" : environment;

            var arcologyFolderPath = this.GetArcologyFolderPath(environment);
            if (!Directory.Exists(arcologyFolderPath))
            {
                throw new ArgumentException(
                    "Failed to find tracking information for environment: " + " expected information at: " + arcologyFolderPath);
            }

            var instanceFiles = Directory.GetFiles(arcologyFolderPath, InstancePrefix + "*", SearchOption.TopDirectoryOnly);
            var instances = instanceFiles.Select(_ => Serializer.Deserialize<DeployedInstance>(File.ReadAllText(_))).ToList();
            var arcologyInfoFilePath = Path.Combine(arcologyFolderPath, "ArcologyInfo.json");
            var arcologyInfoText = File.ReadAllText(arcologyInfoFilePath);
            var arcologyInfo = Serializer.Deserialize<ArcologyInfo>(arcologyInfoText);

            var ret = new Arcology(environment, arcologyInfo, instances);
            return ret;
        }

        private void SaveArcology(Arcology arcology)
        {
            var arcologyFolderPath = this.GetArcologyFolderPath(arcology.Environment);

            foreach (var instanceWrapper in arcology.Instances)
            {
                var instanceFileContents = Serializer.Serialize(instanceWrapper);

                var instanceFilePathIp = GetInstanceFilePathIp(arcologyFolderPath, instanceWrapper);
                if (string.IsNullOrEmpty(instanceWrapper.InstanceDescription.Name))
                {
                    File.WriteAllText(instanceFilePathIp, instanceFileContents);
                }
                else
                {
                    var instanceFilePathNamed = GetInstanceFilePathNamed(arcologyFolderPath, instanceWrapper);

                    File.WriteAllText(instanceFilePathNamed, instanceFileContents);

                    // clean up the file before it had a name (if applicable)
                    if (File.Exists(instanceFilePathIp))
                    {
                        File.Delete(instanceFilePathIp);
                    }
                }
            }
        }

        private static string GetInstanceFilePathIp(string arcologyFolderPath, DeployedInstance deployedInstance)
        {
            var instanceFilePathIp = Path.Combine(
                arcologyFolderPath,
                InstancePrefix + IpInfix + deployedInstance.InstanceDescription.PrivateIpAddress + ".json");
            return instanceFilePathIp;
        }

        private static string GetInstanceFilePathNamed(string arcologyFolderPath, DeployedInstance deployedInstance)
        {
            var instanceFilePathNamed = Path.Combine(
                arcologyFolderPath,
                InstancePrefix + deployedInstance.InstanceDescription.Name + ".json");
            return instanceFilePathNamed;
        }

        private string GetArcologyFolderPath(string environment)
        {
            var arcologyFolderPath = Path.Combine(this.rootFolderPath, environment);
            return arcologyFolderPath;
        }
    }
}
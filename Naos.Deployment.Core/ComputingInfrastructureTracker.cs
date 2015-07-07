﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputingInfrastructureTracker.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Naos.Deployment.Contract;

    /// <inheritdoc />
    public class ComputingInfrastructureTracker : ITrackComputingInfrastructure, IGetCertificates
    {
        private readonly object fileSync = new object();
        private readonly string filePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputingInfrastructureTracker"/> class.
        /// </summary>
        /// <param name="filePath">Path to save all state to.</param>
        public ComputingInfrastructureTracker(string filePath)
        {
            this.filePath = filePath;
        }

        /// <inheritdoc />
        public ICollection<InstanceDescription> GetInstancesByDeployedPackages(ICollection<PackageDescription> packages)
        {
            lock (this.fileSync)
            {
                var theSafe = this.LoadStateFromDisk();

                var instancesThatHaveAnyOfTheProvidedPackages =
                    theSafe.Instances.Where(
                        _ =>
                        _.InstanceDescription.DeployedPackages.Intersect(
                            packages,
                            new PackageDescriptionIdOnlyEqualityComparer()).Any()).ToList();

                var ret = instancesThatHaveAnyOfTheProvidedPackages.Select(_ => _.InstanceDescription).ToList();
                return ret;
            }
        }

        /// <inheritdoc />
        public void ProcessInstanceTermination(string systemId)
        {
            lock (this.fileSync)
            {
                var theSafe = this.LoadStateFromDisk();

                var toDelete = theSafe.Instances.SingleOrDefault(_ => _.InstanceDescription.Id == systemId);
                if (toDelete != null)
                {
                    theSafe.Instances.Remove(toDelete);
                }

                this.SaveStateToDisk(theSafe);
            }
        }

        /// <inheritdoc />
        public void ProcessInstanceCreation(InstanceDescription instanceDescription)
        {
            lock (this.fileSync)
            {
                var theSafe = this.LoadStateFromDisk();

                var toUpdate =
                    theSafe.Instances.SingleOrDefault(
                        _ => _.InstanceCreationDetails.PrivateIpAddress == instanceDescription.PrivateIpAddress);
                if (toUpdate == null)
                {
                    throw new DeploymentException(
                        "Expected to find a tracked instance (pre-creation) with private IP: "
                        + instanceDescription.PrivateIpAddress);
                }

                toUpdate.InstanceDescription = instanceDescription;

                this.SaveStateToDisk(theSafe);
            }
        }

        /// <inheritdoc />
        public void ProcessDeployedPackage(string systemId, PackageDescription package)
        {
            lock (this.fileSync)
            {
                var theSafe = this.LoadStateFromDisk();

                var toUpdate =
                    theSafe.Instances.SingleOrDefault(
                        _ => _.InstanceDescription.Id == systemId);
                if (toUpdate == null)
                {
                    throw new DeploymentException(
                        "Expected to find a tracked instance (post-creation) with system ID: "
                        + systemId);
                }

                toUpdate.InstanceDescription.DeployedPackages.Add(package);

                this.SaveStateToDisk(theSafe);
            }
        }

        /// <inheritdoc />
        public InstanceDescription GetInstanceDescriptionById(string systemId)
        {
            lock (this.fileSync)
            {
                var theSafe = this.LoadStateFromDisk();

                var wrapped = theSafe.Instances.FirstOrDefault(_ => _.InstanceDescription.Id == systemId);

                return wrapped == null ? null : wrapped.InstanceDescription;
            }
        }

        /// <inheritdoc />
        public string GetInstanceIdByName(string name)
        {
            lock (this.fileSync)
            {
                var theSafe = this.LoadStateFromDisk();

                var wrapped = theSafe.Instances.FirstOrDefault(_ => _.InstanceDescription.Name == name);

                return wrapped == null ? null : wrapped.InstanceDescription.Id;
            }
        }

        /// <inheritdoc />
        public string GetPrivateKeyOfInstanceById(string systemId)
        {
            lock (this.fileSync)
            {
                var theSafe = this.LoadStateFromDisk();

                var wrapped = theSafe.Instances.FirstOrDefault(_ => _.InstanceDescription.Id == systemId);

                if (wrapped == null)
                {
                    return null;
                }

                var containerId = wrapped.InstanceCreationDetails.ContainerDetails.ContainerId;

                var container = theSafe.Containers.SingleOrDefault(_ => _.ContainerId == containerId);

                if (container == null)
                {
                    throw new DeploymentException("Could not find Container: " + containerId);
                }

                return container.PrivateKey;
            }
        }

        /// <inheritdoc />
        public string GetDomainZoneId(string domain)
        {
            lock (this.fileSync)
            {
                var theSafe = this.LoadStateFromDisk();

                string ret = null;
                var found = theSafe.RootDomainHostingIdMap.TryGetValue(domain, out ret);
                return found ? ret : null;
            }
        }

        /// <inheritdoc />
        public string GetInstancePrivateDnsRootDomain()
        {
            lock (this.fileSync)
            {
                var theSafe = this.LoadStateFromDisk();

                var ret = theSafe.InstancePrivateDnsRootDomain;
                return ret;
            }
        }

        /// <inheritdoc />
        public CertificateDetails GetCertificateByName(string name)
        {
            lock (this.fileSync)
            {
                var theSafe = this.LoadStateFromDisk();

                var certContainer = theSafe.Certificates.SingleOrDefault(_ => _.Name == name);
                var certDetails = certContainer == null ? null : certContainer.ToCertificateDetails();
                return certDetails;
            }
        }

        /// <inheritdoc />
        public InstanceCreationDetails GetNewInstanceCreationDetails(string environment, DeploymentConfiguration deploymentConfig)
        {
            lock (this.fileSync)
            {
                var theSafe = this.LoadStateFromDisk();

                var amiSearchPattern = theSafe.FindImageSearchPattern(environment, deploymentConfig);
                var privateIpAddress = theSafe.FindIpAddress(environment, deploymentConfig);
                var keyName = theSafe.FindKeyName(environment, deploymentConfig);
                var securityGroupId = theSafe.FindSecurityGroupId(environment, deploymentConfig);
                var location = theSafe.FindLocation(environment, deploymentConfig);
                var containerLocation = theSafe.FindContainerLocation(environment, deploymentConfig);
                var containerId = theSafe.FindContainerId(environment, deploymentConfig);

                var ret = new InstanceCreationDetails()
                              {
                                  DefaultDriveType = "gp2",
                                  ImageDetails =
                                      new ImageDetails()
                                          {
                                              OwnerAlias = "amazon",
                                              SearchPattern = amiSearchPattern,
                                              ShouldHaveSingleMatch = false,
                                          },
                                  PrivateIpAddress = privateIpAddress,
                                  KeyName = keyName,
                                  SecurityGroupId = securityGroupId,
                                  Location = location,
                                  ContainerDetails =
                                      new ContainerDetails()
                                          {
                                              ContainerId = containerId,
                                              ContainerLocation = containerLocation,
                                          },
                              };

                var newTracked = new InstanceWrapper()
                                     {
                                         InstanceDescription = new InstanceDescription()
                                                                   {
                                                                       Location = ret.Location,
                                                                       PrivateIpAddress = ret.PrivateIpAddress,
                                                                       DeployedPackages = new List<PackageDescription>(),
                                                                   },
                                         InstanceCreationDetails = ret,
                                         DeploymentConfig = deploymentConfig,
                                     };

                theSafe.Instances.Add(newTracked);

                this.SaveStateToDisk(theSafe);
                return ret;
            }
        }

        private TheSafe LoadStateFromDisk()
        {
            if (!File.Exists(this.filePath))
            {
                this.SaveStateToDisk(new TheSafe());
            }

            var raw = File.ReadAllText(this.filePath);
            var ret = Serializer.Deserialize<TheSafe>(raw);
            if (ret.Instances == null)
            {
                ret.Instances = new List<InstanceWrapper>();
            }

            return ret;
        }

        private void SaveStateToDisk(TheSafe theSafe)
        {
            var serialized = Serializer.Serialize(theSafe);
            File.WriteAllText(this.filePath, serialized);
        }
    }

    /// <summary>
    /// Container object for storing instances in tracking.
    /// </summary>
    public class InstanceWrapper
    {
        /// <summary>
        /// Gets or sets the related instance description.
        /// </summary>
        public InstanceDescription InstanceDescription { get; set; }

        /// <summary>
        /// Gets or sets the related instance details.
        /// </summary>
        public InstanceCreationDetails InstanceCreationDetails { get; set; }

        /// <summary>
        /// Gets or sets the related deployment configuration.
        /// </summary>
        public DeploymentConfiguration DeploymentConfig { get; set; }
    }
}

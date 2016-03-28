﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputingInfrastructureManagerSettings.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Domain
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Settings to be provided to the ComputingInfrastructureManager (instance type map, etc.).
    /// </summary>
    public class ComputingInfrastructureManagerSettings
    {
        /// <summary>
        /// Gets or sets a map of drive letters to AWS volume descriptors.
        /// </summary>
        public Dictionary<string, string> DriveLetterVolumeDescriptorMap { get; set; }

        /// <summary>
        /// Gets or sets the map of the volume type to the system specific values.
        /// </summary>
        public IDictionary<VolumeType, string> VolumeTypeValueMap { get; set; }

        /// <summary>
        /// Gets or sets a list (in order) of the AWS instance types and their core/RAM details.
        /// </summary>
        public ICollection<AwsInstanceType> AwsInstanceTypes { get; set; }

        /// <summary>
        /// Gets or sets a list (in order) of the AWS instance types and their core/RAM details to be used for SQL instances.
        /// </summary>
        public ICollection<AwsInstanceType> AwsInstanceTypesForSqlWeb { get; set; }

        /// <summary>
        /// Gets or sets a list (in order) of the AWS instance types and their core/RAM details to be used for SQL instances.
        /// </summary>
        public ICollection<AwsInstanceType> AwsInstanceTypesForSqlStandard { get; set; }

        /// <summary>
        /// Gets or sets the user data to use when creating an instance (list allows for keeping multiple lines in JSON format).
        /// </summary>
        public ICollection<string> InstanceCreationUserDataLines { get; set; }

        /// <summary>
        /// Gets or sets a list of package ID's that should be disregarded when looking to replace packages with instance terminations.
        /// </summary>
        public ICollection<string> PackageIdsToIgnoreDuringTerminationSearch { get; set; }

        /// <summary>
        /// Gets or sets the name of the key to use when tagging an instance with its name.
        /// </summary>
        public string NameTagKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the key to use when tagging an instance with its environment.
        /// </summary>
        public string EnvironmentTagKey { get; set; }

        /// <summary>
        /// Combines the lines of user data and replaces the token '{ComputerName}' with the name provided.
        /// </summary>
        /// <returns>User data as an un-encoded string to provide to AWS for creating an instance.</returns>
        public string GetInstanceCreationUserData()
        {
            var userData = string.Join(Environment.NewLine, this.InstanceCreationUserDataLines);
            return userData;
        }
    }

    /// <summary>
    /// Settings class with an AWS instance type and its core/RAM details.
    /// </summary>
    public class AwsInstanceType
    {
        /// <summary>
        /// Gets or sets the number of cores on the instance type.
        /// </summary>
        public int VirtualCores { get; set; }

        /// <summary>
        /// Gets or sets the amount of RAM on the instance type.
        /// </summary>
        public double RamInGb { get; set; }

        /// <summary>
        /// Gets or sets the AWS instance type descriptor.
        /// </summary>
        public string AwsInstanceTypeDescriptor { get; set; }
    }
}
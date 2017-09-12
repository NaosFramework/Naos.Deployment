﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeploymentConfiguration.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Naos.Packaging.Domain;

    using OBeautifulCode.Math;

    /// <summary>
    /// Model object with necessary details to deploy software to a machine.
    /// </summary>
    public class DeploymentConfiguration : IEquatable<DeploymentConfiguration>
    {
        /// <summary>
        /// Gets or sets the type of instance to deploy to.
        /// </summary>
        public InstanceType InstanceType { get; set; }

        /// <summary>
        /// Gets or sets the accessibility of the instance.
        /// </summary>
        public InstanceAccessibility InstanceAccessibility { get; set; }

        /// <summary>
        /// Gets or sets the number of instances to create with specified configuration.
        /// </summary>
        public int InstanceCount { get; set; }

        /// <summary>
        /// Gets or sets the volumes to add to the instance.
        /// </summary>
        public IReadOnlyCollection<Volume> Volumes { get; set; }

        /// <summary>
        /// Gets or sets the Chocolatey packages to install during the deployment.
        /// </summary>
        public IReadOnlyCollection<PackageDescription> ChocolateyPackages { get; set; }

        /// <summary>
        /// Gets or sets the deployment strategy to describe how certain things should be handled.
        /// </summary>
        public DeploymentStrategy DeploymentStrategy { get; set; }

        /// <summary>
        /// Gets or sets the post deployment strategy to describe any steps to perform when the deployment is finished.
        /// </summary>
        public PostDeploymentStrategy PostDeploymentStrategy { get; set; }

        /// <summary>
        /// Equal operator.
        /// </summary>
        /// <param name="first">Left item.</param>
        /// <param name="second">Right item.</param>
        /// <returns>Value indicating if equal.</returns>
        public static bool operator ==(DeploymentConfiguration first, DeploymentConfiguration second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            if (ReferenceEquals(first, null) || ReferenceEquals(second, null))
            {
                return false;
            }

            return (first.InstanceType == second.InstanceType) && (first.InstanceAccessibility == second.InstanceAccessibility) && (first.InstanceCount == second.InstanceCount) && (first.Volumes ?? new Volume[0]).SequenceEqual(second.Volumes ?? new Volume[0]) && (first.ChocolateyPackages ?? new PackageDescription[0]).SequenceEqual(second.ChocolateyPackages ?? new PackageDescription[0]) && (first.DeploymentStrategy == second.DeploymentStrategy) && (first.PostDeploymentStrategy == second.PostDeploymentStrategy);
        }

        /// <summary>
        /// Not equal operator.
        /// </summary>
        /// <param name="first">Left item.</param>
        /// <param name="second">Right item.</param>
        /// <returns>Value indicating if not equal.</returns>
        public static bool operator !=(DeploymentConfiguration first, DeploymentConfiguration second) => !(first == second);

        /// <inheritdoc />
        public bool Equals(DeploymentConfiguration other) => this == other;

        /// <inheritdoc />
        public override bool Equals(object obj) => this == (obj as DeploymentConfiguration);

        /// <inheritdoc />
        public override int GetHashCode() => HashCodeHelper.Initialize().Hash(this.InstanceType).Hash(this.InstanceAccessibility).Hash(this.InstanceCount).HashElements(this.Volumes).HashElements(this.ChocolateyPackages).Hash(this.DeploymentStrategy).Hash(this.PostDeploymentStrategy).Value;
    }
}

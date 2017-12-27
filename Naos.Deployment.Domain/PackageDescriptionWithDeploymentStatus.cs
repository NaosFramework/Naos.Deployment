﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageDescriptionWithDeploymentStatus.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Domain
{
    /// <summary>
    /// Package description that can contain a deployment status for use with tracking.
    /// </summary>
    public class PackageDescriptionWithDeploymentStatus : PackageDescriptionWithOverrides
    {
        /// <summary>
        /// Gets or sets the deployment status of the package.
        /// </summary>
        public PackageDeploymentStatus DeploymentStatus { get; set; }
    }
}
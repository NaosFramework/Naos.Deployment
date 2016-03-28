﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Enums.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Domain
{
    /// <summary>
    /// Enumeration of the types of deployed status of a package.
    /// </summary>
    public enum PackageDeploymentStatus
    {
        /// <summary>
        /// Nothing specified.
        /// </summary>
        Unknown,

        /// <summary>
        /// Package is known to not yet be deployed.
        /// </summary>
        NotYetDeployed,

        /// <summary>
        /// Package was deployed successfully.
        /// </summary>
        DeployedSuccessfully
    }

    /// <summary>
    /// Enumeration of the types of startup modes for IIS Application Pools.
    /// </summary>
    public enum ApplicationPoolStartMode
    {
        /// <summary>
        /// Nothing specified.
        /// </summary>
        None,

        /// <summary>
        /// IIS will manage as necessary with traffic.
        /// </summary>
        OnDemand,

        /// <summary>
        /// IIS will keep it running all the time.
        /// </summary>
        AlwaysRunning,
    }

    /// <summary>
    /// Enumeration of the type of access an instance needs.
    /// </summary>
    public enum InstanceAccessibility
    {
        /// <summary>
        /// Indicates that it's irrelevant to this deployment.
        /// </summary>
        DoesNotMatter = 0,

        /// <summary>
        /// Indicates accessible only inside the private network.
        /// </summary>
        Private = 1,

        /// <summary>
        /// Indicates accessible to the public internet.
        /// </summary>
        Public = 2,
    }

    /// <summary>
    /// Enumeration of the different SKU's of windows that are available.
    /// </summary>
    public enum WindowsSku
    {
        /// <summary>
        /// Indicates that it's irrelevant to this deployment.
        /// </summary>
        DoesNotMatter,

        /// <summary>
        /// Core SKU (without UI or SQL).
        /// </summary>
        Core,

        /// <summary>
        /// Base SKU (without SQL).
        /// </summary>
        Base,

        /// <summary>
        /// SQL Web SKU.
        /// </summary>
        SqlWeb,

        /// <summary>
        /// SQL Standard SKU.
        /// </summary>
        SqlStandard,

        /// <summary>
        /// SQL Enterprise SKU.
        /// </summary>
        SqlEnterprise,

        /// <summary>
        /// Unused because a specific image was supplied.
        /// </summary>
        SpecificImageSupplied,
    }

    /// <summary>
    /// Enumeration of the types of volumes that can be created.
    /// </summary>
    public enum VolumeType
    {
        /// <summary>
        /// Indicates that it's irrelevant to this deployment.
        /// </summary>
        DoesNotMatter,

        /// <summary>
        /// Low performance volume type.
        /// </summary>
        LowPerformance,

        /// <summary>
        /// Standard volume type.
        /// </summary>
        Standard,

        /// <summary>
        /// High performance volume type.
        /// </summary>
        HighPerformance
    }

    /// <summary>
    /// States an instance can be in.
    /// </summary>
    public enum InstanceState
    {
        /// <summary>
        /// Unknown state.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Pending state.
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Running state.
        /// </summary>
        Running = 2,

        /// <summary>
        /// Shutting down state.
        /// </summary>
        ShuttingDown = 3,

        /// <summary>
        /// Terminated state.
        /// </summary>
        Terminated = 4,

        /// <summary>
        /// Stopping state.
        /// </summary>
        Stopping = 5,

        /// <summary>
        /// Stopped state.
        /// </summary>
        Stopped = 6,
    }

    /// <summary>
    /// State a check can be in.
    /// </summary>
    public enum CheckState
    {
        /// <summary>
        /// Unknown state.
        /// </summary>
        Unknown,

        /// <summary>
        /// Initializing state.
        /// </summary>
        Initializing,

        /// <summary>
        /// Insufficient data state.
        /// </summary>
        InsufficientData,

        /// <summary>
        /// Failed state.
        /// </summary>
        Failed,

        /// <summary>
        /// Passed state.
        /// </summary>
        Passed
    }
}
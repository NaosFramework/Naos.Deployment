﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareInstanceTargeters.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.MessageBus.Contract
{
    using Naos.Deployment.Domain;
    using Naos.MessageBus.Domain;

    /// <summary>
    /// Interface to support sharing the object being used to target the instance
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Targeters", Justification = "Spelling/name is correct.")]
    public interface IShareInstanceTargeters : IShare
    {
        /// <summary>
        /// Gets or sets the targeter to find an instance.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Targeters", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Shares need to be arrays.")]
        InstanceTargeterBase[] InstanceTargeters { get; set; }
    }
}

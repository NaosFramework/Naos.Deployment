// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHaveInitializationStrategies.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for a collection of initialization strategies.
    /// </summary>
    public interface IHaveInitializationStrategies
    {
        /// <summary>
        /// Gets or sets the initialization strategies.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Keeping without constructor for now due to serialization issues.")]
        IReadOnlyCollection<InitializationStrategyBase> InitializationStrategies { get; set; }
    }
}
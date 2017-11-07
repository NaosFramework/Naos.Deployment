// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WellKnownConsoleVerb.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in Naos.Recipes.Console.Bootstrapper source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Console
{
    /// <summary>
    /// Well known verbs used in command line harness.
    /// </summary>
    public enum WellKnownConsoleVerb
    {
        /// <summary>
        /// Print help.
        /// </summary>
        Help,

        /// <summary>
        /// Log a message and exit gracefully.
        /// </summary>
        Pass,

        /// <summary>
        /// Simulate a failure by throwing a new exception.
        /// </summary>
        Fail,

        /// <summary>
        /// Monitor for items (usually in a message bus/work queue or self-hosted API context).
        /// </summary>
        Listen,

        /// <summary>
        /// Send an item (usually in a message bus/work queue or wrapped API context).
        /// </summary>
        Send,
    }
}
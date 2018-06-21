﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainFactory.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Diagnostics.Recipes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualBasic.Devices;

    using Naos.Diagnostics.Domain;

    /// <summary>
    /// Factory methods for domain objects.
    /// </summary>
#if NaosDiagnosticsRecipes
    public
#else
    [System.CodeDom.Compiler.GeneratedCode("Naos.Diagnostics", "See package version number")]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal
#endif
    static class DomainFactory
    {
        /// <summary>
        /// Samples the provided performance counter.
        /// </summary>
        /// <param name="description">Description to query with.</param>
        /// <returns>Description and sample in one object.</returns>
        public static PerformanceCounterSample Sample(this PerformanceCounterDescription description)
        {
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            var result = description.ToRecipe().Sample().ToModel();
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="OperatingSystemDetails"/> from executing context.
        /// </summary>
        /// <returns>New <see cref="OperatingSystemDetails"/>.</returns>
        public static OperatingSystemDetails CreateOperatingSystemDetails()
        {
            // this is only in VisualBasic...
            var computerInfo = new ComputerInfo();
            var servicePack =
                (string.IsNullOrEmpty(Environment.OSVersion.ServicePack) ? "(No Service Packs)" : Environment.OSVersion.ServicePack).Replace(
                    Environment.NewLine,
                    string.Empty);

            return new OperatingSystemDetails(computerInfo.OSFullName, Environment.OSVersion.Version, servicePack);
        }

        /// <summary>
        /// Creates a new <see cref="MachineDetails"/> from executing context.
        /// </summary>
        /// <returns>New <see cref="MachineDetails"/>.</returns>
        public static MachineDetails CreateMachineDetails()
        {
            var operatingSystemDetails = CreateOperatingSystemDetails();

            var memoryInGb = MachineMemory.GetMachineMemoryInGb();

            var report = new MachineDetails(
                MachineName.GetMachineNames().ToDictionary(_ => _.Key.ToString(), _ => _.Value),
                Environment.ProcessorCount,
                memoryInGb.ToDictionary(_ => _.Key.ToString(), _ => _.Value),
                Environment.Is64BitOperatingSystem,
                operatingSystemDetails,
                Environment.Version.ToString());

            return report;
        }

        /// <summary>
        /// Creates a new <see cref="ProcessDetails" /> from the executing context.
        /// </summary>
        /// <returns>New <see cref="ProcessDetails" />.</returns>
        public static ProcessDetails CreateProcessDetails()
        {
            var process = ProcessHelpers.GetRunningProcess();
            var result = new ProcessDetails(
                process.GetName(),
                process.GetFilePath(),
                process.GetFileVersion(),
                process.GetProductVersion(),
                ProcessHelpers.IsCurrentlyRunningAsAdmin());

            return result;
        }

        /// <summary>
        /// Extracts the correctly typed dictionary using the <see cref="MachineNameKind" /> key from <see cref="MachineDetails" />.
        /// </summary>
        /// <param name="machineDetails">Machine details to use.</param>
        /// <returns>Typed dictionary.</returns>
        public static IReadOnlyDictionary<MachineNameKind, string> GetTypedMachineNameKindToNameMap(this MachineDetails machineDetails)
        {
            if (machineDetails == null)
            {
                throw new ArgumentNullException(nameof(machineDetails));
            }

            var result = machineDetails.MachineNameKindToNameMap.ToDictionary(
                k => (MachineNameKind)Enum.Parse(typeof(MachineNameKind), k.Key),
                v => v.Value);

            return result;
        }

        /// <summary>
        /// Extracts the correctly typed dictionary using the <see cref="MachineMemoryKind" /> key from <see cref="MachineDetails" />.
        /// </summary>
        /// <param name="machineDetails">Machine details to use.</param>
        /// <returns>Typed dictionary.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Gb", Justification = "Name/spelling is correct.")]
        public static IReadOnlyDictionary<MachineMemoryKind, decimal> GetTypedMemoryKindToValueInGbMap(this MachineDetails machineDetails)
        {
            if (machineDetails == null)
            {
                throw new ArgumentNullException(nameof(machineDetails));
            }

            var result = machineDetails.MemoryKindToValueInGbMap.ToDictionary(
                k => (MachineMemoryKind)Enum.Parse(typeof(MachineMemoryKind), k.Key),
                v => v.Value);

            return result;
        }

        /// <summary>
        /// Converts from model to recipe.
        /// </summary>
        /// <param name="description">Model description.</param>
        /// <returns>Recipe description.</returns>
        public static RecipePerformanceCounterDescription ToRecipe(this PerformanceCounterDescription description)
        {
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            var result = new RecipePerformanceCounterDescription(description.CategoryName, description.CounterName, description.InstanceName, description.ExpectedMinValue, description.ExpectedMaxValue);
            return result;
        }

        /// <summary>
        /// Converts from model to recipe.
        /// </summary>
        /// <param name="description">Model description.</param>
        /// <returns>Recipe description.</returns>
        public static RecipePerformanceCounterDescription FromModel(this PerformanceCounterDescription description)
        {
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            return description.ToRecipe();
        }

        /// <summary>
        /// Converts from recipe to model.
        /// </summary>
        /// <param name="description">Recipe description.</param>
        /// <returns>Model description.</returns>
        public static PerformanceCounterDescription ToModel(this RecipePerformanceCounterDescription description)
        {
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            var result = new PerformanceCounterDescription(description.CategoryName, description.CounterName, description.InstanceName, description.ExpectedMinValue, description.ExpectedMaxValue);
            return result;
        }

        /// <summary>
        /// Converts from recipe to model.
        /// </summary>
        /// <param name="description">Recipe description.</param>
        /// <returns>Model description.</returns>
        public static PerformanceCounterDescription FromRecipe(this RecipePerformanceCounterDescription description)
        {
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            return description.ToModel();
        }

        /// <summary>
        /// Converts from model to recipe.
        /// </summary>
        /// <param name="sample">Model sample.</param>
        /// <returns>Recipe description.</returns>
        public static RecipePerformanceCounterSample ToRecipe(this PerformanceCounterSample sample)
        {
            if (sample == null)
            {
                throw new ArgumentNullException(nameof(sample));
            }

            var result = new RecipePerformanceCounterSample(sample.Description.ToRecipe(), sample.Value);
            return result;
        }

        /// <summary>
        /// Converts from model to recipe.
        /// </summary>
        /// <param name="sample">Model sample.</param>
        /// <returns>Recipe description.</returns>
        public static RecipePerformanceCounterSample FromModel(this PerformanceCounterSample sample)
        {
            if (sample == null)
            {
                throw new ArgumentNullException(nameof(sample));
            }

            return sample.ToRecipe();
        }

        /// <summary>
        /// Converts from recipe to model.
        /// </summary>
        /// <param name="sample">Recipe sample.</param>
        /// <returns>Model description.</returns>
        public static PerformanceCounterSample ToModel(this RecipePerformanceCounterSample sample)
        {
            if (sample == null)
            {
                throw new ArgumentNullException(nameof(sample));
            }

            var result = new PerformanceCounterSample(sample.Description.ToModel(), sample.Value);
            return result;
        }

        /// <summary>
        /// Converts from recipe to model.
        /// </summary>
        /// <param name="sample">Recipe sample.</param>
        /// <returns>Model description.</returns>
        public static PerformanceCounterSample FromRecipe(this RecipePerformanceCounterSample sample)
        {
            if (sample == null)
            {
                throw new ArgumentNullException(nameof(sample));
            }

            return sample.ToModel();
        }
    }
}
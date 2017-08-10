﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoStartProvider.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Domain
{
    using System;

    /// <summary>
    /// Model object to describe an Auto Start Provider in IIS
    /// </summary>
    public class AutoStartProvider : ICloneable
    {
        /// <summary>
        /// Gets or sets the name of the auto start provider.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the auto start provider (i.e. "MyNamespace.MyAutoStartProviderClass, MyAssembly").
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "This is the name I want.")]
        public string Type { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            var ret = new AutoStartProvider { Name = this.Name, Type = this.Type };
            return ret;
        }
    }
}
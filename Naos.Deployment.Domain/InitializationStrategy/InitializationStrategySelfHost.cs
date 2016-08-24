﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializationStrategySelfHost.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Custom extension of the DeploymentConfiguration to accommodate self host deployments.
    /// </summary>
    public class InitializationStrategySelfHost : InitializationStrategyBase
    {
        /// <summary>
        /// Gets or sets the name of the executable that is hosting.
        /// </summary>
        public string SelfHostExeName { get; set; }

        /// <summary>
        /// Gets or sets the DNS entries to support access of the self hosted deployment.
        /// </summary>
        public IReadOnlyCollection<string> SelfHostSupportedDnsEntries { get; set; }

        /// <summary>
        /// Gets or sets the name of the SSL certificate to lookup and use.
        /// </summary>
        public string SslCertificateName { get; set; }

        /// <summary>
        /// Gets or sets the account to configure the scheduled task that runs the executable.
        /// </summary>
        public string ScheduledTaskAccount { get; set; }

        /// <inheritdoc />
        public override object Clone()
        {
            var ret = new InitializationStrategySelfHost
                          {
                              SelfHostExeName = this.SelfHostExeName,
                              SslCertificateName = this.SslCertificateName,
                              SelfHostSupportedDnsEntries =
                                  this.SelfHostSupportedDnsEntries.Select(_ => _.Clone().ToString()).ToList(),
                              ScheduledTaskAccount = this.ScheduledTaskAccount
                          };

            return ret;
        }
    }
}

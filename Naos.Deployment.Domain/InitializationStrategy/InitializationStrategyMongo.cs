﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializationStrategyMongo.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Domain
{
    /// <summary>
    /// Custom extension of the InitializationStrategyBase to accommodate creating a mongo database.
    /// </summary>
    public class InitializationStrategyMongo : InitializationStrategyBase
    {
        /// <summary>
        /// Gets or sets name of the document database.
        /// </summary>
        public string DocumentDatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the administrator password to use.
        /// </summary>
        public string AdministratorPassword { get; set; }

        /// <summary>
        /// Gets or sets the directory to save data files in.
        /// </summary>
        public string DataDirectory { get; set; }

        /// <summary>
        /// Gets or sets the directory to save log files in.
        /// </summary>
        public string LogDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to disable journaling.
        /// </summary>
        public bool NoJournaling { get; set; }

        /// <inheritdoc />
        public override object Clone()
        {
            var ret = new InitializationStrategyMongo
                          {
                              DocumentDatabaseName = this.DocumentDatabaseName,
                              AdministratorPassword = this.AdministratorPassword,
                              DataDirectory = this.DataDirectory,
                              LogDirectory = this.LogDirectory,
                              NoJournaling = this.NoJournaling
                          };
            return ret;
        }
    }
}
﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializationStrategyDatabase.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Contract
{
    /// <summary>
    /// Custom extension of the DeploymentConfiguration to accommodate database deployments.
    /// </summary>
    public class InitializationStrategyDatabase : InitializationStrategyBase
    {
        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets restore information to apply prior to a migration.
        /// </summary>
        public DatabaseRestoreBase Restore { get; set; }

        /// <summary>
        /// Gets or sets the migration number to run the migration up to, null will skip.
        /// </summary>
        public long? Version { get; set; }

        /// <summary>
        /// Gets or sets the administrator password to use.
        /// </summary>
        public string AdministratorPassword { get; set; }

        /// <summary>
        /// Gets or sets the directory to save backup files in.
        /// </summary>
        public string BackupDirectory { get; set; }

        /// <summary>
        /// Gets or sets the directory to save data files in.
        /// </summary>
        public string DataDirectory { get; set; }

        /// <summary>
        /// Gets or sets the database settings to use.
        /// </summary>
        public DatabaseSettings DatabaseSettings { get; set; }
    }
}

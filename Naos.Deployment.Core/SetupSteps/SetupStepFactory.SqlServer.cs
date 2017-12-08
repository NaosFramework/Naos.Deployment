﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetupStepFactory.SqlServer.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Naos.Database.Domain;
    using Naos.Database.Migrator;
    using Naos.Database.SqlServer;
    using Naos.Deployment.Domain;
    using Naos.Packaging.Domain;

    using OBeautifulCode.Reflection.Recipes;

    using Spritely.Recipes;

    using static System.FormattableString;

    /// <summary>
    /// Factory to create a list of setup steps from various situations (abstraction to actual machine setup).
    /// </summary>
    internal partial class SetupStepFactory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Like it this way.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Like it this way.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom", Justification = "This pattern works better for correctly loading dependencies.")]
        private List<SetupStep> GetSqlServerSpecificSteps(InitializationStrategySqlServer sqlServerStrategy, Package package)
        {
            if (sqlServerStrategy.Create != null && sqlServerStrategy.Restore != null)
            {
                throw new NotSupportedException(
                    "A create and restore on a single database initialization strategy is not supported.");
            }

            var databaseSteps = new List<SetupStep>();
            var sqlServiceAccount = this.Settings.DatabaseServerSettings.SqlServiceAccount;

            var backupDirectory = sqlServerStrategy.BackupDirectory ?? this.Settings.DatabaseServerSettings.DefaultBackupDirectory;
            var createBackupDirScript = this.Settings.DeploymentScriptBlocks.CreateDirectoryWithFullControl;
            var createBackupDirParams = new[] { backupDirectory, sqlServiceAccount };
            databaseSteps.Add(
                new SetupStep
                    {
                        Description = "Create " + backupDirectory + " and grant rights to SQL service account.",
                        SetupFunc =
                            machineManager =>
                            machineManager.RunScript(createBackupDirScript.ScriptText, createBackupDirParams),
                    });
            var backupProcessAccount = this.Settings.DatabaseServerSettings.BackupProcessServiceAccount;
            var addBackupProcessAclsToBackupDirScript = this.Settings.DeploymentScriptBlocks.CreateDirectoryWithFullControl;
            var addBackupProcessAclsToBackupDirParams = new[] { backupDirectory, backupProcessAccount };
            databaseSteps.Add(
                new SetupStep
                    {
                        Description = "Add rights to " + backupDirectory + " for backup process account.",
                        SetupFunc =
                            machineManager =>
                            machineManager.RunScript(addBackupProcessAclsToBackupDirScript.ScriptText, addBackupProcessAclsToBackupDirParams),
                    });

            var dataDirectory = sqlServerStrategy.DataDirectory ?? this.Settings.DatabaseServerSettings.DefaultDataDirectory;
            var createDatabaseDirScript = this.Settings.DeploymentScriptBlocks.CreateDirectoryWithFullControl;
            var createDatabaseDirParams = new[] { dataDirectory, sqlServiceAccount };
            databaseSteps.Add(
                new SetupStep
                {
                    Description = "Create " + dataDirectory + " and grant rights to SQL service account.",
                    SetupFunc =
                        machineManager =>
                        machineManager.RunScript(createDatabaseDirScript.ScriptText, createDatabaseDirParams),
                });

            var enableSaSetPasswordScript = this.Settings.DeploymentScriptBlocks.EnableSaAccountAndSetPassword;
            var enableSaSetPasswordParams = new[] { sqlServerStrategy.AdministratorPassword };
            databaseSteps.Add(
                new SetupStep
                {
                    Description = "Turn on Mixed Mode Auth, enable SA account, and set password.",
                    SetupFunc =
                        machineManager =>
                        machineManager.RunScript(enableSaSetPasswordScript.ScriptText, enableSaSetPasswordParams),
                });

            var updateDefaultInstancePathsScript = this.Settings.DeploymentScriptBlocks.SetDefaultDirectories;
            var updateDefaultInstancePathsParams = new[] { dataDirectory, dataDirectory, backupDirectory };

            databaseSteps.Add(
                new SetupStep
                {
                    Description = "Update default instance paths on database.",
                    SetupFunc =
                        machineManager =>
                        machineManager.RunScript(updateDefaultInstancePathsScript.ScriptText, updateDefaultInstancePathsParams),
                });

            var restartSqlServerScript = this.Settings.DeploymentScriptBlocks.RestartWindowsService;
            var restartSqlServerParams = new[] { this.Settings.DatabaseServerSettings.SqlServiceName };
            databaseSteps.Add(
                new SetupStep
                {
                    Description = "Restart SQL server for account change(s) to take effect.",
                    SetupFunc =
                        machineManager =>
                        machineManager.RunScript(restartSqlServerScript.ScriptText, restartSqlServerParams),
                });

            var connectionString = sqlServerStrategy.CreateLocalhostConnectionString();
            var databaseConfigurationForCreation = this.BuildDatabaseConfiguration(
                sqlServerStrategy.Name,
                dataDirectory,
                sqlServerStrategy.RecoveryMode,
                sqlServerStrategy.Create?.DatabaseFileNameSettings,
                sqlServerStrategy.Create?.DatabaseFileSizeSettings);

            databaseSteps.Add(
                new SetupStep
                    {
                        Description = Invariant($"Create database: {sqlServerStrategy.Name} on instance {sqlServerStrategy.InstanceName ?? "DEFAULT"}"),
                        SetupFunc = machineManager =>
                            {
                                var realRemoteConnectionString = connectionString.Replace("localhost", machineManager.IpAddress);
                                SqlServerDatabaseManager.Create(realRemoteConnectionString, databaseConfigurationForCreation);
                                return new dynamic[0];
                            },
                    });

            if (sqlServerStrategy.Restore != null)
            {
                var awsRestore = sqlServerStrategy.Restore as DatabaseRestoreFromS3;
                if (awsRestore == null)
                {
                    throw new NotSupportedException("Currently no support for type of database restore: " + sqlServerStrategy.Restore.GetType());
                }

                var databaseConfigurationForRestore = this.BuildDatabaseConfiguration(
                    sqlServerStrategy.Name,
                    dataDirectory,
                    sqlServerStrategy.RecoveryMode,
                    sqlServerStrategy.Restore?.DatabaseFileNameSettings,
                    sqlServerStrategy.Restore?.DatabaseFileSizeSettings);

                databaseSteps.Add(
                    new SetupStep
                        {
                            Description = Invariant($"Restore - Region: {awsRestore.Region}; Bucket: {awsRestore.BucketName}; File: {awsRestore.FileName}"),
                            SetupFunc = machineManager =>
                                {
                                    var restoreFilePath = Path.Combine(sqlServerStrategy.BackupDirectory, awsRestore.FileName);

                                    var remoteDownloadBackupScriptBlock = this.Settings.DeploymentScriptBlocks.DownloadS3Object.ScriptText;
                                    var remoteDownloadBackupScriptParams = new[]
                                                                               {
                                                                                   awsRestore.BucketName, awsRestore.FileName, restoreFilePath, awsRestore.Region,
                                                                                   awsRestore.DownloadAccessKey, awsRestore.DownloadSecretKey,
                                                                               };

                                    machineManager.RunScript(remoteDownloadBackupScriptBlock, remoteDownloadBackupScriptParams);
                                    var realRemoteConnectionString = connectionString.Replace("localhost", machineManager.IpAddress);

                                    var restoreFileUri = new Uri(restoreFilePath);
                                    var checksumOption = awsRestore.RunChecksum ? ChecksumOption.Checksum : ChecksumOption.NoChecksum;
                                    var restoreDetails = new RestoreDetails
                                                             {
                                                                 ChecksumOption = checksumOption,
                                                                 Device = Device.Disk,
                                                                 ErrorHandling = ErrorHandling.StopOnError,
                                                                 DataFilePath = databaseConfigurationForRestore.DataFilePath,
                                                                 LogFilePath = databaseConfigurationForRestore.LogFilePath,
                                                                 RecoveryOption = RecoveryOption.NoRecovery,
                                                                 ReplaceOption = ReplaceOption.ReplaceExistingDatabase,
                                                                 RestoreFrom = restoreFileUri,
                                                                 RestrictedUserOption = RestrictedUserOption.Normal,
                                                             };
                                    SqlServerDatabaseManager.RestoreFull(realRemoteConnectionString, sqlServerStrategy.Name, restoreDetails);
                                    return new dynamic[0];
                                },
                        });
            }

            if (sqlServerStrategy.Migration != null)
            {
                var fluentMigration = sqlServerStrategy.Migration as DatabaseMigrationFluentMigrator;
                if (fluentMigration == null)
                {
                    throw new NotSupportedException(
                        "Currently no support for type of database migration: " + sqlServerStrategy.Migration.GetType());
                }

                databaseSteps.Add(
                    new SetupStep
                        {
                            Description = "Run Database Fluent Migration to Version: " + fluentMigration.Version,
                            SetupFunc = machineManager =>
                                {
                                    var realRemoteConnectionString = connectionString.Replace("localhost", machineManager.IpAddress);

                                    var workingPath = Path.Combine(this.workingDirectory, "DeployMigration-" + Guid.NewGuid().ToString().Substring(0, 4));
                                    this.packageManager.DownloadPackages(new[] { package.PackageDescription }, workingPath, true);

                                    var allFilePaths = Directory.GetFiles(workingPath, "*", SearchOption.AllDirectories);
                                    var migrationAssemblyFilePath =
                                        allFilePaths.Where(_ => Path.GetFileNameWithoutExtension(_) == package.PackageDescription.Id)
                                            .SingleOrDefault(_ => _.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase) || _.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase));

                                    new { migrationAssemblyFilePath }.Must()
                                        .NotBeNull()
                                        .Because(Invariant($"Needs assembly named for package ID: {package.PackageDescription.Id} in downloaded path: {workingPath}"))
                                        .OrThrow();

                                    // Need to run loose because FluentMigrator doesn't play nice...
                                    using (var loader = AssemblyLoader.CreateAndLoadFromDirectory(
                                        workingPath,
                                        suppressBadImageFormatException: true,
                                        suppressFileLoadException: true))
                                    {
                                        var assembly = loader.FilePathToAssemblyMap[migrationAssemblyFilePath];
                                        MigrationExecutor.Up(
                                            assembly,
                                            realRemoteConnectionString,
                                            sqlServerStrategy.Name,
                                            fluentMigration.Version,
                                            this.debugAnnouncer);
                                    }

                                    return new dynamic[0];
                                },
                        });
            }

            return databaseSteps;
        }

        private DatabaseConfiguration BuildDatabaseConfiguration(string databaseName, string dataDirectory, string recoveryMode, DatabaseFileNameSettings databaseFileNameSettings, DatabaseFileSizeSettings databaseFileSizeSettings)
        {
            var localDatabaseFileNameSettings = databaseFileNameSettings
                                                ?? new DatabaseFileNameSettings
                                                {
                                                    DataFileLogicalName = databaseName + "Dat",
                                                    DataFileNameOnDisk = databaseName + ".mdf",
                                                    LogFileLogicalName = databaseName + "Log",
                                                    LogFileNameOnDisk = databaseName + ".log",
                                                };

            var localDatabaseFileSizeSettings = databaseFileSizeSettings
                                                ?? this.Settings.DefaultDatabaseFileSizeSettings;

            var recoveryModeEnum = RecoveryMode.Unspecified;
            if (!string.IsNullOrEmpty(recoveryMode))
            {
                recoveryModeEnum = (RecoveryMode)Enum.Parse(typeof(RecoveryMode), recoveryMode, true);
            }

            var databaseConfiguration = new DatabaseConfiguration
            {
                DatabaseName = databaseName,
                DatabaseType = DatabaseType.User,
                RecoveryMode = recoveryModeEnum,
                DataFileLogicalName = localDatabaseFileNameSettings.DataFileLogicalName,
                DataFilePath = Path.Combine(dataDirectory, localDatabaseFileNameSettings.DataFileNameOnDisk),
                DataFileCurrentSizeInKb = localDatabaseFileSizeSettings.DataFileCurrentSizeInKb,
                DataFileMaxSizeInKb = localDatabaseFileSizeSettings.DataFileMaxSizeInKb,
                DataFileGrowthSizeInKb = localDatabaseFileSizeSettings.DataFileGrowthSizeInKb,
                LogFileLogicalName = localDatabaseFileNameSettings.LogFileLogicalName,
                LogFilePath = Path.Combine(dataDirectory, localDatabaseFileNameSettings.LogFileNameOnDisk),
                LogFileCurrentSizeInKb = localDatabaseFileSizeSettings.LogFileCurrentSizeInKb,
                LogFileMaxSizeInKb = localDatabaseFileSizeSettings.LogFileMaxSizeInKb,
                LogFileGrowthSizeInKb = localDatabaseFileSizeSettings.LogFileGrowthSizeInKb,
            };

            return databaseConfiguration;
        }
    }
}

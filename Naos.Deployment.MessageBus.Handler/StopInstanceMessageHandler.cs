﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StopInstanceMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.MessageBus.Handler
{
    using System;
    using System.Threading.Tasks;

    using Its.Configuration;
    using Its.Log.Instrumentation;

    using Naos.Deployment.CloudManagement;
    using Naos.Deployment.Contract;
    using Naos.Deployment.MessageBus.Contract;
    using Naos.MessageBus.HandlingContract;

    /// <summary>
    /// Handler for stop instance messages.
    /// </summary>
    public class StopInstanceMessageHandler : IHandleMessages<StopInstanceMessage>, IShareInstanceTargeter
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public InstanceTargeterBase InstanceTargeter { get; set; }

        /// <inheritdoc />
        public async Task HandleAsync(StopInstanceMessage message)
        {
            var settings = Settings.Get<DeploymentMessageHandlerSettings>();
            var cloudInfrastructureManagerSettings = Settings.Get<CloudInfrastructureManagerSettings>();
            await Task.Run(() => this.Handle(message, settings, cloudInfrastructureManagerSettings));
        }

        /// <summary>
        /// Handle a stop instance message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="settings">Settings necessary to handle the message.</param>
        /// <param name="cloudInfrastructureManagerSettings">Settings for the cloud infrastructure manager.</param>
        public void Handle(StopInstanceMessage message, DeploymentMessageHandlerSettings settings, CloudInfrastructureManagerSettings cloudInfrastructureManagerSettings)
        {
            if (message == null)
            {
                throw new ArgumentException("Cannot have a null message.");
            }

            if (message.InstanceTargeter == null)
            {
                throw new ArgumentException("Must specify instance targeter to use for specifying an instance.");
            }

            var cloudManager = CloudManagerHelper.CreateCloudManager(settings, cloudInfrastructureManagerSettings);
            var task = Task.Run(() => CloudManagerHelper.GetSystemIdFromTargeterAsync(message.InstanceTargeter, settings, cloudManager));
            task.Wait();
            var systemId = task.Result;

            Log.Write(() => new { Info = "Stopping Instance", MessageJson = Serializer.Serialize(message), SystemId = systemId });
            Task.Run(() => cloudManager.TurnOffInstanceAsync(systemId, settings.SystemLocation, message.WaitUntilOff)).Wait();

            this.InstanceTargeter = message.InstanceTargeter;
        }
    }
}

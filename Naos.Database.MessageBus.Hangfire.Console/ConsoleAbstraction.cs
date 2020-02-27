// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultMessageBusCommandLineAbstraction.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in Naos.Recipes.MessageBus.Hangfire.Bootstrapper source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Database.MessageBus.Hangfire.Console
{
    using CLAP;

    using Naos.Cron;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Hangfire.Bootstrapper;

    using Naos.Configuration.Domain;
    using Naos.Deployment.Domain;
    using OBeautifulCode.Serialization;
    using OBeautifulCode.Serialization.Json;

    /// <summary>
    /// Abstraction for use with <see cref="CLAP" /> to provide basic command line interaction.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "Cannot be static for command line contract.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
#if !NaosMessageBusHangfireConsole
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCode("Naos.Recipes.MessageBus.Hangfire.Bootstrapper", "See package version number")]
#endif
    public partial class ConsoleAbstraction : ConsoleAbstractionBase
    {        /// <summary>
             /// Monitor for items in Hangfire.
             /// </summary>
             /// <param name="debug">Optional indication to launch the debugger from inside the application (default is false).</param>
             /// <param name="environment">Optional value to use when setting the Its.Configuration precedence to use specific settings.</param>
        [Verb(Aliases = nameof(WellKnownConsoleVerb.Monitor), IsDefault = false, Description = "Runs the Hangfire Harness listening on configured channels until it's triggered to end or fails;\r\n            example usage: [Harness].exe monitor\r\n                           [Harness].exe monitor /debug=true\r\n                           [Harness].exe monitor /environment=ExampleDevelopment\r\n                           [Harness].exe monitor /environment=ExampleDevelopment /debug=true\r\n")]
        public static void Monitor(
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("")] [Description("Sets the Its.Configuration precedence to use specific settings.")] [DefaultValue(null)] string environment)
        {
            /*---------------------------------------------------------------------------*
             * Any method should run this logic to debug, setup config & logging, etc.   *
             *---------------------------------------------------------------------------*/
            CommonSetup(debug, environment);

            /*---------------------------------------------------------------------------*
             * Any method should run this logic to write telemetry info to the log.      *
             *---------------------------------------------------------------------------*/
            WriteStandardTelemetry();

            /*---------------------------------------------------------------------------*
             * Necessary configuration.                                                *
             *---------------------------------------------------------------------------*/
            var messageBusConnectionConfiguration = Config.Get<MessageBusConnectionConfiguration>(typeof(MessageBusJsonConfiguration));
            var messageBusLaunchConfig = Config.Get<MessageBusLaunchConfiguration>(typeof(MessageBusJsonConfiguration));

            /*---------------------------------------------------------------------------*
             * Launch the harness here, it will run until the TimeToLive has expired AND *
             * there are no active messages being handled or if there is an internal     *
             * error.  Failed message handling is logged and does not crash the harness. *
             *---------------------------------------------------------------------------*/
            using (var handlerBuilder = HandlerFactory.Build())
            {
                HangfireHarnessManager.Launch(messageBusConnectionConfiguration, messageBusLaunchConfig, handlerBuilder);
            }
        }

        /// <summary>
        /// Main entry point of the application; if no exceptions are thrown then the exit code will be 0.
        /// </summary>
        /// <param name="parcelJson">Parcel to send as JSON.</param>
        /// <param name="scheduleJson">Optional recurring schedule as JSON; default will be a single send and NOT recurring.</param>
        /// <param name="debug">Optional indication to launch the debugger from inside the application (default is false).</param>
        /// <param name="environment">Optional value to use when setting the Its.Configuration precedence to use specific settings.</param>
        [Verb(Aliases = nameof(WellKnownConsoleVerb.Send), IsDefault = false, Description = "Runs the Hangfire Harness listening on configured channels until it's triggered to end or fails;\r\n            example usage: [Harness].exe listen\r\n                           [Harness].exe listen /debug=true\r\n                           [Harness].exe listen /environment=ExampleDevelopment\r\n                           [Harness].exe listen /environment=ExampleDevelopment /debug=true\r\n")]
        public static void Send(
            [Aliases("")] [Required] [Description("Parcel to send as JSON.")] string parcelJson,
            [Aliases("")] [Description("Optional recurring schedule as JSON.")] [DefaultValue(null)] string scheduleJson,
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("")] [Description("Sets the Its.Configuration precedence to use specific settings.")] [DefaultValue(null)] string environment)
        {
            /*---------------------------------------------------------------------------*
             * Any method should run this logic to debug, setup config & logging, etc.   *
             *---------------------------------------------------------------------------*/
            CommonSetup(debug, environment);

            /*---------------------------------------------------------------------------*
             * Necessary configuration.                                                *
             *---------------------------------------------------------------------------*/
            var serializer = new ObcJsonSerializer(typeof(MessageBusJsonConfiguration), UnregisteredTypeEncounteredStrategy.Attempt);
            var parcel = (Parcel)serializer.Deserialize(parcelJson, typeof(Parcel));
            var schedule = string.IsNullOrWhiteSpace(scheduleJson) ? null : (ScheduleBase)serializer.Deserialize(scheduleJson, typeof(ScheduleBase));
            var messageBusConnectionConfiguration = Config.Get<MessageBusConnectionConfiguration>(typeof(MessageBusJsonConfiguration));
            var messageBusLaunchConfig = Config.Get<MessageBusLaunchConfiguration>(typeof(MessageBusJsonConfiguration));

            /*---------------------------------------------------------------------------*
             * Send the parcel here.                                                     *
             *---------------------------------------------------------------------------*/
            HangfireHarnessManager.Send(messageBusConnectionConfiguration, messageBusLaunchConfig.TypeMatchStrategyForMessageResolution, parcel, schedule);
        }
    }
}

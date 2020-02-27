// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HandlerFactory.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in Naos.MessageBus.Hangfire.Bootstrapper source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

#if NaosMessageBusHangfireConsole
namespace Naos.MessageBus.Hangfire.Console
#else
namespace Naos.Database.MessageBus.Hangfire.Console
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using Its.Log.Instrumentation;

	using Naos.Configuration.Domain;
    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;

    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Type;
    using OBeautifulCode.Assertion.Recipes;

    using static System.FormattableString;

    /// <summary>
    /// Factory builder to provide logic to resolve the appropriate <see cref="IHandleMessages" /> for a dispatched <see cref="IMessage" /> implementation.
    /// </summary>
#if !NaosMessageBusHangfireConsole
    [System.Diagnostics.DebuggerStepThrough]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCode("Naos.MessageBus.Hangfire.Bootstrapper", "See package version number")]
#endif
    public static partial class HandlerFactory
    {
        /// <summary>
        /// Build the appropriate <see cref="IHandlerFactory" /> to use.
        /// </summary>
        /// <returns>Factory to use.</returns>
        internal static IHandlerFactory Build()
        {
            var localDictionary = new Dictionary<Type, Type>();

            var configuredEntires = MessageTypeToHandlerTypeMap?.ToList() ?? new List<KeyValuePair<Type, Type>>();

            IHandlerFactory ret;
            if (configuredEntires.Count != 0 && !(configuredEntires.Count == 1 && configuredEntires.Single().Key == typeof(ExampleMessage)))
            {
                configuredEntires.ForEach(
                    _ =>
                        {
                            if (!localDictionary.ContainsKey(_.Key))
                            {
                                localDictionary.Add(_.Key, _.Value);
                            }
                        });

                ret = new MappedTypeHandlerFactory(MessageTypeToHandlerTypeMap, TypeMatchStrategy.NamespaceAndName);
            }
            else
            {
                ret = BuildReflectionHandlerFactoryFromSettings();
            }

            return ret;
        }

        /// <summary>
        /// Discover all implementations of <see cref="MessageHandlerBase{T}" /> and put them in the map with the specified message type.
        /// </summary>
        /// <param name="assembliesToLookIn">List of assemblies to look in.</param>
        /// <param name="includeInternalHandlers">Optional value indicating whether to include the internal handlers; DEFAULT is true.</param>
        /// <returns>Map of message type to handler type.</returns>
        internal static IReadOnlyDictionary<Type, Type> DiscoverHandlersInAssemblies(IReadOnlyCollection<Assembly> assembliesToLookIn, bool includeInternalHandlers = true)
        {
            new { assembliesToLookIn }.AsArg().Must().NotBeNullNorEmptyEnumerableNorContainAnyNulls();

            var localDictionary = new Dictionary<Type, Type>();

            // Add the MessageBus.Domain assembly to get basic included handlers as well as the MessageBus.Core assembly for more specialized included handlers.
            var internalAssembliesToAdd = includeInternalHandlers ? new[] { typeof(IMessage).Assembly, typeof(MessageDispatcher).Assembly } : new Assembly[0];
            var assembliesToLoad = assembliesToLookIn.Concat(internalAssembliesToAdd).Distinct().ToList();

            ReflectionHandlerFactory.LoadHandlerTypeMapFromAssemblies(localDictionary, assembliesToLoad);

            return localDictionary;
        }

        private static IHandlerFactory BuildReflectionHandlerFactoryFromSettings()
        {
            var configuration = Config.Get<HandlerFactoryConfiguration>(typeof(MessageBusJsonConfiguration));

            new { configuration }.AsArg().Must().NotBeNull();

            var ret = !string.IsNullOrWhiteSpace(configuration.HandlerAssemblyPath)
                          ? new ReflectionHandlerFactory(configuration.HandlerAssemblyPath, configuration.TypeMatchStrategyForMessageResolution)
                          : new ReflectionHandlerFactory(configuration.TypeMatchStrategyForMessageResolution);

            return ret;
        }
    }

    /// <summary>
    /// Example of an <see cref="IMessage" />.
    /// </summary>
    public class ExampleMessage : IMessage
    {
        /// <inheritdoc cref="IMessage" />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets an example of a group of work to process.
        /// </summary>
        public string GroupToProcess { get; set; }
    }

    /// <summary>
    /// Handler for <see cref="ExampleMessage" />.
    /// </summary>
    public class ExampleMessageHandler : MessageHandlerBase<ExampleMessage>
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(ExampleMessage message)
        {
            await Task.Run(() => { });

            Log.Write(() => Invariant($"Finished processing group: {message.GroupToProcess}"));
        }
    }
}

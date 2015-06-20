﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializerTest.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core.Test
{
    using System.Linq;

    using Naos.Deployment.Contract;

    using Xunit;

    public class SerializerTest
    {
        [Fact]
        public static void Deserialize_SingleMessageBusHandlerInitStrategy_Valid()
        {
            var input = @"
{
	""InstanceType"": { ""VirtualCores"": 2, ""RamInGb"": 3, },
	""IsPubliclyAccessible"": false,
	""Volumes"": [{
		""DriveLetter"": ""C"",
		""SizeInGb"": ""50"",
	}],
	""InitializationStrategies"": [{
		""ChannelsToMonitor"": [""MyChannel""],
	}],
}
";

            var deserialized = Serializer.Deserialize<DeploymentConfigurationWithStrategies>(input);

            Assert.Equal(typeof(InitializationStrategyMessageBusHandler), deserialized.InitializationStrategies.Single().GetType());
            Assert.Equal("MyChannel", deserialized.InitializationStrategies.Cast<InitializationStrategyMessageBusHandler>().Single().ChannelsToMonitor.Single());
        }

        [Fact]
        public static void Deserialize_SingleDatabaseInitStrategy_Valid()
        {
            var input = @"
{
	""InstanceType"": { ""VirtualCores"": 2, ""RamInGb"": 3, },
	""IsPubliclyAccessible"": false,
	""Volumes"": [{
		""DriveLetter"": ""C"",
		""SizeInGb"": ""50"",
	}],
	""InitializationStrategies"": [{
		""Name"": ""Monkey"",
		""AdministratorPassword"": ""MyPassWord1234"",
	}],
}
";

            var deserialized = Serializer.Deserialize<DeploymentConfigurationWithStrategies>(input);

            Assert.Equal(typeof(InitializationStrategyDatabase), deserialized.InitializationStrategies.Single().GetType());
            Assert.Equal("Monkey", deserialized.InitializationStrategies.Cast<InitializationStrategyDatabase>().Single().Name);
        }

        [Fact]
        public static void Deserialize_SingleWebInitStrategy_Valid()
        {
            var input = @"
{
	""InstanceType"": { ""VirtualCores"": 2, ""RamInGb"": 3, },
	""IsPubliclyAccessible"": false,
	""Volumes"": [{
		""DriveLetter"": ""C"",
		""SizeInGb"": ""50"",
	}],
	""InitializationStrategies"": [{
		""PrimaryDns"": ""reports.coopmetrics.coop"",
	}],
}
";

            var deserialized = Serializer.Deserialize<DeploymentConfigurationWithStrategies>(input);

            Assert.Equal(typeof(InitializationStrategyWeb), deserialized.InitializationStrategies.Single().GetType());
            Assert.Equal("reports.coopmetrics.coop", deserialized.InitializationStrategies.Cast<InitializationStrategyWeb>().Single().PrimaryDns);
        }
    }
}

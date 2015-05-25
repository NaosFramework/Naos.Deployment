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
        public static void Deserialize_SingleConsoleInitStrategy_Valid()
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
		""Arguments"": ""/go"",
	}],
}
";

            var deserialized = Serializer.Deserialize<DeploymentConfiguration>(input);

            Assert.Equal(typeof(InitializationStrategyConsole), deserialized.InitializationStrategies.Single().GetType());
            Assert.Equal("/go", deserialized.InitializationStrategies.Cast<InitializationStrategyConsole>().Single().Arguments);
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
		""DatabaseName"": ""Monkey"",
	}],
}
";

            var deserialized = Serializer.Deserialize<DeploymentConfiguration>(input);

            Assert.Equal(typeof(InitializationStrategyDatabase), deserialized.InitializationStrategies.Single().GetType());
            Assert.Equal("Monkey", deserialized.InitializationStrategies.Cast<InitializationStrategyDatabase>().Single().DatabaseName);
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

            var deserialized = Serializer.Deserialize<DeploymentConfiguration>(input);

            Assert.Equal(typeof(InitializationStrategyWeb), deserialized.InitializationStrategies.Single().GetType());
            Assert.Equal("reports.coopmetrics.coop", deserialized.InitializationStrategies.Cast<InitializationStrategyWeb>().Single().PrimaryDns);
        }
    }
}

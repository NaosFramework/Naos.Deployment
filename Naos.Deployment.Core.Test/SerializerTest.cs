﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializerTest.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core.Test
{
    using System.Collections.Generic;
    using System.Linq;

    using Naos.Deployment.Contract;

    using Xunit;

    public class SerializerTest
    {
        [Fact]
        public static void Deserialize_PrivateDnsEntry_Valid()
        {
            var input = @"
[{
	""id"": ""Naos.Something"",
	""initializationStrategies"": [{
		""privateDnsEntry"": ""something.database.development.cometrics.com""
	}]
}]";
            var deserialized = Serializer.Deserialize<ICollection<PackageDescriptionWithOverrides>>(input);

            Assert.NotNull(deserialized);
            var actualDns =
                deserialized.Single()
                    .InitializationStrategies.OfType<InitializationStrategyPrivateDnsEntry>()
                    .Single()
                    .PrivateDnsEntry;
            Assert.Equal("something.database.development.cometrics.com", actualDns);
        }

        [Fact]
        public static void Deserialize_DirectoryToCreate_Valid()
        {
            var input = @"
[{
	""id"": ""Naos.Something"",
	""initializationStrategies"": [{
		""directoryToCreate"": {""fullPath"": ""C:\\MyPath\\Is\\Here"", ""FullControlAccount"": ""Administrator"" }
	}]
}]";
            var deserialized = Serializer.Deserialize<ICollection<PackageDescriptionWithOverrides>>(input);

            Assert.NotNull(deserialized);
            var actualPath =
                deserialized.Single()
                    .InitializationStrategies.OfType<InitializationStrategyDirectoryToCreate>()
                    .Single()
                    .DirectoryToCreate.FullPath;
            Assert.Equal("C:\\MyPath\\Is\\Here", actualPath);
            var actualAccount = deserialized.Single()
                .InitializationStrategies.OfType<InitializationStrategyDirectoryToCreate>()
                .Single()
                .DirectoryToCreate.FullControlAccount;
            Assert.Equal("Administrator", actualAccount);
        }

        [Fact]
        public static void Deserialize_CertificateToInstall_Valid()
        {
            var input = @"
[{
	""id"": ""Naos.Something"",
	""initializationStrategies"": [{
		""certificateToInstall"": ""ThisIsTheNameOfTheCertInCertRetriever...""
	}]
}]";
            var deserialized = Serializer.Deserialize<ICollection<PackageDescriptionWithOverrides>>(input);

            Assert.NotNull(deserialized);
            var actualCert = deserialized.Single()
                .InitializationStrategies.OfType<InitializationStrategyCertificateToInstall>()
                .Single()
                .CertificateToInstall;
            Assert.Equal("ThisIsTheNameOfTheCertInCertRetriever...", actualCert);
        }

        [Fact]
        public static void Deserialize_SingleMessageBusHandlerInitStrategy_Valid()
        {
            var input = @"
{
	""instanceType"": { ""VirtualCores"": 2, ""RamInGb"": 3, },
	""isPubliclyAccessible"": false,
	""volumes"": [{
		""driveLetter"": ""C"",
		""sizeInGb"": ""50"",
	}],
	""initializationStrategies"": [{
		""channelsToMonitor"": [{""Name"":""MyChannel""}],
	}],
}
";

            var deserialized = Serializer.Deserialize<DeploymentConfigurationWithStrategies>(input);

            Assert.Equal(typeof(InitializationStrategyMessageBusHandler), deserialized.InitializationStrategies.Single().GetType());
            Assert.Equal("MyChannel", deserialized.InitializationStrategies.Cast<InitializationStrategyMessageBusHandler>().Single().ChannelsToMonitor.Single().Name);
        }

        [Fact]
        public static void Deserialize_SingleDatabaseInitStrategy_Valid()
        {
            var input = @"
{
	""instanceType"": { ""VirtualCores"": 2, ""RamInGb"": 3, },
	""isPubliclyAccessible"": false,
	""volumes"": [{
		""driveLetter"": ""C"",
		""sizeInGb"": ""50"",
	}],
	""initializationStrategies"": [{
		""name"": ""Monkey"",
		""administratorPassword"": ""MyPassWord1234"",
	}],
}
";

            var deserialized = Serializer.Deserialize<DeploymentConfigurationWithStrategies>(input);

            Assert.Equal(typeof(InitializationStrategySqlServer), deserialized.InitializationStrategies.Single().GetType());
            Assert.Equal("Monkey", deserialized.InitializationStrategies.Cast<InitializationStrategySqlServer>().Single().Name);
        }

        [Fact]
        public static void Deserialize_SingleWebInitStrategy_Valid()
        {
            var input = @"
{
	""instanceType"": { ""virtualCores"": 2, ""ramInGb"": 3, },
	""isPubliclyAccessible"": false,
	""volumes"": [{
		""driveLetter"": ""C"",
		""sizeInGb"": ""50"",
	}],
	""InitializationStrategies"": [{
		""primaryDns"": ""reports.coopmetrics.coop"",
	}],
}
";

            var deserialized = Serializer.Deserialize<DeploymentConfigurationWithStrategies>(input);

            Assert.Equal(typeof(InitializationStrategyIis), deserialized.InitializationStrategies.Single().GetType());
            Assert.Equal("reports.coopmetrics.coop", deserialized.InitializationStrategies.Cast<InitializationStrategyIis>().Single().PrimaryDns);
        }
    }
}

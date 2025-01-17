﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageHelperTests.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core.Test
{
    using System;
    using System.IO;
    using System.Linq;

    using FluentAssertions;
    using Naos.Configuration.Domain;
    using Naos.Deployment.Domain;
    using Naos.Packaging.Domain;
    using Naos.Packaging.NuGet;

    using Spritely.Recipes;

    using Xunit;

    using static System.FormattableString;

    public static class PackageHelperTests
    {
        [Fact]
        public static void FindExtraneousFrameworksToDelete__MultipleDirectories__ThinnedTo45()
        {
            // arrange
            var input = new[]
                            {
                                "net20",
                                "netstandard1.3",
                                "net35",
                                "net45",
                            };

            // act
            var output = PackageHelper.FindExtraneousFrameworksToDelete(input);

            // assert
            output.Should().HaveCount(3);
            output.SingleOrDefault(_ => _ == "net20").Should().NotBeNull();
            output.SingleOrDefault(_ => _ == "net35").Should().NotBeNull();
            output.SingleOrDefault(_ => _ == "netstandard1.3").Should().NotBeNull();
        }

        [Fact(Skip = "Debug test designed to aid in fetching bundled packages.")]
        public static void DownloadPackage()
        {
            var packageId = "Naos.FileJanitor.MessageBus.Handler";
            var outputPath = Invariant($@"D:\Deployments\Temp\{packageId}.zip");

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().ToUpperInvariant());
            Directory.CreateDirectory(tempPath);
            var repoConfig = new PackageRepositoryConfiguration
                                 {
                                     ProtocolVersion = 2,
                                     Source = "https://www.nuget.org/api/v2/",
                                     SourceName = "NuGet",
                                     ClearTextPassword = string.Empty,
                                     UserName = "user",
                                 };

            var packageManager = new PackageRetriever(tempPath, new[] { repoConfig });
            var setupFactorySettings = Config.Get<SetupStepFactorySettings>();
            var packageHelper = new PackageHelper(packageManager, setupFactorySettings.RootPackageDirectoriesToPrune, tempPath);

            var package = new PackageDescription { Id = packageId };
            var bundled = packageHelper.GetPackage(package, true);
            File.WriteAllBytes(outputPath, bundled.Package.PackageFileBytes);
            Directory.Delete(tempPath, true);
        }
    }
}

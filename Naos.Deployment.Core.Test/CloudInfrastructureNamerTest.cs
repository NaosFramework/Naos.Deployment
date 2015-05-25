﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CloudInfrastructureNamerTest.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core.Test
{
    using System;
    using System.IO;
    using System.Linq;

    using Xunit;

    public class CloudInfrastructureNamerTest
    {
        [Fact]
        public static void GetInstanceName_ValidInputs_ValidName()
        {
            var expected = "instance-theComputer@use-east-1a";
            var actual =
                new CloudInfrastructureNamer("theComputer", "demo", "use-east-1a", "boxes.monkey.com").GetInstanceName();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void GetVolumeName_ValidInputs_ValidName()
        {
            var expected = "ebs-G-theComputer@use-east-1a";
            var actual =
                new CloudInfrastructureNamer("theComputer", "demo", "use-east-1a", "boxes.monkey.com").GetVolumeName(
                    "G");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void GetPrviateDns_ValidInputs_ValidName()
        {
            var expected = "theComputer.demo.boxes.monkey.com";
            var actual =
                new CloudInfrastructureNamer("theComputer", "demo", "use-east-1a", "boxes.monkey.com").GetPrivateDns();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Constructor_InvalidSubDomainName_ThrowsArgumentException()
        {
            var manualInvalidNamesToTest = new[]
                                         {
                                             null,
                                             string.Empty,
                                             "the.computer", 
                                             "-thecomputer", 
                                             "thecomputer-",
                                             "symbols!",
                                             "symbols@",
                                             "symbols#",
                                             "symbols$",
                                             "symbols%",
                                             "symbols^",
                                             "symbols&",
                                             "symbols*",
                                             "symbols(",
                                             "symbols)",
                                             "symbols=",
                                             "symbols+",
                                         };

            var invalidNamesToTest = Path.GetInvalidPathChars().Select(_ => _.ToString()).ToList();
            invalidNamesToTest.AddRange(Path.GetInvalidFileNameChars().Select(_ => _.ToString()));
            invalidNamesToTest.AddRange(manualInvalidNamesToTest);

            foreach (var invalidName in invalidNamesToTest)
            {
                try
                {
                    var neverGotten = new CloudInfrastructureNamer(
                        invalidName,
                        "demo",
                        "use-east-1a",
                        "boxes.monkey.com");

                    throw new NotSupportedException("An invalid name was allowed. Name: " + invalidName);
                }
                catch (ArgumentException)
                {
                    /* no-op as this is inteneded... */
                }
            }
        }
    }
}

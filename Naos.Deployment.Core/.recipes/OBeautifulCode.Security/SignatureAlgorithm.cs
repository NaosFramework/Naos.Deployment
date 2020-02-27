﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SignatureAlgorithm.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in OBeautifulCode.Security.Recipes source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.Security.Recipes
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A signature algorithm.
    /// </summary>
#if !OBeautifulCodeSecurityRecipesProject
    [System.CodeDom.Compiler.GeneratedCode("OBeautifulCode.Security.Recipes", "See package version number")]
    internal
#else
    public
#endif
    enum SignatureAlgorithm
    {
#pragma warning disable CS1591
#pragma warning disable SA1602
        None,

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Md", Justification = "This is cased correctly.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Md", Justification = "This is spelled correctly")]
        Md2WithRsaEncryption,

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Md", Justification = "This is cased correctly.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Md", Justification = "This is spelled correctly")]
        Md5WithRsaEncryption,

        Sha1WithRsaEncryption,

        Sha224WithRsaEncryption,

        Sha256WithRsaEncryption,

        Sha384WithRsaEncryption,

        Sha512WithRsaEncryption,

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rsassa", Justification = "This is spelled correctly")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pss", Justification = "This is spelled correctly")]
        IdRsassaPss,

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Md", Justification = "This is cased correctly.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Md", Justification = "This is spelled correctly")]
        RsaSignatureWithRipeMd160,

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Md", Justification = "This is cased correctly.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Md", Justification = "This is spelled correctly")]
        RsaSignatureWithRipeMd128,

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Md", Justification = "This is cased correctly.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Md", Justification = "This is spelled correctly")]
        RsaSignatureWithRipeMd256,

        IdDsaWithSha1,

        DsaWithSha224,

        DsaWithSha256,

        DsaWithSha384,

        DsaWithSha512,

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ec", Justification = "This is cased correctly.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ec", Justification = "This is spelled correctly")]
        EcDsaWithSha1,

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ec", Justification = "This is cased correctly.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ec", Justification = "This is spelled correctly")]
        EcDsaWithSha224,

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ec", Justification = "This is cased correctly.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ec", Justification = "This is spelled correctly")]
        EcDsaWithSha256,

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ec", Justification = "This is cased correctly.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ec", Justification = "This is spelled correctly")]
        EcDsaWithSha384,

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ec", Justification = "This is cased correctly.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ec", Justification = "This is spelled correctly")]
        EcDsaWithSha512,

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "This is cased correctly.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gost", Justification = "This is spelled correctly")]
        // ReSharper disable once InconsistentNaming
        GostR3411x94WithGostR3410x94,

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "This is cased correctly.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gost", Justification = "This is spelled correctly")]
        // ReSharper disable once InconsistentNaming
        GostR3411x94WithGostR3410x2001,

#pragma warning restore SA1602
#pragma warning restore CS1591
    }
}

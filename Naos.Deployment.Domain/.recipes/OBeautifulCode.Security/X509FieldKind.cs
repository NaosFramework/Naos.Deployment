﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="X509FieldKind.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in OBeautifulCode.Security.Recipes source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.Security.Recipes
{
    /// <summary>
    /// The kind of field of an X509 certificate.
    /// </summary>
#if !OBeautifulCodeSecuritySolution
    [global::System.CodeDom.Compiler.GeneratedCode("OBeautifulCode.Security.Recipes", "See package version number")]
    internal
#else
    public
#endif
    enum X509FieldKind
    {
        /// <summary>
        /// Unknown (default) field.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        Unknown,

        /// <summary>
        /// The distinguished name of the certificate issuer.
        /// </summary>
        IssuerName,

        /// <summary>
        /// The date/time after which a certificate is no longer valid.
        /// </summary>
        NotAfter,

        /// <summary>
        /// The date/time on which a certificate becomes valid.
        /// </summary>
        NotBefore,

        /// <summary>
        /// The serial number of the certificate.
        /// </summary>
        SerialNumber,

        /// <summary>
        /// The name of the algorithm used to create the signature of the certificate.
        /// </summary>
        SignatureAlgorithmName,

        /// <summary>
        /// The subject distinguished name of the certificate.
        /// </summary>
        SubjectName,

        /// <summary>
        /// The x509 format version of the certificate.
        /// </summary>
        Version,
    }
}

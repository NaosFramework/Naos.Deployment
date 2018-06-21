﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="X509SubjectAttributeKind.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in OBeautifulCode.Security source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.Security.Recipes
{
    /// <summary>
    /// The kind of attribute contained within the subject of an X509 certificate.
    /// </summary>
#if !OBeautifulCodeSecurityRecipesProject
    [System.CodeDom.Compiler.GeneratedCode("OBeautifulCode.Security", "See package version number")]
    internal
#else
    public
#endif
    enum X509SubjectAttributeKind
    {
#pragma warning disable CS1591
#pragma warning disable SA1602
        // ReSharper disable once UnusedMember.Global
        Unknown,

        Country,

        Organization,

        OrganizationalUnit,

        Title,

        CommonName,

        Street,

        SerialNumber,

        Locality,

        State,

#pragma warning restore SA1602
#pragma warning restore CS1591
    }
}

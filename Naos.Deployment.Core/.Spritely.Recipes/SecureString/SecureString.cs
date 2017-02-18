﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecureString.cs">
//     Copyright (c) 2016. All rights reserved. Licensed under the MIT license. See LICENSE file in
//     the project root for full license information.
// </copyright>
// <auto-generated>
// Sourced from NuGet package. Will be overwritten with package update except in Spritely.Recipes source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace Spritely.Recipes
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Contains methods for converting to/from a secure string.
    /// </summary>
#if !SpritelyRecipesProject
    [System.Diagnostics.DebuggerStepThrough]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCode("Spritely.Recipes", "See package version number")]
#pragma warning disable 0436
#endif
    internal static partial class SecureStringExtensions
    {
        /// <summary>
        /// Converts the source string into a secure string. Caller should dispose of the secure string appropriately.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>A secure version of the source string.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is expected to dispose of object.")]
        public static SecureString ToSecureString(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var result = new SecureString();

            foreach (var c in source.ToCharArray())
            {
                result.AppendChar(c);
            }

            result.MakeReadOnly();

            return result;
        }

        /// <summary>
        /// Converts the source secure string into a standard insecure string.
        /// </summary>
        /// <param name="source">The source secure string.</param>
        /// <returns>The standard insecure string.</returns>
        public static string ToInsecureString(this SecureString source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(source);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
#if !SpritelyRecipesProject
#pragma warning restore 0436
#endif
}
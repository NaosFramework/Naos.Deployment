﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PemHelper.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in OBeautifulCode.Security.Recipes source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.Security.Recipes
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;

    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.OpenSsl;
    using Org.BouncyCastle.Pkcs;
    using Org.BouncyCastle.X509;

    /// <summary>
    /// Contains helper methods for creating PEM encoded data.
    /// </summary>
#if !OBeautifulCodeSecurityRecipesProject
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCode("OBeautifulCode.Security.Recipes", "See package version number")]
    internal
#else
    public
#endif
    static class PemHelper
    {
        /// <summary>
        /// Encodes a certificate signing request in PEM.
        /// </summary>
        /// <param name="csr">The certificate signing request.</param>
        /// <returns>
        /// The certificate signing request encoded in PEM.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="csr"/> is null.</exception>
        public static string AsPemEncodedString(
            this Pkcs10CertificationRequest csr)
        {
            if (csr == null)
            {
                throw new ArgumentNullException(nameof(csr));
            }

            var result = EncodeAsPem(csr);
            return result;
        }

        /// <summary>
        /// Encodes the private key of an asymmetric key pair in PEM.
        /// </summary>
        /// <param name="keyPair">The asymmetric cipher key pair.</param>
        /// <returns>
        /// The asymmetric key pair's private key encoded in PEM.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="keyPair"/> is null.</exception>
        public static string AsPemEncodedString(
            this AsymmetricCipherKeyPair keyPair)
        {
            if (keyPair == null)
            {
                throw new ArgumentNullException(nameof(keyPair));
            }

            var result = EncodeAsPem(keyPair);

            return result;
        }

        /// <summary>
        /// Encodes an asymmetric key in PEM.
        /// </summary>
        /// <param name="key">The asymmetric key.</param>
        /// <returns>
        /// The asymmetric key encoded in PEM.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public static string AsPemEncodedString(
            this AsymmetricKeyParameter key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var result = EncodeAsPem(key);

            return result;
        }

        /// <summary>
        /// Encodes an x509 certificate in PEM.
        /// </summary>
        /// <param name="cert">The certificate.</param>
        /// <returns>
        /// The certificate encoded in PEM.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="cert"/> is null.</exception>
        public static string AsPemEncodedString(
            this X509Certificate cert)
        {
            if (cert == null)
            {
                throw new ArgumentNullException(nameof(cert));
            }

            var result = EncodeAsPem(cert);

            return result;
        }

        /// <summary>
        /// Encodes a x509 certificate chain in PEM.
        /// </summary>
        /// <param name="certChain">The certificate chain.</param>
        /// <returns>
        /// The certificate chain encoded in PEM.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="certChain"/> is null.</exception>
        public static string AsPemEncodedString(
            this IReadOnlyList<X509Certificate> certChain)
        {
            if (certChain == null)
            {
                throw new ArgumentNullException(nameof(certChain));
            }

            var stringBuilder = new StringBuilder();

            foreach (var cert in certChain)
            {
                stringBuilder.Append(cert.AsPemEncodedString());

                stringBuilder.AppendLine();
            }

            var result = stringBuilder.ToString();

            return result;
        }

        private static string EncodeAsPem(
            object item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            string result;

            var stringBuilder = new StringBuilder();

            using (var stringWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture))
            {
                var pemWriter = new PemWriter(stringWriter);

                pemWriter.WriteObject(item);

                pemWriter.Writer.Flush();
                
                result = stringBuilder.ToString();
            }

            return result;
        }
    }
}

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Encryptor.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Domain
{
    using System;
    using static System.FormattableString;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;

    using Its.Configuration;

    /// <summary>
    /// Class to encrypt and decrypt text.
    /// </summary>
    public static class Encryptor
    {
        /// <summary>
        /// Encrypts input using a certificate found on the local computer.
        /// </summary>
        /// <param name="input">Input to encrypt.</param>
        /// <param name="encryptingCertificate">Certificate locator of certificate to for encryption.</param>
        /// <returns>Encrypted text.</returns>
        public static string Encrypt(string input, CertificateLocator encryptingCertificate)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (encryptingCertificate == null)
            {
                throw new ArgumentNullException(nameof(encryptingCertificate));
            }

            Func<X509Certificate2, string> funcToRunWithCertificate = input.Encrypt;
            var ret = RunWithCertificate(encryptingCertificate, funcToRunWithCertificate);
            return ret;
        }

        /// <summary>
        /// Decrypts encrypted input using a certificate found on the local computer.
        /// </summary>
        /// <param name="encryptedInput">Input that is encrypted to decrypt.</param>
        /// <param name="encryptingCertificate">Certificate locator of certificate to for encryption.</param>
        /// <returns>Decrypted text.</returns>
        public static string Decrypt(string encryptedInput, CertificateLocator encryptingCertificate)
        {
            if (encryptedInput == null)
            {
                throw new ArgumentNullException(nameof(encryptedInput));
            }

            if (encryptingCertificate == null)
            {
                throw new ArgumentNullException(nameof(encryptingCertificate));
            }

            Func<X509Certificate2, string> funcToRunWithCertificate = certificate => encryptedInput.Decrypt(certificate);
            var ret = RunWithCertificate(encryptingCertificate, funcToRunWithCertificate);
            return ret;
        }

        private static string RunWithCertificate(CertificateLocator encryptingCertificate, Func<X509Certificate2, string> funcToRunWithCertificate)
        {
            var certificateThumbprint = encryptingCertificate.CertificateThumbprint;

            string result;
            var certificateStore = new X509Store(encryptingCertificate.CertificateStoreName, encryptingCertificate.CertificateStoreLocation);
            try
            {
                certificateStore.Open(OpenFlags.OpenExistingOnly);

                var thumbprint = Regex.Replace(certificateThumbprint, @"[^\da-zA-z]", string.Empty).ToUpperInvariant();

                var certificates = certificateStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, encryptingCertificate.CertificateIsValid);

                if (certificates.Count == 0)
                {
                    throw new ArgumentException(Invariant($"Could not find certificate; thumbprint: {certificateThumbprint}, is valid: {encryptingCertificate.CertificateIsValid}, store name: {encryptingCertificate.CertificateStoreName}, store location: {encryptingCertificate.CertificateStoreLocation}"));
                }

                var x509Certificate2 = certificates[0];
                result = funcToRunWithCertificate(x509Certificate2);
            }
            finally
            {
                certificateStore.Close();
            }

            return result;
        }
    }
}
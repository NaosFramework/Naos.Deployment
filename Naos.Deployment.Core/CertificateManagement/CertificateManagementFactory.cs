// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CertificateManagementFactory.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core
{
    using System;
    using System.IO;
    using System.Linq;

    using Naos.Deployment.Core.CertificateManagement;
    using Naos.Deployment.Domain;
    using Naos.Deployment.Persistence;
    using Naos.MessageBus.Domain;

    using OBeautifulCode.Security;

    /// <summary>
    /// Factory for creating certificate retrievers.
    /// </summary>
    public static class CertificateManagementFactory
    {
        /// <summary>
        /// Creates an implementation of the <see cref="IGetCertificates"/> from configuration provided.
        /// </summary>
        /// <param name="certificateManagementConfigurationBase">Configuration to use when creating a certificate retriever.</param>
        /// <returns>An implementation of the <see cref="IGetCertificates"/>.</returns>
        public static IGetCertificates CreateReader(CertificateManagementConfigurationBase certificateManagementConfigurationBase)
        {
            IGetCertificates ret;

            if (certificateManagementConfigurationBase is CertificateManagementConfigurationFile)
            {
                var configAsFile = (CertificateManagementConfigurationFile)certificateManagementConfigurationBase;
                ret = new CertificateRetrieverFromFile(configAsFile.FilePath);
            }
            else if (certificateManagementConfigurationBase is CertificateManagementConfigurationDatabase)
            {
                var configAsDb = (CertificateManagementConfigurationDatabase)certificateManagementConfigurationBase;
                var certificateContainerQueries = configAsDb.Database.GetQueriesInterface<CertificateContainer>();
                ret = new CertificateRetrieverFromMongo(certificateContainerQueries);
            }
            else
            {
                throw new NotSupportedException($"Configuration is not valid: {certificateManagementConfigurationBase.ToJson()}");
            }

            return ret;
        }

        /// <summary>
        /// Creates an implementation of the <see cref="IPersistCertificates"/> from configuration provided.
        /// </summary>
        /// <param name="certificateManagementConfigurationBase">Configuration to use when creating a certificate writer.</param>
        /// <returns>An implementation of the <see cref="IPersistCertificates"/>.</returns>
        public static IPersistCertificates CreateWriter(CertificateManagementConfigurationBase certificateManagementConfigurationBase)
        {
            IPersistCertificates ret;

            if (certificateManagementConfigurationBase is CertificateManagementConfigurationFile)
            {
                var configAsFile = (CertificateManagementConfigurationFile)certificateManagementConfigurationBase;
                ret = new CertificateWriterToFile(configAsFile.FilePath);
            }
            else if (certificateManagementConfigurationBase is CertificateManagementConfigurationDatabase)
            {
                var configAsDb = (CertificateManagementConfigurationDatabase)certificateManagementConfigurationBase;
                var certificateContainerQueries = configAsDb.Database.GetCommandsInterface<string, CertificateContainer>();
                ret = new CertificateWriterToMongo(certificateContainerQueries);
            }
            else
            {
                throw new NotSupportedException($"Configuration is not valid: {certificateManagementConfigurationBase.ToJson()}");
            }

            return ret;
        }

        /// <summary>
        /// Builds a <see cref="CertificateDescriptionWithClearPfxPayload"/> by dynamically extracting additional data from the a PFX file's bytes, a friendly name and it's clear text password (can take optional PEM encoded certificate signing request).
        /// </summary>
        /// <param name="friendlyName">Friendly name of the certificate (does NOT have to match the "Common Name").</param>
        /// <param name="pfxBytes">Bytes of the certificate's PFX file.</param>
        /// <param name="pfxPasswordInClearText">Password of the certificate's PFX file in clear text.</param>
        /// <param name="certificateSigningRequestPemEncoded">Optional PEM Encoded certificate signing request (default will be NULL).</param>
        /// <returns>New <see cref="CertificateDescriptionWithClearPfxPayload"/> with additional info extracted dynamically from file.</returns>
        public static CertificateDescriptionWithClearPfxPayload BuildCertificateDescriptionWithClearPfxPayload(string friendlyName, byte[] pfxBytes, string pfxPasswordInClearText, string certificateSigningRequestPemEncoded = null)
        {
            var endUserCert = CertHelper.ExtractCertChainFromPfx(pfxBytes, pfxPasswordInClearText).GetEndUserCertFromCertChain();
            var certFields = endUserCert.GetX509Fields();
            var certFieldsAsStrings = certFields.ToDictionary(k => k.Key.ToString(), v => v.Value?.ToString());
            var certValidityPeriod = endUserCert.GetValidityPeriod();
            return new CertificateDescriptionWithClearPfxPayload(
                       friendlyName,
                       new Domain.DateTimeRange(certValidityPeriod.StartDateTimeInUtc, certValidityPeriod.EndDateTimeInUtc),
                       certFieldsAsStrings,
                       pfxBytes,
                       pfxPasswordInClearText,
                       certificateSigningRequestPemEncoded);
        }
    }
}
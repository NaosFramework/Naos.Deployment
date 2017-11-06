// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CertificateRetrieverFromMongo.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core.CertificateManagement
{
    using System;
    using System.Threading.Tasks;

    using Naos.Deployment.Domain;
    using Naos.Deployment.Persistence;
    using Naos.Serialization.Bson;

    using Spritely.ReadModel;
    using Spritely.Recipes;

    /// <summary>
    /// Implementation using Mongo of <see cref="IGetCertificates"/>.
    /// </summary>
    public class CertificateRetrieverFromMongo : IGetCertificates
    {
        private readonly IQueries<CertificateContainer> certificateContainerQueries;

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateRetrieverFromMongo"/> class.
        /// </summary>
        /// <param name="certificateContainerQueries">Query interface for retrieving the certificates.</param>
        public CertificateRetrieverFromMongo(IQueries<CertificateContainer> certificateContainerQueries)
        {
            new { certificateContainerQueries }.Must().NotBeNull().OrThrowFirstFailure();

            this.certificateContainerQueries = certificateContainerQueries;

            BsonConfigurationManager.Configure<DeploymentBsonConfiguration>();
        }

        /// <inheritdoc />
        public async Task<CertificateDescriptionWithClearPfxPayload> GetCertificateByNameAsync(string name)
        {
            new { name }.Must().NotBeNull().And().NotBeWhiteSpace().OrThrowFirstFailure();

            // ReSharper disable once SpecifyStringComparison - can't because the expression tree it converts to isn't supported by Mongo...
            var certificateContainer = await this.certificateContainerQueries.GetOneAsync(_ => _.Id.ToUpperInvariant() == name.ToUpperInvariant());

            var certificateDetails = certificateContainer?.Certificate;

            var certDetails = certificateDetails?.ToDecryptedVersion();

            return certDetails;
        }

        /// <summary>
        /// Builds a new <see cref="CertificateRetrieverFromMongo" /> from the provided connection information.
        /// </summary>
        /// <param name="database">Database connection information.</param>
        /// <returns>Built reader.</returns>
        public static CertificateRetrieverFromMongo Build(DeploymentDatabase database)
        {
            new { database }.Must().NotBeNull().OrThrowFirstFailure();

            var ret = new CertificateRetrieverFromMongo(database.GetQueriesInterface<CertificateContainer>());
            return ret;
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CertificateCollection.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core.CertificateManagement
{
    using System.Collections.Generic;

    using Naos.Deployment.Domain;

    /// <summary>
    /// Class to be used to hold certificates for an environment.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Name I want.")]
    public class CertificateCollection
    {
        /// <summary>
        /// Gets or sets the certificates.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Currently using a method on this and don't want to refactor right now.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Keeping without constructor for now due to serialization issues.")]
        public List<CertificateDescriptionWithEncryptedPfxPayload> Certificates { get; set; }
    }
}
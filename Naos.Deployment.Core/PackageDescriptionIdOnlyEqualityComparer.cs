// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageDescriptionIdOnlyEqualityComparer.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Core
{
    using System;
    using System.Collections.Generic;

    using Naos.Deployment.Contract;

    internal class PackageDescriptionIdOnlyEqualityComparer : IEqualityComparer<PackageDescription>, IEqualityComparer<PackageDescriptionWithOverrides>
    {
        public bool Equals(PackageDescription x, PackageDescription y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return x.Id == y.Id;
        }

        public int GetHashCode(PackageDescription obj)
        {
            var id = obj == null ? null : obj.Id;

            var hashCode = new Tuple<string>(id).GetHashCode();
            return hashCode;
        }

        public bool Equals(PackageDescriptionWithOverrides x, PackageDescriptionWithOverrides y)
        {
            return this.Equals((PackageDescription)x, (PackageDescription)y);
        }

        public int GetHashCode(PackageDescriptionWithOverrides obj)
        {
            return this.GetHashCode((PackageDescription)obj);
        }
    }
}
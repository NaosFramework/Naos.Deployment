﻿// --------------------------------------------------------------------------------------------------------------------
// <auto-generated>
//   Generated using OBeautifulCode.CodeGen.ModelObject (1.0.177.0)
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Deployment.Domain
{
    using global::System;
    using global::System.CodeDom.Compiler;
    using global::System.Collections.Concurrent;
    using global::System.Collections.Generic;
    using global::System.Collections.ObjectModel;
    using global::System.Diagnostics.CodeAnalysis;
    using global::System.Globalization;
    using global::System.Linq;

    using global::Naos.FileJanitor.Domain;

    using global::OBeautifulCode.Cloning.Recipes;
    using global::OBeautifulCode.Equality.Recipes;
    using global::OBeautifulCode.Type;
    using global::OBeautifulCode.Type.Recipes;

    using static global::System.FormattableString;

    [Serializable]
    public partial class DownloadAndRestoreDatabaseOp : IModel<DownloadAndRestoreDatabaseOp>
    {
        /// <summary>
        /// Determines whether two objects of type <see cref="DownloadAndRestoreDatabaseOp"/> are equal.
        /// </summary>
        /// <param name="left">The object to the left of the equality operator.</param>
        /// <param name="right">The object to the right of the equality operator.</param>
        /// <returns>true if the two items are equal; otherwise false.</returns>
        public static bool operator ==(DownloadAndRestoreDatabaseOp left, DownloadAndRestoreDatabaseOp right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            var result = left.Equals(right);

            return result;
        }

        /// <summary>
        /// Determines whether two objects of type <see cref="DownloadAndRestoreDatabaseOp"/> are not equal.
        /// </summary>
        /// <param name="left">The object to the left of the equality operator.</param>
        /// <param name="right">The object to the right of the equality operator.</param>
        /// <returns>true if the two items are not equal; otherwise false.</returns>
        public static bool operator !=(DownloadAndRestoreDatabaseOp left, DownloadAndRestoreDatabaseOp right) => !(left == right);

        /// <inheritdoc />
        public bool Equals(DownloadAndRestoreDatabaseOp other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            var result = this.DatabaseName.IsEqualTo(other.DatabaseName, StringComparer.Ordinal)
                      && this.Timeout.IsEqualTo(other.Timeout)
                      && this.KeyPrefixSearchPattern.IsEqualTo(other.KeyPrefixSearchPattern, StringComparer.Ordinal)
                      && this.MultipleKeysFoundStrategy.IsEqualTo(other.MultipleKeysFoundStrategy);

            return result;
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => this == (obj as DownloadAndRestoreDatabaseOp);

        /// <inheritdoc />
        public override int GetHashCode() => HashCodeHelper.Initialize()
            .Hash(this.DatabaseName)
            .Hash(this.Timeout)
            .Hash(this.KeyPrefixSearchPattern)
            .Hash(this.MultipleKeysFoundStrategy)
            .Value;

        /// <inheritdoc />
        public new DownloadAndRestoreDatabaseOp DeepClone() => (DownloadAndRestoreDatabaseOp)this.DeepCloneInternal();

        /// <summary>
        /// Deep clones this object with a new <see cref="DatabaseName" />.
        /// </summary>
        /// <param name="databaseName">The new <see cref="DatabaseName" />.  This object will NOT be deep cloned; it is used as-is.</param>
        /// <returns>New <see cref="DownloadAndRestoreDatabaseOp" /> using the specified <paramref name="databaseName" /> for <see cref="DatabaseName" /> and a deep clone of every other property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        [SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix")]
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
        [SuppressMessage("Microsoft.Naming", "CA1722:IdentifiersShouldNotHaveIncorrectPrefix")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public DownloadAndRestoreDatabaseOp DeepCloneWithDatabaseName(string databaseName)
        {
            var result = new DownloadAndRestoreDatabaseOp(
                                 databaseName,
                                 this.Timeout.DeepClone(),
                                 this.KeyPrefixSearchPattern?.DeepClone(),
                                 this.MultipleKeysFoundStrategy.DeepClone());

            return result;
        }

        /// <summary>
        /// Deep clones this object with a new <see cref="Timeout" />.
        /// </summary>
        /// <param name="timeout">The new <see cref="Timeout" />.  This object will NOT be deep cloned; it is used as-is.</param>
        /// <returns>New <see cref="DownloadAndRestoreDatabaseOp" /> using the specified <paramref name="timeout" /> for <see cref="Timeout" /> and a deep clone of every other property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        [SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix")]
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
        [SuppressMessage("Microsoft.Naming", "CA1722:IdentifiersShouldNotHaveIncorrectPrefix")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public DownloadAndRestoreDatabaseOp DeepCloneWithTimeout(TimeSpan timeout)
        {
            var result = new DownloadAndRestoreDatabaseOp(
                                 this.DatabaseName?.DeepClone(),
                                 timeout,
                                 this.KeyPrefixSearchPattern?.DeepClone(),
                                 this.MultipleKeysFoundStrategy.DeepClone());

            return result;
        }

        /// <summary>
        /// Deep clones this object with a new <see cref="KeyPrefixSearchPattern" />.
        /// </summary>
        /// <param name="keyPrefixSearchPattern">The new <see cref="KeyPrefixSearchPattern" />.  This object will NOT be deep cloned; it is used as-is.</param>
        /// <returns>New <see cref="DownloadAndRestoreDatabaseOp" /> using the specified <paramref name="keyPrefixSearchPattern" /> for <see cref="KeyPrefixSearchPattern" /> and a deep clone of every other property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        [SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix")]
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
        [SuppressMessage("Microsoft.Naming", "CA1722:IdentifiersShouldNotHaveIncorrectPrefix")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public DownloadAndRestoreDatabaseOp DeepCloneWithKeyPrefixSearchPattern(string keyPrefixSearchPattern)
        {
            var result = new DownloadAndRestoreDatabaseOp(
                                 this.DatabaseName?.DeepClone(),
                                 this.Timeout.DeepClone(),
                                 keyPrefixSearchPattern,
                                 this.MultipleKeysFoundStrategy.DeepClone());

            return result;
        }

        /// <summary>
        /// Deep clones this object with a new <see cref="MultipleKeysFoundStrategy" />.
        /// </summary>
        /// <param name="multipleKeysFoundStrategy">The new <see cref="MultipleKeysFoundStrategy" />.  This object will NOT be deep cloned; it is used as-is.</param>
        /// <returns>New <see cref="DownloadAndRestoreDatabaseOp" /> using the specified <paramref name="multipleKeysFoundStrategy" /> for <see cref="MultipleKeysFoundStrategy" /> and a deep clone of every other property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        [SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix")]
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
        [SuppressMessage("Microsoft.Naming", "CA1722:IdentifiersShouldNotHaveIncorrectPrefix")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public DownloadAndRestoreDatabaseOp DeepCloneWithMultipleKeysFoundStrategy(MultipleKeysFoundStrategy multipleKeysFoundStrategy)
        {
            var result = new DownloadAndRestoreDatabaseOp(
                                 this.DatabaseName?.DeepClone(),
                                 this.Timeout.DeepClone(),
                                 this.KeyPrefixSearchPattern?.DeepClone(),
                                 multipleKeysFoundStrategy);

            return result;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected override OperationBase DeepCloneInternal()
        {
            var result = new DownloadAndRestoreDatabaseOp(
                                 this.DatabaseName?.DeepClone(),
                                 this.Timeout.DeepClone(),
                                 this.KeyPrefixSearchPattern?.DeepClone(),
                                 this.MultipleKeysFoundStrategy.DeepClone());

            return result;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public override string ToString()
        {
            var result = Invariant($"Naos.Deployment.Domain.DownloadAndRestoreDatabaseOp: DatabaseName = {this.DatabaseName?.ToString(CultureInfo.InvariantCulture) ?? "<null>"}, Timeout = {this.Timeout.ToString() ?? "<null>"}, KeyPrefixSearchPattern = {this.KeyPrefixSearchPattern?.ToString(CultureInfo.InvariantCulture) ?? "<null>"}, MultipleKeysFoundStrategy = {this.MultipleKeysFoundStrategy.ToString() ?? "<null>"}.");

            return result;
        }
    }
}
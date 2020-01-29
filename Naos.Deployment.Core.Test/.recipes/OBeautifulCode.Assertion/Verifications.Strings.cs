﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Verifications.Strings.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in OBeautifulCode.Assertion.Recipes source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.Assertion.Recipes
{
    using System.Collections;

    using static System.FormattableString;

#if !OBeautifulCodeAssertionRecipesProject
    internal
#else
    public
#endif
        static partial class Verifications
    {
#pragma warning disable 1591
#pragma warning disable SA1600

        public const string ImproperUseOfFrameworkErrorMessage = "The assertion framework is being used improperly; see: https://github.com/OBeautifulCode/OBeautifulCode.Assertion for documentation on proper usage.";

        public const string SubjectUnexpectedTypeErrorMessage = "Called {0}() on a value of type {1}, which is not one of the following expected type(s): {2}.";

        public const string VerificationParameterUnexpectedTypeErrorMessage = "Called {0}({1}:) where '{2}' is of type {3}, which is not one of the following expected type(s): {4}.";

        public const string AnonymousObjectDoesNotHaveSinglePropertyErrorMessage = "Provided value is an anonymous object having {0} properties.  Only single-property anonymous objects are supported.  Found the following properties: {1}.";

        public const string VerificationParameterIsNullErrorMessage = "Called {0}({1}:) where parameter '{1}' is null.";

        public const string VerificationParameterIsLessThanZeroErrorMessage = "Called {0}({1}:) where parameter '{1}' is < 0.  Specified value for '{1}' is {2}.";

        public const string AnyReferenceTypeName = "Any Reference Type";

        public const string AnyValueTypeName = "Any Value Type";

        public const string EnumerableOfAnyReferenceTypeName = "IEnumerable<" + AnyReferenceTypeName + ">";

        public const string EnumerableWhenNotEnumerableOfAnyValueTypeName = "IEnumerable when not IEnumerable<" + AnyValueTypeName + ">";

        public const string NullableGenericTypeName = "Nullable<T>";

        public const string EnumerableOfNullableGenericTypeName = "IEnumerable<" + NullableGenericTypeName + ">";

        public const string DictionaryTypeName = nameof(IDictionary);

        public const string DictionaryWithValueOfAnyReferenceTypeName = "IDictionary<TKey," + AnyReferenceTypeName + ">";

        public const string DictionaryWithValueOfNullableGenericTypeName = "IDictionary<TKey," + NullableGenericTypeName + ">";

        public const string ReadOnlyDictionaryWithValueOfAnyReferenceTypeName = "IReadOnlyDictionary<TKey," + AnyReferenceTypeName + ">";

        public const string ReadOnlyDictionaryWithValueOfNullableGenericTypeName = "IReadOnlyDictionary<TKey," + NullableGenericTypeName + ">";

        public const string NullValueToString = "<null>";

        public const string EmptyEnumerableToString = "[<empty>]";

        public const string EnumerableElementCountContextualInfo = "Enumerable has {0} element(s).";

        public const string DictionaryCountContextualInfo = "Dictionary contains {0} key/value pair(s).";

        public const string DictionaryKeyExampleContextualInfo = "For example, see this key: {0}.";

        public const string DefaultValueContextualInfo = "default(T) is {0}.";

        public const string SubjectTypeContextualInfo = "The type of the provided value is '{0}'.";

        public const string ElementTypeContextualInfo = "The type of the element is '{0}'.";

        public const string BeNullExceptionMessageSuffix = "is not null";

        public const string NotBeNullExceptionMessageSuffix = "is null";

        public const string BeTrueExceptionMessageSuffix = "is not true";

        public const string NotBeTrueExceptionMessageSuffix = "is true";

        public const string BeFalseExceptionMessageSuffix = "is not false";

        public const string NotBeFalseExceptionMessageSuffix = "is false";

        public const string NotBeNullNorWhiteSpaceExceptionMessageSuffix = "is white space";

        public const string BeNullOrNotWhiteSpaceExceptionMessageSuffix = "is not null and is white space";

        public const string BeEmptyGuidExceptionMessageSuffix = "is not an empty guid";

        public const string NotBeEmptyGuidExceptionMessageSuffix = "is an empty guid";

        public const string BeEmptyStringExceptionMessageSuffix = "is not an empty string";

        public const string NotBeEmptyStringExceptionMessageSuffix = "is an empty string";

        public const string BeEmptyEnumerableExceptionMessageSuffix = "is not an empty enumerable";

        public const string NotBeEmptyEnumerableExceptionMessageSuffix = "is an empty enumerable";

        public const string BeEmptyDictionaryExceptionMessageSuffix = "is not an empty dictionary";

        public const string NotBeEmptyDictionaryExceptionMessageSuffix = "is an empty dictionary";

        public const string ContainSomeNullElementsExceptionMessageSuffix = "contains no null elements";

        public const string NotContainAnyNullElementsExceptionMessageSuffix = "contains at least one null element";

        public const string ContainSomeKeyValuePairsWithNullValueExceptionMessageSuffix = "contains no key-value pairs with a null value";

        public const string NotContainAnyKeyValuePairsWithNullValueExceptionMessageSuffix = "contains at least one key-value pair with a null value";

        public const string IsEqualToMethod = "EqualityExtensions.IsEqualTo<T>";

        public const string DefaultComparer = "Comparer<T>.Default";

        public const string UsingIsEqualToMethodology = "using " + IsEqualToMethod + ", where T: {0}";

        public const string UsingDefaultComparerMethodology = "using " + DefaultComparer + ", where T: {0}";

        public const string BeDefaultExceptionMessageSuffix = "is not equal to default(T)";

        public const string NotBeDefaultExceptionMessageSuffix = "is equal to default(T)";

        public const string BeLessThanExceptionMessageSuffix = "is not less than the comparison value";

        public const string NotBeLessThanExceptionMessageSuffix = "is less than the comparison value";

        public const string BeGreaterThanExceptionMessageSuffix = "is not greater than the comparison value";

        public const string NotBeGreaterThanExceptionMessageSuffix = "is greater than the comparison value";

        public const string BeLessThanOrEqualToExceptionMessageSuffix = "is not less than or equal to the comparison value";

        public const string NotBeLessThanOrEqualToExceptionMessageSuffix = "is less than or equal to the comparison value";

        public const string BeGreaterThanOrEqualToExceptionMessageSuffix = "is not greater than or equal to the comparison value";

        public const string NotBeGreaterThanOrEqualToExceptionMessageSuffix = "is greater than or equal to the comparison value";

        public const string BeEqualToExceptionMessageSuffix = "is not equal to the comparison value";

        public const string NotBeEqualToExceptionMessageSuffix = "is equal to the comparison value";

        public const string BeInRangeExceptionMessageSuffix = "is not within the specified range";

        public const string NotBeInRangeExceptionMessageSuffix = "is within the specified range";

        public const string ContainElementExceptionMessageSuffix = "does not contain the item to search for";

        public const string NotContainElementExceptionMessageSuffix = "contains the item to search for";

        public const string BeAlphabeticExceptionMessageSuffix = "is not alphabetic";

        public const string BeAlphanumericExceptionMessageSuffix = "is not alphanumeric";

        public const string BeAsciiPrintableExceptionMessageSuffix = "is not ASCII Printable";

        public const string BeMatchedByRegexExceptionMessageSuffix = "is not matched by the specified regex";

        public const string NotBeMatchedByRegexExceptionMessageSuffix = "is matched by the specified regex";

        public const string MalformedRangeExceptionMessage = "The specified range is invalid because '{1}' is less than '{0}'.  Specified '{0}' is {2}.  Specified '{1}' is {3}.";

        public const string StartWithExceptionMessageSuffix = "does not start with the specified comparison value";

        public const string NotStartWithExceptionMessageSuffix = "starts with the specified comparison value";

        public const string BeSameReferenceAsExceptionMessageSuffix = "is not the same reference as the comparison value";

        public const string NotBeSameReferenceAsExceptionMessageSuffix = "is the same reference as the comparison value";

        public const string ContainStringExceptionMessageSuffix = "does not contain the specified comparison value";

        public const string NotContainStringExceptionMessageSuffix = "contains the specified comparison value";

        public const string HaveCountExceptionMessageSuffix = "is an enumerable whose element count does not equal the expected count";

        public const string NotHaveCountExceptionMessageSuffix = "is an enumerable whose element count equals the unexpected count";

        public const string BeOfTypeExceptionMessageSuffix = "is not of the expected type";

        public const string NotBeOfTypeExceptionMessageSuffix = "is of the unexpected type";

        public const string BeAssignableToTypeExceptionMessageSuffix = "is not assignable to the specified type";

        public const string NotBeAssignableToTypeExceptionMessageSuffix = "is assignable to the specified type";

        public static readonly string SubjectAndOperationSequencingErrorMessage = Invariant($"There is an issue with sequencing of the provided value and the supported assertion operators: {nameof(WorkflowExtensions.AsArg)}, {nameof(WorkflowExtensions.AsOp)}, {nameof(WorkflowExtensions.AsTest)}, {nameof(WorkflowExtensions.Must)}, {nameof(WorkflowExtensions.MustForArg)}, {nameof(WorkflowExtensions.MustForOp)}, {nameof(WorkflowExtensions.MustForTest)}, {nameof(WorkflowExtensions.And)}, {nameof(WorkflowExtensions.Each)}.");

#pragma warning restore SA1600
#pragma warning restore 1591
    }
}
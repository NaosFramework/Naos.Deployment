﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionHelper.Property.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode. All rights reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in OBeautifulCode.Math source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.Reflection.Recipes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Spritely.Recipes;

    /// <summary>
    /// Provides useful methods related to reflection.
    /// </summary>
#if !OBeautifulCodeReflectionRecipesProject
    internal
#else
    public
#endif
    static partial class ReflectionHelper
    {
        /// <summary>
        /// Determines if an object has a given property.
        /// </summary>
        /// <param name="item">Object for which to check for the given property.</param>
        /// <param name="propertyName">The name of the property to check for.</param>
        /// <param name="bindingFlags">Optional binding flags to use during reflection operations.</param>
        /// <returns>
        /// true if the object has the specified property, false if not.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="propertyName"/> is whitespace.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Justification = "Correct name.")]
        public static bool HasProperty(this object item, string propertyName, BindingFlags bindingFlags = DefaultBindingFlags) => GetPropertyInfo(item?.GetType(), propertyName, bindingFlags) != null;

        /// <summary>
        /// Gets the names of all public properties.
        /// </summary>
        /// <returns>Collection of public property names.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Justification = "Correct name.")]
        public static IReadOnlyCollection<string> GetPropertyNames(this Type type, BindingFlags bindingFlags = DefaultBindingFlags)
        {
            new { type }.Must().NotBeNull().OrThrow();
            var allProperties = type.GetProperties(bindingFlags);
            var ret = allProperties.Select(_ => _.Name).ToList();
            return ret;
        }

        /// <summary>
        /// Retrieves a property value from a given object.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="type">Type to get property value on (will only get static properties).</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="bindingFlags">Optional binding flags to use during reflection operations.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="propertyName"/> is whitespace.</exception>
        /// <exception cref="InvalidOperationException">The property was not found.</exception>
        /// <exception cref="InvalidCastException">The property is not of the specified type.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)", Justification = "This is a developer-facing string, not a user-facing string.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Justification = "Correct name.")]
        public static T GetPropertyValue<T>(
            this Type type,
            string propertyName,
            BindingFlags bindingFlags = DefaultBindingFlags)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var pi = type.GetPropertyInfo(propertyName, bindingFlags);
            if (pi == null)
            {
                throw new InvalidOperationException($"Property {propertyName} was not found on type {type.FullName}");
            }

            var ret = pi.GetPropertyValue<T>(null);
            return ret;
        }

        /// <summary>
        /// Retrieves a property value from a given object.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="item">Object from which the property value is returned.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="bindingFlags">Optional binding flags to use during reflection operations.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="propertyName"/> is whitespace.</exception>
        /// <exception cref="InvalidOperationException">The property was not found.</exception>
        /// <exception cref="InvalidCastException">The property is not of the specified type.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)", Justification = "This is a developer-facing string, not a user-facing string.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Justification = "Correct name.")]
        public static T GetPropertyValue<T>(
            this object item,
            string propertyName,
            BindingFlags bindingFlags = DefaultBindingFlags)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var pi = item.GetType().GetPropertyInfo(propertyName, bindingFlags);
            if (pi == null)
            {
                throw new InvalidOperationException($"Property {propertyName} was not found on type {item.GetType().FullName}");
            }

            var ret = pi.GetPropertyValue<T>(item);
            return ret;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)", Justification = "This is a developer-facing string, not a user-facing string.")]
        private static T GetPropertyValue<T>(this PropertyInfo pi, object item)
        {
            if (pi == null)
            {
                throw new ArgumentException(nameof(PropertyInfo) + " must not be null", nameof(pi));
            }

            Type t = typeof(T);
            try
            {
                // can't solely rely on the ( T ) cast - if pi.GetValue returns null, then null can be casted to any reference type.
                if (!pi.PropertyType.IsAssignableFrom(t))
                {
                    throw new InvalidCastException($"Unable to cast object of type '{pi.PropertyType.FullName}' to type '{t.FullName}'.");
                }

                return (T)pi.GetValue(item, null);
            }
            catch (NullReferenceException)
            {
                // if result the value is null, then attempt to cast to value type will result in NullReferenceException
                throw new InvalidCastException($"Unable to cast object of type '{pi.PropertyType.FullName}' to type '{t.FullName}'.");
            }
        }

        /// <summary>
        /// Sets a property value in a given Object.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="type">Type to set property value on (will only set static properties).</param>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="bindingFlags">Optional binding flags to use during reflection operations.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="propertyName"/> is whitespace.</exception>
        /// <exception cref="InvalidOperationException">The property was not found.</exception>
        /// <exception cref="InvalidCastException">The property is not of type T.</exception>
        /// <remarks>
        /// adapted from: <a href="http://stackoverflow.com/questions/1565734/is-it-possible-to-set-private-property-via-reflection/1565766#1565766" />
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)", Justification = "This is a developer-facing string, not a user-facing string.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Justification = "Correct name.")]
        public static void SetPropertyValue<T>(
            this Type type,
            string propertyName,
            T value,
            BindingFlags bindingFlags = DefaultBindingFlags)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var pi = type.GetPropertyInfo(propertyName, bindingFlags);
            if (pi == null)
            {
                throw new InvalidOperationException($"Property {propertyName} was not found on type {type.FullName}");
            }

            pi.SetPropertyValue(null, value);
        }
        /// <summary>
        /// Sets a property value in a given Object.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="item">Object containing property to set.</param>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="bindingFlags">Optional binding flags to use during reflection operations.</param>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="propertyName"/> is whitespace.</exception>
        /// <exception cref="InvalidOperationException">The property was not found.</exception>
        /// <exception cref="InvalidCastException">The property is not of type T.</exception>
        /// <remarks>
        /// adapted from: <a href="http://stackoverflow.com/questions/1565734/is-it-possible-to-set-private-property-via-reflection/1565766#1565766" />
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)", Justification = "This is a developer-facing string, not a user-facing string.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Justification = "Correct name.")]
        public static void SetPropertyValue<T>(
            this object item,
            string propertyName,
            T value,
            BindingFlags bindingFlags = DefaultBindingFlags)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var pi = item.GetType().GetPropertyInfo(propertyName, bindingFlags);
            if (pi == null)
            {
                throw new InvalidOperationException($"Property {propertyName} was not found in Type {item.GetType().FullName}");
            }

            pi.SetPropertyValue(item, value);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)", Justification = "This is a developer-facing string, not a user-facing string.")]
        private static void SetPropertyValue<T>(this PropertyInfo pi, object item, T value)
        {
            if (pi == null)
            {
                throw new ArgumentException(nameof(PropertyInfo) + " must not be null", nameof(pi));
            }

            try
            {
                pi.SetValue(item, value, null);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidCastException(ex.Message);
            }
        }

        private static PropertyInfo GetPropertyInfo(this Type type, string propertyName, BindingFlags bindingFlags)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("The name of the property is whitespace.", nameof(propertyName));
            }

            PropertyInfo pi = null;

            while ((pi == null) && (type != null))
            {
                pi = type.GetProperty(propertyName, bindingFlags);
                type = type.BaseType;
            }

            return pi;
        }
    }
}

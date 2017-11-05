// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    public static class AssertExtensions
    {
        /// <summary>
        /// Verifies that an object is exactly the given type (and not a derived type).
        /// </summary>
        /// <typeparam name="T">The type the object should be</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <returns>The object, casted to type T when successful</returns>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static T IsType<T>(object @object)
        {
            IsType(typeof(T), @object);
            return (T)@object;
        }

        /// <summary>
        /// Verifies that an object is exactly the given type (and not a derived type).
        /// </summary>
        /// <param name="expectedType">The type the object should be</param>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static void IsType(Type expectedType, object @object)
        {
            if (expectedType == null)
                throw new ArgumentNullException(nameof(expectedType));

            if (@object == null)
                throw new ArgumentNullException(nameof(@object));

            Type actualType = @object.GetType();
            if (expectedType != actualType)
            {
                string expectedTypeName = expectedType.FullName;
                string actualTypeName = actualType.FullName;

                if (expectedTypeName == actualTypeName)
                {
                    expectedTypeName += string.Format(" ({0})", expectedType.Assembly.GetName().FullName);
                    actualTypeName += string.Format(" ({0})", actualType.Assembly.GetName().FullName);
                }

                Assert.AreEqual(expectedTypeName, actualTypeName);
            }
        }

        public static T Throws<T>(Action action)
            where T : Exception
        {
            try { action(); }
            catch (T ex)
            {
                if (ex.GetType() != typeof(T))
                {
                    Assert.Fail(
                        "An exception is caught but was a derived type: {0}",
                        ex.GetType());
                    return null;
                }

                return ex;
            }
            catch (Exception ex)
            {
                Assert.Fail(
                    "An exception is caught but was unexpected type: {0}",
                    ex.GetType());
                return null;
            }

            Assert.Fail("No exception was caught");
            return null;
        }

        public static T Throws<T>(Func<object> testCode)
            where T : Exception
        {
            return Throws<T>(new Action(() => testCode()));
        }

        public static Exception Throws(Type exceptionType, Action action)
        {
            try { action(); }
            catch (Exception ex)
            {
                if (ex.GetType() != exceptionType)
                {
                    Assert.Fail(
                        "An exception is caught but was unexpected type: {0}",
                        ex.GetType());
                    return null;
                }

                return ex;
            }

            Assert.Fail("No exception was caught");
            return null;
        }

        public static T ThrowsAny<T>(Action action)
            where T : Exception
        {
            try { action(); }
            catch (T ex)
            {
                return ex;
            }
            catch (Exception ex)
            {
                Assert.Fail(
                    "An exception is caught but was unexpected type: {0}",
                    ex.GetType());
                return null;
            }

            Assert.Fail("No exception was caught");
            return null;
        }

        public static T ThrowsAny<T>(Func<object> testCode)
            where T : Exception
        {
            return ThrowsAny<T>(new Action(() => testCode()));
        }

        public static void Throws<T>(Action action, string message)
            where T : Exception
        {
            Assert.AreEqual(Throws<T>(action).Message, message);
        }

#if DEBUG2
        public static void Throws<T>(string netCoreParamName, string netFxParamName, Action action)
            where T : ArgumentException
        {
            T exception = Assert.Throws<T>(action);

            if (netFxParamName == null && IsFullFramework)
            {
                // Param name varies between NETFX versions -- skip checking it
                return;
            }

            string expectedParamName =
                IsFullFramework ?
                netFxParamName : netCoreParamName;

            if (!RuntimeInformation.FrameworkDescription.StartsWith(".NET Native"))
                Assert.Equal(expectedParamName, exception.ParamName);
        }
#endif


        public static T Throws<T>(string paramName, Action action)
            where T : ArgumentException
        {
            T exception = Throws<T>(action);
#if !WindowsCE
            Assert.AreEqual(paramName, exception.ParamName);
#endif

            return exception;
        }

#if DEBUG2
        public static T Throws<T>(string paramName, Func<object> testCode)
            where T : ArgumentException
        {
            T exception = Assert.Throws<T>(testCode);

            if (!RuntimeInformation.FrameworkDescription.StartsWith(".NET Native"))
                Assert.Equal(paramName, exception.ParamName);

            return exception;
        }

        public static async Task<T> ThrowsAsync<T>(string paramName, Func<Task> testCode)
            where T : ArgumentException
        {
            T exception = await Assert.ThrowsAsync<T>(testCode);

            if (!RuntimeInformation.FrameworkDescription.StartsWith(".NET Native"))
                Assert.Equal(paramName, exception.ParamName);

            return exception;
        }

        public static void Throws<TNetCoreExceptionType, TNetFxExceptionType>(string paramName, Action action)
            where TNetCoreExceptionType : ArgumentException
            where TNetFxExceptionType : ArgumentException
        {
            Throws<TNetCoreExceptionType, TNetFxExceptionType>(paramName, paramName, action);
        }

        public static void Throws<TNetCoreExceptionType, TNetFxExceptionType>(string netCoreParamName, string netFxParamName, Action action)
            where TNetCoreExceptionType : ArgumentException
            where TNetFxExceptionType : ArgumentException
        {
            if (IsFullFramework)
            {
                Throws<TNetFxExceptionType>(netFxParamName, action);
            }
            else
            {
                Throws<TNetCoreExceptionType>(netCoreParamName, action);
            }
        }

        public static void ThrowsAny(Type firstExceptionType, Type secondExceptionType, Action action)
        {
            ThrowsAnyInternal(action, firstExceptionType, secondExceptionType);
        }

        private static void ThrowsAnyInternal(Action action, params Type[] exceptionTypes)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Type exceptionType = e.GetType();
                if (exceptionTypes.Any(t => t.Equals(exceptionType)))
                    return;

                throw new XunitException($"Expected one of: ({string.Join<Type>(", ", exceptionTypes)}) -> Actual: ({e.GetType()})");
            }

            throw new XunitException($"Expected one of: ({string.Join<Type>(", ", exceptionTypes)}) -> Actual: No exception thrown");
        }

        public static void ThrowsAny<TFirstExceptionType, TSecondExceptionType>(Action action)
            where TFirstExceptionType : Exception
            where TSecondExceptionType : Exception
        {
            ThrowsAnyInternal(action, typeof(TFirstExceptionType), typeof(TSecondExceptionType));
        }

        public static void ThrowsAny<TFirstExceptionType, TSecondExceptionType, TThirdExceptionType>(Action action)
            where TFirstExceptionType : Exception
            where TSecondExceptionType : Exception
            where TThirdExceptionType : Exception
        {
            ThrowsAnyInternal(action, typeof(TFirstExceptionType), typeof(TSecondExceptionType), typeof(TThirdExceptionType));
        }

        public static void ThrowsIf<T>(bool condition, Action action)
            where T : Exception
        {
            if (condition)
            {
                Assert.Throws<T>(action);
            }
            else
            {
                action();
            }
        }

        private static string AddOptionalUserMessage(string message, string userMessage)
        {
            if (userMessage == null)
                return message;
            else
                return $"{message} {userMessage}";
        }

        /// <summary>
        /// Validate that a given value is greater than another value.
        /// </summary>
        /// <param name="actual">The value that should be greater than <paramref name="greaterThan"/>.</param>
        /// <param name="greaterThan">The value that <paramref name="actual"/> should be greater than.</param>
        public static void GreaterThan<T>(T actual, T greaterThan, string userMessage = null) where T : IComparable
        {
            if (actual == null)
                throw new XunitException(
                    greaterThan == null
                        ? AddOptionalUserMessage($"Expected: <null> to be greater than <null>.", userMessage)
                        : AddOptionalUserMessage($"Expected: <null> to be greater than {greaterThan}.", userMessage));

            if (actual.CompareTo(greaterThan) <= 0)
                throw new XunitException(AddOptionalUserMessage($"Expected: {actual} to be greater than {greaterThan}", userMessage));
        }

        /// <summary>
        /// Validate that a given value is less than another value.
        /// </summary>
        /// <param name="actual">The value that should be less than <paramref name="lessThan"/>.</param>
        /// <param name="lessThan">The value that <paramref name="actual"/> should be less than.</param>
        public static void LessThan<T>(T actual, T lessThan, string userMessage = null) where T : IComparable
        {
            if (actual == null)
            {
                if (lessThan == null)
                {
                    throw new XunitException(AddOptionalUserMessage($"Expected: <null> to be less than <null>.", userMessage));
                }
                else
                {
                    // Null is always less than non-null
                    return;
                }
            }

            if (actual.CompareTo(lessThan) >= 0)
                throw new XunitException(AddOptionalUserMessage($"Expected: {actual} to be less than {lessThan}", userMessage));
        }

        /// <summary>
        /// Validate that a given value is less than or equal to another value.
        /// </summary>
        /// <param name="actual">The value that should be less than or equal to <paramref name="lessThanOrEqualTo"/></param>
        /// <param name="lessThanOrEqualTo">The value that <paramref name="actual"/> should be less than or equal to.</param>
        public static void LessThanOrEqualTo<T>(T actual, T lessThanOrEqualTo, string userMessage = null) where T : IComparable
        {
            // null, by definition is always less than or equal to
            if (actual == null)
                return;

            if (actual.CompareTo(lessThanOrEqualTo) > 0)
                throw new XunitException(AddOptionalUserMessage($"Expected: {actual} to be less than or equal to {lessThanOrEqualTo}", userMessage));
        }

        /// <summary>
        /// Validate that a given value is greater than or equal to another value.
        /// </summary>
        /// <param name="actual">The value that should be greater than or equal to <paramref name="greaterThanOrEqualTo"/></param>
        /// <param name="greaterThanOrEqualTo">The value that <paramref name="actual"/> should be greater than or equal to.</param>
        public static void GreaterThanOrEqualTo<T>(T actual, T greaterThanOrEqualTo, string userMessage = null) where T : IComparable
        {
            // null, by definition is always less than or equal to
            if (actual == null)
            {
                if (greaterThanOrEqualTo == null)
                {
                    // We're equal
                    return;
                }
                else
                {
                    // Null is always less than non-null
                    throw new XunitException(AddOptionalUserMessage($"Expected: <null> to be greater than or equal to <null>.", userMessage));
                }
            }

            if (actual.CompareTo(greaterThanOrEqualTo) < 0)
                throw new XunitException(AddOptionalUserMessage($"Expected: {actual} to be greater than or equal to {greaterThanOrEqualTo}", userMessage));
        }

        /// <summary>
        /// Validates that the actual byte array is equal to the expected byte array. XUnit only displays the first 5 values
        /// of each collection if the test fails. This doesn't display at what point or how the equality assertion failed.
        /// </summary>
        /// <param name="expected">The byte array that <paramref name="actual"/> should be equal to.</param>
        /// <param name="actual"></param>
        public static void Equal(byte[] expected, byte[] actual)
        {
            try
            {
                Assert.Equal(expected, actual);
            }
            catch (AssertActualExpectedException)
            {
                string expectedString = string.Join(", ", expected);
                string actualString = string.Join(", ", actual);
                throw new AssertActualExpectedException(expectedString, actualString, null);
            }
        }
#endif
    }
}
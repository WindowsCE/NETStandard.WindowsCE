// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mock.System;
using Mock.System.Threading;
using System;

namespace Tests
{
    [TestClass]
    public class LazyOfTMetadataTests
    {
        [TestMethod]
        public void LazyOfTMetadata_Ctor_TMetadata()
        {
            var lazy = new Lazy<int, string>("metadata1");
            VerifyLazy(lazy, 0, "metadata1");

            lazy = new Lazy<int, string>(null);
            VerifyLazy(lazy, 0, null);
        }

        [TestMethod]
        public void LazyOfTMetadata_Ctor_TMetadata_Bool()
        {
            var lazy = new Lazy<int, string>("metadata2", false);
            VerifyLazy(lazy, 0, "metadata2");
        }

        [TestMethod]
        public void LazyOfTMetadata_Ctor_TMetadata_LazyThreadSaftetyMode()
        {
            var lazy = new Lazy<int, string>("metadata3", LazyThreadSafetyMode.PublicationOnly);
            VerifyLazy(lazy, 0, "metadata3");
        }

        [TestMethod]
        public void LazyOfTMetadata_Ctor_TMetadata_LazyThreadSaftetyMode_InvalidMode_ThrowsArgumentOutOfRangeException()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("mode", () => new Lazy<int, string>("test", LazyThreadSafetyMode.None - 1)); // Invalid mode
            AssertExtensions.Throws<ArgumentOutOfRangeException>("mode", () => new Lazy<int, string>("test", LazyThreadSafetyMode.ExecutionAndPublication + 1)); // Invalid mode
        }

        [TestMethod]
        public void LazyOfTMetadata_Ctor_ValueFactory_TMetadata()
        {
            var lazy = new Lazy<string, int>(() => "foo", 4);
            VerifyLazy(lazy, "foo", 4);
        }

        [TestMethod]
        public void LazyOfTMetadata_Ctor_ValueFactory_TMetadata_NullValueFactory_ThrowsArgumentNullException()
        {
            AssertExtensions.Throws<ArgumentNullException>("valueFactory", () => new Lazy<int, string>(null, "test")); // Value factory is null
        }

        [TestMethod]
        public void LazyOfTMetadata_Ctor_ValueFactory_TMetadata_Bool()
        {
            var lazy = new Lazy<string, int>(() => "foo", 5, false);
            VerifyLazy(lazy, "foo", 5);
        }

        [TestMethod]
        public void LazyOfTMetadata_Ctor_ValueFactory_TMetadata_Bool_NullValueFactory_ThrowsArgumentNullException()
        {
            AssertExtensions.Throws<ArgumentNullException>("valueFactory", () => new Lazy<int, string>(null, "test", false)); // Value factory is null
        }

        [TestMethod]
        public void LazyOfTMetadata_Ctor_ValueFactory_TMetadata_LazyThreadSaftetyMode()
        {
            var lazy = new Lazy<string, int>(() => "foo", 6, LazyThreadSafetyMode.None);
            VerifyLazy(lazy, "foo", 6);
        }

        [TestMethod]
        public void LazyOfTMetadata_Ctor_ValueFactory_TMetadata_LazyThreadSaftetyMode_Invalid()
        {
            AssertExtensions.Throws<ArgumentNullException>("valueFactory", () => new Lazy<int, string>(null, "test", LazyThreadSafetyMode.PublicationOnly)); // Value factory is null

            AssertExtensions.Throws<ArgumentOutOfRangeException>("mode", () => new Lazy<int, string>(() => 42, "test", LazyThreadSafetyMode.None - 1)); // Invalid mode
            AssertExtensions.Throws<ArgumentOutOfRangeException>("mode", () => new Lazy<int, string>(() => 42, "test", LazyThreadSafetyMode.ExecutionAndPublication + 1)); // Invalid mode
        }

        private static void VerifyLazy<T, TMetadata>(Lazy<T, TMetadata> lazy, T expectedValue, TMetadata expectedMetadata)
        {
            // Accessing metadata doesn't create the value
            Assert.IsFalse(lazy.IsValueCreated);
            Assert.AreEqual(expectedMetadata, lazy.Metadata);
            Assert.IsFalse(lazy.IsValueCreated);

            Assert.AreEqual(expectedValue, lazy.Value);
            Assert.IsTrue(lazy.IsValueCreated);
        }
    }
}
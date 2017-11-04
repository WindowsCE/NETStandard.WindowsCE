// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;

#if WindowsCE
using System.Collections.Concurrent;
#else
using Mock.System.Collections.Concurrent;
#endif

namespace Tests
{
    [TestClass]
    public class ConcurrentDictionaryTests
    {
        [TestMethod]
        public void ConcurrentDictionary_TestRemove3()
        {
            ConcurrentDictionary<int, int> dict = new ConcurrentDictionary<int, int>();

            dict[99] = -99;

            ICollection<KeyValuePair<int, int>> col = dict;

            // Make sure we cannot "remove" a key/value pair which is not in the dictionary
            for (int i = 0; i < 200; i++)
            {
                if (i != 99)
                {
                    Assert.IsFalse(col.Remove(new KeyValuePair<int, int>(i, -99)), "Should not remove not existing a key/value pair - new KeyValuePair<int, int>(i, -99)");
                    Assert.IsFalse(col.Remove(new KeyValuePair<int, int>(99, -i)), "Should not remove not existing a key/value pair - new KeyValuePair<int, int>(99, -i)");
                }
            }

            // Can we remove a key/value pair successfully?
            Assert.IsTrue(col.Remove(new KeyValuePair<int, int>(99, -99)), "Failed to remove existing key/value pair");

            // Make sure the key/value pair is gone
            Assert.IsFalse(col.Remove(new KeyValuePair<int, int>(99, -99)), "Should not remove the key/value pair which has been removed");

            // And that the dictionary is empty. We will check the count in a few different ways:
            Assert.AreEqual(0, dict.Count);
            Assert.AreEqual(0, dict.ToArray().Length);
        }

        [TestMethod]
        public void ConcurrentDictionary_TestBugFix669376()
        {
            var cd = new ConcurrentDictionary<string, int>(new OrdinalStringComparer());
            cd["test"] = 10;
            Assert.IsTrue(cd.ContainsKey("TEST"), "Customized comparer didn't work");
        }

        private class OrdinalStringComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                var xlower = x.ToLowerInvariant();
                var ylower = y.ToLowerInvariant();
                return string.CompareOrdinal(xlower, ylower) == 0;
            }

            public int GetHashCode(string obj)
            {
                return 0;
            }
        }

        [TestMethod]
        public void ConcurrentDictionary_TestConstructor()
        {
            var dictionary = new ConcurrentDictionary<int, int>(new[] { new KeyValuePair<int, int>(1, 1) });
            Assert.IsFalse(dictionary.IsEmpty);
            Assert.AreEqual(1, dictionary.Keys.Count);
            Assert.AreEqual(1, dictionary.Values.Count);
        }

        [TestMethod]
        public void ConcurrentDictionary_TestConstructor_Negative()
        {
            AssertException.Throws<ArgumentNullException>(
               () => new ConcurrentDictionary<int, int>((ICollection<KeyValuePair<int, int>>)null));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when null collection is passed");

            AssertException.Throws<ArgumentNullException>(
               () => new ConcurrentDictionary<int, int>((IEqualityComparer<int>)null));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when null IEqualityComparer is passed");

            AssertException.Throws<ArgumentNullException>(
               () => new ConcurrentDictionary<int, int>((ICollection<KeyValuePair<int, int>>)null, EqualityComparer<int>.Default));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when null collection and non null IEqualityComparer passed");

            AssertException.Throws<ArgumentNullException>(
               () => new ConcurrentDictionary<int, int>(new[] { new KeyValuePair<int, int>(1, 1) }, null));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when non null collection and null IEqualityComparer passed");

            AssertException.Throws<ArgumentNullException>(
               () => new ConcurrentDictionary<string, int>(new[] { new KeyValuePair<string, int>(null, 1) }));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when collection has null key passed");
            AssertException.Throws<ArgumentException>(
               () => new ConcurrentDictionary<int, int>(new[] { new KeyValuePair<int, int>(1, 1), new KeyValuePair<int, int>(1, 2) }));
            // "TestConstructor:  FAILED.  Constructor didn't throw AE when collection has duplicate keys passed");

            AssertException.Throws<ArgumentNullException>(
               () => new ConcurrentDictionary<int, int>(1, null, EqualityComparer<int>.Default));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when null collection is passed");

            AssertException.Throws<ArgumentNullException>(
               () => new ConcurrentDictionary<int, int>(1, new[] { new KeyValuePair<int, int>(1, 1) }, null));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when null comparer is passed");

            AssertException.Throws<ArgumentNullException>(
               () => new ConcurrentDictionary<int, int>(1, 1, null));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when null comparer is passed");

            AssertException.Throws<ArgumentOutOfRangeException>(
               () => new ConcurrentDictionary<int, int>(0, 10));
            // "TestConstructor:  FAILED.  Constructor didn't throw AORE when <1 concurrencyLevel passed");

            AssertException.Throws<ArgumentOutOfRangeException>(
               () => new ConcurrentDictionary<int, int>(-1, 0));
            // "TestConstructor:  FAILED.  Constructor didn't throw AORE when < 0 capacity passed");
        }

        [TestMethod]
        public void ConcurrentDictionary_TestExceptions()
        {
            var dictionary = new ConcurrentDictionary<string, int>();
            AssertException.Throws<ArgumentNullException>(
               () => dictionary.TryAdd(null, 0));
            //  "TestExceptions:  FAILED.  TryAdd didn't throw ANE when null key is passed");

            AssertException.Throws<ArgumentNullException>(
               () => dictionary.ContainsKey(null));
            // "TestExceptions:  FAILED.  Contains didn't throw ANE when null key is passed");

            int item;
            AssertException.Throws<ArgumentNullException>(
               () => dictionary.TryRemove(null, out item));
            //  "TestExceptions:  FAILED.  TryRemove didn't throw ANE when null key is passed");
            AssertException.Throws<ArgumentNullException>(
               () => dictionary.TryGetValue(null, out item));
            // "TestExceptions:  FAILED.  TryGetValue didn't throw ANE when null key is passed");

            AssertException.Throws<ArgumentNullException>(
               () => { var x = dictionary[null]; });
            // "TestExceptions:  FAILED.  this[] didn't throw ANE when null key is passed");
            AssertException.Throws<KeyNotFoundException>(
               () => { var x = dictionary["1"]; });
            // "TestExceptions:  FAILED.  this[] TryGetValue didn't throw KeyNotFoundException!");

            AssertException.Throws<ArgumentNullException>(
               () => dictionary[null] = 1);
            // "TestExceptions:  FAILED.  this[] didn't throw ANE when null key is passed");

            AssertException.Throws<ArgumentNullException>(
               () => dictionary.GetOrAdd(null, (k) => 0));
            // "TestExceptions:  FAILED.  GetOrAdd didn't throw ANE when null key is passed");
            AssertException.Throws<ArgumentNullException>(
               () => dictionary.GetOrAdd("1", null));
            // "TestExceptions:  FAILED.  GetOrAdd didn't throw ANE when null valueFactory is passed");
            AssertException.Throws<ArgumentNullException>(
               () => dictionary.GetOrAdd(null, 0));
            // "TestExceptions:  FAILED.  GetOrAdd didn't throw ANE when null key is passed");

            AssertException.Throws<ArgumentNullException>(
               () => dictionary.AddOrUpdate(null, (k) => 0, (k, v) => 0));
            // "TestExceptions:  FAILED.  AddOrUpdate didn't throw ANE when null key is passed");
            AssertException.Throws<ArgumentNullException>(
               () => dictionary.AddOrUpdate("1", null, (k, v) => 0));
            // "TestExceptions:  FAILED.  AddOrUpdate didn't throw ANE when null updateFactory is passed");
            AssertException.Throws<ArgumentNullException>(
               () => dictionary.AddOrUpdate(null, (k) => 0, null));
            // "TestExceptions:  FAILED.  AddOrUpdate didn't throw ANE when null addFactory is passed");

            dictionary.TryAdd("1", 1);
            AssertException.Throws<ArgumentException>(
               () => ((IDictionary<string, int>)dictionary).Add("1", 2));
            // "TestExceptions:  FAILED.  IDictionary didn't throw AE when duplicate key is passed");
        }

        [TestMethod]
        public void ConcurrentDictionary_TestIDictionary()
        {
            IDictionary dictionary = new ConcurrentDictionary<string, int>();
            Assert.IsFalse(dictionary.IsReadOnly);

            foreach (var entry in dictionary)
            {
                Assert.IsFalse(true, "TestIDictionary:  FAILED.  GetEnumerator returned items when the dictionary is empty");
            }

            const int SIZE = 10;
            for (int i = 0; i < SIZE; i++)
                dictionary.Add(i.ToString(), i);

            Assert.AreEqual(SIZE, dictionary.Count);

            //test contains
            Assert.IsFalse(dictionary.Contains(1), "TestIDictionary:  FAILED.  Contain returned true for incorrect key type");
            Assert.IsFalse(dictionary.Contains("100"), "TestIDictionary:  FAILED.  Contain returned true for incorrect key");
            Assert.IsTrue(dictionary.Contains("1"), "TestIDictionary:  FAILED.  Contain returned false for correct key");

            //test GetEnumerator
            int count = 0;
            foreach (var obj in dictionary)
            {
                DictionaryEntry entry = (DictionaryEntry)obj;
                string key = (string)entry.Key;
                int value = (int)entry.Value;
                int expectedValue = int.Parse(key);
                Assert.IsTrue(value == expectedValue,
                    string.Format("TestIDictionary:  FAILED.  Unexpected value returned from GetEnumerator, expected {0}, actual {1}", value, expectedValue));
                count++;
            }

            Assert.AreEqual(SIZE, count);
            Assert.AreEqual(SIZE, dictionary.Keys.Count);
            Assert.AreEqual(SIZE, dictionary.Values.Count);

            //Test Remove
            dictionary.Remove("9");
            Assert.AreEqual(SIZE - 1, dictionary.Count);

            //Test this[]
            for (int i = 0; i < dictionary.Count; i++)
                Assert.AreEqual(i, (int)dictionary[i.ToString()]);

            dictionary["1"] = 100; // try a valid setter
            Assert.AreEqual(100, (int)dictionary["1"]);

            //nonsexist key
            Assert.IsNull(dictionary["NotAKey"]);
        }

        [TestMethod]
        public void ConcurrentDictionary_TestIDictionary_Negative()
        {
            IDictionary dictionary = new ConcurrentDictionary<string, int>();
            AssertException.Throws<ArgumentNullException>(
               () => dictionary.Add(null, 1));
            // "TestIDictionary:  FAILED.  Add didn't throw ANE when null key is passed");

            AssertException.Throws<ArgumentException>(
               () => dictionary.Add(1, 1));
            // "TestIDictionary:  FAILED.  Add didn't throw AE when incorrect key type is passed");

            AssertException.Throws<ArgumentException>(
               () => dictionary.Add("1", "1"));
            // "TestIDictionary:  FAILED.  Add didn't throw AE when incorrect value type is passed");

            AssertException.Throws<ArgumentNullException>(
               () => dictionary.Contains(null));
            // "TestIDictionary:  FAILED.  Contain didn't throw ANE when null key is passed");

            //Test Remove
            AssertException.Throws<ArgumentNullException>(
               () => dictionary.Remove(null));
            // "TestIDictionary:  FAILED.  Remove didn't throw ANE when null key is passed");

            //Test this[]
            AssertException.Throws<ArgumentNullException>(
               () => { object val = dictionary[null]; });
            // "TestIDictionary:  FAILED.  this[] getter didn't throw ANE when null key is passed");
            AssertException.Throws<ArgumentNullException>(
               () => dictionary[null] = 0);
            // "TestIDictionary:  FAILED.  this[] setter didn't throw ANE when null key is passed");

            AssertException.Throws<ArgumentException>(
               () => dictionary[1] = 0);
            // "TestIDictionary:  FAILED.  this[] setter didn't throw AE when invalid key type is passed");

            AssertException.Throws<ArgumentException>(
               () => dictionary["1"] = "0");
            // "TestIDictionary:  FAILED.  this[] setter didn't throw AE when invalid value type is passed");
        }

        [TestMethod]
        public void ConcurrentDictionary_TestICollection()
        {
            ICollection dictionary = new ConcurrentDictionary<int, int>();
            Assert.IsFalse(dictionary.IsSynchronized, "TestICollection:  FAILED.  IsSynchronized returned true!");

            int key = -1;
            int value = +1;
            //add one item to the dictionary
            ((ConcurrentDictionary<int, int>)dictionary).TryAdd(key, value);

            var objectArray = new Object[1];
            dictionary.CopyTo(objectArray, 0);

            Assert.AreEqual(key, ((KeyValuePair<int, int>)objectArray[0]).Key);
            Assert.AreEqual(value, ((KeyValuePair<int, int>)objectArray[0]).Value);

            var keyValueArray = new KeyValuePair<int, int>[1];
            dictionary.CopyTo(keyValueArray, 0);
            Assert.AreEqual(key, keyValueArray[0].Key);
            Assert.AreEqual(value, keyValueArray[0].Value);

            var entryArray = new DictionaryEntry[1];
            dictionary.CopyTo(entryArray, 0);
            Assert.AreEqual(key, (int)entryArray[0].Key);
            Assert.AreEqual(value, (int)entryArray[0].Value);
        }

        [TestMethod]
        public void ConcurrentDictionary_TestICollection_Negative()
        {
            ICollection dictionary = new ConcurrentDictionary<int, int>();
            Assert.IsFalse(dictionary.IsSynchronized, "TestICollection:  FAILED.  IsSynchronized returned true!");

            AssertException.Throws<NotSupportedException>(() => { var obj = dictionary.SyncRoot; });
            // "TestICollection:  FAILED.  SyncRoot property didn't throw");
            AssertException.Throws<ArgumentNullException>(() => dictionary.CopyTo(null, 0));
            // "TestICollection:  FAILED.  CopyTo didn't throw ANE when null Array is passed");
            AssertException.Throws<ArgumentOutOfRangeException>(() => dictionary.CopyTo(new object[] { }, -1));
            // "TestICollection:  FAILED.  CopyTo didn't throw AORE when negative index passed");

            //add one item to the dictionary
            ((ConcurrentDictionary<int, int>)dictionary).TryAdd(1, 1);
            AssertException.Throws<ArgumentException>(() => dictionary.CopyTo(new object[] { }, 0));
            // "TestICollection:  FAILED.  CopyTo didn't throw AE when the Array size is smaller than the dictionary count");
        }

        [TestMethod]
        public void ConcurrentDictionary_TestClear()
        {
            var dictionary = new ConcurrentDictionary<int, int>();
            for (int i = 0; i < 10; i++)
                dictionary.TryAdd(i, i);

            Assert.AreEqual(10, dictionary.Count);

            dictionary.Clear();
            Assert.AreEqual(0, dictionary.Count);

            int item;
            Assert.IsFalse(dictionary.TryRemove(1, out item), "TestClear: FAILED.  TryRemove succeeded after Clear");
            Assert.IsTrue(dictionary.IsEmpty, "TestClear: FAILED.  IsEmpty returned false after Clear");
        }

        #region Helper Classes and Methods

        internal static class AssertException
        {
            public static void Throws<T>(Action action)
                where T : Exception
            {
                try { action(); }
                catch (T)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Assert.Fail(
                        "An exception is caught but was unexpected type: {0}",
                        ex.GetType());
                    return;
                }

                Assert.Fail("No exception was caught");
            }
        }

        #endregion
    }
}
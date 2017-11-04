using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#if WindowsCE
using System.Collections.Concurrent;
#else
using Mock.System.Collections.Concurrent;
#endif

namespace Tests
{
    [TestClass]
    public class AsyncTests
    {
        [TestMethod]
        public void ConcurrentDictionary_TestBasicScenarios()
        {
            ConcurrentDictionary<int, int> cd = new ConcurrentDictionary<int, int>();
            Task[] tks = new Task[2];
            tks[0] = Task.Factory.StartNew(() =>
            {
                var ret = cd.TryAdd(1, 11);
                if (!ret)
                {
                    ret = cd.TryUpdate(1, 11, 111);
                    Assert.IsTrue(ret);
                }
                ret = cd.TryAdd(2, 22);
                if (!ret)
                {
                    ret = cd.TryUpdate(2, 22, 222);
                    Assert.IsTrue(ret);
                }
            });
            tks[1] = Task.Factory.StartNew(() =>
            {
                var ret = cd.TryAdd(2, 222);
                if (!ret)
                {
                    ret = cd.TryUpdate(2, 222, 22);
                    Assert.IsTrue(ret);
                }
                ret = cd.TryAdd(1, 111);
                if (!ret)
                {
                    ret = cd.TryUpdate(1, 111, 11);
                    Assert.IsTrue(ret);
                }
            });
            Task.WaitAll(tks);
        }

        [TestMethod]
        public void ConcurrentDictionary_TestAdd1()
        {
            TestAdd1(1, 1, 1, 10000);
            TestAdd1(5, 1, 1, 10000);
            TestAdd1(1, 1, 2, 5000);
            TestAdd1(1, 1, 5, 2000);
            TestAdd1(4, 0, 4, 2000);
            TestAdd1(16, 31, 4, 2000);
            TestAdd1(64, 5, 5, 5000);
            TestAdd1(5, 5, 5, 2500);
        }

        private static void TestAdd1(int cLevel, int initSize, int threads, int addsPerThread)
        {
            ConcurrentDictionary<int, int> dictConcurrent = new ConcurrentDictionary<int, int>(cLevel, 1);
            IDictionary<int, int> dict = dictConcurrent;
            int count = threads;
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                for (int i = 0; i < threads; i++)
                {
                    int ii = i;
                    Task.Factory.StartNew(
                        () =>
                        {
                            for (int j = 0; j < addsPerThread; j++)
                            {
                                dict.Add(j + ii * addsPerThread, -(j + ii * addsPerThread));
                            }
                            if (Interlocked.Decrement(ref count) == 0) mre.Set();
                        });
                }
                mre.WaitOne();
            }
            foreach (var pair in dict)
            {
                Assert.AreEqual(pair.Key, -pair.Value);
            }
            List<int> gotKeys = new List<int>();
            foreach (var pair in dict)
                gotKeys.Add(pair.Key);
            gotKeys.Sort();
            List<int> expectKeys = new List<int>();
            int itemCount = threads * addsPerThread;
            for (int i = 0; i < itemCount; i++)
                expectKeys.Add(i);
            Assert.AreEqual(expectKeys.Count, gotKeys.Count);
            for (int i = 0; i < expectKeys.Count; i++)
            {
                Assert.IsTrue(expectKeys[i].Equals(gotKeys[i]),
                    String.Format("The set of keys in the dictionary is are not the same as the expected" + Environment.NewLine +
                            "TestAdd1(cLevel={0}, initSize={1}, threads={2}, addsPerThread={3})", cLevel, initSize, threads, addsPerThread)
                   );
            }
            // Finally, let's verify that the count is reported correctly.
            int expectedCount = threads * addsPerThread;
            Assert.AreEqual(expectedCount, dict.Count);
            Assert.AreEqual(expectedCount, dictConcurrent.ToArray().Length);
        }

        [TestMethod]
        public void ConcurrentDictionary_TestUpdate1()
        {
            TestUpdate1(1, 1, 10000);
            TestUpdate1(5, 1, 10000);
            TestUpdate1(1, 2, 5000);
            TestUpdate1(1, 5, 2001);
            TestUpdate1(4, 4, 2001);
            TestUpdate1(15, 5, 2001);
            TestUpdate1(64, 5, 5000);
            TestUpdate1(5, 5, 25000);
        }

        private static void TestUpdate1(int cLevel, int threads, int updatesPerThread)
        {
            IDictionary<int, int> dict = new ConcurrentDictionary<int, int>(cLevel, 1);
            for (int i = 1; i <= updatesPerThread; i++) dict[i] = i;
            int running = threads;
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                for (int i = 0; i < threads; i++)
                {
                    int ii = i;
                    Task.Factory.StartNew(
                        () =>
                        {
                            for (int j = 1; j <= updatesPerThread; j++)
                            {
                                dict[j] = (ii + 2) * j;
                            }
                            if (Interlocked.Decrement(ref running) == 0) mre.Set();
                        });
                }
                mre.WaitOne();
            }
            foreach (var pair in dict)
            {
                var div = pair.Value / pair.Key;
                var rem = pair.Value % pair.Key;
                Assert.AreEqual(0, rem);
                Assert.IsTrue(div > 1 && div <= threads + 1,
                    String.Format("* Invalid value={3}! TestUpdate1(cLevel={0}, threads={1}, updatesPerThread={2})", cLevel, threads, updatesPerThread, div));
            }
            List<int> gotKeys = new List<int>();
            foreach (var pair in dict)
                gotKeys.Add(pair.Key);
            gotKeys.Sort();
            List<int> expectKeys = new List<int>();
            for (int i = 1; i <= updatesPerThread; i++)
                expectKeys.Add(i);
            Assert.AreEqual(expectKeys.Count, gotKeys.Count);
            for (int i = 0; i < expectKeys.Count; i++)
            {
                Assert.IsTrue(expectKeys[i].Equals(gotKeys[i]),
                   String.Format("The set of keys in the dictionary is are not the same as the expected." + Environment.NewLine +
                           "TestUpdate1(cLevel={0}, threads={1}, updatesPerThread={2})", cLevel, threads, updatesPerThread)
                  );
            }
        }

        [TestMethod]
        public void ConcurrentDictionary_TestRead1()
        {
            TestRead1(1, 1, 10000);
            TestRead1(5, 1, 10000);
            TestRead1(1, 2, 5000);
            TestRead1(1, 5, 2001);
            TestRead1(4, 4, 2001);
            TestRead1(15, 5, 2001);
            TestRead1(64, 5, 5000);
            TestRead1(5, 5, 25000);
        }

        private static void TestRead1(int cLevel, int threads, int readsPerThread)
        {
            IDictionary<int, int> dict = new ConcurrentDictionary<int, int>(cLevel, 1);
            for (int i = 0; i < readsPerThread; i += 2) dict[i] = i;
            int count = threads;
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                for (int i = 0; i < threads; i++)
                {
                    int ii = i;
                    Task.Factory.StartNew(
                        () =>
                        {
                            for (int j = 0; j < readsPerThread; j++)
                            {
                                int val = 0;
                                if (dict.TryGetValue(j, out val))
                                {
                                    if (j % 2 == 1 || j != val)
                                    {
                                        Console.WriteLine("* TestRead1(cLevel={0}, threads={1}, readsPerThread={2})", cLevel, threads, readsPerThread);
                                        Assert.IsFalse(true, "  > FAILED. Invalid element in the dictionary.");
                                    }
                                }
                                else
                                {
                                    if (j % 2 == 0)
                                    {
                                        Console.WriteLine("* TestRead1(cLevel={0}, threads={1}, readsPerThread={2})", cLevel, threads, readsPerThread);
                                        Assert.IsFalse(true, "  > FAILED. Element missing from the dictionary");
                                    }
                                }
                            }
                            if (Interlocked.Decrement(ref count) == 0) mre.Set();
                        });
                }
                mre.WaitOne();
            }
        }

        [TestMethod]
        public void ConcurrentDictionary_TestRemove1()
        {
            TestRemove1(1, 1, 10000);
            TestRemove1(5, 1, 1000);
            TestRemove1(1, 5, 2001);
            TestRemove1(4, 4, 2001);
            TestRemove1(15, 5, 2001);
            TestRemove1(64, 5, 5000);
        }

        private static void TestRemove1(int cLevel, int threads, int removesPerThread)
        {
            ConcurrentDictionary<int, int> dict = new ConcurrentDictionary<int, int>(cLevel, 1);
            string methodparameters = string.Format("* TestRemove1(cLevel={0}, threads={1}, removesPerThread={2})", cLevel, threads, removesPerThread);
            int N = 2 * threads * removesPerThread;
            for (int i = 0; i < N; i++) dict[i] = -i;
            // The dictionary contains keys [0..N), each key mapped to a value equal to the key.
            // Threads will cooperatively remove all even keys
            int running = threads;
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                for (int i = 0; i < threads; i++)
                {
                    int ii = i;
                    Task.Factory.StartNew(
                        () =>
                        {
                            for (int j = 0; j < removesPerThread; j++)
                            {
                                int value;
                                int key = 2 * (ii + j * threads);
                                Assert.IsTrue(dict.TryRemove(key, out value), "Failed to remove an element! " + methodparameters);
                                Assert.AreEqual(-key, value);
                            }
                            if (Interlocked.Decrement(ref running) == 0) mre.Set();
                        });
                }
                mre.WaitOne();
            }
            foreach (var pair in dict)
            {
                Assert.AreEqual(pair.Key, -pair.Value);
            }
            List<int> gotKeys = new List<int>();
            foreach (var pair in dict)
                gotKeys.Add(pair.Key);
            gotKeys.Sort();
            List<int> expectKeys = new List<int>();
            for (int i = 0; i < (threads * removesPerThread); i++)
                expectKeys.Add(2 * i + 1);
            Assert.AreEqual(expectKeys.Count, gotKeys.Count);
            for (int i = 0; i < expectKeys.Count; i++)
            {
                Assert.IsTrue(expectKeys[i].Equals(gotKeys[i]), "  > Unexpected key value! " + methodparameters);
            }
            // Finally, let's verify that the count is reported correctly.
            Assert.AreEqual(expectKeys.Count, dict.Count);
            Assert.AreEqual(expectKeys.Count, dict.ToArray().Length);
        }

        [TestMethod]
        public void ConcurrentDictionary_TestRemove2()
        {
            TestRemove2(1);
            TestRemove2(10);
            TestRemove2(5000);
        }

        private static void TestRemove2(int removesPerThread)
        {
            ConcurrentDictionary<int, int> dict = new ConcurrentDictionary<int, int>();
            for (int i = 0; i < removesPerThread; i++) dict[i] = -i;
            // The dictionary contains keys [0..N), each key mapped to a value equal to the key.
            // Threads will cooperatively remove all even keys.
            const int SIZE = 2;
            int running = SIZE;
            bool[][] seen = new bool[SIZE][];
            for (int i = 0; i < SIZE; i++) seen[i] = new bool[removesPerThread];
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                for (int t = 0; t < SIZE; t++)
                {
                    int thread = t;
                    Task.Factory.StartNew(
                        () =>
                        {
                            for (int key = 0; key < removesPerThread; key++)
                            {
                                int value;
                                if (dict.TryRemove(key, out value))
                                {
                                    seen[thread][key] = true;
                                    Assert.AreEqual(-key, value);
                                }
                            }
                            if (Interlocked.Decrement(ref running) == 0) mre.Set();
                        });
                }
                mre.WaitOne();
            }
            Assert.AreEqual(0, dict.Count);
            for (int i = 0; i < removesPerThread; i++)
            {
                Assert.IsFalse(seen[0][i] == seen[1][i],
                    String.Format("> FAILED. Two threads appear to have removed the same element. TestRemove2(removesPerThread={0})", removesPerThread)
                    );
            }
        }

        [TestMethod]
        public void ConcurrentDictionary_TestGetOrAdd()
        {
            TestGetOrAddOrUpdate(1, 1, 1, 10000, true);
            TestGetOrAddOrUpdate(5, 1, 1, 10000, true);
            TestGetOrAddOrUpdate(1, 1, 2, 5000, true);
            TestGetOrAddOrUpdate(1, 1, 5, 2000, true);
            TestGetOrAddOrUpdate(4, 0, 4, 2000, true);
            TestGetOrAddOrUpdate(16, 31, 4, 2000, true);
            TestGetOrAddOrUpdate(64, 5, 5, 5000, true);
            TestGetOrAddOrUpdate(5, 5, 5, 25000, true);
        }

        [TestMethod]
        public void ConcurrentDictionary_TestAddOrUpdate()
        {
            TestGetOrAddOrUpdate(1, 1, 1, 10000, false);
            TestGetOrAddOrUpdate(5, 1, 1, 10000, false);
            TestGetOrAddOrUpdate(1, 1, 2, 5000, false);
            TestGetOrAddOrUpdate(1, 1, 5, 2000, false);
            TestGetOrAddOrUpdate(4, 0, 4, 2000, false);
            TestGetOrAddOrUpdate(16, 31, 4, 2000, false);
            TestGetOrAddOrUpdate(64, 5, 5, 5000, false);
            TestGetOrAddOrUpdate(5, 5, 5, 25000, false);
        }

        private static void TestGetOrAddOrUpdate(int cLevel, int initSize, int threads, int addsPerThread, bool isAdd)
        {
            ConcurrentDictionary<int, int> dict = new ConcurrentDictionary<int, int>(cLevel, 1);
            int count = threads;
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                for (int i = 0; i < threads; i++)
                {
                    int ii = i;
                    Task.Factory.StartNew(
                        () =>
                        {
                            for (int j = 0; j < addsPerThread; j++)
                            {
                                if (isAdd)
                                {
                                    //call either of the two overloads of GetOrAdd
                                    if (j + ii % 2 == 0)
                                    {
                                        dict.GetOrAdd(j, -j);
                                    }
                                    else
                                    {
                                        dict.GetOrAdd(j, x => -x);
                                    }
                                }
                                else
                                {
                                    if (j + ii % 2 == 0)
                                    {
                                        dict.AddOrUpdate(j, -j, (k, v) => -j);
                                    }
                                    else
                                    {
                                        dict.AddOrUpdate(j, (k) => -k, (k, v) => -k);
                                    }
                                }
                            }
                            if (Interlocked.Decrement(ref count) == 0) mre.Set();
                        });
                }
                mre.WaitOne();
            }
            foreach (var pair in dict)
            {
                Assert.AreEqual(pair.Key, -pair.Value);
            }
            List<int> gotKeys = new List<int>();
            foreach (var pair in dict)
                gotKeys.Add(pair.Key);
            gotKeys.Sort();
            List<int> expectKeys = new List<int>();
            for (int i = 0; i < addsPerThread; i++)
                expectKeys.Add(i);
            Assert.AreEqual(expectKeys.Count, gotKeys.Count);
            for (int i = 0; i < expectKeys.Count; i++)
            {
                Assert.IsTrue(expectKeys[i].Equals(gotKeys[i]),
                    String.Format("* Test '{4}': Level={0}, initSize={1}, threads={2}, addsPerThread={3})" + Environment.NewLine +
                    "> FAILED.  The set of keys in the dictionary is are not the same as the expected.",
                    cLevel, initSize, threads, addsPerThread, isAdd ? "GetOrAdd" : "GetOrUpdate"));
            }
            // Finally, let's verify that the count is reported correctly.
            Assert.AreEqual(addsPerThread, dict.Count);
            Assert.AreEqual(addsPerThread, dict.ToArray().Length);
        }

        /*
        [TestMethod]
        public void ConcurrentDictionary_TestDebuggerAttributes()
        {
            DebuggerAttributes.ValidateDebuggerDisplayReferences(new ConcurrentDictionary<string, int>());
            DebuggerAttributes.ValidateDebuggerTypeProxyProperties(new ConcurrentDictionary<string, int>());
        }
        */

        /*
        public static void TestTryUpdate()
        {
            var dictionary = new ConcurrentDictionary<string, int>();
            AssertException.Throws<ArgumentNullException>(
               () => dictionary.TryUpdate(null, 0, 0));
            // "TestTryUpdate:  FAILED.  TryUpdate didn't throw ANE when null key is passed");
            for (int i = 0; i < 10; i++)
                dictionary.TryAdd(i.ToString(), i);
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(dictionary.TryUpdate(i.ToString(), i + 1, i), "TestTryUpdate:  FAILED.  TryUpdate failed!");
                Assert.AreEqual(i + 1, dictionary[i.ToString()]);
            }
            //test TryUpdate concurrently
            dictionary.Clear();
            for (int i = 0; i < 1000; i++)
                dictionary.TryAdd(i.ToString(), i);
            var mres = new ManualResetEventSlim();
            Task[] tasks = new Task[10];
            ThreadLocal<ThreadData> updatedKeys = new ThreadLocal<ThreadData>(true);
            for (int i = 0; i < tasks.Length; i++)
            {
                // We are creating the Task using TaskCreationOptions.LongRunning because...
                // there is no guarantee that the Task will be created on another thread.
                // There is also no guarantee that using this TaskCreationOption will force
                // it to be run on another thread.
                tasks[i] = Task.Factory.StartNew((obj) =>
                {
                    mres.Wait();
                    int index = (((int)obj) + 1) + 1000;
                    updatedKeys.Value = new ThreadData();
                    updatedKeys.Value.ThreadIndex = index;
                    for (int j = 0; j < dictionary.Count; j++)
                    {
                        if (dictionary.TryUpdate(j.ToString(), index, j))
                        {
                            if (dictionary[j.ToString()] != index)
                            {
                                updatedKeys.Value.Succeeded = false;
                                return;
                            }
                            updatedKeys.Value.Keys.Add(j.ToString());
                        }
                    }
                }, i, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            mres.Set();
            Task.WaitAll(tasks);
            int numberSucceeded = 0;
            int totalKeysUpdated = 0;
            foreach (var threadData in updatedKeys.Values)
            {
                totalKeysUpdated += threadData.Keys.Count;
                if (threadData.Succeeded)
                    numberSucceeded++;
            }
            Assert.IsTrue(numberSucceeded == tasks.Length, "One or more threads failed!");
            Assert.IsTrue(totalKeysUpdated == dictionary.Count,
               String.Format("TestTryUpdate:  FAILED.  The updated keys count doesn't match the dictionary count, expected {0}, actual {1}", dictionary.Count, totalKeysUpdated));
            foreach (var value in updatedKeys.Values)
            {
                for (int i = 0; i < value.Keys.Count; i++)
                    Assert.IsTrue(dictionary[value.Keys[i]] == value.ThreadIndex,
                       String.Format("TestTryUpdate:  FAILED.  The updated value doesn't match the thread index, expected {0} actual {1}", value.ThreadIndex, dictionary[value.Keys[i]]));
            }
            //test TryUpdate with non atomic values (intPtr > 8)
            var dict = new ConcurrentDictionary<int, Struct16>();
            dict.TryAdd(1, new Struct16(1, -1));
            Assert.IsTrue(dict.TryUpdate(1, new Struct16(2, -2), new Struct16(1, -1)), "TestTryUpdate:  FAILED.  TryUpdate failed for non atomic values ( > 8 bytes)");
        }
        */

        #region Helper Classes and Methods

        /*
        private class ThreadData
        {
            public int ThreadIndex;
            public bool Succeeded = true;
            public List<string> Keys = new List<string>();
        }

        private struct Struct16 : IEqualityComparer<Struct16>
        {
            public long L1, L2;
            public Struct16(long l1, long l2)
            {
                L1 = l1;
                L2 = l2;
            }

            public bool Equals(Struct16 x, Struct16 y)
            {
                return x.L1 == y.L1 && x.L2 == y.L2;
            }

            public int GetHashCode(Struct16 obj)
            {
                return (int)L1;
            }
        }
        */

        #endregion
    }
}

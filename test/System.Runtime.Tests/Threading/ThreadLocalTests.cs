// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Threading
{
    /// <summary>The class that contains the unit tests of the ThreadLocal.</summary>
    [TestClass]
    public class ThreadLocalTests
    {
        /// <summary>Tests for the Ctor.</summary>
        /// <returns>True if the tests succeeds, false otherwise.</returns>
        [TestMethod]
        public void RunThreadLocalTest1_Ctor()
        {
            ThreadLocal<object> testObject;
            testObject = new ThreadLocal<object>();
            testObject = new ThreadLocal<object>(true);
            testObject = new ThreadLocal<object>(() => new object());
            testObject = new ThreadLocal<object>(() => new object(), true);
        }

        [TestMethod]
        public void RunThreadLocalTest1_Ctor_Negative()
        {
            try
            {
                new ThreadLocal<object>(null);
            }
            catch (ArgumentNullException)
            {
                // No other exception should be thrown.
                // With a previous issue, if the constructor threw an exception, the finalizer would throw an exception as well.
            }
        }

        /// <summary>Tests for the ToString.</summary>
        /// <returns>True if the tests succeeds, false otherwise.</returns>
        [TestMethod]
        public void RunThreadLocalTest2_ToString()
        {
            ThreadLocal<object> tlocal = new ThreadLocal<object>(() => (object)1);
            Assert.AreEqual(1.ToString(), tlocal.ToString());
        }

        /// <summary>Tests for the Initialized property.</summary>
        /// <returns>True if the tests succeeds, false otherwise.</returns>
        [TestMethod]
        public void RunThreadLocalTest3_IsValueCreated()
        {
            ThreadLocal<string> tlocal = new ThreadLocal<string>(() => "Test");
            Assert.IsFalse(tlocal.IsValueCreated);
            string temp = tlocal.Value;
            Assert.IsTrue(tlocal.IsValueCreated);
        }

        [TestMethod]
        public void RunThreadLocalTest4_Value()
        {
            ThreadLocal<string> tlocal = null;

            // different threads call Value
            int numOfThreads = 10;
            Task[] threads = new Task[numOfThreads];
            object alock = new object();
            List<string> seenValuesFromAllThreads = new List<string>();
            int counter = 0;
            tlocal = new ThreadLocal<string>(() => (++counter).ToString());
            //CancellationToken ct = new CancellationToken();
            for (int i = 0; i < threads.Length; ++i)
            {
                // We are creating the task using TaskCreationOptions.LongRunning because...
                // there is no guarantee that the Task will be created on another thread.
                // There is also no guarantee that using this TaskCreationOption will force
                // it to be run on another thread.
                threads[i] = new Task(() =>
                {
                    string value = tlocal.Value;
                    Debug.WriteLine("Val: " + value);
                    seenValuesFromAllThreads.Add(value);
                });
                //}, TaskCreationOptions.LongRunning);
                //threads[i].Start(TaskScheduler.Default);
                threads[i].Start();
                threads[i].Wait();
            }
            //Assert.AreEqual(Enumerable.Range(1, threads.Length).Select(x => x.ToString()), seenValuesFromAllThreads);
            Assert.IsTrue(Enumerable.Range(1, threads.Length).Select(x => x.ToString()).SequenceEqual(seenValuesFromAllThreads));
        }

        [TestMethod]
        public void RunThreadLocalTest4_Value_NegativeCases()
        {
            ThreadLocal<string> tlocal = null;
            AssertExtensions.Throws<InvalidOperationException>(() =>
            {
                int x = 0;
                tlocal = new ThreadLocal<string>(delegate
                {
                    if (x++ < 5)
                        return tlocal.Value;
                    else
                        return "Test";
                });
                string str = tlocal.Value;
            });
        }

        [TestMethod]
        public void RunThreadLocalTest5_Dispose()
        {
            // test recycling the combination index;
            ThreadLocal<string> tl = new ThreadLocal<string>(() => null);
            Assert.IsFalse(tl.IsValueCreated);
            Assert.IsNull(tl.Value);

            // Test that a value is not kept alive by a departed thread
            var threadLocal = new ThreadLocal<SetMreOnFinalize>();
            var mres = new ManualResetEventSlim(false);

            // (Careful when editing this test: saving the created thread into a local variable would likely break the 
            // test in Debug build.)
            // We are creating the task using TaskCreationOptions.LongRunning because...
            // there is no guarantee that the Task will be created on another thread.
            // There is also no guarantee that using this TaskCreationOption will force
            // it to be run on another thread.
            //new Task(() => { threadLocal.Value = new SetMreOnFinalize(mres); }, TaskCreationOptions.LongRunning).Start(TaskScheduler.Default);
            new Task(() => { threadLocal.Value = new SetMreOnFinalize(mres); }).Start();

            SpinWait.SpinUntil(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                return mres.IsSet;
            }, 5000);

            Assert.IsTrue(mres.IsSet);
        }

        [TestMethod]
        public void RunThreadLocalTest5_Dispose_Negative()
        {
            ThreadLocal<string> tl = new ThreadLocal<string>(() => "dispose test");
            string value = tl.Value;

            tl.Dispose();

            AssertExtensions.Throws<ObjectDisposedException>(() => { string tmp = tl.Value; });
            // Failure Case: The Value property of the disposed ThreadLocal object should throw ODE

            AssertExtensions.Throws<ObjectDisposedException>(() => { bool tmp = tl.IsValueCreated; });
            // Failure Case: The IsValueCreated property of the disposed ThreadLocal object should throw ODE

            AssertExtensions.Throws<ObjectDisposedException>(() => { string tmp = tl.ToString(); });
            // Failure Case: The ToString method of the disposed ThreadLocal object should throw ODE

        }

        [TestMethod]
        public void RunThreadLocalTest6_SlowPath()
        {
            // the maximum fast path instances for each type is 16 ^ 3 = 4096, when this number changes in the product code, it should be changed here as well
            int MaximumFastPathPerInstance = 4096;
            ThreadLocal<int>[] locals_int = new ThreadLocal<int>[MaximumFastPathPerInstance + 10];
            for (int i = 0; i < locals_int.Length; i++)
            {
                locals_int[i] = new ThreadLocal<int>(() => i);
                var val = locals_int[i].Value;
            }
            //Assert.AreEqual(Enumerable.Range(0, locals_int.Length), locals_int.Select(x => x.Value));
            Assert.IsTrue(Enumerable.Range(0, locals_int.Length).SequenceEqual(locals_int.Select(x => x.Value)));

            // The maximum slowpath for all Ts is MaximumFastPathPerInstance * 4;
            locals_int = new ThreadLocal<int>[4096];
            ThreadLocal<long>[] locals_long = new ThreadLocal<long>[4096];
            ThreadLocal<float>[] locals_float = new ThreadLocal<float>[4096];
            ThreadLocal<double>[] locals_double = new ThreadLocal<double>[4096];
            for (int i = 0; i < locals_int.Length; i++)
            {
                locals_int[i] = new ThreadLocal<int>(() => i);
                locals_long[i] = new ThreadLocal<long>(() => i);
                locals_float[i] = new ThreadLocal<float>(() => i);
                locals_double[i] = new ThreadLocal<double>(() => i);
            }

            ThreadLocal<string> local = new ThreadLocal<string>(() => "slow path");
            Assert.AreEqual("slow path", local.Value);
        }

        private class ThreadLocalWeakReferenceTest
        {
            private object _foo;
            private WeakReference _wFoo;
            [MethodImplAttribute(MethodImplOptions.NoInlining)]

            private void Method()
            {
                _foo = new Object();
                _wFoo = new WeakReference(_foo);

                new ThreadLocal<object>() { Value = _foo }.Dispose();
            }

            public void Run()
            {
                Method();
                _foo = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // s_foo should have been garbage collected
                Assert.IsFalse(_wFoo.IsAlive);
            }
        }

        [TestMethod]
        public void RunThreadLocalTest7_WeakReference()
        {
            var threadLocalWeakReferenceTest = new ThreadLocalWeakReferenceTest();
            threadLocalWeakReferenceTest.Run();
        }

        [TestMethod]
        public void RunThreadLocalTest8_Values()
        {
            // Test adding values and updating values
            {
                var threadLocal = new ThreadLocal<int>(true);
                Assert.IsTrue(threadLocal.Values.Count == 0, "RunThreadLocalTest8_Values: Expected thread local to initially have 0 values");
                Assert.IsTrue(threadLocal.Value == 0, "RunThreadLocalTest8_Values: Expected initial value of 0");
                Assert.IsTrue(threadLocal.Values.Count == 1, "RunThreadLocalTest8_Values: Expected values count to now be 1 from initialized value");
                Assert.IsTrue(threadLocal.Values[0] == 0, "RunThreadLocalTest8_Values: Expected values to contain initialized value");

                threadLocal.Value = 1000;
                Assert.IsTrue(threadLocal.Values.Count == 1, "RunThreadLocalTest8_Values: Expected values count to still be 1 after updating existing value");
                Assert.IsTrue(threadLocal.Values[0] == 1000, "RunThreadLocalTest8_Values: Expected values to contain updated value");

                ((IAsyncResult)Task.Run(() => threadLocal.Value = 1001)).AsyncWaitHandle.WaitOne();
                Assert.IsTrue(threadLocal.Values.Count == 2, "RunThreadLocalTest8_Values: Expected values count to be 2 now that another thread stored a value");
                Assert.IsTrue(threadLocal.Values.Contains(1000) && threadLocal.Values.Contains(1001), "RunThreadLocalTest8_Values: Expected values to contain both thread's values");

                int numTasks = 1000;
                Task[] allTasks = new Task[numTasks];
                for (int i = 0; i < numTasks; i++)
                {
                    // We are creating the task using TaskCreationOptions.LongRunning because...
                    // there is no guarantee that the Task will be created on another thread.
                    // There is also no guarantee that using this TaskCreationOption will force
                    // it to be run on another thread.
                    //var task = Task.Factory.StartNew(() => threadLocal.Value = i, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    var task = Task.Factory.StartNew(() => threadLocal.Value = i, CancellationToken.None);
                    task.Wait();
                }

                var values = threadLocal.Values;
                if (values.Count != 1002)
                {
                    string message =
                        "RunThreadLocalTest8_Values: Expected values to contain both previous values and 1000 new values. Actual count: " +
                        values.Count +
                        '.';
                    if (values.Count != 0)
                    {
                        message += " Missing items:";
                        for (int i = 0; i < 1002; i++)
                        {
                            if (!values.Contains(i))
                            {
                                message += " " + i;
                            }
                        }
                    }
                    Assert.IsTrue(false, message);
                }
                for (int i = 0; i < 1000; i++)
                {
                    Assert.IsTrue(values.Contains(i), "RunThreadLocalTest8_Values: Expected values to contain value for thread #: " + i);
                }

                threadLocal.Dispose();
            }

            // Test that thread values remain after threads depart
            {
                var tl = new ThreadLocal<string>(true);
                var t = Task.Run(() => tl.Value = "Parallel");
                t.Wait();
                t = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Assert.IsTrue(tl.Values.Count == 1, "RunThreadLocalTest8_Values: Expected values count to be 1 from other thread's initialization");
                Assert.IsTrue(tl.Values.Contains("Parallel"), "RunThreadLocalTest8_Values: Expected values to contain 'Parallel'");
            }
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private static void RunThreadLocalTest8Helper(ManualResetEventSlim mres)
        {
            var tl = new ThreadLocal<object>(true);
            var t = Task.Run(() => tl.Value = new SetMreOnFinalize(mres));
            t.Wait();
            t = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Assert.IsTrue(tl.Values.Count == 1, "RunThreadLocalTest8_Values: Expected other thread to have set value");
            Assert.IsTrue(tl.Values[0] is SetMreOnFinalize, "RunThreadLocalTest8_Values: Expected other thread's value to be of the right type");
            tl.Dispose();

            object values;
            AssertExtensions.Throws<ObjectDisposedException>(() => values = tl.Values);
        }

        [TestMethod]
        public void RunThreadLocalTest8_Values_NegativeCases()
        {
            // Test that Dispose works and that objects are released on dispose
            {
                var mres = new ManualResetEventSlim();
                RunThreadLocalTest8Helper(mres);

                SpinWait.SpinUntil(() =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    return mres.IsSet;
                }, 5000);

                Assert.IsTrue(mres.IsSet, "RunThreadLocalTest8_Values: Expected thread local to release the object and for it to be finalized");
            }

            // Test that Values property throws an exception unless true was passed into the constructor
            {
                ThreadLocal<int> t = new ThreadLocal<int>();
                t.Value = 5;
                Exception exceptionCaught = null;
                try
                {
                    var randomValue = t.Values.Count;
                }
                catch (Exception ex)
                {
                    exceptionCaught = ex;
                }

                Assert.IsTrue(exceptionCaught != null, "RunThreadLocalTest8_Values: Expected Values to throw an InvalidOperationException. No exception was thrown.");
                Assert.IsTrue(
                   exceptionCaught != null && exceptionCaught is InvalidOperationException,
                   "RunThreadLocalTest8_Values: Expected Values to throw an InvalidOperationException. Wrong exception was thrown: " + exceptionCaught.GetType().ToString());
            }
        }

        [TestMethod]
        public void RunThreadLocalTest9_Uninitialized()
        {
            for (int iter = 0; iter < 10; iter++)
            {
                ThreadLocal<int> t1 = new ThreadLocal<int>();
                t1.Value = 177;
                ThreadLocal<int> t2 = new ThreadLocal<int>();
                Assert.IsTrue(!t2.IsValueCreated, "RunThreadLocalTest9_Uninitialized: The ThreadLocal instance should have been uninitialized.");
            }
        }

        private class SetMreOnFinalize
        {
            private ManualResetEventSlim _mres;
            public SetMreOnFinalize(ManualResetEventSlim mres)
            {
                _mres = mres;
            }
            ~SetMreOnFinalize()
            {
                _mres.Set();
            }
        }
    }
}
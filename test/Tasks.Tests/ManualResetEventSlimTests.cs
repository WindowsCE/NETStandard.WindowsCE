// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class ManualResetEventSlimTests
    {
        [TestMethod]
        public void ManualResetEventSlim_Test0_StateTrans()
        {
            RunManualResetEventSlimTest0_StateTrans(false);
            RunManualResetEventSlimTest0_StateTrans(true);
        }

        // Validates init, set, reset state transitions.
        private static void RunManualResetEventSlimTest0_StateTrans(bool init)
        {
            ManualResetEventSlim ev = new ManualResetEventSlim(init);
            Assert.AreEqual(init, ev.IsSet);

            for (int i = 0; i < 50; i++)
            {
                ev.Set();
                Assert.IsTrue(ev.IsSet);

                ev.Reset();
                Assert.IsFalse(ev.IsSet);
            }
        }

        // Uses 3 events to coordinate between two threads. Very little validation.
        [TestMethod]
        public void ManualResetEventSlim_Test1_SimpleWait()
        {
            ManualResetEventSlim ev1 = new ManualResetEventSlim(false);
            ManualResetEventSlim ev2 = new ManualResetEventSlim(false);
            ManualResetEventSlim ev3 = new ManualResetEventSlim(false);

            Task.Run(delegate
            {
                ev2.Set();
                ev1.Wait();
                ev3.Set();
            });

            ev2.Wait();
            //Thread.Sleep(100);
            ev1.Set();
            ev3.Wait();
        }

        // Tests timeout on an event that is never set.
        [TestMethod]
        public void ManualResetEventSlim_Test2_TimeoutWait()
        {
            for (int i = 0; i < 2; i++)
            {
                ManualResetEventSlim ev = null;
                if (i == 0) // no custom SpinCount
                    ev = new ManualResetEventSlim(false);
                else
                    ev = new ManualResetEventSlim(false, 500);
                Assert.IsFalse(ev.Wait(0));
                Assert.IsFalse(ev.Wait(100));
                Assert.IsFalse(ev.Wait(TimeSpan.FromMilliseconds(100)));

                ev.Dispose();
            }
        }

        // Tests timeout on an event that is never set.
        [TestMethod]
        public void ManualResetEventSlim_Test3_ConstructorTests()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => new ManualResetEventSlim(false, 2048)); //max value is 2047.

            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => new ManualResetEventSlim(false, -1));
        }

        // Tests that the shared state variable seems to be working correctly.
        [TestMethod]
        public void ManualResetEventSlim_Test4_CombinedStateTests()
        {
            ManualResetEventSlim mres = new ManualResetEventSlim(false, 100);
            Assert.IsFalse(mres.IsSet,
               "RunManualResetEventSlimTest4_CombinedStateTests:  FAILED.  Set did not read correctly.");
            mres.Set();
            Assert.IsTrue(mres.IsSet,
               "RunManualResetEventSlimTest4_CombinedStateTests:  FAILED.  Set did not write/read correctly.");
        }

        [TestMethod]
        public void ManualResetEventSlim_Test5_Dispose()
        {
            ManualResetEventSlim mres = new ManualResetEventSlim(false);
            mres.Dispose();
        }

        [TestMethod]
        public void ManualResetEventSlim_Test5_Dispose_Negative()
        {
            ManualResetEventSlim mres = new ManualResetEventSlim(false);
            mres.Dispose();

            AssertExtensions.Throws<ObjectDisposedException>(() => mres.Reset());
            // Failure Case: The object has been disposed, should throw ObjectDisposedException.

            AssertExtensions.Throws<ObjectDisposedException>(() => mres.Wait(0));
            // Failure Case: The object has been disposed, should throw ObjectDisposedException.

            AssertExtensions.Throws<ObjectDisposedException>(
                () =>
                {
                    WaitHandle handle = mres.WaitHandle;
                });
            // Failure Case: The object has been disposed, should throw ObjectDisposedException.

            mres = new ManualResetEventSlim(false);

            ManualResetEvent mre = (ManualResetEvent)mres.WaitHandle;
            mres.Dispose();

            AssertExtensions.Throws<ObjectDisposedException>(() => mre.WaitOne(0));
            // Failure Case: The underlying event object has been disposed, should throw ObjectDisposedException.

        }

        [TestMethod]
        public void ManualResetEventSlim_Test6_Exceptions()
        {
            ManualResetEventSlim mres = null;
            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => mres = new ManualResetEventSlim(false, -1));
            // Failure Case: Constructor didn't throw AORE when -1 passed

            mres = new ManualResetEventSlim(false);

            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => mres.Wait(-2));
            // Failure Case: Wait(int) didn't throw AORE when the totalmilliseconds < -1

            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => mres.Wait(TimeSpan.FromDays(-1)));
            // Failure Case: Wait(TimeSpan) didn't throw AORE when the totalmilliseconds < -1

            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => mres.Wait(TimeSpan.MaxValue));
            // Failure Case: Wait(TimeSpan, CancellationToken) didn't throw AORE when the totalmilliseconds > int.max

            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => mres.Wait(TimeSpan.FromDays(-1), new CancellationToken()));
            // Failure Case: Wait(TimeSpan) didn't throw AORE when the totalmilliseconds < -1

            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => mres.Wait(TimeSpan.MaxValue, new CancellationToken()));
            // Failure Case: Wait(TimeSpan, CancellationToken) didn't throw AORE when the totalmilliseconds > int.max
        }
    }
}
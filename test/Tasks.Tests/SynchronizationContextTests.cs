// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

#if !WindowsCE
using SynchronizationContext = Mock.System.Threading.SynchronizationContext;
#endif

namespace Tests
{
    [TestClass]
    public class SynchronizationContextTests
    {
        [TestMethod]
        public void SynchronizationContext_Current()
        {
            CreateSetAndTest(1);

            for (int i = 0; i < 10; i++)
            {
                int expectedId = i + 2;
                var th = new Thread(() => CreateSetAndTest(expectedId));
                th.Start();
                Assert.IsTrue(th.Join(ThreadTestHelpers.UnexpectedTimeoutMilliseconds));
            }

            var syncContext = SynchronizationContext.Current;
            Assert.IsNotNull(syncContext);
            Assert.IsTrue(syncContext is TestSynchronizationContext);
            Assert.AreEqual(1, ((TestSynchronizationContext)syncContext).id);
        }

        private static void CreateSetAndTest(int expectedId)
        {
            Assert.IsNull(SynchronizationContext.Current);

            var testSyncContext = new TestSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(testSyncContext);

            var currSyncContext = SynchronizationContext.Current;
            Assert.IsNotNull(currSyncContext);
            Assert.AreSame(testSyncContext, currSyncContext);
            Assert.AreEqual(expectedId, testSyncContext.id);
        }

        //[TestMethod]
        //public void SynchronizationContext_WaitTest()
        //{
        //    var tsc = new TestSynchronizationContext();
        //    AssertExtensions.Throws<ArgumentNullException>(() => tsc.Wait(null, false, 0));

        //    var e = new ManualResetEvent(false);
        //    IntPtr eventHandle = e.SafeWaitHandle.DangerousGetHandle();
        //    var handles = new IntPtr[] { eventHandle, eventHandle };
        //    Assert.AreEqual(WaitHandle.WaitTimeout, tsc.Wait(handles, false, 0));
        //    AssertExtensions.Throws<DuplicateWaitObjectException>(() => Task.Run(() => tsc.Wait(handles, true, 0)).GetAwaiter().GetResult()); // ensure Wait runs on MTA thread

        //    var e2 = new ManualResetEvent(false);
        //    handles = new IntPtr[] { eventHandle, e2.SafeWaitHandle.DangerousGetHandle() };
        //    Assert.AreEqual(WaitHandle.WaitTimeout, tsc.Wait(handles, false, 0));
        //    Assert.AreEqual(WaitHandle.WaitTimeout, tsc.Wait(handles, true, 0));

        //    e.Set();
        //    Assert.AreEqual(0, tsc.Wait(handles, false, 0));
        //    Assert.AreEqual(WaitHandle.WaitTimeout, tsc.Wait(handles, true, 0));

        //    e2.Set();
        //    Assert.AreEqual(0, tsc.Wait(handles, false, 0));
        //    Assert.AreEqual(0, tsc.Wait(handles, true, 0));
        //}

        //[TestMethod]
        ////[SkipOnTargetFramework(TargetFrameworkMonikers.NetFramework)] // desktop framework does not check for null and crashes
        //public void SynchronizationContext_WaitTest_ChangedInDotNetCore()
        //{
        //    AssertExtensions.Throws<ArgumentNullException>(() => TestSynchronizationContext.WaitHelper(null, false, 0));
        //}

        //[TestMethod]
        //public void SynchronizationContext_WaitNotificationTest()
        //{
        //    ThreadTestHelpers.RunTestInBackgroundThread(() =>
        //    {
        //        var tsc = new TestSynchronizationContext();
        //        SynchronizationContext.SetSynchronizationContext(tsc);
        //        Assert.AreSame(tsc, SynchronizationContext.Current);

        //        var e = new ManualResetEvent(false);
        //        tsc.WaitAction = () => e.Set();
        //        Assert.IsFalse(tsc.IsWaitNotificationRequired());
        //        Assert.IsFalse(e.WaitOne(0));
        //        tsc.SetWaitNotificationRequired();
        //        Assert.IsTrue(tsc.IsWaitNotificationRequired());
        //        Assert.IsTrue(e.WaitOne(0));

        //        //var mres = new ManualResetEventSlim();
        //        var mres = new ManualResetEvent(false);
        //        tsc.WaitAction = () => mres.Set();
        //        mres.Reset();
        //        mres.CheckedWait();

        //        e.Reset();
        //        tsc.WaitAction = () => e.Set();
        //        SynchronizationContext.SetSynchronizationContext(new TestSynchronizationContext());
        //        Assert.IsFalse(e.WaitOne(0));
        //        SynchronizationContext.SetSynchronizationContext(tsc);
        //        Assert.IsTrue(e.WaitOne(0));
        //        e.Reset();
        //        e.CheckedWait();

        //        e.Reset();
        //        var lockObj = new object();
        //        var lockAcquiredFromBackground = new AutoResetEvent(false);
        //        Action waitForThread;
        //        Thread t =
        //            ThreadTestHelpers.CreateGuardedThread(out waitForThread, () =>
        //            {
        //                lock (lockObj)
        //                {
        //                    lockAcquiredFromBackground.Set();
        //                    e.CheckedWait();
        //                }
        //            });
        //        t.IsBackground = true;
        //        t.Start();
        //        lockAcquiredFromBackground.CheckedWait();
        //        Assert.IsTrue(Monitor.TryEnter(lockObj, ThreadTestHelpers.UnexpectedTimeoutMilliseconds));
        //        Monitor.Exit(lockObj);
        //        waitForThread();

        //        e.Reset();
        //        var m = new Mutex();
        //        t = ThreadTestHelpers.CreateGuardedThread(out waitForThread, () =>
        //        {
        //            m.CheckedWait();
        //            try
        //            {
        //                lockAcquiredFromBackground.Set();
        //                e.CheckedWait();
        //            }
        //            finally
        //            {
        //                m.ReleaseMutex();
        //            }
        //        });
        //        t.IsBackground = true;
        //        t.Start();
        //        lockAcquiredFromBackground.CheckedWait();
        //        m.CheckedWait();
        //        m.ReleaseMutex();
        //        waitForThread();
        //    });
        //}

        private class TestSynchronizationContext : SynchronizationContext
        {
            private static int seed = 0;
            public readonly int id;

            public TestSynchronizationContext()
            {
                id = Interlocked.Increment(ref seed);
            }

            public Action WaitAction { get; set; }

            //public new void SetWaitNotificationRequired()
            //{
            //    base.SetWaitNotificationRequired();
            //}

            //public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
            //{
            //    if (WaitAction != null)
            //    {
            //        WaitAction();
            //    }
            //    return base.Wait(waitHandles, waitAll, millisecondsTimeout);
            //}

            //public static new int WaitHelper(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
            //{
            //    return SynchronizationContext.WaitHelper(waitHandles, waitAll, millisecondsTimeout);
            //}
        }
    }
}
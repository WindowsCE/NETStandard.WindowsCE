// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using NativeWaitHandle = System.Threading.WaitHandle;
using WaitHandle = System.Threading.WaitHandle2;

namespace Tests
{
    [TestClass]
    public partial class WaitHandleTests
    {
        [TestMethod]
        public void WaitHandle_WaitOne()
        {
            ManualResetEvent h = new ManualResetEvent(true);

            Assert.IsTrue(h.WaitOne());
            Assert.IsTrue(h.WaitOne(1));
            Assert.IsTrue(h.WaitOne(TimeSpan.FromMilliseconds(1)));

            h.Reset();

            Assert.IsFalse(h.WaitOne(1));
            Assert.IsFalse(h.WaitOne(TimeSpan.FromMilliseconds(1)));
        }

        [TestMethod]
        public void WaitHandle_WaitAny()
        {
            var handles = new ManualResetEvent[] {
                new ManualResetEvent(false),
                new ManualResetEvent(false),
                new ManualResetEvent(true)
            };

            Assert.AreEqual(2, WaitHandle.WaitAny(handles));
            Assert.AreEqual(2, WaitHandle.WaitAny(handles, 1));
            Assert.AreEqual(2, WaitHandle.WaitAny(handles, TimeSpan.FromMilliseconds(1)));

            handles[2].Reset();

            Assert.AreEqual(WaitHandle.WaitTimeout, WaitHandle.WaitAny(handles, 1));
            Assert.AreEqual(WaitHandle.WaitTimeout, WaitHandle.WaitAny(handles, TimeSpan.FromMilliseconds(1)));
        }

        [TestMethod]
        public void WaitHandle_WaitAny_SameHandles()
        {
            ManualResetEvent[] wh = new ManualResetEvent[2];
            wh[0] = new ManualResetEvent(true);
            wh[1] = wh[0];

            Assert.AreEqual(0, WaitHandle.WaitAny(wh));
        }

        [TestMethod]
        public void WaitHandle_WaitAll()
        {
            var handles = new ManualResetEvent[] {
                new ManualResetEvent(true),
                new ManualResetEvent(true),
                new ManualResetEvent(true)
            };

            Assert.IsTrue(WaitHandle.WaitAll(handles));
            Assert.IsTrue(WaitHandle.WaitAll(handles, 1));
            Assert.IsTrue(WaitHandle.WaitAll(handles, TimeSpan.FromMilliseconds(1)));

            handles[2].Reset();

            Assert.IsFalse(WaitHandle.WaitAll(handles, 1));
            Assert.IsFalse(WaitHandle.WaitAll(handles, TimeSpan.FromMilliseconds(1)));
        }

        [TestMethod]
        public void WaitHandle_WaitAll_SameHandles()
        {
            ManualResetEvent[] wh = new ManualResetEvent[2];
            wh[0] = new ManualResetEvent(true);
            wh[1] = wh[0];

            AssertExtensions.ThrowsAny<ArgumentException>(() => WaitHandle.WaitAll(wh));
        }

        //[TestMethod]
        //[PlatformSpecific(TestPlatforms.Windows)] // names aren't supported on Unix
#if !WindowsCE  // names aren't supported on Windows CE either
        public void WaitHandle_WaitAll_SameNames()
        {
            Mutex[] wh = new Mutex[2];
            wh[0] = new Mutex(false, "test");
            wh[1] = new Mutex(false, "test");

            AssertExtensions.Throws<ArgumentException>(null, () => WaitHandle.WaitAll(wh));
        }
#endif

        [TestMethod]
        public void WaitHandle_WaitTimeout()
        {
            Assert.AreEqual(WaitHandle.WaitTimeout, WaitHandle.WaitAny(new[] { new ManualResetEvent(false) }, 0));
        }

        [TestMethod]
        public void WaitHandle_Close()
        {
            var wh = new ManualResetEvent(false);
            wh.Close();
            AssertExtensions.Throws<ObjectDisposedException>(() => wh.WaitOne(0));
        }

        [TestMethod]
        public void WaitHandle_CloseVirtual_ThroughDispose()
        {
            var wh = new TestWaitHandle();
            wh.Close();
            Assert.IsTrue(wh.WasExplicitlyDisposed);
        }

        private partial class TestWaitHandle : NativeWaitHandle
        {
            public TestWaitHandle()
            {
                WasExplicitlyDisposed = false;
            }

            public bool WasExplicitlyDisposed { get; private set; }

            protected override void Dispose(bool explicitDisposing)
            {
                if (explicitDisposing)
                {
                    WasExplicitlyDisposed = true;
                }
                base.Dispose(explicitDisposing);
            }
        }

#pragma warning disable 0618 // 'WaitHandle.Handle' is obsolete: 'Use the SafeWaitHandle property instead.'
        [TestMethod]
        public void WaitHandle_Handle()
        {
            var eventWaitHandle = new ManualResetEvent(false);
            var eventRawWaitHandle = eventWaitHandle.Handle;
            var testWaitHandle = new TestWaitHandle();
            testWaitHandle.Handle = eventRawWaitHandle;
            Assert.IsFalse(testWaitHandle.WaitOne(0));
            eventWaitHandle.Set();
            Assert.IsTrue(testWaitHandle.WaitOne(0));
            testWaitHandle.ClearHandle();
            AssertExtensions.Throws<ObjectDisposedException>(() => testWaitHandle.WaitOne(0));
        }
#pragma warning restore 0618 // 'WaitHandle.Handle' is obsolete: 'Use the SafeWaitHandle property instead.'

#if !WindowsCE
        [TestMethod]
        public void WaitHandle_SafeWaitHandle()
        {
            var eventWaitHandle = new ManualResetEvent(false);
            var eventSafeWaitHandle = eventWaitHandle.SafeWaitHandle;
            var testWaitHandle = new TestWaitHandle();
            testWaitHandle.SafeWaitHandle = eventSafeWaitHandle;
            Assert.IsFalse(testWaitHandle.WaitOne(0));
            eventWaitHandle.Set();
            Assert.IsTrue(testWaitHandle.WaitOne(0));
            testWaitHandle.SafeWaitHandle = null;
            AssertExtensions.Throws<ObjectDisposedException>(() => testWaitHandle.WaitOne(0));
        }
#endif

        private partial class TestWaitHandle : NativeWaitHandle
        {
#pragma warning disable 0618 // 'WaitHandle.Handle' is obsolete: 'Use the SafeWaitHandle property instead.'
            public void ClearHandle()
            {
                Handle = InvalidHandle;
            }
#pragma warning restore 0618 // 'WaitHandle.Handle' is obsolete: 'Use the SafeWaitHandle property instead.'
        }
    }
}
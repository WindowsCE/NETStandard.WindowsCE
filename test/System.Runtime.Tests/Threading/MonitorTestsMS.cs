// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

using Monitor = System.Threading.Monitor2;

namespace Tests
{
    [TestClass]
    public class MonitorTests
    {
        private const int FailTimeoutMilliseconds = 30000;

        // Attempts a single recursive acquisition/release cycle of a newly-created lock.
        [TestMethod]
        public void Monitor_BasicRecursion()
        {
            var obj = new object();
            Assert.IsTrue(Monitor.TryEnter(obj));
            Assert.IsTrue(Monitor.TryEnter(obj));
            Monitor.Exit(obj);
            //Assert.IsTrue(Monitor.IsEntered(obj));
            Monitor.Enter(obj);
            //Assert.IsTrue(Monitor.IsEntered(obj));
            Monitor.Exit(obj);
            //Assert.IsTrue(Monitor.IsEntered(obj));
            Monitor.Exit(obj);
            //Assert.IsFalse(Monitor.IsEntered(obj));
        }

        // Attempts to overflow the recursion count of a newly-created lock.
        [TestMethod]
        public void Monitor_DeepRecursion()
        {
            var obj = new object();
            var hc = obj.GetHashCode();
            // reduced from "(long)int.MaxValue + 2;" to something that will return in a more meaningful time
            const int limit = 10000;

            for (var i = 0L; i < limit; i++)
            {
                Assert.IsTrue(Monitor.TryEnter(obj));
            }

            for (var j = 0L; j < (limit - 1); j++)
            {
                Monitor.Exit(obj);
                //Assert.IsTrue(Monitor.IsEntered(obj));
            }

            Monitor.Exit(obj);
            //Assert.IsTrue(Monitor.IsEntered(obj));
        }

        //[TestMethod]
        //public void Monitor_IsEntered()
        //{
        //    var obj = new object();
        //    Assert.IsFalse(Monitor.IsEntered(obj));
        //    lock (obj)
        //    {
        //        Assert.IsTrue(Monitor.IsEntered(obj));
        //    }
        //    Assert.IsFalse(Monitor.IsEntered(obj));
        //}

        //[TestMethod]
        //public void Monitor_IsEntered_WhenHeldBySomeoneElse_ThrowsSynchronizationLockException()
        //{
        //    var obj = new object();
        //    var b = new Barrier(2);

        //    Task t = Task.Run(() =>
        //    {
        //        lock (obj)
        //        {
        //            b.SignalAndWait();
        //            Assert.IsTrue(Monitor.IsEntered(obj));
        //            b.SignalAndWait();
        //        }
        //    });

        //    b.SignalAndWait();
        //    Assert.IsFalse(Monitor.IsEntered(obj));
        //    b.SignalAndWait();

        //    t.Wait();
        //}

        [TestMethod]
        public void Monitor_Enter_SetsLockTaken()
        {
            bool lockTaken = false;
            var obj = new object();

            Monitor.Enter(obj, ref lockTaken);
            Assert.IsTrue(lockTaken);
            Monitor.Exit(obj);
            // The documentation does not specifies that lockTaken variable is
            // set to false after Exit is called
            // Assert.IsFalse(lockTaken);
        }

        [TestMethod]
        public void Monitor_Enter_Invalid()
        {
            bool lockTaken = false;
            var obj = new object();

            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.Enter(null));
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.Enter(null, ref lockTaken));
            Assert.IsFalse(lockTaken);

            lockTaken = true;
            AssertExtensions.Throws<ArgumentException>("lockTaken", () => Monitor.Enter(obj, ref lockTaken));
            //Assert.IsFalse(lockTaken);
            Assert.IsTrue(lockTaken);
        }

        [TestMethod]
        public void Monitor_Exit_Invalid()
        {
            var obj = new object();
            int valueType = 1;
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.Exit(null));

#if !WindowsCE
            AssertExtensions.Throws<SynchronizationLockException>(() => Monitor.Exit(obj));
            AssertExtensions.Throws<SynchronizationLockException>(() => Monitor.Exit(new object()));
            AssertExtensions.Throws<SynchronizationLockException>(() => Monitor.Exit(valueType));
#else
            AssertExtensions.Throws<ArgumentException>(() => Monitor.Exit(obj));
            AssertExtensions.Throws<ArgumentException>(() => Monitor.Exit(new object()));
            AssertExtensions.Throws<ArgumentException>(() => Monitor.Exit(valueType));
#endif
        }

        //[TestMethod]
        //public void Monitor_Exit_WhenHeldBySomeoneElse_ThrowsSynchronizationLockException()
        //{
        //    var obj = new object();
        //    var b = new Barrier(2);

        //    Task t = Task.Run(() =>
        //    {
        //        lock (obj)
        //        {
        //            b.SignalAndWait();
        //            b.SignalAndWait();
        //        }
        //    });

        //    b.SignalAndWait();
        //    Assert.Throws<SynchronizationLockException>(() => Monitor.Exit(obj));
        //    b.SignalAndWait();

        //    t.Wait();
        //}

        //[TestMethod]
        //public void Monitor_IsEntered_Invalid()
        //{
        //    var obj = new object();
        //    AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.IsEntered(null));
        //}

        [TestMethod]
        public void Monitor_Pulse_Invalid()
        {
            var obj = new object();
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.Pulse(null));
        }

        [TestMethod]
        public void Monitor_PulseAll_Invalid()
        {
            var obj = new object();
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.PulseAll(null));
        }

        [TestMethod]
        public void Monitor_TryEnter_SetsLockTaken()
        {
            bool lockTaken = false;
            var obj = new object();

            Monitor.TryEnter(obj, ref lockTaken);
            Assert.IsTrue(lockTaken);
            Monitor.Exit(obj);
            // The documentation does not specifies that lockTaken variable is
            // set to false after Exit is called
            //Assert.IsFalse(lockTaken);
        }

        [TestMethod]
        public void Monitor_TryEnter_Invalid()
        {
            bool lockTaken = false;
            var obj = new object();

            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.TryEnter(null));
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.TryEnter(null, ref lockTaken));
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.TryEnter(null, 1));
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.TryEnter(null, 1, ref lockTaken));
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.TryEnter(null, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.TryEnter(null, TimeSpan.Zero, ref lockTaken));

            //AssertExtensions.Throws<ArgumentOutOfRangeException>("millisecondsTimeout", () => Monitor.TryEnter(null, -1));
            //AssertExtensions.Throws<ArgumentOutOfRangeException>("millisecondsTimeout", () => Monitor.TryEnter(null, -1, ref lockTaken));
            //AssertExtensions.Throws<ArgumentOutOfRangeException>("timeout", () => Monitor.TryEnter(null, TimeSpan.FromMilliseconds(-1)));
            //AssertExtensions.Throws<ArgumentOutOfRangeException>("timeout", () => Monitor.TryEnter(null, TimeSpan.FromMilliseconds(-1), ref lockTaken));

            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.TryEnter(null, -1));
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.TryEnter(null, -1, ref lockTaken));
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.TryEnter(null, TimeSpan.FromMilliseconds(-1)));
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.TryEnter(null, TimeSpan.FromMilliseconds(-1), ref lockTaken));

            lockTaken = true;
            AssertExtensions.Throws<ArgumentException>("lockTaken", () => Monitor.TryEnter(obj, ref lockTaken));
            lockTaken = true;
            AssertExtensions.Throws<ArgumentException>("lockTaken", () => Monitor.TryEnter(obj, 0, ref lockTaken));
            lockTaken = true;
            AssertExtensions.Throws<ArgumentException>("lockTaken", () => Monitor.TryEnter(obj, TimeSpan.Zero, ref lockTaken));
        }

        [TestMethod]
        public void Monitor_Wait_Invalid()
        {
            var obj = new object();
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.Wait(null));
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.Wait(null, 1));
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.Wait(null, TimeSpan.Zero));
            //AssertExtensions.Throws<ArgumentOutOfRangeException>("millisecondsTimeout", () => Monitor.Wait(null, -1));
            //AssertExtensions.Throws<ArgumentOutOfRangeException>("timeout", () => Monitor.Wait(null, TimeSpan.FromMilliseconds(-1)));
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.Wait(null, -1));
            AssertExtensions.Throws<ArgumentNullException>("obj", () => Monitor.Wait(null, TimeSpan.FromMilliseconds(-1)));
        }

        [TestMethod]
        public void Monitor_WaitTest()
        {
            var obj = new object();
            var waitTests =
                new Func<bool>[]
                {
                    () => Monitor.Wait(obj, FailTimeoutMilliseconds),
                    () => Monitor.Wait(obj, FailTimeoutMilliseconds),
                    () => Monitor.Wait(obj, TimeSpan.FromMilliseconds(FailTimeoutMilliseconds)),
                    () => Monitor.Wait(obj, TimeSpan.FromMilliseconds(FailTimeoutMilliseconds)),
                };

            var t =
                new Thread(() =>
                {
                    Monitor.Enter(obj);
                    for (int i = 0; i < waitTests.Length; ++i)
                    {
                        Monitor.Pulse(obj);
                        Monitor.Wait(obj, FailTimeoutMilliseconds);
                    }
                    Monitor.Exit(obj);
                });
            t.IsBackground = true;

            Monitor.Enter(obj);
            t.Start();
            int counter = 0;
            foreach (var waitTest in waitTests)
            {
                Assert.IsTrue(waitTest(), "#" + counter.ToString());
                Monitor.Pulse(obj);
                counter++;
            }
            Monitor.Exit(obj);
        }
    }
}
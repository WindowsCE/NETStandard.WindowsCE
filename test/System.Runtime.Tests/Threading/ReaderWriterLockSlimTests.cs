// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using System;

#if !WindowsCE
using Mock.System.Threading;
using ReaderWriterLockSlim = Mock.System.Threading.ReaderWriterLockSlim;
using LockRecursionPolicy = Mock.System.Threading.LockRecursionPolicy;
using SynchronizationLockException = Mock.System.Threading.SynchronizationLockException;
using LockRecursionException = Mock.System.Threading.LockRecursionException;
#endif

namespace Tests.Threading
{
    [TestClass]
    public class ReaderWriterLockSlimTests
    {
        [TestMethod]
        public void Ctor()
        {
            ReaderWriterLockSlim rwls;

            using (rwls = new ReaderWriterLockSlim())
            {
                Assert.AreEqual(LockRecursionPolicy.NoRecursion, rwls.RecursionPolicy);
            }

            using (rwls = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion))
            {
                Assert.AreEqual(LockRecursionPolicy.NoRecursion, rwls.RecursionPolicy);
            }

            using (rwls = new ReaderWriterLockSlim((LockRecursionPolicy)12345))
            {
                Assert.AreEqual(LockRecursionPolicy.NoRecursion, rwls.RecursionPolicy);
            }

            using (rwls = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion))
            {
                Assert.AreEqual(LockRecursionPolicy.SupportsRecursion, rwls.RecursionPolicy);
            }
        }

        [TestMethod]
        public void Dispose()
        {
            ReaderWriterLockSlim rwls;

            rwls = new ReaderWriterLockSlim();
            rwls.Dispose();
            AssertExtensions.Throws<ObjectDisposedException>(() => rwls.TryEnterReadLock(0));
            AssertExtensions.Throws<ObjectDisposedException>(() => rwls.TryEnterUpgradeableReadLock(0));
            AssertExtensions.Throws<ObjectDisposedException>(() => rwls.TryEnterWriteLock(0));
            rwls.Dispose();

            for (int i = 0; i < 3; i++)
            {
                rwls = new ReaderWriterLockSlim();
                switch (i)
                {
                    case 0: rwls.EnterReadLock(); break;
                    case 1: rwls.EnterUpgradeableReadLock(); break;
                    case 2: rwls.EnterWriteLock(); break;
                }
                AssertExtensions.Throws<SynchronizationLockException>(() => rwls.Dispose());
            }
        }

        [TestMethod]
        public void EnterExit()
        {
            using (ReaderWriterLockSlim rwls = new ReaderWriterLockSlim())
            {
                Assert.IsFalse(rwls.IsReadLockHeld);
                rwls.EnterReadLock();
                Assert.IsTrue(rwls.IsReadLockHeld);
                rwls.ExitReadLock();
                Assert.IsFalse(rwls.IsReadLockHeld);

                Assert.IsFalse(rwls.IsUpgradeableReadLockHeld);
                rwls.EnterUpgradeableReadLock();
                Assert.IsTrue(rwls.IsUpgradeableReadLockHeld);
                rwls.ExitUpgradeableReadLock();
                Assert.IsFalse(rwls.IsUpgradeableReadLockHeld);

                Assert.IsFalse(rwls.IsWriteLockHeld);
                rwls.EnterWriteLock();
                Assert.IsTrue(rwls.IsWriteLockHeld);
                rwls.ExitWriteLock();
                Assert.IsFalse(rwls.IsWriteLockHeld);

                Assert.IsFalse(rwls.IsUpgradeableReadLockHeld);
                rwls.EnterUpgradeableReadLock();
                Assert.IsFalse(rwls.IsWriteLockHeld);
                Assert.IsTrue(rwls.IsUpgradeableReadLockHeld);
                rwls.EnterWriteLock();
                Assert.IsTrue(rwls.IsWriteLockHeld);
                rwls.ExitWriteLock();
                Assert.IsFalse(rwls.IsWriteLockHeld);
                Assert.IsTrue(rwls.IsUpgradeableReadLockHeld);
                rwls.ExitUpgradeableReadLock();
                Assert.IsFalse(rwls.IsUpgradeableReadLockHeld);

                Assert.IsTrue(rwls.TryEnterReadLock(0));
                rwls.ExitReadLock();

                Assert.IsTrue(rwls.TryEnterReadLock(Timeout2.InfiniteTimeSpan));
                rwls.ExitReadLock();

                Assert.IsTrue(rwls.TryEnterUpgradeableReadLock(0));
                rwls.ExitUpgradeableReadLock();

                Assert.IsTrue(rwls.TryEnterUpgradeableReadLock(Timeout2.InfiniteTimeSpan));
                rwls.ExitUpgradeableReadLock();

                Assert.IsTrue(rwls.TryEnterWriteLock(0));
                rwls.ExitWriteLock();

                Assert.IsTrue(rwls.TryEnterWriteLock(Timeout2.InfiniteTimeSpan));
                rwls.ExitWriteLock();
            }
        }

        [TestMethod]
        public void DeadlockAvoidance()
        {
            using (ReaderWriterLockSlim rwls = new ReaderWriterLockSlim())
            {
                rwls.EnterReadLock();
                AssertExtensions.Throws<LockRecursionException>(() => rwls.EnterReadLock());
                AssertExtensions.Throws<LockRecursionException>(() => rwls.EnterUpgradeableReadLock());
                AssertExtensions.Throws<LockRecursionException>(() => rwls.EnterWriteLock());
                rwls.ExitReadLock();

                rwls.EnterUpgradeableReadLock();
                rwls.EnterReadLock();
                AssertExtensions.Throws<LockRecursionException>(() => rwls.EnterReadLock());
                rwls.ExitReadLock();
                AssertExtensions.Throws<LockRecursionException>(() => rwls.EnterUpgradeableReadLock());
                rwls.EnterWriteLock();
                AssertExtensions.Throws<LockRecursionException>(() => rwls.EnterWriteLock());
                rwls.ExitWriteLock();
                rwls.ExitUpgradeableReadLock();

                rwls.EnterWriteLock();
                AssertExtensions.Throws<LockRecursionException>(() => rwls.EnterReadLock());
                AssertExtensions.Throws<LockRecursionException>(() => rwls.EnterUpgradeableReadLock());
                AssertExtensions.Throws<LockRecursionException>(() => rwls.EnterWriteLock());
                rwls.ExitWriteLock();
            }

            using (ReaderWriterLockSlim rwls = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion))
            {
                rwls.EnterReadLock();
                AssertExtensions.Throws<LockRecursionException>(() => rwls.EnterWriteLock());
                rwls.EnterReadLock();
                AssertExtensions.Throws<LockRecursionException>(() => rwls.EnterUpgradeableReadLock());
                rwls.ExitReadLock();
                rwls.ExitReadLock();

                rwls.EnterUpgradeableReadLock();
                rwls.EnterReadLock();
                rwls.EnterUpgradeableReadLock();
                rwls.ExitUpgradeableReadLock();
                rwls.EnterReadLock();
                rwls.ExitReadLock();
                rwls.ExitReadLock();
                rwls.EnterWriteLock();
                rwls.EnterWriteLock();
                rwls.ExitWriteLock();
                rwls.ExitWriteLock();
                rwls.ExitUpgradeableReadLock();

                rwls.EnterWriteLock();
                rwls.EnterReadLock();
                rwls.ExitReadLock();
                rwls.EnterUpgradeableReadLock();
                rwls.ExitUpgradeableReadLock();
                rwls.EnterWriteLock();
                rwls.ExitWriteLock();
                rwls.ExitWriteLock();
            }
        }

        //[TestMethod]
        public void InvalidExits_Theory()
        {
            InvalidExits(LockRecursionPolicy.NoRecursion);
            InvalidExits(LockRecursionPolicy.SupportsRecursion);
        }

        //[Theory]
        //[InlineData(LockRecursionPolicy.NoRecursion)]
        //[InlineData(LockRecursionPolicy.SupportsRecursion)]
        private static void InvalidExits(LockRecursionPolicy policy)
        {
            using (ReaderWriterLockSlim rwls = new ReaderWriterLockSlim(policy))
            {
                AssertExtensions.Throws<SynchronizationLockException>(() => rwls.ExitReadLock());
                AssertExtensions.Throws<SynchronizationLockException>(() => rwls.ExitUpgradeableReadLock());
                AssertExtensions.Throws<SynchronizationLockException>(() => rwls.ExitWriteLock());

                rwls.EnterReadLock();
                AssertExtensions.Throws<SynchronizationLockException>(() => rwls.ExitUpgradeableReadLock());
                AssertExtensions.Throws<SynchronizationLockException>(() => rwls.ExitWriteLock());
                rwls.ExitReadLock();

                rwls.EnterUpgradeableReadLock();
                AssertExtensions.Throws<SynchronizationLockException>(() => rwls.ExitReadLock());
                AssertExtensions.Throws<SynchronizationLockException>(() => rwls.ExitWriteLock());
                rwls.ExitUpgradeableReadLock();

                rwls.EnterWriteLock();
                AssertExtensions.Throws<SynchronizationLockException>(() => rwls.ExitReadLock());
                AssertExtensions.Throws<SynchronizationLockException>(() => rwls.ExitUpgradeableReadLock());
                rwls.ExitWriteLock();

                using (Barrier barrier = new Barrier(2))
                {
                    Task t = Task.Factory.StartNew(() =>
                    {
                        rwls.EnterWriteLock();
                        barrier.SignalAndWait();
                        barrier.SignalAndWait();
                        rwls.ExitWriteLock();
                    }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    barrier.SignalAndWait();
                    AssertExtensions.Throws<SynchronizationLockException>(() => rwls.ExitWriteLock());
                    barrier.SignalAndWait();

                    t.GetAwaiter().GetResult();
                }
            }
        }

        [TestMethod]
        public void InvalidTimeouts()
        {
            using (ReaderWriterLockSlim rwls = new ReaderWriterLockSlim())
            {
                AssertExtensions.Throws<ArgumentOutOfRangeException>(() => rwls.TryEnterReadLock(-2));
                AssertExtensions.Throws<ArgumentOutOfRangeException>(() => rwls.TryEnterUpgradeableReadLock(-3));
                AssertExtensions.Throws<ArgumentOutOfRangeException>(() => rwls.TryEnterWriteLock(-4));

                AssertExtensions.Throws<ArgumentOutOfRangeException>(() => rwls.TryEnterReadLock(TimeSpan.MaxValue));
                AssertExtensions.Throws<ArgumentOutOfRangeException>(() => rwls.TryEnterUpgradeableReadLock(TimeSpan.MinValue));
                AssertExtensions.Throws<ArgumentOutOfRangeException>(() => rwls.TryEnterWriteLock(TimeSpan.FromMilliseconds(-2)));
            }
        }

        [TestMethod]
        public void WritersAreMutuallyExclusiveFromReaders()
        {
            using (Barrier barrier = new Barrier(2))
            using (ReaderWriterLockSlim rwls = new ReaderWriterLockSlim())
            {
                Task.WaitAll(
                    Task.Run(() =>
                    {
                        rwls.EnterWriteLock();
                        barrier.SignalAndWait();
                        Assert.IsTrue(rwls.IsWriteLockHeld);
                        barrier.SignalAndWait();
                        rwls.ExitWriteLock();
                    }),
                    Task.Run(() =>
                    {
                        barrier.SignalAndWait();
                        Assert.IsFalse(rwls.TryEnterReadLock(0));
                        Assert.IsFalse(rwls.IsReadLockHeld);
                        barrier.SignalAndWait();
                    }));
            }
        }

        [TestMethod]
        public void WritersAreMutuallyExclusiveFromWriters()
        {
            using (Barrier barrier = new Barrier(2))
            using (ReaderWriterLockSlim rwls = new ReaderWriterLockSlim())
            {
                Task.WaitAll(
                    Task.Run(() =>
                    {
                        rwls.EnterWriteLock();
                        barrier.SignalAndWait();
                        Assert.IsTrue(rwls.IsWriteLockHeld);
                        barrier.SignalAndWait();
                        rwls.ExitWriteLock();
                    }),
                    Task.Run(() =>
                    {
                        barrier.SignalAndWait();
                        Assert.IsFalse(rwls.TryEnterWriteLock(0));
                        Assert.IsFalse(rwls.IsReadLockHeld);
                        barrier.SignalAndWait();
                    }));
            }
        }

        [TestMethod]
        public void ReadersMayBeConcurrent()
        {
            using (Barrier barrier = new Barrier(2))
            using (ReaderWriterLockSlim rwls = new ReaderWriterLockSlim())
            {
                Assert.AreEqual(0, rwls.CurrentReadCount);
                Task.WaitAll(
                    Task.Run(() =>
                    {
                        rwls.EnterReadLock();
                        barrier.SignalAndWait(); // 1
                        Assert.IsTrue(rwls.IsReadLockHeld);
                        barrier.SignalAndWait(); // 2
                        Assert.AreEqual(2, rwls.CurrentReadCount);
                        barrier.SignalAndWait(); // 3
                        barrier.SignalAndWait(); // 4
                        rwls.ExitReadLock();
                    }),
                    Task.Run(() =>
                    {
                        barrier.SignalAndWait(); // 1
                        rwls.EnterReadLock();
                        barrier.SignalAndWait(); // 2
                        Assert.IsTrue(rwls.IsReadLockHeld);
                        Assert.AreEqual(0, rwls.WaitingReadCount);
                        barrier.SignalAndWait(); // 3
                        rwls.ExitReadLock();
                        barrier.SignalAndWait(); // 4
                    }));
                Assert.AreEqual(0, rwls.CurrentReadCount);
            }
        }

        [TestMethod]
        public void WriterToWriterChain()
        {
            using (AutoResetEvent are = new AutoResetEvent(false))
            using (ReaderWriterLockSlim rwls = new ReaderWriterLockSlim())
            {
                rwls.EnterWriteLock();
                Task t = Task.Factory.StartNew(() =>
                {
                    Assert.IsFalse(rwls.TryEnterWriteLock(10));
                    Task.Run(() => are.Set()); // ideally this won't fire until we've called EnterWriteLock, but it's a benign race in that the test will succeed either way
                    rwls.EnterWriteLock();
                    rwls.ExitWriteLock();
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                are.WaitOne();
                rwls.ExitWriteLock();
                t.GetAwaiter().GetResult();
            }
        }

        [TestMethod]
        public void WriterToReaderChain()
        {
            using (AutoResetEvent are = new AutoResetEvent(false))
            using (ReaderWriterLockSlim rwls = new ReaderWriterLockSlim())
            {
                rwls.EnterWriteLock();
                Task t = Task.Factory.StartNew(() =>
                {
                    Assert.IsFalse(rwls.TryEnterReadLock(TimeSpan.FromMilliseconds(10)));
                    Task.Run(() => are.Set()); // ideally this won't fire until we've called EnterReadLock, but it's a benign race in that the test will succeed either way
                    rwls.EnterReadLock();
                    rwls.ExitReadLock();
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                are.WaitOne();
                rwls.ExitWriteLock();
                t.GetAwaiter().GetResult();
            }
        }

        [TestMethod]
        public void WriterToUpgradeableReaderChain()
        {
            using (AutoResetEvent are = new AutoResetEvent(false))
            using (ReaderWriterLockSlim rwls = new ReaderWriterLockSlim())
            {
                rwls.EnterWriteLock();
                Task t = Task.Factory.StartNew(() =>
                {
                    Assert.IsFalse(rwls.TryEnterUpgradeableReadLock(TimeSpan.FromMilliseconds(10)));
                    Task.Run(() => are.Set()); // ideally this won't fire until we've called EnterReadLock, but it's a benign race in that the test will succeed either way
                    rwls.EnterUpgradeableReadLock();
                    rwls.ExitUpgradeableReadLock();
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                are.WaitOne();
                rwls.ExitWriteLock();
                t.GetAwaiter().GetResult();
            }
        }

        [TestMethod]
        //[OuterLoop]
        //[SkipOnTargetFramework(TargetFrameworkMonikers.NetFramework, "Hangs in desktop, issue dotnet/corefx#3364 is not fixed there")]
        public void ReleaseReadersWhenWaitingWriterTimesOut()
        {
            using (var rwls = new ReaderWriterLockSlim())
            {
                // Enter the read lock
                rwls.EnterReadLock();
                // Typical order of execution: 0

                Thread writeWaiterThread;
                using (var beforeTryEnterWriteLock = new ManualResetEvent(false))
                {
                    writeWaiterThread =
                        new Thread(() =>
                        {
                            // Typical order of execution: 1

                            // Add a writer to the wait list for enough time to allow successive readers to enter the wait list while this
                            // writer is waiting
                            beforeTryEnterWriteLock.Set();
                            if (rwls.TryEnterWriteLock(1000))
                            {
                                // The typical order of execution is not guaranteed, as sleep times are not guaranteed. For
                                // instance, before this write lock is added to the wait list, the two new read locks may be
                                // acquired. In that case, the test may complete before or while the write lock is taken.
                                rwls.ExitWriteLock();
                            }

                            // Typical order of execution: 4
                        });
                    writeWaiterThread.IsBackground = true;
                    writeWaiterThread.Start();
                    beforeTryEnterWriteLock.WaitOne();
                }
                Thread.Sleep(500); // wait for TryEnterWriteLock to enter the wait list

                // A writer should now be waiting, add readers to the wait list. Since a read lock is still acquired, the writer
                // should time out waiting, then these readers should enter and exit the lock.
                ThreadStart EnterAndExitReadLock = () =>
                {
                    // Typical order of execution: 2, 3
                    rwls.EnterReadLock();
                    // Typical order of execution: 5, 6
                    rwls.ExitReadLock();
                };
                var readerThreads =
                    new Thread[]
                    {
                        new Thread(EnterAndExitReadLock),
                        new Thread(EnterAndExitReadLock)
                    };
                foreach (var readerThread in readerThreads)
                {
                    readerThread.IsBackground = true;
                    readerThread.Start();
                }
                foreach (var readerThread in readerThreads)
                {
                    readerThread.Join();
                }

                rwls.ExitReadLock();
                // Typical order of execution: 7

                writeWaiterThread.Join();
            }
        }

        [TestMethod]
        //[OuterLoop]
        public void DontReleaseWaitingReadersWhenThereAreWaitingWriters()
        {
            using (var rwls = new ReaderWriterLockSlim())
            {
                rwls.EnterUpgradeableReadLock();
                rwls.EnterWriteLock();
                // Typical order of execution: 0

                // Add a waiting writer
                var threads = new Thread[2];
                using (var beforeEnterWriteLock = new ManualResetEvent(false))
                {
                    var thread =
                        new Thread(() =>
                        {
                            beforeEnterWriteLock.Set();
                            rwls.EnterWriteLock();
                            // Typical order of execution: 3
                            rwls.ExitWriteLock();
                        });
                    thread.IsBackground = true;
                    thread.Start();
                    threads[0] = thread;
                    beforeEnterWriteLock.WaitOne();
                }

                // Add a waiting reader
                using (var beforeEnterReadLock = new ManualResetEvent(false))
                {
                    var thread =
                        new Thread(() =>
                        {
                            beforeEnterReadLock.Set();
                            rwls.EnterReadLock();
                            // Typical order of execution: 4
                            rwls.ExitReadLock();
                        });
                    thread.IsBackground = true;
                    thread.Start();
                    threads[1] = thread;
                    beforeEnterReadLock.WaitOne();
                }

                // Wait for the background threads to block waiting for their locks
                Thread.Sleep(1000);

                // Typical order of execution: 1
                rwls.ExitWriteLock();
                // At this point there is still one reader and one waiting writer, so the reader-writer lock should not try to
                // release any of the threads waiting for a lock

                // Typical order of execution: 2
                rwls.ExitUpgradeableReadLock();
                // At this point, the waiting writer should be released, and the waiting reader should not

                foreach (var thread in threads)
                    thread.Join();
                // Typical order of execution: 5
            }
        }
    }
}
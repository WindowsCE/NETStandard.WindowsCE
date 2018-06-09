// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#if !WindowsCE
using OperationCanceledException = Mock.System.OperationCanceledException;
#endif

namespace Tests.Runtime.CompilerServices
{
    [TestClass]
    public class AsyncTaskMethodBuilderTests
    {
        // Test captured sync context with successful completion (SetResult)
        [TestMethod]
        public void VoidMethodBuilder_TrackedContext()
        {
            SynchronizationContext previousContext = SynchronizationContext.Current;
            try
            {
                var trackedContext = new TrackOperationsSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(trackedContext);

                // TrackedCount should increase as Create() is called, and decrease as SetResult() is called.

                // Completing in opposite order as created.
                var avmb1 = AsyncVoidMethodBuilder.Create();
                //Assert.AreEqual(1, trackedContext.TrackedCount);
                var avmb2 = AsyncVoidMethodBuilder.Create();
                //Assert.AreEqual(2, trackedContext.TrackedCount);
                avmb2.SetResult();
                //Assert.AreEqual(1, trackedContext.TrackedCount);
                avmb1.SetResult();
                //Assert.AreEqual(0, trackedContext.TrackedCount);

                // Completing in same order as created
                avmb1 = AsyncVoidMethodBuilder.Create();
                //Assert.AreEqual(1, trackedContext.TrackedCount);
                avmb2 = AsyncVoidMethodBuilder.Create();
                //Assert.AreEqual(2, trackedContext.TrackedCount);
                avmb1.SetResult();
                //Assert.AreEqual(1, trackedContext.TrackedCount);
                avmb2.SetResult();
                //Assert.AreEqual(0, trackedContext.TrackedCount);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        // Test not having a sync context with successful completion (SetResult)
        [TestMethod]
        public void VoidMethodBuilder_NoContext()
        {
            SynchronizationContext previousContext = SynchronizationContext.Current;
            try
            {
                // Make sure not having a sync context doesn't cause us to blow up
                SynchronizationContext.SetSynchronizationContext(null);
                var avmb = AsyncVoidMethodBuilder.Create();
                avmb.SetResult();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        // AsyncTaskMethodBuilder
        [TestMethod]
        public void TaskMethodBuilder_Basic()
        {
            // Creating a task builder, building it, completing it successfully
            {
                var atmb = AsyncTaskMethodBuilder.Create();
                var t = atmb.Task;
                Assert.AreEqual(TaskStatus.WaitingForActivation, t.Status);
                atmb.SetResult();
                Assert.AreEqual(TaskStatus.RanToCompletion, t.Status);
            }
        }

        [TestMethod]
        public void TaskMethodBuilder_DoesNotTouchSyncContext()
        {
            // Verify that AsyncTaskMethodBuilder is not touching sync context
            SynchronizationContext previousContext = SynchronizationContext.Current;
            try
            {
                var trackedContext = new TrackOperationsSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(trackedContext);

                var atmb = AsyncTaskMethodBuilder.Create();
                //Assert.AreEqual(0, trackedContext.TrackedCount);
                atmb.SetResult();
                //Assert.AreEqual(0, trackedContext.TrackedCount);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        // AsyncTaskMethodBuilder<T>

        [TestMethod]
        public void TaskMethodBuilderT_Basic()
        {
            // Creating a task builder, building it, completing it successfully
            var atmb = AsyncTaskMethodBuilder<int>.Create();
            var t = atmb.Task;
            Assert.AreEqual(TaskStatus.WaitingForActivation, t.Status);
            atmb.SetResult(43);
            Assert.AreEqual(TaskStatus.RanToCompletion, t.Status);
            Assert.AreEqual(43, t.Result);
        }

        [TestMethod]
        public void TaskMethodBuilderT_DoesNotTouchSyncContext()
        {
            // Verify that AsyncTaskMethodBuilder<T> is not touching sync context
            SynchronizationContext previousContext = SynchronizationContext.Current;
            try
            {
                var trackedContext = new TrackOperationsSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(trackedContext);

                var atmb = AsyncTaskMethodBuilder<string>.Create();
                //Assert.AreEqual(0, trackedContext.TrackedCount);
                atmb.SetResult("async");
                //Assert.AreEqual(0, trackedContext.TrackedCount);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        // Incorrect usage for AsyncTaskMethodBuilder
        [TestMethod]
        public void TaskMethodBuilder_IncorrectUsage()
        {
            var atmb = new AsyncTaskMethodBuilder();
            AssertExtensions.Throws<ArgumentNullException>(() => { atmb.SetException(null); });
        }

        // Incorrect usage for AsyncVoidMethodBuilder
        [TestMethod]
        public void VoidMethodBuilder_IncorrectUsage()
        {
            var avmb = AsyncVoidMethodBuilder.Create();
            AssertExtensions.Throws<ArgumentNullException>(() => { avmb.SetException(null); });
            avmb.SetResult();
        }

        // Creating a task builder, building it, completing it successfully, and making sure it can't be reset
        [TestMethod]
        public void TaskMethodBuilder_CantBeReset()
        {
            var atmb = AsyncTaskMethodBuilder.Create();
            atmb.SetResult();
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetResult(); });
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetException(new Exception()); });
        }

        // Incorrect usage for AsyncTaskMethodBuilder<T>
        [TestMethod]
        public void TaskMethodBuilderT_IncorrectUsage()
        {
            var atmb = new AsyncTaskMethodBuilder<int>();
            AssertExtensions.Throws<ArgumentNullException>(() => { atmb.SetException(null); });
        }

        // Creating a task builder <T>, building it, completing it successfully, and making sure it can't be reset
        [TestMethod]
        public void TaskMethodBuilderT_CantBeReset()
        {
            var atmb = AsyncTaskMethodBuilder<int>.Create();
            atmb.SetResult(43);
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetResult(44); });
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetException(new Exception()); });
        }

        // Creating a task builder, building it, completing it faulted, and making sure it can't be reset
        [TestMethod]
        public void TaskMethodBuilder_SetException_CantBeReset0()
        {
            var atmb = AsyncTaskMethodBuilder.Create();
            var t = atmb.Task;
            Assert.AreEqual(TaskStatus.WaitingForActivation, t.Status);
            atmb.SetException(new InvalidCastException());
            Assert.AreEqual(TaskStatus.Faulted, t.Status);
            Assert.IsTrue(t.Exception.InnerException is InvalidCastException, "Wrong exception found in builder (ATMB, build then fault)");
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetResult(); });
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetException(new Exception()); });
        }

        // Creating a task builder, completing it faulted, building it, and making sure it can't be reset
        [TestMethod]
        public void TaskMethodBuilder_SetException_CantBeReset1()
        {
            var atmb = AsyncTaskMethodBuilder.Create();
            atmb.SetException(new InvalidCastException());
            var t = atmb.Task;
            Assert.AreEqual(TaskStatus.Faulted, t.Status);
            Assert.IsTrue(t.Exception.InnerException is InvalidCastException, "Wrong exception found in builder (ATMB, fault then build)");
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetResult(); });
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetException(new Exception()); });
        }

        // Test cancellation
        [TestMethod]
        public void TaskMethodBuilder_Cancellation()
        {
            var atmb = AsyncTaskMethodBuilder.Create();
            var oce = new OperationCanceledException();
            atmb.SetException(oce);
            var t = atmb.Task;
            Assert.AreEqual(TaskStatus.Canceled, t.Status);

            OperationCanceledException caught = AssertExtensions.Throws<OperationCanceledException>(() =>
            {
                t.GetAwaiter().GetResult();
            });
            //Assert.AreSame(oce, caught);
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetResult(); });
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetException(new Exception()); });
        }

        [TestMethod]
        public void AsyncMethodBuilderCreate_SetExceptionTest2()
        {
            // Test captured sync context with exceptional completion

            SynchronizationContext previousContext = SynchronizationContext.Current;
            try
            {
                var trackedContext = new TrackOperationsSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(trackedContext);

                // Completing in opposite order as created
                var avmb1 = AsyncVoidMethodBuilder.Create();
                //Assert.AreEqual(1, trackedContext.TrackedCount);
                var avmb2 = AsyncVoidMethodBuilder.Create();
                //Assert.AreEqual(2, trackedContext.TrackedCount);
                avmb2.SetException(new InvalidOperationException("uh oh 1"));
                //Assert.AreEqual(1, trackedContext.TrackedCount);
                avmb1.SetException(new InvalidCastException("uh oh 2"));
                //Assert.AreEqual(0, trackedContext.TrackedCount);

                Assert.AreEqual(2, trackedContext.PostExceptions.Count);
                AssertExtensions.IsType<InvalidOperationException>(trackedContext.PostExceptions[0]);
                AssertExtensions.IsType<InvalidCastException>(trackedContext.PostExceptions[1]);

                // Completing in same order as created
                var avmb3 = AsyncVoidMethodBuilder.Create();
                //Assert.AreEqual(1, trackedContext.TrackedCount);
                var avmb4 = AsyncVoidMethodBuilder.Create();
                //Assert.AreEqual(2, trackedContext.TrackedCount);
                avmb3.SetException(new InvalidOperationException("uh oh 3"));
                //Assert.AreEqual(1, trackedContext.TrackedCount);
                avmb4.SetException(new InvalidCastException("uh oh 4"));
                //Assert.AreEqual(0, trackedContext.TrackedCount);

                Assert.AreEqual(4, trackedContext.PostExceptions.Count);
                AssertExtensions.IsType<InvalidOperationException>(trackedContext.PostExceptions[2]);
                AssertExtensions.IsType<InvalidCastException>(trackedContext.PostExceptions[3]);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        // Creating a task builder, building it, completing it faulted, and making sure it can't be reset
        [TestMethod]
        public void TaskMethodBuilderT_SetExceptionTest0()
        {
            var atmb = AsyncTaskMethodBuilder<int>.Create();
            var t = atmb.Task;
            Assert.AreEqual(TaskStatus.WaitingForActivation, t.Status);
            atmb.SetException(new InvalidCastException());
            Assert.AreEqual(TaskStatus.Faulted, t.Status);
            AssertExtensions.IsType<InvalidCastException>(t.Exception.InnerException);
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetResult(44); });
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetException(new Exception()); });
        }

        // Creating a task builder, completing it faulted, building it, and making sure it can't be reset
        [TestMethod]
        public void TaskMethodBuilderT_SetExceptionTest1()
        {
            var atmb = AsyncTaskMethodBuilder<int>.Create();
            atmb.SetException(new InvalidCastException());
            var t = atmb.Task;
            Assert.AreEqual(TaskStatus.Faulted, t.Status);
            AssertExtensions.IsType<InvalidCastException>(t.Exception.InnerException);
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetResult(44); });
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetException(new Exception()); });
        }

        // Test cancellation
        [TestMethod]
        public void TaskMethodBuilderT_Cancellation()
        {
            var atmb = AsyncTaskMethodBuilder<int>.Create();
            var oce = new OperationCanceledException();
            atmb.SetException(oce);
            var t = atmb.Task;
            Assert.AreEqual(TaskStatus.Canceled, t.Status);

            OperationCanceledException e = AssertExtensions.Throws<OperationCanceledException>(() =>
            {
                t.GetAwaiter().GetResult();
            });
            //Assert.AreSame(oce, e);
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetResult(44); });
            AssertExtensions.Throws<InvalidOperationException>(() => { atmb.SetException(new Exception()); });
        }

        [TestMethod]
        public void TaskMethodBuilder_TaskIsCached()
        {
            var atmb = new AsyncTaskMethodBuilder();
            var t1 = atmb.Task;
            var t2 = atmb.Task;
            Assert.IsNotNull(t1);
            Assert.IsNotNull(t2);
            Assert.AreSame(t1, t2);
        }

        [TestMethod]
        public void TaskMethodBuilderT_TaskIsCached()
        {
            var atmb = new AsyncTaskMethodBuilder<int>();
            var t1 = atmb.Task;
            var t2 = atmb.Task;
            Assert.IsNotNull(t1);
            Assert.IsNotNull(t2);
            Assert.AreSame(t1, t2);
        }

        [TestMethod]
        //[ActiveIssue("TFS 450361 - Codegen optimization issue", TargetFrameworkMonikers.UapAot)]
        public void TaskMethodBuilder_UsesCompletedCache()
        {
            var atmb1 = new AsyncTaskMethodBuilder();
            var atmb2 = new AsyncTaskMethodBuilder();
            atmb1.SetResult();
            atmb2.SetResult();
            Assert.AreSame(atmb1.Task, atmb2.Task);
        }

        //[TestMethod]
        public void TaskMethodBuilderBoolean_UsesCompletedCache_Theory()
        {
            TaskMethodBuilderBoolean_UsesCompletedCache(true);
            TaskMethodBuilderBoolean_UsesCompletedCache(false);
        }

        //[ActiveIssue("TFS 450361 - Codegen optimization issue", TargetFrameworkMonikers.UapAot)]
        private static void TaskMethodBuilderBoolean_UsesCompletedCache(bool result)
        {
            TaskMethodBuilderT_UsesCompletedCache(result, true);
        }

        //[TestMethod]
        public void TaskMethodBuilderInt32_UsesCompletedCache_Theory()
        {
            TaskMethodBuilderInt32_UsesCompletedCache(0, true);
            TaskMethodBuilderInt32_UsesCompletedCache(5, true);
            TaskMethodBuilderInt32_UsesCompletedCache(-5, false);
            TaskMethodBuilderInt32_UsesCompletedCache(42, false);
        }

        //[ActiveIssue("TFS 450361 - Codegen optimization issue", TargetFrameworkMonikers.UapAot)]
        private static void TaskMethodBuilderInt32_UsesCompletedCache(int result, bool shouldBeCached)
        {
            TaskMethodBuilderT_UsesCompletedCache(result, shouldBeCached);
        }

        //[TestMethod]
        public void TaskMethodBuilderRef_UsesCompletedCache_Theory()
        {
            TaskMethodBuilderRef_UsesCompletedCache((string)null, true);
            TaskMethodBuilderRef_UsesCompletedCache("test", false);

        }

        //[ActiveIssue("TFS 450361 - Codegen optimization issue", TargetFrameworkMonikers.UapAot)]
        private static void TaskMethodBuilderRef_UsesCompletedCache(string result, bool shouldBeCached)
        {
            TaskMethodBuilderT_UsesCompletedCache(result, shouldBeCached);
        }

        private static void TaskMethodBuilderT_UsesCompletedCache<T>(T result, bool shouldBeCached)
        {
            var atmb1 = new AsyncTaskMethodBuilder<T>();
            var atmb2 = new AsyncTaskMethodBuilder<T>();

            atmb1.SetResult(result);
            atmb2.SetResult(result);

            Assert.AreEqual(shouldBeCached, object.ReferenceEquals(atmb1.Task, atmb2.Task));
        }

        [TestMethod]
        public void Tcs_ValidateFaultedTask()
        {
            var tcs = new TaskCompletionSource<int>();
            try { throw new InvalidOperationException(); }
            catch (Exception e) { tcs.SetException(e); }
            ValidateFaultedTask(tcs.Task);
        }

        [TestMethod]
        public void TaskMethodBuilder_ValidateFaultedTask()
        {
            var atmb = AsyncTaskMethodBuilder.Create();
            try { throw new InvalidOperationException(); }
            catch (Exception e) { atmb.SetException(e); }
            ValidateFaultedTask(atmb.Task);
        }

        [TestMethod]
        public void TaskMethodBuilderT_ValidateFaultedTask()
        {
            var atmbtr = AsyncTaskMethodBuilder<object>.Create();
            try { throw new InvalidOperationException(); }
            catch (Exception e) { atmbtr.SetException(e); }
            ValidateFaultedTask(atmbtr.Task);
        }

        [TestMethod]
        public void TrackedSyncContext_ValidateException()
        {
            SynchronizationContext previousContext = SynchronizationContext.Current;
            try
            {
                var tosc = new TrackOperationsSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(tosc);
                var avmb = AsyncVoidMethodBuilder.Create();
                try { throw new InvalidOperationException(); }
                catch (Exception exc) { avmb.SetException(exc); }
                //Assert.NotEmpty(tosc.PostExceptions);
                Assert.IsNotNull(tosc.PostExceptions);
                Assert.IsTrue(tosc.PostExceptions.Count > 0);
                ValidateException(tosc.PostExceptions[0]);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        // Running tasks with exceptions.
        [TestMethod]
        public void FaultedTaskExceptions()
        {
            var twa1 = Task.Run(() => { throw new Exception("uh oh"); });
            var twa2 = Task.Factory.StartNew(() => { throw new Exception("uh oh"); });
            var tasks = new Task[]
            {
                Task.Run(() => { throw new Exception("uh oh"); }),
                Task.Factory.StartNew<int>(() => { throw new Exception("uh oh"); }),
                Task.WhenAll(Task.Run(() => { throw new Exception("uh oh"); }), Task.Run(() => { throw new Exception("uh oh"); })),
                Task.WhenAll<int>(Task.Run(new Func<int>(() => { throw new Exception("uh oh"); })), Task.Run(new Func<int>(() => { throw new Exception("uh oh"); }))),
                Task.WhenAny(twa1, twa2).Unwrap(),
                Task.WhenAny<int>(Task.Run(new Func<Task<int>>(() => { throw new Exception("uh oh"); }))).Unwrap(),
                Task.Factory.StartNew(() => Task.Factory.StartNew(() => { throw new Exception("uh oh"); })).Unwrap(),
                Task.Factory.StartNew<Task<int>>(() => Task.Factory.StartNew<int>(() => { throw new Exception("uh oh"); })).Unwrap(),
                Task.Run(() => Task.Run(() => { throw new Exception("uh oh"); })),
                Task.Run(() => Task.Run(new Func<int>(() => { throw new Exception("uh oh"); }))),
                Task.Run(new Func<Task>(() => { throw new Exception("uh oh"); })),
                Task.Run(new Func<Task<int>>(() => { throw new Exception("uh oh"); }))
            };

            for (int i = 0; i < tasks.Length; i++)
            {
                ValidateFaultedTask(tasks[i]);
            }

            ((IAsyncResult)twa1).AsyncWaitHandle.WaitOne();
            ((IAsyncResult)twa2).AsyncWaitHandle.WaitOne();
            Exception ignored = twa1.Exception;
            ignored = twa2.Exception;
        }

        // TODO: Implement Barrier class
        // Test that OCEs don't result in the unobserved event firing
        [TestMethod]
        public void CancellationDoesntResultInEventFiring()
        {
            var cts = new CancellationTokenSource();
            var oce = new OperationCanceledException(cts.Token);

            // A Task that throws an exception to cancel
            var b = new Barrier(2);
            Task t1 = Task.Factory.StartNew(() =>
            {
                b.SignalAndWait();
                b.SignalAndWait();
                throw oce;
            }, cts.Token);
            b.SignalAndWait(); // make sure task is started before we cancel
            cts.Cancel();
            b.SignalAndWait(); // release task to complete

            // This test may be run concurrently with other tests in the suite, 
            // which can be problematic as TaskScheduler.UnobservedTaskException
            // is global state.  The handler is carefully written to be non-problematic
            // if it happens to be set during the execution of another test that has 
            // an unobserved exception.
            //EventHandler<UnobservedTaskExceptionEventArgs> handler =
            //    (s, e) => Assert.DoesNotContain(oce, e.Exception.InnerExceptions);
            //TaskScheduler.UnobservedTaskException += handler;
            ((IAsyncResult)t1).AsyncWaitHandle.WaitOne();
            t1 = null;
            for (int i = 0; i < 10; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            //TaskScheduler.UnobservedTaskException -= handler;
        }

        #region Helper Methods / Classes

        private static void ValidateFaultedTask(Task t)
        {
            ((IAsyncResult)t).AsyncWaitHandle.WaitOne();
            Assert.IsTrue(t.IsFaulted);
            Exception e = AssertExtensions.ThrowsAny<Exception>(() =>
            {
                t.GetAwaiter().GetResult();
            });
            ValidateException(e);
        }

        private static void ValidateException(Exception e)
        {
            Assert.IsNotNull(e);
            Assert.IsNotNull(e.StackTrace);
            //Assert.Contains("End of stack trace", e.StackTrace);
            //Assert.IsTrue(e.StackTrace.IndexOf("End of stack trace") > -1);
        }

        private class TrackOperationsSynchronizationContext : SynchronizationContext
        {
            //private int _trackedCount;
            private int _postCount;
            //ConcurrentQueue
            private List<Exception> _postExceptions = new List<Exception>();

            //public int TrackedCount { get { return _trackedCount; } }
            public List<Exception> PostExceptions
            {
                get
                {
                    List<Exception> returnValue;
                    lock (_postExceptions)
                    {
                        returnValue = new List<Exception>(_postExceptions);
                        return returnValue;
                    }
                }
            }
            public int PostCount { get { return _postCount; } }

            //public override void OperationStarted() { Interlocked.Increment(ref _trackedCount); }
            //public override void OperationCompleted() { Interlocked.Decrement(ref _trackedCount); }

            public override void Post(SendOrPostCallback callback, object state)
            {
                try
                {
                    Interlocked.Increment(ref _postCount);
                    callback(state);
                }
                catch (Exception exc)
                {
                    lock (_postExceptions)
                    {
                        _postExceptions.Add(exc);
                    }
                }
            }
        }

        #endregion
    }
}
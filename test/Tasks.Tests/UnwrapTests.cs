// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#if NET35_CF
using InternalOCE = System.OperationCanceledException;
#else
using InternalOCE = Mock.System.OperationCanceledException;
#endif

// Regexes
// \[Theory\]\r\n +\[MemberData\(nameof\((\w+)\)\)\]\r\n( +)public void (\w+)\(
// [TestMethod]\r\n$2public void $3_Theory()\r\n$2{\r\n$2$2foreach (var fact in $1)\r\n$2$2$2$3();\r\n$2}\r\n\r\n$2private static void $3(

namespace Tests
{
    [TestClass]
    public class UnwrapTests
    {
        /// <summary>Tests unwrap argument validation.</summary>
        [TestMethod]
        public void TaskUnwrap_ArgumentValidation()
        {
            AssertExtensions.Throws<ArgumentNullException>(() => { ((Task<Task>)null).Unwrap(); });
            AssertExtensions.Throws<ArgumentNullException>(() => { ((Task<Task<int>>)null).Unwrap(); });
            AssertExtensions.Throws<ArgumentNullException>(() => { ((Task<Task<string>>)null).Unwrap(); });
        }

        [TestMethod]
        public void TaskUnwrap_NonGeneric_Completed_Completed_Theory()
        {
            foreach (var fact in CompletedNonGenericTasks)
                NonGeneric_Completed_Completed((Task)fact[0]);
        }

        /// <summary>
        /// Tests Unwrap when both the outer task and non-generic inner task have completed by the time Unwrap is called.
        /// </summary>
        /// <param name="inner">Will be run with a RanToCompletion, Faulted, and Canceled task.</param>
        private static void NonGeneric_Completed_Completed(Task inner)
        {
            Task<Task> outer = Task.FromResult(inner);
            Task unwrappedInner = outer.Unwrap();
            Assert.IsTrue(unwrappedInner.IsCompleted);
            AssertTasksAreEqual(inner, unwrappedInner);
        }

        [TestMethod]
        public void TaskUnwrap_NonGeneric_Completed_Completed_OptimizeToUseSameInner_Theory()
        {
            foreach (var fact in CompletedNonGenericTasks)
                NonGeneric_Completed_Completed_OptimizeToUseSameInner((Task)fact[0]);
        }

        /// <summary>
        /// Tests Unwrap when both the outer task and non-generic inner task have completed by the time Unwrap is called.
        /// </summary>
        /// <param name="inner">Will be run with a RanToCompletion, Faulted, and Canceled task.</param>
        //[SkipOnTargetFramework(TargetFrameworkMonikers.NetFramework, ".NET Core optimization to return the exact same object")]
        private static void NonGeneric_Completed_Completed_OptimizeToUseSameInner(Task inner)
        {
            Task<Task> outer = Task.FromResult(inner);
            Task unwrappedInner = outer.Unwrap();
            Assert.IsTrue(unwrappedInner.IsCompleted);
            Assert.AreSame(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when both the outer task and generic inner task have completed by the time Unwrap is called.
        /// </summary>
        /// <param name="inner">The inner task.</param>
        [TestMethod]
        public void TaskUnwrap_Generic_Completed_Completed_Theory()
        {
            foreach (var fact in CompletedStringTasks)
                Generic_Completed_Completed((Task<string>)fact[0]);
        }

        private static void Generic_Completed_Completed(Task<string> inner)
        {
            Task<Task<string>> outer = Task.FromResult(inner);
            Task<string> unwrappedInner = outer.Unwrap();
            Assert.IsTrue(unwrappedInner.IsCompleted);
            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when both the outer task and generic inner task have completed by the time Unwrap is called.
        /// </summary>
        /// <param name="inner">The inner task.</param>
        //[SkipOnTargetFramework(TargetFrameworkMonikers.NetFramework, ".NET Core optimization to return the exact same object")]
        [TestMethod]
        public void TaskUnwrap_Generic_Completed_Completed_OptimizeToUseSameInner_Theory()
        {
            foreach (var fact in CompletedStringTasks)
                Generic_Completed_Completed_OptimizeToUseSameInner((Task<string>)fact[0]);
        }

        private static void Generic_Completed_Completed_OptimizeToUseSameInner(Task<string> inner)
        {
            Task<Task<string>> outer = Task.FromResult(inner);
            Task<string> unwrappedInner = outer.Unwrap();
            Assert.IsTrue(unwrappedInner.IsCompleted);
            Assert.AreSame(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when the non-generic inner task has completed but the outer task has not completed by the time Unwrap is called.
        /// </summary>
        /// <param name="inner">The inner task.</param>
        [TestMethod]
        public void TaskUnwrap_NonGeneric_NotCompleted_Completed_Theory()
        {
            foreach (var fact in CompletedNonGenericTasks)
                NonGeneric_NotCompleted_Completed((Task)fact[0]);
        }

        private static void NonGeneric_NotCompleted_Completed(Task inner)
        {
            var outerTcs = new TaskCompletionSource<Task>();
            Task<Task> outer = outerTcs.Task;

            Task unwrappedInner = outer.Unwrap();
            Assert.IsFalse(unwrappedInner.IsCompleted);

            outerTcs.SetResult(inner);
            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when the generic inner task has completed but the outer task has not completed by the time Unwrap is called.
        /// </summary>
        /// <param name="inner">The inner task.</param>
        [TestMethod]
        public void TaskUnwrap_Generic_NotCompleted_Completed_Theory()
        {
            foreach (var fact in CompletedStringTasks)
                Generic_NotCompleted_Completed((Task<string>)fact[0]);
        }

        private static void Generic_NotCompleted_Completed(Task<string> inner)
        {
            var outerTcs = new TaskCompletionSource<Task<string>>();
            Task<Task<string>> outer = outerTcs.Task;

            Task<string> unwrappedInner = outer.Unwrap();
            Assert.IsFalse(unwrappedInner.IsCompleted);

            outerTcs.SetResult(inner);
            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when the non-generic inner task has not yet completed but the outer task has completed by the time Unwrap is called.
        /// </summary>
        /// <param name="innerStatus">How the inner task should be completed.</param>
        [TestMethod]
        public void TaskUnwrap_NonGeneric_Completed_NotCompleted_Theory()
        {
            NonGeneric_Completed_NotCompleted(TaskStatus.RanToCompletion);
            NonGeneric_Completed_NotCompleted(TaskStatus.Faulted);
            NonGeneric_Completed_NotCompleted(TaskStatus.Canceled);
        }

        private static void NonGeneric_Completed_NotCompleted(TaskStatus innerStatus)
        {
            var innerTcs = new TaskCompletionSource<bool>();
            Task inner = innerTcs.Task;

            Task<Task> outer = Task.FromResult(inner);
            Task unwrappedInner = outer.Unwrap();
            Assert.IsFalse(unwrappedInner.IsCompleted);

            switch (innerStatus)
            {
                case TaskStatus.RanToCompletion:
                    innerTcs.SetResult(true);
                    break;
                case TaskStatus.Faulted:
                    innerTcs.SetException(new InvalidProgramException());
                    break;
                case TaskStatus.Canceled:
                    innerTcs.SetCanceled();
                    break;
            }

            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when the non-generic inner task has not yet completed but the outer task has completed by the time Unwrap is called.
        /// </summary>
        /// <param name="innerStatus">How the inner task should be completed.</param>
        [TestMethod]
        public void TaskUnwrap_Generic_Completed_NotCompleted_Theory()
        {
            Generic_Completed_NotCompleted(TaskStatus.RanToCompletion);
            Generic_Completed_NotCompleted(TaskStatus.Faulted);
            Generic_Completed_NotCompleted(TaskStatus.Canceled);
        }

        private static void Generic_Completed_NotCompleted(TaskStatus innerStatus)
        {
            var innerTcs = new TaskCompletionSource<int>();
            Task<int> inner = innerTcs.Task;

            Task<Task<int>> outer = Task.FromResult(inner);
            Task<int> unwrappedInner = outer.Unwrap();
            Assert.IsFalse(unwrappedInner.IsCompleted);

            switch (innerStatus)
            {
                case TaskStatus.RanToCompletion:
                    innerTcs.SetResult(42);
                    break;
                case TaskStatus.Faulted:
                    innerTcs.SetException(new InvalidProgramException());
                    break;
                case TaskStatus.Canceled:
                    innerTcs.SetCanceled();
                    break;
            }

            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when neither the non-generic inner task nor the outer task has completed by the time Unwrap is called.
        /// </summary>
        /// <param name="outerCompletesFirst">Whether the outer task or the inner task completes first.</param>
        /// <param name="innerStatus">How the inner task should be completed.</param>
        [TestMethod]
        public void TaskUnwrap_NonGeneric_NotCompleted_NotCompleted_Theory()
        {
            NonGeneric_NotCompleted_NotCompleted(true, TaskStatus.RanToCompletion);
            NonGeneric_NotCompleted_NotCompleted(true, TaskStatus.Canceled);
            NonGeneric_NotCompleted_NotCompleted(true, TaskStatus.Faulted);
            NonGeneric_NotCompleted_NotCompleted(false, TaskStatus.RanToCompletion);
            NonGeneric_NotCompleted_NotCompleted(false, TaskStatus.Canceled);
            NonGeneric_NotCompleted_NotCompleted(false, TaskStatus.Faulted);
        }

        private static void NonGeneric_NotCompleted_NotCompleted(bool outerCompletesFirst, TaskStatus innerStatus)
        {
            var innerTcs = new TaskCompletionSource<bool>();
            Task inner = innerTcs.Task;

            var outerTcs = new TaskCompletionSource<Task>();
            Task<Task> outer = outerTcs.Task;

            Task unwrappedInner = outer.Unwrap();
            Assert.IsFalse(unwrappedInner.IsCompleted);

            if (outerCompletesFirst)
            {
                outerTcs.SetResult(inner);
                Assert.IsFalse(unwrappedInner.IsCompleted);
            }

            switch (innerStatus)
            {
                case TaskStatus.RanToCompletion:
                    innerTcs.SetResult(true);
                    break;
                case TaskStatus.Faulted:
                    innerTcs.SetException(new InvalidOperationException());
                    break;
                case TaskStatus.Canceled:
                    innerTcs.TrySetCanceled(CreateCanceledToken());
                    break;
            }

            if (!outerCompletesFirst)
            {
                Assert.IsFalse(unwrappedInner.IsCompleted);
                outerTcs.SetResult(inner);
            }

            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when neither the generic inner task nor the outer task has completed by the time Unwrap is called.
        /// </summary>
        /// <param name="outerCompletesFirst">Whether the outer task or the inner task completes first.</param>
        /// <param name="innerStatus">How the inner task should be completed.</param>
        [TestMethod]
        public void TaskUnwrap_Generic_NotCompleted_NotCompleted_Theory()
        {
            Generic_NotCompleted_NotCompleted(true, TaskStatus.RanToCompletion);
            Generic_NotCompleted_NotCompleted(true, TaskStatus.Canceled);
            Generic_NotCompleted_NotCompleted(true, TaskStatus.Faulted);
            Generic_NotCompleted_NotCompleted(false, TaskStatus.RanToCompletion);
            Generic_NotCompleted_NotCompleted(false, TaskStatus.Canceled);
            Generic_NotCompleted_NotCompleted(false, TaskStatus.Faulted);
        }

        private static void Generic_NotCompleted_NotCompleted(bool outerCompletesFirst, TaskStatus innerStatus)
        {
            var innerTcs = new TaskCompletionSource<int>();
            Task<int> inner = innerTcs.Task;

            var outerTcs = new TaskCompletionSource<Task<int>>();
            Task<Task<int>> outer = outerTcs.Task;

            Task<int> unwrappedInner = outer.Unwrap();
            Assert.IsFalse(unwrappedInner.IsCompleted);

            if (outerCompletesFirst)
            {
                outerTcs.SetResult(inner);
                Assert.IsFalse(unwrappedInner.IsCompleted);
            }

            switch (innerStatus)
            {
                case TaskStatus.RanToCompletion:
                    innerTcs.SetResult(42);
                    break;
                case TaskStatus.Faulted:
                    innerTcs.SetException(new InvalidOperationException());
                    break;
                case TaskStatus.Canceled:
                    innerTcs.TrySetCanceled(CreateCanceledToken());
                    break;
            }

            if (!outerCompletesFirst)
            {
                Assert.IsFalse(unwrappedInner.IsCompleted);
                outerTcs.SetResult(inner);
            }

            AssertTasksAreEqual(inner, unwrappedInner);
        }

        /// <summary>
        /// Tests Unwrap when the outer task for a non-generic inner fails in some way.
        /// </summary>
        /// <param name="outerCompletesFirst">Whether the outer task completes before Unwrap is called.</param>
        /// <param name="outerStatus">How the outer task should be completed (RanToCompletion means returning null).</param>
        [TestMethod]
        public void TaskUnwrap_NonGeneric_UnsuccessfulOuter_Theory()
        {
            NonGeneric_UnsuccessfulOuter(true, TaskStatus.RanToCompletion);
            NonGeneric_UnsuccessfulOuter(true, TaskStatus.Faulted);
            NonGeneric_UnsuccessfulOuter(true, TaskStatus.Canceled);
            NonGeneric_UnsuccessfulOuter(false, TaskStatus.RanToCompletion);
            NonGeneric_UnsuccessfulOuter(false, TaskStatus.Faulted);
            NonGeneric_UnsuccessfulOuter(false, TaskStatus.Canceled);
        }

        private static void NonGeneric_UnsuccessfulOuter(bool outerCompletesBeforeUnwrap, TaskStatus outerStatus)
        {
            var outerTcs = new TaskCompletionSource<Task>();
            Task<Task> outer = outerTcs.Task;

            Task unwrappedInner = null;

            if (!outerCompletesBeforeUnwrap)
                unwrappedInner = outer.Unwrap();

            switch (outerStatus)
            {
                case TaskStatus.RanToCompletion:
                    outerTcs.SetResult(null);
                    break;
                case TaskStatus.Canceled:
                    outerTcs.TrySetCanceled(CreateCanceledToken());
                    break;
                case TaskStatus.Faulted:
                    outerTcs.SetException(new InvalidCastException());
                    break;
            }

            if (outerCompletesBeforeUnwrap)
                unwrappedInner = outer.Unwrap();

            WaitNoThrow(unwrappedInner);

            switch (outerStatus)
            {
                case TaskStatus.RanToCompletion:
                    Assert.IsTrue(unwrappedInner.IsCanceled);
                    break;
                default:
                    AssertTasksAreEqual(outer, unwrappedInner);
                    break;
            }
        }

        /// <summary>
        /// Tests Unwrap when the outer task for a generic inner fails in some way.
        /// </summary>
        /// <param name="outerCompletesFirst">Whether the outer task completes before Unwrap is called.</param>
        /// <param name="outerStatus">How the outer task should be completed (RanToCompletion means returning null).</param>
        [TestMethod]
        public void TaskUnwrap_Generic_UnsuccessfulOuter_Theory()
        {
            Generic_UnsuccessfulOuter(true, TaskStatus.RanToCompletion);
            Generic_UnsuccessfulOuter(true, TaskStatus.Faulted);
            Generic_UnsuccessfulOuter(true, TaskStatus.Canceled);
            Generic_UnsuccessfulOuter(false, TaskStatus.RanToCompletion);
            Generic_UnsuccessfulOuter(false, TaskStatus.Faulted);
            Generic_UnsuccessfulOuter(false, TaskStatus.Canceled);
        }

        private static void Generic_UnsuccessfulOuter(bool outerCompletesBeforeUnwrap, TaskStatus outerStatus)
        {
            var outerTcs = new TaskCompletionSource<Task<int>>();
            Task<Task<int>> outer = outerTcs.Task;

            Task<int> unwrappedInner = null;

            if (!outerCompletesBeforeUnwrap)
                unwrappedInner = outer.Unwrap();

            switch (outerStatus)
            {
                case TaskStatus.RanToCompletion:
                    outerTcs.SetResult(null); // cancellation
                    break;
                case TaskStatus.Canceled:
                    outerTcs.TrySetCanceled(CreateCanceledToken());
                    break;
                case TaskStatus.Faulted:
                    outerTcs.SetException(new InvalidCastException());
                    break;
            }

            if (outerCompletesBeforeUnwrap)
                unwrappedInner = outer.Unwrap();

            WaitNoThrow(unwrappedInner);

            switch (outerStatus)
            {
                case TaskStatus.RanToCompletion:
                    Assert.IsTrue(unwrappedInner.IsCanceled);
                    break;
                default:
                    AssertTasksAreEqual(outer, unwrappedInner);
                    break;
            }
        }

        ///// <summary>
        ///// Test Unwrap when the outer task for a non-generic inner task is marked as AttachedToParent.
        ///// </summary>
        //[TestMethod]
        //public void TaskUnwrap_NonGeneric_AttachedToParent()
        //{
        //    Exception error = new InvalidTimeZoneException();
        //    Task parent = Task.Factory.StartNew(() =>
        //    {
        //        var outerTcs = new TaskCompletionSource<Task>(TaskCreationOptions.AttachedToParent);
        //        Task<Task> outer = outerTcs.Task;

        //        Task inner = Task.FromException(error);

        //        Task unwrappedInner = outer.Unwrap();
        //        Assert.AreEqual(TaskCreationOptions.AttachedToParent, unwrappedInner.CreationOptions);

        //        outerTcs.SetResult(inner);
        //        AssertTasksAreEqual(inner, unwrappedInner);
        //    }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        //    WaitNoThrow(parent);
        //    Assert.AreEqual(TaskStatus.Faulted, parent.Status);
        //    Assert.AreSame(error, parent.Exception.Flatten().InnerException);
        //}

        ///// <summary>
        ///// Test Unwrap when the outer task for a generic inner task is marked as AttachedToParent.
        ///// </summary>
        //[TestMethod]
        //public void TaskUnwrap_Generic_AttachedToParent()
        //{
        //    Exception error = new InvalidTimeZoneException();
        //    Task parent = Task.Factory.StartNew(() =>
        //    {
        //        var outerTcs = new TaskCompletionSource<Task<object>>(TaskCreationOptions.AttachedToParent);
        //        Task<Task<object>> outer = outerTcs.Task;

        //        Task<object> inner = Task.FromException<object>(error);

        //        Task<object> unwrappedInner = outer.Unwrap();
        //        Assert.AreEqual(TaskCreationOptions.AttachedToParent, unwrappedInner.CreationOptions);

        //        outerTcs.SetResult(inner);
        //        AssertTasksAreEqual(inner, unwrappedInner);
        //    }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        //    WaitNoThrow(parent);
        //    Assert.AreEqual(TaskStatus.Faulted, parent.Status);
        //    Assert.AreSame(error, parent.Exception.Flatten().InnerException);
        //}

        ///// <summary>
        ///// Test that Unwrap with a non-generic task doesn't use TaskScheduler.Current.
        ///// </summary>
        //[TestMethod]
        //public void TaskUnwrap_NonGeneric_DefaultSchedulerUsed()
        //{
        //    var scheduler = new CountingScheduler();
        //    Task.Factory.StartNew(() =>
        //    {
        //        int initialCallCount = scheduler.QueueTaskCalls;

        //        Task<Task> outer = Task.Factory.StartNew(() => Task.Run(() => { }),
        //            CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        //        Task unwrappedInner = outer.Unwrap();
        //        unwrappedInner.Wait();

        //        Assert.AreEqual(initialCallCount, scheduler.QueueTaskCalls);
        //    }, CancellationToken.None, TaskCreationOptions.None, scheduler).GetAwaiter().GetResult();
        //}

        ///// <summary>
        ///// Test that Unwrap with a generic task doesn't use TaskScheduler.Current.
        ///// </summary>
        //[TestMethod]
        //public void TaskUnwrap_Generic_DefaultSchedulerUsed()
        //{
        //    var scheduler = new CountingScheduler();
        //    Task.Factory.StartNew(() =>
        //    {
        //        int initialCallCount = scheduler.QueueTaskCalls;

        //        Task<Task<int>> outer = Task.Factory.StartNew(() => Task.Run(() => 42),
        //            CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        //        Task<int> unwrappedInner = outer.Unwrap();
        //        unwrappedInner.Wait();

        //        Assert.AreEqual(initialCallCount, scheduler.QueueTaskCalls);
        //    }, CancellationToken.None, TaskCreationOptions.None, scheduler).GetAwaiter().GetResult();
        //}

        // TODO: More efficient Unwrap
        /// <summary>
        /// Test that a long chain of Unwraps can execute without overflowing the stack.
        /// </summary>
        [TestMethod]
        public void TaskUnwrap_RunStackGuardTests()
        {
            Assert.Inconclusive("Should not overflow the stack");
            //const int DiveDepth = 12000;

            //Func<int, Task<int>> func = null;
            //func = count =>
            //    ++count < DiveDepth ?
            //        Task.Factory.StartNew(() => func(count), CancellationToken.None/*, TaskCreationOptions.None, TaskScheduler.Default*/).Unwrap() :
            //        Task.FromResult(count);

            //// This test will overflow if it fails.
            //Assert.AreEqual(DiveDepth, func(0).Result);
        }

        /// <summary>Gets an enumerable of already completed non-generic tasks.</summary>
        private static IEnumerable<object[]> CompletedNonGenericTasks
        {
            get
            {
                yield return new object[] { Task.CompletedTask };
                yield return new object[] { Task.FromCanceled(CreateCanceledToken()) };
                yield return new object[] { Task.FromException(new FormatException()) };

                var tcs = new TaskCompletionSource<int>();
                tcs.SetCanceled(); // cancel task without a token
                yield return new object[] { tcs.Task };
            }
        }

        /// <summary>Gets an enumerable of already completed generic tasks.</summary>
        private static IEnumerable<object[]> CompletedStringTasks
        {
            get
            {
                yield return new object[] { Task.FromResult("Tasks") };
                yield return new object[] { Task.FromCanceled<string>(CreateCanceledToken()) };
                yield return new object[] { Task.FromException<string>(new FormatException()) };

                var tcs = new TaskCompletionSource<string>();
                tcs.SetCanceled(); // cancel task without a token
                yield return new object[] { tcs.Task };
            }
        }

        /// <summary>Asserts that two non-generic tasks are logically equal with regards to completion status.</summary>
        private static void AssertTasksAreEqual(Task expected, Task actual)
        {
            Assert.IsNotNull(actual);
            WaitNoThrow(actual);

            Assert.AreEqual(expected.Status, actual.Status);
            switch (expected.Status)
            {
                case TaskStatus.Faulted:
                    if (!expected.Exception.InnerExceptions.SequenceEqual(actual.Exception.InnerExceptions))
                        Assert.Fail("Task exceptions does not match");
                    break;
                case TaskStatus.Canceled:
                    Assert.AreEqual(GetCanceledTaskToken(expected), GetCanceledTaskToken(actual));
                    break;
            }
        }

        /// <summary>Asserts that two non-generic tasks are logically equal with regards to completion status.</summary>
        private static void AssertTasksAreEqual<T>(Task<T> expected, Task<T> actual)
        {
            AssertTasksAreEqual((Task)expected, actual);
            if (expected.Status == TaskStatus.RanToCompletion)
            {
                if (typeof(T).GetTypeInfo().IsValueType)
                    Assert.AreEqual(expected.Result, actual.Result);
                else
                    Assert.AreSame(expected.Result, actual.Result);
            }
        }

        /// <summary>Creates an already canceled token.</summary>
        private static CancellationToken CreateCanceledToken()
        {
            // Create an already canceled token.  We construct a new CTS rather than
            // just using CT's Boolean ctor in order to better validate the right
            // token ends up in the resulting unwrapped task.
            var cts = new CancellationTokenSource();
            cts.Cancel();
            return cts.Token;
        }

        /// <summary>Waits for a task to complete without throwing any exceptions.</summary>
        private static void WaitNoThrow(Task task)
        {
            ((IAsyncResult)task).AsyncWaitHandle.WaitOne();
        }

        /// <summary>Extracts the CancellationToken associated with a task.</summary>
        private static CancellationToken GetCanceledTaskToken(Task task)
        {
            Assert.IsTrue(task.IsCanceled);
            try
            {
                task.GetAwaiter().GetResult();
                Assert.IsFalse(true, "Canceled task should have thrown from GetResult");
                return default(CancellationToken);
            }
            catch (InternalOCE oce)
            {
                return oce.CancellationToken;
            }
        }

        //private sealed class CountingScheduler : TaskScheduler
        //{
        //    public int QueueTaskCalls = 0;

        //    protected override void QueueTask(Task task)
        //    {
        //        Interlocked.Increment(ref QueueTaskCalls);
        //        Task.Run(() => TryExecuteTask(task));
        //    }

        //    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) { return false; }

        //    protected override IEnumerable<Task> GetScheduledTasks() { return null; }
        //}

    }
}
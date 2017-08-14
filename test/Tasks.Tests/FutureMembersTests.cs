using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    /// <summary>
    /// This is a test class for TaskResultTest and is intended
    ///to contain all TaskResultTest Unit Tests
    /// </summary>
    [TestClass]
    public class FutureMembersTest
    {
        /// <summary>
        /// A test for Result
        /// </summary>
        private static void ResultTestHelper<TResult>(TResult expected)
        {
            Func<TResult> action = () => expected;
            Task<TResult> target = new Task<TResult>(action);
            target.Start();
            target.Wait();
            Assert.AreEqual<TResult>(expected, target.Result);
        }

        [TestMethod]
        public void Future_ResultTest_Integer()
        {
            ResultTestHelper<int>(20);
        }

        [TestMethod]
        public void Future_ResultTest_String()
        {
            ResultTestHelper<string>("Lorem Ipsum");
        }

        /// <summary>
        /// A test for IsFaulted
        /// </summary>
        [TestMethod]
        public void Future_IsFaultedTest()
        {
            Func<int> action = () => 1;
            Task<int> target = new Task<int>(action);
            target.Start();

            Func<int> actionEx = () => { throw new Exception(); };
            Task<int> targetEx = new Task<int>(actionEx);
            targetEx.Start();

            target.Wait();
            try { targetEx.Wait(); }
            catch (Exception) { }

            Assert.IsFalse(target.IsFaulted);
            Assert.IsTrue(targetEx.IsFaulted);
        }

        /// <summary>
        /// A test for IsCompleted
        /// </summary>
        [TestMethod]
        public void Future_IsCompletedTest()
        {
            Func<int> action = () => { Thread.Sleep(100); return 1; };
            Task<int> target = new Task<int>(action);
            target.Start();
            Assert.IsFalse(target.IsCompleted);
            target.Wait();
            Assert.IsTrue(target.IsCompleted);
        }

        /// <summary>
        /// A test for Exception
        /// </summary>
        [TestMethod]
        public void Future_ExceptionTest()
        {
            Func<int> action = () => { throw new ArgumentNullException("none"); };
            Task<int> target = new Task<int>(action);
            target.Start();
            try { target.Wait(); }
            catch (AggregateException) { }

            Assert.AreEqual(1, target.Exception.InnerExceptions.Count);
            var ex = target.Exception.InnerExceptions[0] as ArgumentNullException;
            Assert.IsNotNull(ex);
        }

        /// <summary>
        /// A test for CompletedSynchronously
        /// </summary>
        [TestMethod]
        public void Future_CompletedSynchronouslyTest()
        {
            int counter = 0;
            Func<int> action = () => Interlocked.Increment(ref counter);
            Task<int> target = new Task<int>(action);
            target.Start();
            target.Wait();
            Assert.IsFalse(((IAsyncResult)target).CompletedSynchronously);
            Assert.AreEqual(1, counter);

            target = new Task<int>(action);
            target.RunSynchronously();
            // Always false
            Assert.IsFalse(((IAsyncResult)target).CompletedSynchronously);
            Assert.AreEqual(2, counter);
        }

        /// <summary>
        /// A test for AsyncWaitHandle
        /// </summary>
        [TestMethod]
        public void Future_AsyncWaitHandleTest()
        {
            int counter = 0;
            Func<int> action = () => Interlocked.Increment(ref counter);
            Task<int> target = new Task<int>(action);
            target.Start();
            Assert.IsTrue(((IAsyncResult)target).AsyncWaitHandle.WaitOne());
            Assert.AreEqual(1, counter);
        }

        /// <summary>
        /// A test for AsyncState
        /// </summary>
        [TestMethod]
        public void Future_AsyncStateTest()
        {
            object refobj = new object();
            int counter = 0;
            Func<object, int> action = state =>
            {
                Assert.AreEqual(refobj, state);
                return Interlocked.Increment(ref counter);
            };
            Task<int> target = new Task<int>(action, refobj);
            target.Start();
            target.Wait();
            Assert.AreEqual(refobj, target.AsyncState);
        }

        /// <summary>
        /// A test for Wait
        /// </summary>
        [TestMethod]
        public void Future_WaitTest_Int32()
        {
            Func<int> action = () => { Thread.Sleep(100); return 1; };
            Task<int> target = new Task<int>(action);
            target.Start();
            Assert.IsFalse(target.Wait(0));
            Assert.IsTrue(target.Wait(500));
        }

        /// <summary>
        /// A test for Wait
        /// </summary>
        [TestMethod]
        public void Future_WaitTest_TimeSpan()
        {
            Func<int> action = () => { Thread.Sleep(100); return 1; };
            Task<int> target = new Task<int>(action);
            target.Start();
            Assert.IsFalse(target.Wait(new TimeSpan(0)));
            Assert.IsTrue(target.Wait(new TimeSpan(0, 0, 1)));
        }

        /// <summary>
        /// A test for Wait
        /// </summary>
        [TestMethod]
        public void Future_WaitTest()
        {
            int counter = 0;
            Func<int> action = () => Interlocked.Increment(ref counter);
            Task<int> target = new Task<int>(action);
            target.Start();
            target.Wait();
            Assert.AreEqual(1, counter);
        }

        /// <summary>
        /// A test for TaskCallback
        /// </summary>
        [TestMethod]
        public void Future_TaskStartActionTest()
        {
            int counter = 0;
            Func<int> action = () => Interlocked.Increment(ref counter);
            Task<int> target = new Task<int>(action);
            PrivateObject pvTarget = new PrivateObject(target);
            pvTarget.Invoke("TaskStartAction", new object[] { null });
            Assert.AreEqual(1, counter);
        }

        /// <summary>
        /// A test for Start
        /// </summary>
        [TestMethod]
        public void Future_StartTest()
        {
            int counter = 0;
            Func<int> action = () => Interlocked.Increment(ref counter);
            Task<int> target = new Task<int>(action);
            target.Start();
            Thread.Sleep(50);
            Assert.AreEqual(1, counter);
        }

        /// <summary>
        /// A test for RunSynchronously
        /// </summary>
        [TestMethod]
        public void Future_RunSynchronouslyTest()
        {
            int counter = 0;
            Func<int> action = () => Interlocked.Increment(ref counter);
            Task<int> target = new Task<int>(action);
            target.RunSynchronously();
            var result = target.Result;
            Assert.AreEqual(1, result);
            Assert.AreEqual(1, counter);
        }

        /// <summary>
        /// A test for EnsureStartOnce
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Future_EnsureStartOnceTest()
        {
            Func<int> action = () => 1;
            Task<int> target = new Task<int>(action);
            PrivateObject pvTarget = new PrivateObject(target);
            pvTarget.Invoke("EnsureStartOnce");
            pvTarget.Invoke("EnsureStartOnce");
        }

        /// <summary>
        /// A test for EndWait
        /// </summary>
        [TestMethod]
        public void Future_EndWaitTest()
        {
            Exception ex = new ArgumentNullException("none");
            Func<int> action = () => { throw ex; };
            Task<int> target = new Task<int>(action);
            target.Start();
            var ar = target.BeginWait(null, null);

            bool throwException = false;
            try { target.EndWait(ar); }
            catch (AggregateException)
            {
                throwException = true;
            }

            Assert.IsTrue(throwException);
            Assert.AreEqual(1, target.Exception.InnerExceptions.Count);
            Assert.IsTrue(target.Exception.InnerExceptions[0] is ArgumentNullException);
        }

        /// <summary>
        /// A test for Dispose
        /// </summary>
        [TestMethod]
        public void Future_DisposeTest()
        {
            Func<int> action = () => 1;
            Task<int> target = new Task<int>(action);

            bool throwException = false;
            try { target.Dispose(); }
            catch (InvalidOperationException)
            {
                throwException = true;
            }
            Assert.IsTrue(throwException, "Should not dispose a task that is not completed");

            target.Start();
            target.Wait();
            target.Dispose();
        }

        /// <summary>
        /// A test for ContinueWith
        /// </summary>
        [TestMethod]
        public void Future_ContinueWithTest()
        {
            int value = 1;
            Func<int> action = () => Interlocked.CompareExchange(ref value, 2, 1);
            Task<int> target = new Task<int>(action);
            target.Start();
            Func<Task, int> continueFunction = t =>
            {
                Assert.IsNotNull(t);
                Assert.IsFalse(t.IsFaulted);
                return Interlocked.CompareExchange(ref value, 3, 2);
            };
            Task<int> target2 = target.ContinueWith(continueFunction);
            if (!target2.Wait(100))
                Assert.Fail("Timeout waiting for continuation task signal");

            Assert.IsTrue(target.IsCompleted);
            Assert.IsTrue(target2.IsCompleted);
            Assert.AreEqual(3, value);
        }

        /// <summary>
        /// A test for BeginWait
        /// </summary>
        [TestMethod]
        public void Future_BeginWaitTest()
        {
            int counter = 0;
            Func<int> action = () => Interlocked.Increment(ref counter);
            Task<int> target = new Task<int>(action);
            target.Start();
            target.BeginWait(ar =>
            {
                target.EndWait(ar);
                Assert.AreEqual(1, counter);
                Assert.IsNull(target.Exception);
            }, null);
            target.Wait();
        }

        /// <summary>
        /// A test for Task Constructor
        /// </summary>
        [TestMethod]
        public void Future_ConstructorTest_Action()
        {
            int counter = 0;
            Func<int> action = () => Interlocked.Increment(ref counter);
            Task<int> target = new Task<int>(action);
            target.Start();
            target.Wait();
            Assert.AreEqual(1, counter);
        }

        /// <summary>
        /// A test for Task Constructor
        /// </summary>
        [TestMethod]
        public void Future_ConstructorTest_Action_State()
        {
            int[] values = new int[] { 0 };
            Func<object, int> action = obj =>
            {
                int[] v = obj as int[];
                Assert.IsNotNull(v);
                return Interlocked.Increment(ref v[0]);
            };
            Task<int> target = new Task<int>(action, values);
            target.Start();
            target.Wait();
            Assert.AreEqual(1, values[0]);
        }

        [TestMethod]
        public void FutureDeepNesting()
        {
            int counter = 0;
            Func<int> action = () => Interlocked.Increment(ref counter);
            var task = new Task<int>(action);
            task.Start();

            Func<Task, int> continueFunction = t => Interlocked.Increment(ref counter);
            for (int i = 0; i < TaskMembersTest.NestingCount; i++)
            {
                task = task.ContinueWith(continueFunction);
            }

            Assert.IsNotNull(task);
            task.Wait();
            Assert.AreEqual(TaskMembersTest.NestingCount + 1, counter);
            Assert.IsNull(task.Exception);
        }
    }
}

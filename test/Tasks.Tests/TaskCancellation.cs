using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Tests
{
    [TestClass]
    public class TaskCancellation
    {
        [TestMethod]
        public void TaskCancellation_Simple()
        {
            var tokenSource2 = new CancellationTokenSource();
            CancellationToken ct = tokenSource2.Token;

            var task = Task.Factory.StartNew(() =>
            {
                // Were we already canceled?
                ct.ThrowIfCancellationRequested();

                bool moreToDo = true;
                while (moreToDo)
                {
                    // Poll on this property if you have to do
                    // other cleanup before throwing.
                    if (ct.IsCancellationRequested)
                    {
                        // Clean up here, then...
                        ct.ThrowIfCancellationRequested();
                    }

                }
            }, tokenSource2.Token); // Pass same token to StartNew.

            tokenSource2.Cancel();

            // Just continue on this thread, or Wait/WaitAll with try-catch:
            try
            {
                task.Wait();
                Assert.Fail("Should throw an Exception");
            }
            catch (AggregateException) { }
            finally
            {
                tokenSource2.Dispose();
            }
        }

        [TestMethod]
        public void TaskCancellation_AsyncStateMachine()
        {
            Internal_TaskCancellation_AsyncStateMachine().Wait();
        }

#if WindowsCE
        private Task Internal_TaskCancellation_AsyncStateMachine()
        {
            var stateMachine = new __Internal_TaskCancellation_AsyncStateMachine();
            stateMachine.__this = this;
            stateMachine.__builder = AsyncTaskMethodBuilder.Create();
            stateMachine.__state = -1;
            stateMachine.__builder.Start<__Internal_TaskCancellation_AsyncStateMachine>(ref stateMachine);
            return stateMachine.__builder.Task;
        }

        private sealed class __Internal_TaskCancellation_AsyncStateMachine : IAsyncStateMachine
        {
            public int __state;
            public AsyncTaskMethodBuilder __builder;
            public TaskCancellation __this;
            private CancellationTokenSource tokenSource2;
            private CancellationToken ct;
            private Task task;
            private ConfiguredTaskAwaitable.ConfiguredTaskAwaiter __awaiter1;

            void IAsyncStateMachine.MoveNext()
            {
                int num = __state;
                try
                {
                    if (num != 0)
                    {
                        tokenSource2 = new CancellationTokenSource();
                        ct = tokenSource2.Token;

                        task = Task.Factory.StartNew(() =>
                        {
                            // Were we already canceled?
                            ct.ThrowIfCancellationRequested();

                            bool moreToDo = true;
                            while (moreToDo)
                            {
                                // Poll on this property if you have to do
                                // other cleanup before throwing.
                                if (ct.IsCancellationRequested)
                                {
                                    // Clean up here, then...
                                    ct.ThrowIfCancellationRequested();
                                }
                            }
                        }, tokenSource2.Token);

                        tokenSource2.Cancel();
                    }
                    try
                    {
                        ConfiguredTaskAwaitable.ConfiguredTaskAwaiter awaiter;
                        if (num != 0)
                        {
                            awaiter = task.ConfigureAwait(false).GetAwaiter();
                            if (!awaiter.IsCompleted)
                            {
                                __state = num = 0;
                                __awaiter1 = awaiter;
                                var stateMachine = this;
                                __builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
                                return;
                            }
                        }
                        else
                        {
                            awaiter = __awaiter1;
                            __awaiter1 = default(ConfiguredTaskAwaitable.ConfiguredTaskAwaiter);
                            __state = num = -1;
                        }

                        awaiter.GetResult();
                        Assert.Fail("Should be canceled");
                    }
                    catch (OperationCanceledException ex)
                    {
                        Assert.AreEqual(ct, ex.CancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail(
                            "Should throw an operation canceled exception: {0}",
                            ex.Message);
                    }
                    finally
                    {
                        if (num < 0)
                            tokenSource2.Dispose();
                    }

                    Assert.AreEqual(TaskStatus.Canceled, task.Status);
                    Assert.AreEqual(ct, task.CancellationToken);
                }
                catch (Exception ex)
                {
                    __state = -2;
                    __builder.SetException(ex);
                    return;
                }

                __state = -2;
                __builder.SetResult();
            }

            void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
            {
            }
        }
#else
        private async Task Internal_TaskCancellation_AsyncStateMachine()
        {
            var tokenSource2 = new CancellationTokenSource();
            CancellationToken ct = tokenSource2.Token;

            var task = Task.Factory.StartNew(() =>
            {
                // Were we already canceled?
                ct.ThrowIfCancellationRequested();

                bool moreToDo = true;
                while (moreToDo)
                {
                    // Poll on this property if you have to do
                    // other cleanup before throwing.
                    if (ct.IsCancellationRequested)
                    {
                        // Clean up here, then...
                        ct.ThrowIfCancellationRequested();
                    }

                }
            }, tokenSource2.Token); // Pass same token to StartNew.

            tokenSource2.Cancel();

            try
            {
                await task.ConfigureAwait(false);
                Assert.Fail("Should be canceled");
            }
            catch (Mock.System.OperationCanceledException ex)
            {
                Assert.AreEqual(ct, ex.CancellationToken);
            }
            catch (Exception ex)
            {
                Assert.Fail(
                    "Should throw an operation canceled exception: {0}",
                    ex.Message);
            }
            finally
            {
                tokenSource2.Dispose();
            }

            Assert.AreEqual(TaskStatus.Canceled, task.Status);
            Assert.AreEqual(ct, task.CancellationToken);
        }
#endif

        [TestMethod]
        public void TaskCancellation_CancelBeforeCreate()
        {
            var token = new CancellationToken(true);
            Assert.IsTrue(token.CanBeCanceled);

            using (var t = new Task(() => { }, token))
            {
                Assert.AreEqual(TaskStatus.Canceled, t.Status, "#1");

                try
                {
                    t.Start();
                    Assert.Fail("#2");
                }
                catch (InvalidOperationException ex)
                {
                    GC.KeepAlive(ex);
                }
            }
        }

        [TestMethod]
        public void TaskCancellation_CancelBeforeStart()
        {
            using (var src = new CancellationTokenSource())
            {
                using (var t = new Task(() => { }, src.Token))
                {
                    src.Cancel();
                    Assert.AreEqual(TaskStatus.Canceled, t.Status, "#1");

                    try
                    {
                        t.Start();
                        Assert.Fail("#2");
                    }
                    catch (InvalidOperationException ex)
                    {
                        GC.KeepAlive(ex);
                    }
                }
            }
        }

        [TestMethod]
        public void TaskCancellation_CanceledContinuationExecuteSynchronouslyTest()
        {
            using (var source = new CancellationTokenSource())
            {
                using (var evt = new ManualResetEvent(false))
                {
                    var token = source.Token;
                    var result = false;
                    var thrown = false;

                    var task = Task.Factory.StartNew(() => { Assert.IsTrue(evt.WaitOne(2000, false), "#1"); });
                    // TODO: Create TaskContinuationOptions
                    //var cont = task.ContinueWith(t => result = true, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                    var cont = task.ContinueWith(t => result = true, token);

                    source.Cancel();
                    evt.Set();
                    Assert.IsTrue(task.Wait(2000), "#2");
                    try
                    {
                        Assert.IsFalse(cont.Wait(4000), "#3");
                    }
                    catch (AggregateException ex)
                    {
                        GC.KeepAlive(ex);
                        thrown = true;
                    }

                    Assert.IsTrue(task.IsCompleted, "#4");
                    Assert.IsTrue(cont.IsCanceled, "#5");
                    Assert.IsFalse(result, "#6");
                    Assert.IsTrue(thrown, "#7");
                }
            }
        }
    }
}

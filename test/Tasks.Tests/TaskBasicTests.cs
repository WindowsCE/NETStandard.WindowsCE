using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class TaskBasicTests
    {
        const int Timeout = 150;
        const string ExceptionMessage = "Task Exception";
        const string TaskExecutionTimeout = "Task execution could not be completed into expected time";

        [TestMethod]
        public void TaskContinueExceptionTest()
        {
            TaskContinueExceptionTestAsync().Wait();
        }

#if WindowsCE
        private Task TaskContinueExceptionTestAsync()
        {
            return Task.Factory
                .StartNew(() => { throw new Exception(ExceptionMessage); })
                .ContinueWith(t =>
                {
                    Assert.IsTrue(t.IsFaulted);
                });
        }
#else
        private async Task TaskContinueExceptionTestAsync()
        {
            var task = Task.Factory
                .StartNew(() => { throw new Exception(ExceptionMessage); })
                .ContinueWith(t =>
                {
                    Assert.IsTrue(t.IsFaulted);
                });
            await task;
        }
#endif

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void TaskWaitExceptionTest()
        {
            var task = Task.Factory.StartNew(() =>
            {
                Task.Delay(Timeout).Wait();
                throw new Exception(ExceptionMessage);
            });
            if (!task.Wait(Timeout * 2))
                Assert.Fail(TaskExecutionTimeout);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void TaskResultExceptionStarted()
        {
            var task = Task.Factory.StartNew<int>(() => { throw new Exception(ExceptionMessage); });
            Assert.AreEqual(0, task.Result);
        }

        [TestMethod]
        public void TaskResultCreatedTest()
        {
            var outerTask = Task.Run(() =>
            {
                var task = new Task<int>(() => 15);
                return task.Result;
            });
            Assert.IsFalse(outerTask.Wait(Timeout));
        }

        [TestMethod]
        public void TaskAwaitCreatedTest()
        {
            TaskAwaitCreatedTestAsync().Wait();
        }

#if WindowsCE
        private Task TaskAwaitCreatedTestAsync()
        {
            int counter = 0;
            var task = new Task(() => Interlocked.Increment(ref counter));
            return Task.WhenAny(task, Task.Delay(Timeout))
                .ContinueWith(t =>
                {
                    var result = ((Task<Task>)t).Result;
                    if (result == task)
                        Assert.Fail("Awaiting for task should not start it");

                    Assert.AreEqual(0, counter);
                    Assert.AreEqual(TaskStatus.Created, task.Status);
                });
        }
#else
        private async Task TaskAwaitCreatedTestAsync()
        {
            int counter = 0;
            var task = new Task(() => Interlocked.Increment(ref counter));
            if (await Task.WhenAny(task, Task.Delay(Timeout)) == task)
                Assert.Fail("Awaiting for task should not start it");

            Assert.AreEqual(0, counter);
            Assert.AreEqual(TaskStatus.Created, task.Status);
        }
#endif

        [TestMethod]
        public void TaskWaitCreatedTest()
        {
            int counter = 0;
            var task = new Task(() => Interlocked.Increment(ref counter));
            if (task.Wait(Timeout))
                Assert.Fail("Waiting for task should not start it");

            Assert.AreEqual(0, counter);
            Assert.AreEqual(TaskStatus.Created, task.Status);
        }

        [TestMethod]
        public void TaskWaitStartedTest()
        {
            int counter = 0;
            var task = new Task(() => Interlocked.Increment(ref counter));

            task.Start();
            //Assert.AreEqual(TaskStatus.WaitingToRun, task.Status);

            if (!task.Wait(Timeout))
                Assert.Fail(TaskExecutionTimeout);

            Assert.AreEqual(1, counter);
            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }

        [TestMethod]
        public void TaskRunSyncExceptionTest()
        {
            var task = new Task(() => { throw new Exception(ExceptionMessage); });
            task.RunSynchronously();
        }

        [TestMethod]
        public void TaskContinueCreatedTest()
        {
            var task = new Task(() => { });
            Assert.IsFalse(task.ContinueWith(t => { }).Wait(Timeout));
        }

        [TestMethod]
        [Timeout(Timeout)]
        public void TaskWhenAllTest()
        {
            int counter = 0;
            var t1 = Task.Run(() =>
            {
                Thread.Sleep(10);
                Interlocked.Increment(ref counter);
            });
            var t2 = Task.Run(() =>
            {
                Thread.Sleep(8);
                Interlocked.Increment(ref counter);
                Interlocked.Increment(ref counter);
            });
            var whenAll = Task.WhenAll(t1, t2);

            whenAll.Wait();
            Assert.AreEqual(3, counter);
            Assert.AreEqual(TaskStatus.RanToCompletion, whenAll.Status);
        }

        [TestMethod]
        public void TaskWhenAllSteps()
        {
            var tcsList = new[]
            {
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
            };

            Task[] tasks = new Task[tcsList.Length];
            for (int i = 0; i < tcsList.Length; i++)
                tasks[i] = tcsList[i].Task;

            var whenAll = Task.WhenAll(tasks);

            foreach (var tcs in tcsList)
            {
                Assert.AreEqual(TaskStatus.WaitingForActivation, whenAll.Status);
                tcs.SetResult(0);
            }

            Assert.AreEqual(TaskStatus.RanToCompletion, whenAll.Status);
        }

        [TestMethod]
        [Timeout(Timeout)]
        public void TaskWhenAnyTest()
        {
            int counter = 0;
            var t1 = Task.Run(() =>
            {
                Thread.Sleep(10);
                Interlocked.Increment(ref counter);
            });
            var t2 = Task.Run(() =>
            {
                Thread.Sleep(1000);
                Interlocked.Increment(ref counter);
                Interlocked.Increment(ref counter);
            });
            var whenAny = Task.WhenAny(t1, t2);

            whenAny.Wait();
            Assert.AreEqual(t1, whenAny.Result);
            Assert.AreEqual(1, counter);
            Assert.AreEqual(TaskStatus.RanToCompletion, whenAny.Status);
        }

        [TestMethod]
        public void TaskWhenAnySteps()
        {
            var tcsList = new[]
            {
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
                new TaskCompletionSource<int>(),
            };

            Task[] tasks = new Task[tcsList.Length];
            for (int i = 0; i < tcsList.Length; i++)
                tasks[i] = tcsList[i].Task;

            var whenAny = Task.WhenAny(tasks);
            Assert.AreEqual(TaskStatus.WaitingForActivation, whenAny.Status);

            for (int i = tcsList.Length - 1; i >= 0; --i)
            {
                tcsList[i].SetResult(0);
                Assert.AreEqual(TaskStatus.RanToCompletion, whenAny.Status);
            }

            Assert.AreEqual(tcsList[tcsList.Length - 1].Task, whenAny.Result);
        }
    }
}

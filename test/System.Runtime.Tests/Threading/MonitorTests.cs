using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;

namespace System.Runtime.Tests.Threading
{
    [TestClass]
    public class MonitorTests
    {
        [TestMethod]
        public void MonitorTryEnterTimeout()
        {
            object lockObj = new object();
            var thread = new Thread(() =>
            {
                lock (lockObj)
                    Thread.Sleep(10);
            });
            thread.Start();

            Thread.Sleep(1);
            Assert.IsFalse(Monitor.TryEnter(lockObj));

            Assert.IsTrue(Mock.System.Threading.Monitor2.TryEnter(lockObj, 15));
            Monitor.Exit(lockObj);

            Assert.IsTrue(thread.Join(250));
        }

        [TestMethod]
        public void MonitorSimpleWaitPulse()
        {
            object locker = new object();
            bool go = false;
            bool gotPulse = false;

            var thread = new Thread(() =>
            {
                lock (locker)
                    while (!go)
                        Mock.System.Threading.Monitor2.Wait(locker);

                gotPulse = true;
            });
            thread.Start();

            lock (locker)
            {
                go = true;
                Mock.System.Threading.Monitor2.Pulse(locker);
            }

            Assert.IsTrue(thread.Join(250));
            Assert.IsTrue(gotPulse);
        }

        [TestMethod]
        public void MonitorProducerConsumer()
        {
            const int workersCount = 10;
            const int workCount = 1000;
            int workDone = 0;
            PCQueue q = new PCQueue(workersCount);
            for (int i = 0; i < workCount; i++)
            {
                int itemNumber = i;
                q.EnqueueItem(() =>
                {
                    Thread.Sleep(1);
                    Interlocked.Increment(ref workDone);
                });
            }

            q.Shutdown(true);
            Assert.AreEqual(workCount, workDone);
        }

        class PCQueue
        {
            readonly object _locker = new object();
            Thread[] _workers;
            Queue<Action> _itemQ = new Queue<Action>();

            public PCQueue(int workerCount)
            {
                _workers = new Thread[workerCount];

                // Create and start a separate thread for each worker
                for (int i = 0; i < workerCount; i++)
                    (_workers[i] = new Thread(Consume)).Start();
            }

            public void Shutdown(bool waitForWorkers)
            {
                // Enqueue one null item per worker to make each exit.
                foreach (Thread worker in _workers)
                    EnqueueItem(null);

                // Wait for workers to finish
                if (waitForWorkers)
                    foreach (Thread worker in _workers)
                        worker.Join();
            }

            public void EnqueueItem(Action item)
            {
                lock (_locker)
                {
                    _itemQ.Enqueue(item);                           // We must pulse because we're
                    Mock.System.Threading.Monitor2.Pulse(_locker);  // changing a blocking condition.
                }
            }

            void Consume()
            {
                while (true)                        // Keep consuming until
                {                                   // told otherwise.
                    Action item;
                    lock (_locker)
                    {
                        while (_itemQ.Count == 0)
                            Mock.System.Threading.Monitor2.Wait(_locker);
                        item = _itemQ.Dequeue();
                    }
                    if (item == null) return;         // This signals our exit.
                    item();                           // Execute item.
                }
            }
        }
    }
}

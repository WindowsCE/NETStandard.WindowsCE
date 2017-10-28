using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class TaskDelayTests
    {
        [TestMethod]
        public void Delay_Invalid()
        {
            try
            {
                Task.Delay(-100);
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        [TestMethod]
        public void Delay_Start()
        {
            var t = Task.Delay(5000);
            try
            {
                t.Start();
            }
            catch (InvalidOperationException)
            {
            }
        }

        [TestMethod]
        public void Delay_Simple()
        {
            var t = Task.Delay(300);
            Assert.IsTrue(TaskStatus.WaitingForActivation == t.Status || TaskStatus.Running == t.Status, "#1");
            Assert.IsTrue(t.Wait(1200), "#2");
        }

        [TestMethod]
        public void Delay_Cancelled()
        {
            var cancelation = new CancellationTokenSource();

            var t = Task.Delay(5000, cancelation.Token);
            Assert.IsTrue(TaskStatus.WaitingForActivation == t.Status || TaskStatus.Running == t.Status, "#1");
            cancelation.Cancel();
            try
            {
                t.Wait(1000);
                Assert.Fail("#2");
            }
            catch (AggregateException)
            {
                Assert.AreEqual(TaskStatus.Canceled, t.Status, "#3");
            }

            cancelation = new CancellationTokenSource();
            t = Task.Delay(Timeout.Infinite, cancelation.Token);
            Assert.AreEqual(TaskStatus.WaitingForActivation, t.Status, "#11");
            cancelation.Cancel();
            try
            {
                t.Wait(1000);
                Assert.Fail("#12");
            }
            catch (AggregateException)
            {
                Assert.AreEqual(TaskStatus.Canceled, t.Status, "#13");
            }
        }

        [TestMethod]
        public void Delay_TimeManagement()
        {
            var delay1 = Task.Delay(50);
            var delay2 = Task.Delay(25);
            Assert.IsTrue(Task.WhenAny(new[] { delay1, delay2 }).Wait(1000));
            Assert.AreEqual(TaskStatus.RanToCompletion, delay2.Status);
        }
    }
}

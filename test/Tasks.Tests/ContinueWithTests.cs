using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class ContinueWithTests
    {
        [TestMethod]
        public void ContinueWithInvalidArguments()
        {
            var task = new Task(() => { });
            try
            {
                task.ContinueWith(null);
                Assert.Fail("#1");
            }
            catch (ArgumentNullException) { }

            //try
            //{
            //    task.ContinueWith(delegate { }, null);
            //    Assert.Fail("#2");
            //}
            //catch (ArgumentNullException) { }
        }

        [TestMethod]
        public void ContinueWithWithSharedCancellationToken()
        {
            int parentCounter = 0;
            int continueCounter = 0;
            var cts = new CancellationTokenSource();
            var task = new Task(() => Interlocked.Increment(ref parentCounter), cts.Token);
            task.ContinueWith(t => Interlocked.Increment(ref continueCounter), cts.Token);
            cts.Cancel();

            try
            {
                task.Start();
                Assert.Fail("Should not start a canceled task");
            }
            catch (InvalidOperationException ex)
            {
                GC.KeepAlive(ex);
            }

            Assert.AreEqual(0, parentCounter, "Should not execute a canceled task");
            Assert.AreEqual(0, continueCounter, "Should not execute a canceled continue task");
        }
    }
}

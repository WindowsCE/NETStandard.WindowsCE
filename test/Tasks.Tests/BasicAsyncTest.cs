using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class BasicAsyncTest
    {
        private static async Task<string> GetData()
        {
            var data = await SlowOperation();
            return data.ToString(CultureInfo.InvariantCulture);
        }

        private static Task<int> SlowOperation()
        {
            Thread.Sleep(1);
            return Task.FromResult(7);
        }

        [TestMethod]
        public void SimpleTest()
        {
            Assert.AreEqual("7", GetData().Result);
        }

        private static async Task<string> GetDataWithException()
        {
            try
            {
                var data = await SlowOperationWithException();
                return data.ToString(CultureInfo.InvariantCulture);
            }
            catch (InternalTestFailureException ex)
            {
                Assert.AreEqual("Task exception test", ex.Message);
                return "An exception occurred";
            }
        }

        private static Task<int> SlowOperationWithException()
        {
            Thread.Sleep(1);
            return Task.FromException<int>(new InternalTestFailureException("Task exception test"));
        }

        [TestMethod]
        public void SimpleExceptionTest()
        {
            Assert.AreEqual("An exception occurred", GetDataWithException().Result);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Runtime.Tests
{
    [TestClass]
    public class DateTimeOffsetTests
    {
        [TestMethod]
        public void DateTimeOffset_ToString()
        {
            const string testFormat = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";
            var dtOffset = new Mock.System.DateTimeOffset(
                new DateTime(2017, 2, 11, 19, 11, 20, 228),
                new TimeSpan(-2, 0, 0));

            Assert.AreEqual("2017-02-11T19:11:20.2280000-02:00", dtOffset.ToString(testFormat));
        }
    }
}

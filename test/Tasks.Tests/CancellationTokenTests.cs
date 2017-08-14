using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class CancellationTokenTests
    {
        [TestMethod]
        public void CancellationToken_InitedWithFalseToken()
        {
            var tk = new CancellationToken(false);
            Assert.IsFalse(tk.CanBeCanceled, "#1");
            Assert.IsFalse(tk.IsCancellationRequested, "#2");
        }

        [TestMethod]
        public void CancellationToken_InitedWithTrueToken()
        {
            var tk = new CancellationToken(true);
            Assert.IsTrue(tk.CanBeCanceled, "#1");
            Assert.IsTrue(tk.IsCancellationRequested, "#2");
        }

        [TestMethod]
        public void CancellationToken_CancellationSourceNotCanceled()
        {
            using (var src = new CancellationTokenSource())
            {
                var tk = src.Token;

                Assert.IsTrue(tk.CanBeCanceled);
                Assert.IsFalse(tk.IsCancellationRequested);
            }
        }

        [TestMethod]
        public void CancellationToken_CancellationSourceCanceled()
        {
            using (var src = new CancellationTokenSource())
            {
                var tk = src.Token;
                src.Cancel();

                Assert.IsTrue(tk.CanBeCanceled, "#1");
                Assert.IsTrue(tk.IsCancellationRequested, "#2");
            }
        }

        [TestMethod]
        public void CancellationToken_UninitializedToken()
        {
            var tk = new CancellationToken();
            Assert.IsFalse(tk.CanBeCanceled);
            Assert.IsFalse(tk.IsCancellationRequested);
        }

        [TestMethod]
        public void CancellationToken_NoneProperty()
        {
            var n = CancellationToken.None;
            Assert.IsFalse(n.CanBeCanceled, "#1");
            Assert.IsFalse(n.IsCancellationRequested, "#2");
            Assert.AreEqual(n, CancellationToken.None, "#3");

            n.ThrowIfCancellationRequested();
            n.GetHashCode();
        }

        [TestMethod]
        public void CancellationToken_DefaultCancellationTokenRegistration()
        {
            var registration = new CancellationTokenRegistration();

            // shouldn't throw
            registration.Dispose();
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class FromAsyncTests
    {
        const string TextToTest = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus eu vehicula enim, non euismod libero.";

        private async Task FromAsyncFile(string filename, bool? configAwait)
        {
            byte[] buffer = new byte[256];
            string[] pieces = TextToTest.Split(' ');
            byte[] separator = Encoding.Unicode.GetBytes(" ");
            using (Stream fs = new FileStream(filename, FileMode.OpenOrCreate | FileMode.Truncate))
            {
                for (int i = 0; i < pieces.Length; i++)
                {
                    if (i > 0)
                        fs.Write(separator, 0, separator.Length);

                    int count = Encoding.Unicode.GetBytes(pieces[i].ToCharArray(), 0, pieces[i].Length, buffer, 0);
                    var task = Task.Factory.FromAsync(
                        fs.BeginWrite, fs.EndWrite,
                        buffer, 0, count, null);

                    if (configAwait != null)
                        await task.ConfigureAwait(configAwait.Value);
                    else
                        await task;
                }

                fs.Flush();
                fs.Close();
            }

            string textRead;
            using (StringWriter writer = new StringWriter())
            using (Stream fs = new FileStream(filename, FileMode.Open))
            {
                int read = 0;
                do
                {
                    var task = Task.Factory.FromAsync(
                        fs.BeginRead, fs.EndRead,
                        buffer, 0, buffer.Length, null);
                    if (configAwait != null)
                        read = await task.ConfigureAwait(configAwait.Value);
                    else
                        read = await task;

                    if (read > 0)
                        writer.Write(Encoding.Unicode.GetString(buffer, 0, read));
                } while (read > 0);
                fs.Close();

                writer.Flush();
                textRead = writer.ToString();
            }

            string suffix;
            if (configAwait == null)
                suffix = " (no config)";
            else if (configAwait.Value)
                suffix = " (config: true)";
            else
                suffix = " (config: false)";

            Assert.AreEqual(TextToTest, textRead, "The text read does not match what was written" + suffix);
        }

        [TestMethod]
        public void FromAsyncFileStreamTest()
        {
            string filename = null;
            try
            {
                filename = Path.GetTempFileName();
                Assert.IsNotNull(filename);

                // Without synchronization context
                FromAsyncFile(filename, null).Wait();
                FromAsyncFile(filename, false).Wait();
                FromAsyncFile(filename, true).Wait();

                // With synchronization context
                var context = new TestContext();
                SynchronizationContext.SetSynchronizationContext(context);
                FromAsyncFile(filename, null).Wait();
                Assert.AreEqual(1, context.PostCount);
                Assert.AreEqual(0, context.SendCount);

                FromAsyncFile(filename, false).Wait();
                Assert.AreEqual(1, context.PostCount);
                Assert.AreEqual(0, context.SendCount);

                FromAsyncFile(filename, true).Wait();
                Assert.AreEqual(2, context.PostCount);
                Assert.AreEqual(0, context.SendCount);
            }
            finally
            {
                if (filename != null)
                {
                    try { File.Delete(filename); }
                    catch { }
                }
            }
        }

        class TestContext : SynchronizationContext
        {
            volatile int postCount = 0;
            volatile int sendCount = 0;

            public int PostCount
                => postCount;

            public int SendCount
                => sendCount;

            public override void Post(SendOrPostCallback d, object state)
            {
                Interlocked.Increment(ref postCount);
                base.Post(d, state);
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                Interlocked.Increment(ref sendCount);
                base.Send(d, state);
            }
        }
    }
}

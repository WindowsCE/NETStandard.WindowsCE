// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.IO
{
    [TestClass]
    public class StreamAsync
    {
        protected virtual Stream CreateStream()
        {
            return new MemoryStream();
        }

        [TestMethod]
        public void Stream_CopyToAsyncTest()
        {
            byte[] data = Enumerable.Range(0, 1000).Select(i => (byte)(i % 256)).ToArray();

            Stream ms = CreateStream();
            ms.Write(data, 0, data.Length);
            ms.Position = 0;

            var ms2 = new MemoryStream();
            var task = ms.CopyToAsync(ms2)
                .ContinueWith(t =>
                {
                    if (!data.SequenceEqual(ms2.ToArray()))
                        Assert.Fail("Stream was not copied correctly");
                    //Assert.AreEqual(data, ms2.ToArray());
                });
            task.Wait();
        }
    }
}
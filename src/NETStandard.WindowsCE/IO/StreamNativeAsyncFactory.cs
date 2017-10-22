using System.Threading;
using System.Threading.Tasks;

#if !WindowsCE
using Mock.System;
#endif

namespace System.IO
{
    // Creates asynchronous methods for Stream class based upon native implementation
    internal sealed class StreamNativeAsyncFactory
    {
        private static readonly Type HttpReadStreamType = Type.GetType("System.Net.HttpReadStream, " + Type2.MSCorLibQualifiedName, false, false);
        private static readonly Type HttpWriteStreamType = Type.GetType("System.Net.HttpWriteStream, " + Type2.MSCorLibQualifiedName, false, false);
        private static readonly Type SerialStreamType = Type.GetType("System.IO.Ports.SerialStream, " + Type2.MSCorLibQualifiedName, false, false);

        private readonly Stream stream;
        private readonly bool isApmSupported;

        public StreamNativeAsyncFactory(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            this.stream = stream;
            this.isApmSupported = IsApmSupported(stream);
        }

        public StreamNativeAsyncFactory(Stream stream, bool isApmSupported)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            this.stream = stream;
            this.isApmSupported = isApmSupported;
        }

        public StreamFlushAsync CreateFlushAsyncMethod()
        {
            return FlushAsync;
        }

        public StreamReadAsync CreateReadAsyncMethod()
        {
            if (isApmSupported)
                return ReadAsyncUsingApm;
            else
                return ReadAsyncUsingNewTask;
        }

        public StreamWriteAsync CreateWriteAsyncMethod()
        {
            if (isApmSupported)
                return WriteAsyncUsingApm;
            else
                return WriteAsyncUsingNewTask;
        }

        private Task FlushAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            return Task.Factory.StartNew(
                state => ((Stream)state).Flush(),
                stream,
                cancellationToken);
        }

        public static bool IsApmSupported(Stream stream)
        {
            // Those are the types known to implement APM on Compact Framework
            if (stream is Compression.DeflateStream)
                return true;
            if (stream is Compression.GZipStream)
                return true;
            if (stream is Net.Sockets.NetworkStream)
                return true;

            // TODO: Very expensive to call on loop
#if WindowsCE
            var streamType = stream.GetType();
            if (HttpReadStreamType.IsAssignableFrom(streamType))
                return true;
            if (HttpWriteStreamType.IsAssignableFrom(streamType))
                return true;
            if (SerialStreamType.IsAssignableFrom(streamType))
                return true;
#endif

            return false;
        }

        private Task<int> ReadAsyncUsingApm(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<int>(cancellationToken);

            return Task.Factory.FromAsync(
                (localBuffer, localOffset, localCount, callback, state) => ((Stream)state).BeginRead(localBuffer, localOffset, localCount, callback, state),
                iar => ((Stream)iar.AsyncState).EndRead(iar),
                buffer, offset, count, stream);
        }

        private Task<int> ReadAsyncUsingNewTask(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<int>(cancellationToken);

            return Task.Factory.StartNew(state =>
            {
                var aop = (AsyncOperationParameters)state;
                aop.CancellationToken.ThrowIfCancellationRequested();

                return aop.Source.Read(aop.Buffer, aop.Offset, aop.Count);
            },
            new AsyncOperationParameters(stream, buffer, offset, count, cancellationToken),
            cancellationToken);
        }

        private Task WriteAsyncUsingApm(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<int>(cancellationToken);

            return Task.Factory.FromAsync(
                (localBuffer, localOffset, localCount, callback, state) => ((Stream)state).BeginWrite(localBuffer, localOffset, localCount, callback, state),
                iar => ((Stream)iar.AsyncState).EndWrite(iar),
                buffer, offset, count, stream);
        }

        private Task WriteAsyncUsingNewTask(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<int>(cancellationToken);

            return Task.Factory.StartNew(
                state =>
                {
                    var aop = (AsyncOperationParameters)state;
                    aop.CancellationToken.ThrowIfCancellationRequested();

                    aop.Source.Write(aop.Buffer, aop.Offset, aop.Count);
                },
                new AsyncOperationParameters(stream, buffer, offset, count, cancellationToken),
                cancellationToken);
        }

        private struct AsyncOperationParameters
        {
            public Stream Source;
            public byte[] Buffer;
            public int Offset;
            public int Count;
            public CancellationToken CancellationToken;

            public AsyncOperationParameters(Stream source, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                Source = source;
                Buffer = buffer;
                Offset = offset;
                Count = count;
                CancellationToken = cancellationToken;
            }
        }
    }
}

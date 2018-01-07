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
#if WindowsCE
        private static readonly Type ChunkedReadStreamType = Type.GetType("System.Net.ChunkedReadStream, " + Type2.SystemQualifiedName, true, false);
        private static readonly Type CloseReadStreamType = Type.GetType("System.Net.CloseReadStream, " + Type2.SystemQualifiedName, true, false);
        private static readonly Type ContentLengthReadStreamType = Type.GetType("System.Net.ContentLengthReadStream, " + Type2.SystemQualifiedName, true, false);
        private static readonly Type BufferConnectStreamType = Type.GetType("System.Net.HttpWebRequest+BufferConnectStream, " + Type2.SystemQualifiedName, true, false);
        private static readonly Type WriteConnectStreamType = Type.GetType("System.Net.HttpWebRequest+WriteConnectStream, " + Type2.SystemQualifiedName, true, false);
        private static readonly Type SerialStreamType = Type.GetType("System.IO.Ports.SerialStream, " + Type2.SystemQualifiedName, true, false);
#endif

        private readonly Stream stream;
        private readonly bool isApmSupported;

        public StreamNativeAsyncFactory(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.isApmSupported = IsApmSupported(stream);
        }

        public StreamNativeAsyncFactory(Stream stream, bool isApmSupported)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.isApmSupported = isApmSupported;
        }

        public StreamFlushAsync CreateFlushAsyncMethod()
        {
            return FlushAsync;
        }

        public StreamReadAsync CreateReadAsyncMethod()
        {
            return isApmSupported
                ? (StreamReadAsync)ReadAsyncUsingApm
                : ReadAsyncUsingNewTask;
        }

        public StreamWriteAsync CreateWriteAsyncMethod()
        {
            return isApmSupported
                ? (StreamWriteAsync)WriteAsyncUsingApm
                : WriteAsyncUsingNewTask;
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
            // Those are the public types known to implement APM on Compact Framework
            if (stream is Compression.DeflateStream)
                return true;
            if (stream is Compression.GZipStream)
                return true;
            if (stream is Net.Sockets.NetworkStream)
                return true;

#if WindowsCE
            // TODO: Sort by usage
            Type streamType = stream.GetType();
            return streamType == ChunkedReadStreamType
                || streamType == CloseReadStreamType
                || streamType == ContentLengthReadStreamType
                || streamType == BufferConnectStreamType
                || streamType == WriteConnectStreamType
                || streamType == SerialStreamType;
#else
            return false;
#endif
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

        private sealed class AsyncOperationParameters
        {
            public readonly Stream Source;
            public readonly byte[] Buffer;
            public readonly int Offset;
            public readonly int Count;
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

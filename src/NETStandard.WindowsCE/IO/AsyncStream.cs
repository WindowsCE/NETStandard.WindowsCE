using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    public abstract class AsyncStream : Stream
    {
        private readonly ContingentAsyncMethods externalAsyncMethods;

        protected AsyncStream()
        {
            this.externalAsyncMethods = new ContingentAsyncMethods(this);
        }

        internal AsyncStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            this.externalAsyncMethods = new ContingentAsyncMethods(stream);
        }

        public void CopyTo(Stream destination)
            => externalAsyncMethods.copyTo(destination);

        public virtual void CopyTo(Stream destination, int bufferSize)
            => externalAsyncMethods.copyToWithCustomBufferSize(destination, bufferSize);

        public Task CopyToAsync(Stream destination)
            => externalAsyncMethods.copyToAsync(destination);

        public Task CopyToAsync(Stream destination, CancellationToken cancellationToken)
            => externalAsyncMethods.copyToAsync(destination, cancellationToken);

        public Task CopyToAsync(Stream destination, int bufferSize)
            => externalAsyncMethods.copyToAsyncWithCustomBufferSize(destination, bufferSize);

        public virtual Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => externalAsyncMethods.copyToAsyncWithCustomBufferSize(destination, bufferSize, cancellationToken);

        public Task FlushAsync()
            => externalAsyncMethods.flushAsync();

        public virtual Task FlushAsync(CancellationToken cancellationToken)
            => externalAsyncMethods.flushAsync(cancellationToken);

        public Task<int> ReadAsync(byte[] buffer, int offset, int count)
            => externalAsyncMethods.readAsync(buffer, offset, count);

        public virtual Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => externalAsyncMethods.readAsync(buffer, offset, count, cancellationToken);

        public Task WriteAsync(byte[] buffer, int offset, int count)
            => externalAsyncMethods.writeAsync(buffer, offset, count);

        public virtual Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => externalAsyncMethods.writeAsync(buffer, offset, count, cancellationToken);

        internal sealed class ContingentAsyncMethods
        {
            public readonly StreamCopyTo copyTo;
            public readonly StreamCopyToWithCustomBufferSize copyToWithCustomBufferSize;
            public readonly StreamCopyToAsync copyToAsync;
            public readonly StreamCopyToAsyncWithCustomBufferSize copyToAsyncWithCustomBufferSize;
            public readonly StreamFlushAsync flushAsync;
            public readonly StreamReadAsync readAsync;
            public readonly StreamWriteAsync writeAsync;

            public ContingentAsyncMethods(Stream stream)
            {
                var factory = new StreamNativeAsyncFactory(stream, false);
                flushAsync = factory.CreateFlushAsyncMethod();
                readAsync = factory.CreateReadAsyncMethod();
                writeAsync = factory.CreateWriteAsyncMethod();

                var copyToFactory = new StreamCopyToFactory(stream, factory);
                copyTo = copyToFactory.CreateStreamCopyToMethod();
                copyToWithCustomBufferSize = copyToFactory.CreateStreamCopyToWithCustomBufferSizeMethod();
                copyToAsync = copyToFactory.CreateStreamCopyToAsyncMethod();
                copyToAsyncWithCustomBufferSize = copyToFactory.CreateStreamCopyToAsyncWithCustomBufferSizeMethod();
            }
        }
    }
}

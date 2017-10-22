using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    public abstract class AsyncStream : Stream
    {
        private readonly StreamCopyTo copyToExternal;
        private readonly StreamCopyToWithCustomBufferSize copyToWithCustomBufferSizeExternal;
        private readonly StreamCopyToAsync copyToAsyncExternal;
        private readonly StreamCopyToAsyncWithCustomBufferSize copyToAsyncWithCustomBufferSizeExternal;
        private readonly StreamFlushAsync flushAsyncExternal;
        private readonly StreamReadAsync readAsyncExternal;
        private readonly StreamWriteAsync writeAsyncExternal;

        protected AsyncStream()
        {
            var factory = new StreamNativeAsyncFactory(this, false);
            flushAsyncExternal = factory.CreateFlushAsyncMethod();
            readAsyncExternal = factory.CreateReadAsyncMethod();
            writeAsyncExternal = factory.CreateWriteAsyncMethod();

            var copyToFactory = new StreamCopyToFactory(this, factory);
            copyToExternal = copyToFactory.CreateStreamCopyToMethod();
            copyToWithCustomBufferSizeExternal = copyToFactory.CreateStreamCopyToWithCustomBufferSizeMethod();
            copyToAsyncExternal = copyToFactory.CreateStreamCopyToAsyncMethod();
            copyToAsyncWithCustomBufferSizeExternal = copyToFactory.CreateStreamCopyToAsyncWithCustomBufferSizeMethod();
        }

        public void CopyTo(Stream destination)
            => copyToExternal(destination);

        public virtual void CopyTo(Stream destination, int bufferSize)
            => copyToWithCustomBufferSizeExternal(destination, bufferSize);

        public Task CopyToAsync(Stream destination)
            => copyToAsyncExternal(destination);

        public Task CopyToAsync(Stream destination, CancellationToken cancellationToken)
            => copyToAsyncExternal(destination, cancellationToken);

        public Task CopyToAsync(Stream destination, int bufferSize)
            => copyToAsyncWithCustomBufferSizeExternal(destination, bufferSize);

        public virtual Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => copyToAsyncWithCustomBufferSizeExternal(destination, bufferSize, cancellationToken);

        public Task FlushAsync()
            => flushAsyncExternal();

        public virtual Task FlushAsync(CancellationToken cancellationToken)
            => flushAsyncExternal(cancellationToken);

        public Task<int> ReadAsync(byte[] buffer, int offset, int count)
            => readAsyncExternal(buffer, offset, count);

        public virtual Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => readAsyncExternal(buffer, offset, count, cancellationToken);

        public Task WriteAsync(byte[] buffer, int offset, int count)
            => writeAsyncExternal(buffer, offset, count);

        public virtual Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => writeAsyncExternal(buffer, offset, count, cancellationToken);
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    public abstract class AsyncStream : Stream
    {
        public void CopyTo(Stream destination)
        {
            int bufferSize = Stream2.ResolveBufferSize(this);
            CopyTo(destination, bufferSize);
        }

        public virtual void CopyTo(Stream destination, int bufferSize)
        {
            Stream2.ValidateCopyToArgs(this, destination, bufferSize);
            Stream2.CopyToInternal(this, destination, bufferSize);
        }

        public Task CopyToAsync(Stream destination)
        {
            int bufferSize = Stream2.ResolveBufferSize(this);
            return CopyToAsync(destination, bufferSize, default(CancellationToken));
        }

        public Task CopyToAsync(Stream destination, int bufferSize)
            => CopyToAsync(destination, bufferSize, default(CancellationToken));

        public virtual Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            Stream2.ValidateCopyToArgs(this, destination, bufferSize);
            return Stream2.CopyToInternalAsync(this, destination, bufferSize, cancellationToken);
        }

        public Task FlushAsync()
            => FlushAsync(default(CancellationToken));

        public virtual Task FlushAsync(CancellationToken cancellationToken)
            => Stream2.FlushAsyncInternal(this, cancellationToken);

        public Task<int> ReadAsync(byte[] buffer, int offset, int count)
            => ReadAsync(buffer, offset, count, default(CancellationToken));

        public virtual Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Stream2.ReadAsyncInternal(this, buffer, offset, count, cancellationToken);

        public Task WriteAsync(byte[] buffer, int offset, int count)
            => WriteAsync(buffer, offset, count, default(CancellationToken));

        public virtual Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Stream2.WriteAsyncInternal(this, buffer, offset, count, cancellationToken);
    }
}

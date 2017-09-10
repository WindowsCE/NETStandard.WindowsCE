using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    public abstract class AsyncStream : Stream
    {
        public Task CopyToAsync(Stream destination)
        {
            int bufferSize = Stream2.DefaultCopyBufferSize;

            if (CanSeek)
            {
                long length = Length;
                long position = Position;
                if (length <= position) // Handles negative overflows
                {
                    // If we go down this branch, it means there are
                    // no bytes left in this stream.

                    // Ideally we would just return Task.CompletedTask here,
                    // but CopyToAsync(Stream, int, CancellationToken) was already
                    // virtual at the time this optimization was introduced. So
                    // if it does things like argument validation (checking if destination
                    // is null and throwing an exception), then await fooStream.CopyToAsync(null)
                    // would no longer throw if there were no bytes left. On the other hand,
                    // we also can't roll our own argument validation and return Task.CompletedTask,
                    // because it would be a breaking change if the stream's override didn't throw before,
                    // or in a different order. So for simplicity, we just set the bufferSize to 1
                    // (not 0 since the default implementation throws for 0) and forward to the virtual method.
                    bufferSize = 1;
                }
                else
                {
                    long remaining = length - position;
                    if (remaining > 0) // In the case of a positive overflow, stick to the default size
                        bufferSize = (int)Math.Min(bufferSize, remaining);
                }
            }

            return CopyToAsync(destination, bufferSize);
        }

        public Task CopyToAsync(Stream destination, int bufferSize)
            => CopyToAsync(destination, bufferSize, CancellationToken.None);

        public virtual Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            Stream2.ValidateCopyToArgs(this, destination, bufferSize);
            return Stream2.CopyToAsyncInternal(this, destination, bufferSize, cancellationToken);
        }

        public Task FlushAsync()
            => FlushAsync(CancellationToken.None);

        public virtual Task FlushAsync(CancellationToken cancellationToken)
            => Stream2.FlushAsyncInternal(this, cancellationToken);

        public Task<int> ReadAsync(byte[] buffer, int offset, int count)
            => ReadAsync(buffer, offset, count, CancellationToken.None);

        public virtual Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Stream2.ReadAsyncInternal(this, buffer, offset, count, cancellationToken);

        public Task WriteAsync(byte[] buffer, int offset, int count)
            => WriteAsync(buffer, offset, count, CancellationToken.None);

        public virtual Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Stream2.WriteAsyncInternal(this, buffer, offset, count, cancellationToken);
    }
}

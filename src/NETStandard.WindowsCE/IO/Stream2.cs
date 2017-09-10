using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    public static class Stream2
    {
        //public static readonly Stream Null
        //    = Stream.Null;

        // We pick a value that is multiple of 4096.
        // The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
        // improvement in Copy performance.
        internal const int DefaultCopyBufferSize = 8192;

        public static void CopyTo(this Stream source, Stream destination)
        {
            int bufferSize = DefaultCopyBufferSize;

            if (source.CanSeek)
            {
                long length = source.Length;
                long position = source.Position;
                if (length <= position) // Handles negative overflows
                {
                    // No bytes left in stream
                    // Call the other overload with a bufferSize of 1,
                    // in case it's made virtual in the future
                    bufferSize = 1;
                }
                else
                {
                    long remaining = length - position;
                    if (remaining > 0) // In the case of a positive overflow, stick to the default size
                        bufferSize = (int)Math.Min(bufferSize, remaining);
                }
            }

            CopyTo(source, destination, bufferSize);
        }
        public static void CopyTo(this Stream source, Stream destination, int bufferSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateCopyToArgs(source, destination, bufferSize);

            byte[] buffer = new byte[bufferSize];
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, read);
            }
        }

        public static Task CopyToAsync(this Stream source, Stream destination)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            int bufferSize = DefaultCopyBufferSize;

            if (source.CanSeek)
            {
                long length = source.Length;
                long position = source.Position;
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

            return CopyToAsync(source, destination, bufferSize);
        }

        public static Task CopyToAsync(this Stream source, Stream destination, int bufferSize)
            => CopyToAsync(source, destination, bufferSize, CancellationToken.None);

        public static Task CopyToAsync(this Stream source, Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var asyncSource = source as AsyncStream;
            if (asyncSource != null)
                return asyncSource.CopyToAsync(destination, bufferSize, cancellationToken);

            ValidateCopyToArgs(source, destination, bufferSize);
            return CopyToAsyncInternal(source, destination, bufferSize, cancellationToken);
        }

        internal static async Task CopyToAsyncInternal(Stream source, Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            Debug.Assert(destination != null);
            Debug.Assert(bufferSize > 0);
            Debug.Assert(source.CanRead);
            Debug.Assert(destination.CanWrite);

            byte[] buffer = new byte[bufferSize];
            while (true)
            {
                int bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0) break;
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            }
        }

        public static Task FlushAsync(this Stream source)
            => FlushAsync(source, CancellationToken.None);

        public static Task FlushAsync(this Stream source, CancellationToken cancellationToken)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var asyncSource = source as AsyncStream;
            if (asyncSource != null)
                return asyncSource.FlushAsync(cancellationToken);

            return FlushAsyncInternal(source, cancellationToken);
        }

        internal static Task FlushAsyncInternal(Stream source, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            return Task.Factory.StartNew(
                state => ((Stream)state).Flush(),
                source,
                cancellationToken);
        }

        public static Task<int> ReadAsync(this Stream source, byte[] buffer, int offset, int count)
            => ReadAsync(source, buffer, offset, count, CancellationToken.None);

        public static Task<int> ReadAsync(this Stream source, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var asyncSource = source as AsyncStream;
            if (asyncSource != null)
                return asyncSource.ReadAsync(buffer, offset, count, cancellationToken);

            return ReadAsyncInternal(source, buffer, offset, count, cancellationToken);
        }

        internal static Task<int> ReadAsyncInternal(Stream source, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<int>(cancellationToken);

            return Task.Factory.FromAsync(
                (localBuffer, localOffset, localCount, callback, state) => ((Stream)state).BeginRead(localBuffer, localOffset, localCount, callback, state),
                iar => ((Stream)iar.AsyncState).EndRead(iar),
                buffer, offset, count, source);
        }

        public static Task WriteAsync(this Stream source, byte[] buffer, int offset, int count)
            => WriteAsync(source, buffer, offset, count, CancellationToken.None);

        public static Task WriteAsync(this Stream source, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var asyncSource = source as AsyncStream;
            if (asyncSource != null)
                return asyncSource.WriteAsync(buffer, offset, count, cancellationToken);

            return WriteAsyncInternal(source, buffer, offset, count, cancellationToken);
        }

        internal static Task WriteAsyncInternal(Stream source, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<int>(cancellationToken);

            return Task.Factory.FromAsync(
                (localBuffer, localOffset, localCount, callback, state) => ((Stream)state).BeginWrite(localBuffer, localOffset, localCount, callback, state),
                iar => ((Stream)iar.AsyncState).EndWrite(iar),
                buffer, offset, count, source);
        }

        /// <summary>
        /// Validate the arguments to CopyTo, as would Stream.CopyTo.
        /// </summary>
        internal static void ValidateCopyToArgs(Stream source, Stream destination, int bufferSize)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize),/* bufferSize,*/ "Positive number required.");
            }

            bool sourceCanRead = source.CanRead;
            if (!sourceCanRead && !source.CanWrite)
            {
                throw new ObjectDisposedException(null, "Cannot access a closed Stream.");
            }

            bool destinationCanWrite = destination.CanWrite;
            if (!destinationCanWrite && !destination.CanRead)
            {
                throw new ObjectDisposedException(nameof(destination), "Cannot access a closed Stream.");
            }

            if (!sourceCanRead)
            {
                throw new NotSupportedException("Stream does not support reading.");
            }

            if (!destinationCanWrite)
            {
                throw new NotSupportedException("Stream does not support writing.");
            }
        }
    }
}

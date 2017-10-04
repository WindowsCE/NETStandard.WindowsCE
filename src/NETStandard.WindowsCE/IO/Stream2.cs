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
        internal const int DefaultCopyBufferSize = 40960;

        public static void CopyTo(this Stream source, Stream destination)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            int bufferSize = ResolveBufferSize(source);
            CopyTo(source, destination, bufferSize);
        }
        public static void CopyTo(this Stream source, Stream destination, int bufferSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateCopyToArgs(source, destination, bufferSize);

            CopyToInternal(source, destination, bufferSize);
        }

        internal static void CopyToInternal(Stream source, Stream destination, int bufferSize)
        {
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

            int bufferSize = ResolveBufferSize(source);
            return CopyToAsync(source, destination, bufferSize, default(CancellationToken));
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
            return CopyToInternalAsync(source, destination, bufferSize, cancellationToken);
        }

        internal static Task CopyToInternalAsync(Stream source, Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            Debug.Assert(destination != null);
            Debug.Assert(bufferSize > 0);
            Debug.Assert(source.CanRead);
            Debug.Assert(destination.CanWrite);

            var asyncSource = source as AsyncStream;
            var asyncDestination = destination as AsyncStream;
            bool isSourceAsync = asyncSource != null;
            bool isDestAsync = asyncDestination != null;
            if (isSourceAsync && isDestAsync)
                return CopyToFullAsync(asyncSource, asyncDestination, bufferSize, cancellationToken);
            else if (isSourceAsync)
                return CopyToHalfAsync(asyncSource, destination, bufferSize, cancellationToken);
            else if (isDestAsync)
                return CopyToHalfAsync(source, asyncDestination, bufferSize, cancellationToken);
            else
                return CopyToNoAsync(source, destination, bufferSize, cancellationToken);
        }

        private static async Task CopyToFullAsync(AsyncStream source, AsyncStream destination, int bufferSize, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[bufferSize];
            while (true)
            {
                int bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0) break;
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task CopyToHalfAsync(AsyncStream source, Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            bool isApmSupported = IsApmSupported(destination);
            byte[] buffer = new byte[bufferSize];
            while (true)
            {
                int bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0) break;
                if (isApmSupported)
                    await WriteAsyncApmInternal(destination, buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                else
                    await WriteAsyncNoApmInternal(destination, buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task CopyToHalfAsync(Stream source, AsyncStream destination, int bufferSize, CancellationToken cancellationToken)
        {
            bool isApmSupported = IsApmSupported(source);
            byte[] buffer = new byte[bufferSize];
            while (true)
            {
                int bytesRead;
                if (isApmSupported)
                    bytesRead = await ReadAsyncApmInternal(source, buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                else
                    bytesRead = await ReadAsyncNoApmInternal(source, buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

                if (bytesRead == 0) break;
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task CopyToNoAsync(Stream source, Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            bool isApmSupportedBySource = IsApmSupported(source);
            bool isApmSupportedByDestination = IsApmSupported(destination);
            byte[] buffer = new byte[bufferSize];
            while (true)
            {
                int bytesRead;
                if (isApmSupportedBySource)
                    bytesRead = await ReadAsyncApmInternal(source, buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                else
                    bytesRead = await ReadAsyncNoApmInternal(source, buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

                if (bytesRead == 0) break;

                if (isApmSupportedByDestination)
                    await WriteAsyncApmInternal(destination, buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                else
                    await WriteAsyncNoApmInternal(destination, buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
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
            if (IsApmSupported(source))
                return ReadAsyncApmInternal(source, buffer, offset, count, cancellationToken);
            else
                return ReadAsyncNoApmInternal(source, buffer, offset, count, cancellationToken);
        }

        internal static Task<int> ReadAsyncApmInternal(Stream source, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<int>(cancellationToken);

            return Task.Factory.FromAsync(
                (localBuffer, localOffset, localCount, callback, state) => ((Stream)state).BeginRead(localBuffer, localOffset, localCount, callback, state),
                iar => ((Stream)iar.AsyncState).EndRead(iar),
                buffer, offset, count, source);
        }

        internal static Task<int> ReadAsyncNoApmInternal(Stream source, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<int>(cancellationToken);

            return Task.Factory.StartNew(state =>
            {
                var aop = (AsyncOperationParameters)state;
                aop.cancellationToken.ThrowIfCancellationRequested();

                return aop.source.Read(aop.buffer, aop.offset, aop.count);
            },
            new AsyncOperationParameters(source, buffer, offset, count, cancellationToken),
            cancellationToken);
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
            if (IsApmSupported(source))
                return WriteAsyncApmInternal(source, buffer, offset, count, cancellationToken);
            else
                return WriteAsyncNoApmInternal(source, buffer, offset, count, cancellationToken);
        }

        internal static Task WriteAsyncApmInternal(Stream source, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<int>(cancellationToken);

            return Task.Factory.FromAsync(
                (localBuffer, localOffset, localCount, callback, state) => ((Stream)state).BeginWrite(localBuffer, localOffset, localCount, callback, state),
                iar => ((Stream)iar.AsyncState).EndWrite(iar),
                buffer, offset, count, source);
        }

        internal static Task WriteAsyncNoApmInternal(Stream source, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<int>(cancellationToken);

            return Task.Factory.StartNew(
                state =>
                {
                    var aop = (AsyncOperationParameters)state;
                    aop.cancellationToken.ThrowIfCancellationRequested();

                    aop.source.Write(aop.buffer, aop.offset, aop.count);
                },
                new AsyncOperationParameters(source, buffer, offset, count, cancellationToken),
                cancellationToken);
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

        internal static int ResolveBufferSize(Stream source)
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

            return bufferSize;
        }

        internal static bool IsApmSupported(Stream stream)
        {
            // These are the types known to implement APM on Compact Framework
            if (stream is Compression.DeflateStream)
                return true;
            else if (stream is Compression.GZipStream)
                return true;
            else if (stream is Net.Sockets.NetworkStream)
                return true;

            return false;
        }

        internal struct AsyncOperationParameters
        {
            public Stream source;
            public byte[] buffer;
            public int offset;
            public int count;
            public CancellationToken cancellationToken;

            public AsyncOperationParameters(Stream source, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                this.source = source;
                this.buffer = buffer;
                this.offset = offset;
                this.count = count;
                this.cancellationToken = cancellationToken;
            }
        }
    }
}

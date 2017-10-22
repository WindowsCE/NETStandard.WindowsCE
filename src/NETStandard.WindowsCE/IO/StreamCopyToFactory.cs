using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    internal sealed class StreamCopyToFactory
    {
        // We pick a value that is multiple of 4096.
        // The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
        // improvement in Copy performance.
        // The .NET Compact Framework 3.5 GC does not have generations them we should reduce the collect pressure by
        // reducing this value
        public const int DefaultCopyBufferSize = 16384;

        private readonly StreamReadAsync readAsync;
        private readonly Stream source;

        public StreamCopyToFactory(Stream source, bool buildAsync = true)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            this.source = source;

            if (buildAsync)
            {
                var factory = new StreamLocalAsyncFactory(source);
                readAsync = factory.CreateReadAsyncMethod();
            }
        }

        public StreamCopyToFactory(Stream source, StreamNativeAsyncFactory factory)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            this.source = source;

            readAsync = factory.CreateReadAsyncMethod();
        }

        public StreamCopyTo CreateStreamCopyToMethod()
        {
            return CopyTo;
        }

        public StreamCopyToWithCustomBufferSize CreateStreamCopyToWithCustomBufferSizeMethod()
        {
            return CopyTo;
        }

        public StreamCopyToAsync CreateStreamCopyToAsyncMethod()
        {
            return CopyToAsync;
        }

        public StreamCopyToAsyncWithCustomBufferSize CreateStreamCopyToAsyncWithCustomBufferSizeMethod()
        {
            return CopyToAsync;
        }

        private void CopyTo(Stream destination)
        {
            int bufferSize = GetCopyBufferSize(source);
            ValidateCopyToArgs(source, destination, bufferSize);
            CopyToInternal(destination, bufferSize);
        }

        private void CopyTo(Stream destination, int bufferSize)
        {
            ValidateCopyToArgs(source, destination, bufferSize);
            CopyToInternal(destination, bufferSize);
        }

        private void CopyToInternal(Stream destination, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, read);
            }
        }

        private Task CopyToAsync(Stream destination, CancellationToken cancellationToken)
        {
            int bufferSize = GetCopyBufferSize(source);
            ValidateCopyToArgs(source, destination, bufferSize);

            return CopyToAsyncInternal(destination, bufferSize, cancellationToken);
        }

        private Task CopyToAsync(Stream destination, Int32 bufferSize, CancellationToken cancellationToken)
        {
            ValidateCopyToArgs(source, destination, bufferSize);

            return CopyToAsyncInternal(destination, bufferSize, cancellationToken);
        }

        private async Task CopyToAsyncInternal(Stream destination, Int32 bufferSize, CancellationToken cancellationToken)
        {
            Debug.Assert(destination != null);
            Debug.Assert(bufferSize > 0);
            Debug.Assert(source.CanRead);
            Debug.Assert(destination.CanWrite);

            var destinationFactory = new StreamLocalAsyncFactory(destination);
            var writeAsync = destinationFactory.CreateWriteAsyncMethod();
            destinationFactory = null;

            byte[] buffer = new byte[bufferSize];
            while (true)
            {
                int bytesRead = await readAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0) break;
                await writeAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            }
        }

        private static int GetCopyBufferSize(Stream source)
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
                    if (remaining > 0)
                    {
                        // In the case of a positive overflow, stick to the default size
                        bufferSize = (int)Math.Min(bufferSize, remaining);
                    }
                }
            }

            return bufferSize;
        }

        /// <summary>
        /// Validate the arguments to CopyTo, as would Stream.CopyTo.
        /// </summary>
        private static void ValidateCopyToArgs(Stream source, Stream destination, int bufferSize)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize),/* bufferSize,*/ SR.ArgumentOutOfRange_NeedPosNum);
            }

            bool sourceCanRead = source.CanRead;
            if (!sourceCanRead && !source.CanWrite)
            {
                throw new ObjectDisposedException(null, SR.ObjectDisposed_StreamClosed);
            }

            bool destinationCanWrite = destination.CanWrite;
            if (!destinationCanWrite && !destination.CanRead)
            {
                throw new ObjectDisposedException(nameof(destination), SR.ObjectDisposed_StreamClosed);
            }

            if (!sourceCanRead)
            {
                throw new NotSupportedException(SR.NotSupported_UnreadableStream);
            }

            if (!destinationCanWrite)
            {
                throw new NotSupportedException(SR.NotSupported_UnwritableStream);
            }
        }
    }
}

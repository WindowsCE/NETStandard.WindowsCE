using System.Threading;
using System.Threading.Tasks;

#if !WindowsCE
using Mock.System;
#endif

namespace System.IO
{
    public static class Stream2
    {
        public static AsyncStream ConvertToAsync(this Stream stream)
        {
            AsyncStream asyncStream = stream as AsyncStream;
            if (asyncStream != null)
                return asyncStream;

            return new AsyncStreamWrapper(stream);
        }

        public static void CopyTo(this Stream source, Stream destination)
        {
            var copyTo = new StreamCopyToFactory(source, false).CreateStreamCopyToMethod();
            copyTo(destination);
        }

        public static void CopyTo(this Stream source, Stream destination, int bufferSize)
        {
            var copyTo = new StreamCopyToFactory(source, false).CreateStreamCopyToWithCustomBufferSizeMethod();
            copyTo(destination, bufferSize);
        }

        public static Task CopyToAsync(this Stream source, Stream destination)
        {
            var copyToAsync = new StreamCopyToFactory(source, true).CreateStreamCopyToAsyncMethod();
            return copyToAsync(destination);
        }

        public static Task CopyToAsync(this Stream source, Stream destination, CancellationToken cancellationToken)
        {
            var copyToAsync = new StreamCopyToFactory(source, true).CreateStreamCopyToAsyncMethod();
            return copyToAsync(destination, cancellationToken);
        }

        public static Task CopyToAsync(this Stream source, Stream destination, int bufferSize)
        {
            var copyToAsync = new StreamCopyToFactory(source, true).CreateStreamCopyToAsyncWithCustomBufferSizeMethod();
            return copyToAsync(destination, bufferSize);
        }

        public static Task CopyToAsync(this Stream source, Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            var copyToAsync = new StreamCopyToFactory(source, true).CreateStreamCopyToAsyncWithCustomBufferSizeMethod();
            return copyToAsync(destination, bufferSize, cancellationToken);
        }

        public static Task FlushAsync(this Stream stream)
        {
            var flushAsync = new StreamLocalAsyncFactory(stream).CreateFlushAsyncMethod();
            return flushAsync();
        }

        public static Task FlushAsync(this Stream stream, CancellationToken cancellationToken)
        {
            var flushAsync = new StreamLocalAsyncFactory(stream).CreateFlushAsyncMethod();
            return flushAsync(cancellationToken);
        }

        public static Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            var readAsync = new StreamLocalAsyncFactory(stream).CreateReadAsyncMethod();
            return readAsync(buffer, offset, count);
        }

        public static Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var readAsync = new StreamLocalAsyncFactory(stream).CreateReadAsyncMethod();
            return readAsync(buffer, offset, count, cancellationToken);
        }

        public static Task WriteAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            var writeAsync = new StreamLocalAsyncFactory(stream).CreateWriteAsyncMethod();
            return writeAsync(buffer, offset, count);
        }

        public static Task WriteAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var writeAsync = new StreamLocalAsyncFactory(stream).CreateWriteAsyncMethod();
            return writeAsync(buffer, offset, count, cancellationToken);
        }
    }
}

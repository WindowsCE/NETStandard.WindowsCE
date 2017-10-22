using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    internal delegate void StreamCopyTo(Stream destination);
    internal delegate void StreamCopyToWithCustomBufferSize(Stream destination, int bufferSize);
    internal delegate Task StreamCopyToAsync(Stream destination, CancellationToken cancellationToken = default(CancellationToken));
    internal delegate Task StreamCopyToAsyncWithCustomBufferSize(Stream destination, int bufferSize, CancellationToken cancellationToken = default(CancellationToken));
    internal delegate Task StreamFlushAsync(CancellationToken cancellationToken = default(CancellationToken));
    internal delegate Task<int> StreamReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default(CancellationToken));
    internal delegate Task StreamWriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default(CancellationToken));
}

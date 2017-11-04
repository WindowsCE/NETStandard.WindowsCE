namespace System.IO
{
    internal sealed class AsyncStreamWrapper : AsyncStream
    {
        private readonly Stream underlyingStream;

        public AsyncStreamWrapper(Stream stream)
            : base(stream)
        {
            this.underlyingStream = stream;
        }

        public override bool CanRead => underlyingStream.CanRead;

        public override bool CanSeek => underlyingStream.CanSeek;

        public override bool CanWrite => underlyingStream.CanWrite;

        public override long Length => underlyingStream.Length;

        public override long Position
        {
            get { return underlyingStream.Position; }
            set { underlyingStream.Position = value; }
        }

        public override void Flush()
        {
            underlyingStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return underlyingStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return underlyingStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            underlyingStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            underlyingStream.Write(buffer, offset, count);
        }
    }
}

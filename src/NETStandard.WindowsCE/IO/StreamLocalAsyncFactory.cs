namespace System.IO
{
    internal sealed class StreamLocalAsyncFactory
    {
        private readonly AsyncStream stream;
        private readonly StreamNativeAsyncFactory nativeAsyncFactory;

        public StreamLocalAsyncFactory(Stream stream)
        {
            this.stream = stream as AsyncStream;

            if (this.stream == null)
                nativeAsyncFactory = new StreamNativeAsyncFactory(stream);
        }

        public StreamFlushAsync CreateFlushAsyncMethod()
        {
            if (stream == null)
                return nativeAsyncFactory.CreateFlushAsyncMethod();

            return stream.FlushAsync;
        }

        public StreamReadAsync CreateReadAsyncMethod()
        {
            if (stream == null)
                return nativeAsyncFactory.CreateReadAsyncMethod();

            return stream.ReadAsync;
        }

        public StreamWriteAsync CreateWriteAsyncMethod()
        {
            if (stream == null)
                return nativeAsyncFactory.CreateWriteAsyncMethod();

            return stream.WriteAsync;
        }
    }
}

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net
{
    /// <summary>
    /// Provides asynchronous methods for <see cref="WebRequest"/> class.
    /// </summary>
    public static class WebRequestAsyncExtensions
    {
        /// <summary>
        /// Asynchronously retrieves a <see cref="Stream" /> for writing data
        /// to the Internet resource.
        /// </summary>
        /// <param name="source">The object to retrieve a stream.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public static Task<Stream> GetRequestStreamAsync(this WebRequest source)
        {
            var function = source.Timeout > 0
                ? (Func2<object, Task<Stream>>)ExecuteGetRequestStreamAsyncWithTimeout
                : (Func2<object, Task<Stream>>)ExecuteGetRequestStreamAsync;
            
            // Offload to a different thread to avoid blocking the caller during request submission.
            return Task.Factory.StartNew(function, source).Unwrap();
        }

        /// <summary>
        /// Asynchronously retrieves a response to an Internet request.
        /// </summary>
        /// <param name="source">The object to retrieve a response.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public static Task<WebResponse> GetResponseAsync(this WebRequest source)
        {
            var function = source.Timeout > 0
                ? (Func2<object, Task<WebResponse>>)ExecuteGetResponseAsyncWithTimeout
                : (Func2<object, Task<WebResponse>>)ExecuteGetResponseAsync;
            
            // See comment in GetRequestStreamAsync().  Same logic applies here.
            return Task.Factory.StartNew(function, source).Unwrap();
        }

        private static Task<Stream> ExecuteGetRequestStreamAsync(object state)
        {
            return Task.Factory.FromAsync(
                (callback, state2) => ((WebRequest)state2).BeginGetRequestStream(callback, state2),
                iar => ((WebRequest)iar.AsyncState).EndGetRequestStream(iar),
                state);
        }

        private static Task<Stream> ExecuteGetRequestStreamAsyncWithTimeout(object state)
        {
            var source = (WebRequest)state;
            var timer = new Timer(s => ((WebRequest)s).Abort(), source, source.Timeout, Timeout.Infinite);
            var task = Task.Factory.FromAsync(
                (callback, state2) => ((WebRequest)state2).BeginGetRequestStream(callback, state2),
                iar => ((WebRequest)iar.AsyncState).EndGetRequestStream(iar),
                state);
            task.ContinueWith(
                (t, s) => ((Timer)s).Dispose(),
                timer);
            return task;
        }

        private static Task<WebResponse> ExecuteGetResponseAsync(object state)
        {
            return Task.Factory.FromAsync(
                (callback, state2) => ((WebRequest)state2).BeginGetResponse(callback, state2),
                iar => ((WebRequest)iar.AsyncState).EndGetResponse(iar),
                state);
        }

        private static Task<WebResponse> ExecuteGetResponseAsyncWithTimeout(object state)
        {
            var source = (WebRequest)state;
            var timer = new Timer(s => ((WebRequest)s).Abort(), source, source.Timeout, Timeout.Infinite);
            var task = Task.Factory.FromAsync(
                (callback, state2) => ((WebRequest)state2).BeginGetResponse(callback, state2),
                iar => ((WebRequest)iar.AsyncState).EndGetResponse(iar),
                state);
            task.ContinueWith(
                (t, s) => ((Timer)s).Dispose(),
                timer);
            return task;
        }
    }
}

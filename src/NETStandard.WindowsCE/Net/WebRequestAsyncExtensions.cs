using System.IO;
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
            // Offload to a different thread to avoid blocking the caller during request submission.
            return Task.Run(() =>
                Task.Factory.FromAsync(
                    (callback, state) => ((WebRequest)state).BeginGetRequestStream(callback, state),
                    iar => ((WebRequest)iar.AsyncState).EndGetRequestStream(iar),
                    source));
        }

        /// <summary>
        /// Asynchronously retrieves a response to an Internet request.
        /// </summary>
        /// <param name="source">The object to retrieve a response.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public static Task<WebResponse> GetResponseAsync(this WebRequest source)
        {
            // See comment in GetRequestStreamAsync().  Same logic applies here.
            return Task.Run(() =>
                Task.Factory.FromAsync(
                    (callback, state) => ((WebRequest)state).BeginGetResponse(callback, state),
                    iar => ((WebRequest)iar.AsyncState).EndGetResponse(iar),
                    source));
        }
    }
}

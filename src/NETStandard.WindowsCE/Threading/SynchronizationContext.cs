#if NET35_CF
namespace System.Threading
#else
using System;
using System.Threading;

namespace Mock.System.Threading
#endif
{
    /// <summary>
    /// Represents a method to be called when a message is to be dispatched to
    /// a synchronization context.
    /// </summary>
    /// <param name="state">The object passed to the delegate.</param>
    public delegate void SendOrPostCallback(object state);

    /// <summary>
    /// Provides the basic functionality for propagating a synchronization
    /// context in various synchronization models.
    /// </summary>
    public class SynchronizationContext
    {
        private static readonly LocalDataStoreSlot _syncContextSlot
            = Thread.AllocateDataSlot();

        /// <summary>
        /// Initializes a new instance of <see cref="SynchronizationContext"/> class.
        /// </summary>
        public SynchronizationContext() { }

        /// <summary>
        /// Gets the synchronization context for the current thread.
        /// </summary>
        public static SynchronizationContext Current
        {
            get
            {
                object value = Thread.GetData(_syncContextSlot);
                return value as SynchronizationContext;
            }
        }

        /// <summary>
        /// Sets the current synchronization context.
        /// </summary>
        /// <param name="syncContext">
        /// The <see cref="SynchronizationContext"/> object to be set.
        /// </param>
        public static void SetSynchronizationContext(SynchronizationContext syncContext)
        {
            Thread.SetData(_syncContextSlot, syncContext);
        }

        /// <summary>
        /// When overridden in a derived class, dispatches a synchronous
        /// message to a synchronization context.
        /// </summary>
        /// <param name="d">The <see cref="SendOrPostCallback"/> delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public virtual void Send(SendOrPostCallback d, object state)
        {
            d(state);
        }

        /// <summary>
        /// When overridden in a derived class, dispatches an asynchronous
        /// message to a synchronization context.
        /// </summary>
        /// <param name="d">The <see cref="SendOrPostCallback"/> delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public virtual void Post(SendOrPostCallback d, object state)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(d), state);
        }

        /// <summary>
        /// When overridden in a derived class, creates a copy of the
        /// synchronization context.
        /// </summary>
        /// <returns>A new <see cref="SynchronizationContext"/> object.</returns>
        public virtual SynchronizationContext CreateCopy()
        {
            return new SynchronizationContext();
        }
    }
}

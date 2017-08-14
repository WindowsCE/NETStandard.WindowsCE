// Ref: https://github.com/theraot/Theraot

using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{

    /// <summary>
    /// Provides an awaitable context for switching into a target environment.
    /// </summary>
    public struct YieldAwaitable
    {
        /// <summary>
        /// Gets an awaiter for this <see cref="YieldAwaitable"/>.
        /// </summary>
        /// <returns>An awaiter for this awaitable.</returns>
        public YieldAwaiter GetAwaiter()
            => new YieldAwaiter();

        /// <summary>
        /// Provides an awaiter that switches into a target environment.
        /// </summary>
        public struct YieldAwaiter : INotifyCompletion
        {
            private static readonly WaitCallback _waitCallbackRunAction = RunAction;
            private static readonly SendOrPostCallback _waitSendOrPostCallbackRunAction = RunAction;

            public bool IsCompleted
                => false;

            public void GetResult() { }

            public void OnCompleted(Action continuation)
            {
                if (continuation == null)
                    throw new ArgumentNullException(nameof(continuation));

                var syncCtx = SynchronizationContext.Current;
                if (syncCtx != null && syncCtx.GetType() != typeof(SynchronizationContext))
                {
                    syncCtx.Post(_waitSendOrPostCallbackRunAction, continuation);
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(_waitCallbackRunAction, continuation);
                }
            }

            private static void RunAction(object state)
            {
                ((Action)state)();
            }
        }
    }
}

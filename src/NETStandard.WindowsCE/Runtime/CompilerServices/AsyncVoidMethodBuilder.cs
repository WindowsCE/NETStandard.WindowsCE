using System.Threading;

#if NET35_CF
using System.Runtime.ExceptionServices;
#else
using Mock.System.Runtime.ExceptionServices;
#endif

namespace System.Runtime.CompilerServices
{
    public struct AsyncVoidMethodBuilder
    {
        /// <summary>The synchronization context associated with this operation.</summary>
        private SynchronizationContext m_synchronizationContext;

        public static AsyncVoidMethodBuilder Create()
        {
            var sc = SynchronizationContext.Current;
            return new AsyncVoidMethodBuilder() { m_synchronizationContext = sc };
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // Should not get called as we don't implement the optimization that this method is used for.
            throw new NotImplementedException();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void SetResult()
        { }

        public void SetException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var edi = ExceptionDispatchInfo.Capture(exception);
            if (m_synchronizationContext != null)
            {
                try
                {
                    m_synchronizationContext.Post(state => ((ExceptionDispatchInfo)state).Throw(), edi);
                    return;
                }
                catch (Exception postException)
                {
                    throw new AggregateException(exception, postException);
                }
            }

            edi.Throw();
        }
    }
}

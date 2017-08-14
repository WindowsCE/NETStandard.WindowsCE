using System.Threading.Tasks;

#if NET35_CF
using InternalOCE = System.OperationCanceledException;
#else
using InternalOCE = Mock.System.OperationCanceledException;
#endif

namespace System.Runtime.CompilerServices
{
    public class AsyncTaskMethodBuilder<TResult>
    {
        Task<TResult> _task;

        public Task<TResult> Task
        {
            get
            {
                Task<TResult> task = _task;
                if (task == null)
                    _task = task = new Task<TResult>();

                return task;
            }
        }

        public static AsyncTaskMethodBuilder<TResult> Create()
        {
            return new AsyncTaskMethodBuilder<TResult>();
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // Should not get called as we don't implement the optimization that this method is used for.
            throw new NotImplementedException();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        public void SetResult(TResult result)
        {
            Task<TResult> task = _task;
            if (task == null)
            {
                _task = new Task<TResult>(result, null);
                return;
            }

            if (!task.TrySetResult(result))
                throw new InvalidOperationException("Task is already completed");
        }

        public void SetException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            Task<TResult> task = _task;
            if (task == null)
            {
                _task = new Task<TResult>(default(TResult), exception);
                return;
            }

            var cancelException = exception as InternalOCE;
            bool setException;
            if (cancelException != null)
                setException = task.TrySetCanceled(cancelException.CancellationToken);
            else
                setException = task.TrySetException(exception);

            if (!setException)
                throw new InvalidOperationException("Task is already completed");
        }
    }
}

using System.Collections.Generic;

namespace System.Threading.Tasks
{
    public class TaskCompletionSource<TResult>
    {
        private readonly Task<TResult> _task;

        public Task<TResult> Task => _task;

        public TaskCompletionSource()
        {
            _task = new Task<TResult>();
        }

        public TaskCompletionSource(object state)
        {
            _task = new Task<TResult>(state);
        }

        public void SetException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (_task.TrySetException(exception))
                return;

            throw new InvalidOperationException(SR.TaskT_TransitionToFinal_AlreadyCompleted);
        }

        public void SetException(IEnumerable<Exception> exceptions)
        {
            if (exceptions == null)
                throw new ArgumentNullException(nameof(exceptions));

            AggregateException agg = new AggregateException(exceptions);
            if (_task.TrySetException(agg))
                return;

            throw new InvalidOperationException(SR.TaskT_TransitionToFinal_AlreadyCompleted);
        }

        public void SetResult(TResult result)
        {
            if (_task.TrySetResult(result))
                return;

            throw new InvalidOperationException(SR.TaskT_TransitionToFinal_AlreadyCompleted);
        }

        public void SetCanceled()
        {
            if (_task.TrySetCanceled(default(CancellationToken)))
                return;

            throw new InvalidOperationException(SR.TaskT_TransitionToFinal_AlreadyCompleted);
        }

        public bool TrySetException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return _task.TrySetException(exception);
        }

        public bool TrySetException(IEnumerable<Exception> exceptions)
        {
            if (exceptions == null)
                throw new ArgumentNullException(nameof(exceptions));

            var agg = new AggregateException(exceptions);
            return _task.TrySetException(agg);
        }

        public bool TrySetResult(TResult result)
        {
            return _task.TrySetResult(result);
        }

        public bool TrySetCanceled()
        {
            return TrySetCanceled(default(CancellationToken));
        }

        public bool TrySetCanceled(CancellationToken cancellationToken)
        {
            return _task.TrySetCanceled(cancellationToken);
        }
    }
}

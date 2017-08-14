using System.Collections.Generic;

namespace System.Threading.Tasks
{
    public class TaskCompletionSource<TResult>
    {
        private const string AlreadyCompletedMessage
            = "An attempt was made to transition a task to a final state when it had already completed";

        private readonly Task<TResult> _task;

        public Task<TResult> Task
            => _task;

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

            throw new InvalidOperationException(AlreadyCompletedMessage);
        }

        public void SetException(IEnumerable<Exception> exceptions)
        {
            if (exceptions == null)
                throw new ArgumentNullException(nameof(exceptions));

            AggregateException agg = new AggregateException(exceptions);
            if (_task.TrySetException(agg))
                return;

            throw new InvalidOperationException(AlreadyCompletedMessage);
        }

        public void SetResult(TResult result)
        {
            if (_task.TrySetResult(result))
                return;

            throw new InvalidOperationException(AlreadyCompletedMessage);
        }

        public void SetCanceled()
        {
            if (_task.TrySetCanceled(default(CancellationToken)))
                return;

            throw new InvalidOperationException(AlreadyCompletedMessage);
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

            AggregateException agg = new AggregateException(exceptions);
            return _task.TrySetException(agg);
        }

        public bool TrySetResult(TResult result)
            => _task.TrySetResult(result);

        public bool TrySetCanceled()
            => TrySetCanceled(default(CancellationToken));

        public bool TrySetCanceled(CancellationToken cancellationToken)
            => _task.TrySetCanceled(cancellationToken);
    }
}

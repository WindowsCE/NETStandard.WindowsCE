using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct TaskAwaiter : INotifyCompletion
    {
        readonly Task _task;

        public bool IsCompleted
            => _task.IsCompleted;

        internal TaskAwaiter(Task task)
        {
            _task = task;
        }

        public void OnCompleted(Action continuation)
        {
            OnCompletedInternal(_task, continuation, true);
        }

        public void GetResult()
        {
            ValidateEnd(_task);
        }

        internal static void ValidateEnd(Task task)
        {
            if (!task.IsCompleted)
            {
                bool taskCompleted = task.Wait(Timeout.Infinite, default(CancellationToken));
                Diagnostics.Debug.Assert(taskCompleted, "With an infinite timeout, the task should have always completed.");
            }

            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    return;
                case TaskStatus.Canceled:
                    if (task.CancellationToken.IsCancellationRequested)
                        throw new TaskCanceledException(task);
                    else
                        throw new OperationCanceledException(task.CancellationToken);
                case TaskStatus.Faulted:
                    ExceptionDispatchInfo.Capture(task.Exception.InnerExceptions[0]).Throw();
                    break;
                default:
                    throw new InvalidOperationException("The task has not yet completed.");
            }
        }

        internal static void OnCompletedInternal(Task task, Action continuation, bool continueOnCapturedContext)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            var syncContext = continueOnCapturedContext ? SynchronizationContext.Current : null;
            if (syncContext != null && syncContext.GetType() != typeof(SynchronizationContext))
            {
                task.ContinueWith((t, state) =>
                {
                    syncContext.Post(s => ((Action)s)(), state);
                }, continuation);
            }
            // TODO: TaskScheduler?
            else
            {
                task.ContinueWith((t, state) => ((Action)state)(), continuation);
            }
        }
    }
}

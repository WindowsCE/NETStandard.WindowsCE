using System.Collections.Generic;

namespace System.Threading.Tasks
{
    internal sealed class WhenAllPromise<T> : Task<T[]>
    {
        private readonly Task<T>[] tasks;
        private int count;

        public WhenAllPromise(Task<T>[] tasks)
        {
            _stateFlags = TASK_STATE_WAITINGFORACTIVATION;
            this.tasks = tasks;
            count = tasks.Length;

            for (int i = 0; i < tasks.Length; i++)
            {
                if (!tasks[i].EnqueueContinuation(Invoke))
                    Invoke(null);
            }
        }

        private void Invoke(object ignored)
        {
            if (Interlocked.Decrement(ref count) != 0)
                return;

            T[] result = new T[tasks.Length];
            List<Exception> exceptionList = null;
            Task canceledTask = null;
            for (int i = 0; i < tasks.Length; ++i)
            {
                Task<T> task = tasks[i];
                if (task.IsFaulted)
                {
                    if (exceptionList == null)
                        exceptionList = new List<System.Exception>();
                    exceptionList.AddRange(task.Exception.InnerExceptions);
                }
                else if (task.IsCanceled)
                {
                    if (canceledTask == null)
                        canceledTask = task;
                }
                else
                {
                    result[i] = task.Result;
                }

                tasks[i] = null;
            }

            if (exceptionList != null)
                TrySetException(new AggregateException(exceptionList));
            else if (canceledTask != null)
                TrySetCanceled(canceledTask.CancellationToken);
            else
                TrySetResult(result);
        }
    }
}
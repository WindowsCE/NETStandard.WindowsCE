namespace System.Threading.Tasks
{
    internal sealed class WhenAnyPromise<T> : Task<Task<T>>
    {
        private int counter;

        public WhenAnyPromise()
        {
            _stateFlags = TASK_STATE_WAITINGFORACTIVATION;
            counter = -1;
        }

        public bool Start(Task<T>[] tasks)
        {
            if (Interlocked.CompareExchange(ref counter, 0, -1) != -1)
                return false;

            for (int i = 0; i < tasks.Length; i++)
            {
                var task = tasks[i];
                if (task == null)
                    throw new ArgumentException(SR.Task_MultiTaskContinuation_NullTask);
                if (!task.EnqueueContinuation(s => Invoke(task)))
                    Invoke(task);
                if (IsCompleted)
                    break;
            }

            return true;
        }

        private void Invoke(Task<T> completedTask)
        {
            if (Interlocked.Increment(ref counter) != 1)
                return;

            TrySetResult(completedTask);
        }
    }
}
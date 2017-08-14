using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct TaskAwaiter<TResult> : INotifyCompletion
    {
        readonly Task<TResult> _task;

        public bool IsCompleted
            => _task.IsCompleted;

        internal TaskAwaiter(Task<TResult> task)
        {
            _task = task;
        }

        public void OnCompleted(Action continuation)
        {
            TaskAwaiter.OnCompletedInternal(_task, continuation, true);
        }

        public TResult GetResult()
        {
            TaskAwaiter.ValidateEnd(_task);
            return _task.Result;
        }
    }
}

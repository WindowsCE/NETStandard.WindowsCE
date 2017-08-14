using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct ConfiguredTaskAwaitable<TResult>
    {
        readonly ConfiguredTaskAwaiter _awaiter;

        internal ConfiguredTaskAwaitable(Task<TResult> task, bool continueOnCapturedContext)
        {
            _awaiter = new ConfiguredTaskAwaiter(task, continueOnCapturedContext);
        }

        public ConfiguredTaskAwaiter GetAwaiter()
            => _awaiter;

        public struct ConfiguredTaskAwaiter : INotifyCompletion
        {
            private readonly Task<TResult> _task;
            private readonly bool _continueOnCapturedContext;

            public bool IsCompleted
                => _task.IsCompleted;

            internal ConfiguredTaskAwaiter(Task<TResult> task, bool continueOnCapturedContext)
            {
                _task = task;
                _continueOnCapturedContext = continueOnCapturedContext;
            }

            public void OnCompleted(Action continuation)
            {
                TaskAwaiter.OnCompletedInternal(_task, continuation, _continueOnCapturedContext);
            }

            public TResult GetResult()
            {
                TaskAwaiter.ValidateEnd(_task);
                return _task.Result;
            }
        }
    }
}

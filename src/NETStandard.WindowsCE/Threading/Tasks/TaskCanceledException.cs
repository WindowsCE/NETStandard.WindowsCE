#if NET35_CF
using InternalOCE = System.OperationCanceledException;
#else
using InternalOCE = Mock.System.OperationCanceledException;
#endif

namespace System.Threading.Tasks
{
    public class TaskCanceledException : InternalOCE
    {
        private readonly Task canceledTask;

        public TaskCanceledException()
            : base("A task was canceled")
        { }

        public TaskCanceledException(string message)
            : base(message)
        { }

        public TaskCanceledException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public TaskCanceledException(Task task)
            : base("A task was canceled", task != null ? task.CancellationToken : default(CancellationToken))
        {
            canceledTask = task;
        }

        public Task Task
            => canceledTask;
    }
}

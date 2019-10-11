namespace System.Threading
{
    public delegate void ContextCallback(object state);

    public class ExecutionContext
    {
        private static readonly ExecutionContext Default = new ExecutionContext();

        public static ExecutionContext Capture() => Default;

        public static void Run(ExecutionContext executionContext, ContextCallback callback, object state)
        {
            callback(state);
        }
    }
}

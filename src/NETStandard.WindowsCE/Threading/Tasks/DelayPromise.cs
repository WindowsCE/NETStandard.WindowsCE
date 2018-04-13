namespace System.Threading.Tasks
{
    internal sealed class DelayPromise : Task
    {
        private CancellationTokenRegistration registration;
        private Timer timer;

        public DelayPromise(CancellationToken cancellationToken)
        {
            _stateFlags = TASK_STATE_WAITINGFORACTIVATION;
            m_cancellationToken = cancellationToken;
        }

        public void Complete()
        {
            bool flag = m_cancellationToken.IsCancellationRequested
                ? TrySetCanceled(m_cancellationToken)
                : TrySetCompleted();

            if (!flag)
                return;

            timer?.Dispose();
            registration.Dispose();
        }

        public void Start(int millisecondsDelay)
        {
            if (m_cancellationToken.CanBeCanceled)
            {
                registration = m_cancellationToken.Register(state => ((DelayPromise)state).Complete(), this, false);
            }

            if (millisecondsDelay != Timeout.Infinite)
            {
                timer = new Timer(state => ((DelayPromise)state).Complete(), this, millisecondsDelay, Timeout.Infinite);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            timer?.Dispose();
            registration.Dispose();
        }
    }
}
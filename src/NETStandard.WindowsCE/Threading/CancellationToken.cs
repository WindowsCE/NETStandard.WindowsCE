#if NET35_CF
using InternalOperationCanceledException = System.OperationCanceledException;
#else
using InternalOperationCanceledException = Mock.System.OperationCanceledException;
#endif

namespace System.Threading
{
    [Diagnostics.DebuggerDisplay("IsCancellationRequested = {IsCancellationRequested}")]
    public struct CancellationToken
    {
        private readonly CancellationTokenSource source;

        public CancellationToken(bool canceled)
            : this(canceled ? CancellationTokenSource.CanceledSource : null)
        { }

        internal CancellationToken(CancellationTokenSource source)
        {
            this.source = source;
        }

        public static CancellationToken None
            => new CancellationToken();

        public bool CanBeCanceled
            => source != null;

        public bool IsCancellationRequested
            => source?.IsCancellationRequested ?? false;

        public WaitHandle WaitHandle
            => source.WaitHandle;

        private CancellationTokenSource Source
            => source ?? CancellationTokenSource.NoneSource;

        public static bool operator !=(CancellationToken left, CancellationToken right)
            => !left.Equals(right);

        public static bool operator ==(CancellationToken left, CancellationToken right)
            => left.Equals(right);

        public bool Equals(CancellationToken other)
            => Source == other.Source;

        public override bool Equals(object obj)
            => (obj is CancellationToken) && Equals((CancellationToken)obj);

        public override int GetHashCode()
            => source?.GetHashCode() ?? 0;

        public CancellationTokenRegistration Register(Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            return Register(callback, false);
        }

        public CancellationTokenRegistration Register(Action callback, bool useSynchronizationContext)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (source == null)
                return new CancellationTokenRegistration();

            return source.Register(callback, useSynchronizationContext);
        }

        public CancellationTokenRegistration Register(Action<object> callback, object state)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            return Register(() => callback(state), false);
        }

        public CancellationTokenRegistration Register(Action<object> callback, object state, bool useSynchronizationContext)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            return Register(() => callback(state), useSynchronizationContext);
        }

        public void ThrowIfCancellationRequested()
        {
            if (!ReferenceEquals(source, null) && source.IsCancellationRequested)
                throw new InternalOperationCanceledException(this);
        }
    }
}

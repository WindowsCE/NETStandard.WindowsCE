using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace System.Threading
{
    public class CancellationTokenSource : IDisposable
    {
        internal static readonly CancellationTokenSource CanceledSource = new CancellationTokenSource(true); // Leaked
        internal static readonly CancellationTokenSource NoneSource = new CancellationTokenSource(); // Leaked
        private volatile ManualResetEvent _handle;
        private ConcurrentDictionary<CancellationTokenRegistration, Action> _callbacks;
        private volatile int _cancelRequested;
        private int _currentId = int.MaxValue;
        private volatile int _disposeRequested;
        private CancellationTokenRegistration[] _linkedTokens;
        private Timer _timeout;

        static void TimerCallback(object state)
        {
            var cancellationTokenSource = state as CancellationTokenSource;
            var callbacks = cancellationTokenSource._callbacks;
            if (callbacks != null)
            {
                cancellationTokenSource.CancelExtracted(false, callbacks, true);
            }
        }

        public CancellationTokenSource()
        {
            _callbacks = new ConcurrentDictionary<CancellationTokenRegistration, Action>();
        }

        public CancellationTokenSource(int millisecondsDelay)
            : this()
        {
            if (millisecondsDelay < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
            }
            if (millisecondsDelay != Timeout.Infinite)
            {
                _timeout = new Timer(TimerCallback, this, millisecondsDelay, -1);
            }
        }

        public CancellationTokenSource(TimeSpan delay)
            : this(CheckTimeout(delay))
        {
            //Empty
        }

        private CancellationTokenSource(bool set)
            : this()
        {
            if (set)
                _cancelRequested = 1;
        }

        public bool IsCancellationRequested
        {
            get { return _cancelRequested == 1; }
        }

        public CancellationToken Token
        {
            get
            {
                CheckDisposed();
                return new CancellationToken(this);
            }
        }

        internal WaitHandle WaitHandle
        {
            get
            {
                CheckDisposed();
                if (_handle != null)
                    return _handle;

                var handle = new ManualResetEvent(false);
                if (Interlocked.CompareExchange(ref _handle, handle, null) != null)
                    handle.Close();
                if (IsCancellationRequested)
                    _handle.Set();

                return _handle;
            }
        }

        public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2)
        {
            return CreateLinkedTokenSource(new[] { token1, token2 });
        }

        public static CancellationTokenSource CreateLinkedTokenSource(params CancellationToken[] tokens)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }
            if (tokens.Length == 0)
            {
                throw new ArgumentException("Empty tokens array");
            }
            var src = new CancellationTokenSource();
            Action action = src.SafeLinkedCancel;
            var registrations = new List<CancellationTokenRegistration>(tokens.Length);
            foreach (var token in tokens)
            {
                if (token.CanBeCanceled)
                {
                    registrations.Add(token.Register(action));
                }
            }
            src._linkedTokens = registrations.ToArray();
            return src;
        }

        public void Cancel()
        {
            Cancel(false);
        }

        public void Cancel(bool throwOnFirstException)
        {
            // If throwOnFirstException is true we throw exception as soon as they appear otherwise we aggregate them
            var callbacks = CheckDisposedGetCallbacks();
            CancelExtracted(throwOnFirstException, callbacks, false);
        }

        public void CancelAfter(TimeSpan delay)
        {
            CancelAfter(CheckTimeout(delay));
        }

        public void CancelAfter(int millisecondsDelay)
        {
            if (millisecondsDelay < -1)
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));

            CheckDisposed();

            if (_cancelRequested != 0)
                return;

            if (_timeout == null)
            {
                // Have to be careful not to create secondary background timer
                var newTimer = new Timer(TimerCallback, this, Timeout.Infinite, -1);
                var oldTimer = Interlocked.CompareExchange(ref _timeout, newTimer, null);
                if (!ReferenceEquals(oldTimer, null))
                    newTimer.Dispose();
            }

            try
            {
                _timeout.Change(millisecondsDelay, -1);
            }
            catch (ObjectDisposedException)
            {
                // Just eat the exception.
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        internal void CheckDisposed()
        {
            if (_disposeRequested == 1)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        internal ConcurrentDictionary<CancellationTokenRegistration, Action> CheckDisposedGetCallbacks()
        {
            var result = _callbacks;
            if (result == null || _disposeRequested == 1)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            return result;
        }

        internal CancellationTokenRegistration Register(Action callback, bool useSynchronizationContext)
        {
            // NOTICE this method has no null check
            var callbacks = CheckDisposedGetCallbacks();
            // If the source is already canceled run the callback in-line.
            // if not, we try to add it to the queue and if it is currently being processed.
            // we try to execute it back ourselves to be sure the callback is ran.
            if (_cancelRequested == 1)
            {
                callback();
                return default(CancellationTokenRegistration);
            }
            else
            {
                var tokenReg = new CancellationTokenRegistration(Interlocked.Decrement(ref _currentId), this);
                // Capture execution contexts if the callback may not run in-line.
                if (useSynchronizationContext)
                {
                    var capturedSyncContext = SynchronizationContext.Current;
                    var originalCallback = callback;
                    callback = () => capturedSyncContext.Send(_ => originalCallback(), null);
                }
                callbacks.TryAdd(tokenReg, callback);
                // Check if the source was just canceled and if so, it may be that it executed the callbacks except the one just added...
                // So try to in-line the callback
                if (_cancelRequested == 1 && callbacks.TryRemove(tokenReg, out callback))
                {
                    callback();
                }
                return tokenReg;
            }
        }

        internal bool RemoveCallback(CancellationTokenRegistration reg)
        {
            // Ignore call if the source has been disposed
            if (_disposeRequested == 0)
            {
                var callbacks = _callbacks;
                if (callbacks != null)
                {
                    Action dummy;
                    return callbacks.TryRemove(reg, out dummy);
                }
            }
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && Interlocked.CompareExchange(ref _disposeRequested, 1, 0) == 0)
            {
                if (_cancelRequested == 0)
                {
                    UnregisterLinkedTokens();
                    _callbacks = null;
                }
                var timer = Interlocked.Exchange(ref _timeout, null);
                timer?.Dispose();
                _handle?.Close();
            }
        }

        private static int CheckTimeout(TimeSpan delay)
        {
            try
            {
                return checked((int)delay.TotalMilliseconds);
            }
            catch (OverflowException)
            {
                throw new ArgumentOutOfRangeException(nameof(delay));
            }
        }

        private static void RunCallback(bool throwOnFirstException, Action callback, ref List<Exception> exceptions)
        {
            // NOTICE this method has no null check
            if (throwOnFirstException)
            {
                callback();
            }
            else
            {
                try
                {
                    callback();
                }
                catch (Exception exception)
                {
                    if (ReferenceEquals(exceptions, null))
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(exception);
                }
            }
        }

        private void CancelExtracted(bool throwOnFirstException, ConcurrentDictionary<CancellationTokenRegistration, Action> callbacks, bool ignoreDisposedException)
        {
            if (Interlocked.CompareExchange(ref _cancelRequested, 1, 0) == 0)
            {
                try
                {
                    // The CancellationTokenSource may have been disposed just before this call
                    _handle?.Set();
                }
                catch (ObjectDisposedException)
                {
                    if (!ignoreDisposedException)
                    {
                        throw;
                    }
                }
                UnregisterLinkedTokens();
                List<Exception> exceptions = null;
                var id = _currentId;
                do
                {
                    var regComparee = new CancellationTokenRegistration(id, this);
                    Action callback;
                    //var checkId = id;
                    if (callbacks.TryRemove(regComparee, out callback) && callback != null)
                    {
                        RunCallback(throwOnFirstException, callback, ref exceptions);
                    }
                } while (id++ != int.MaxValue);

                if (exceptions != null)
                {
                    throw new AggregateException(exceptions);
                }
            }
        }

        private void SafeLinkedCancel()
        {
            var callbacks = _callbacks;
            if (callbacks == null || _disposeRequested == 1)
            {
                return;
            }
            CancelExtracted(false, callbacks, true);
        }

        private void UnregisterLinkedTokens()
        {
            var registrations = Interlocked.Exchange(ref _linkedTokens, null);
            if (!ReferenceEquals(registrations, null))
            {
                foreach (var linked in registrations)
                {
                    linked.Dispose();
                }
            }
        }
    }
}

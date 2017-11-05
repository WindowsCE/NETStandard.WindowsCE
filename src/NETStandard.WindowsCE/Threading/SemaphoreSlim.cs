using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace System.Threading
{
    [DebuggerDisplay("Current Count = {CurrentCount}")]
    public class SemaphoreSlim : IDisposable
    {
        private ConcurrentQueue<TaskCompletionSource<bool>> _asyncWaiters;
        private ManualResetEventSlim _event;
        private readonly int _maxCount;
        private volatile int _count;
        private bool _disposed;

        public SemaphoreSlim(int initialCount)
            : this(initialCount, int.MaxValue)
        {
            //Empty
        }

        public SemaphoreSlim(int initialCount, int maxCount)
        {
            if (initialCount < 0 || initialCount > maxCount)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCount), SR.SemaphoreSlim_ctor_InitialCountWrong);
            }
            if (maxCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCount), SR.SemaphoreSlim_ctor_MaxCountWrong);
            }
            _maxCount = maxCount;
            _asyncWaiters = new ConcurrentQueue<TaskCompletionSource<bool>>();
            _count = maxCount - initialCount;
            _event = new ManualResetEventSlim(_count < maxCount);
        }

        public WaitHandle AvailableWaitHandle
        {
            get
            {
                CheckDisposed();
                return _event.WaitHandle;
            }
        }

        public int CurrentCount
        {
            get { return _maxCount - _count; }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public int Release()
        {
            return Release(1);
        }

        public int Release(int releaseCount)
        {
            CheckDisposed();
            if (releaseCount < 1)
            {
                throw new ArgumentOutOfRangeException("releaseCount", SR.SemaphoreSlim_Release_CountWrong);
            }
            int oldValue;
            if (SpinWaitRelativeExchangeUnlessNegativeForCount(-releaseCount, out oldValue))
            {
                Awake();
                return oldValue;
            }
            throw new SemaphoreFullException();
        }

        private void Awake()
        {
            // Call this to notify that there is room in the semaphore
            // Allow sync waiters to proceed
            _event.Set();
            while (_count < _maxCount)
            {
                TaskCompletionSource<bool> waiter;
                if (_asyncWaiters.TryDequeue(out waiter))
                {
                    if (waiter.Task.IsCompleted)
                    {
                        // Skip - either canceled or timed out
                        continue;
                    }
                    if (TryEnter())
                    {
                        waiter.SetResult(true);
                    }
                    else
                    {
                        _asyncWaiters.Enqueue(waiter);
                    }
                }
            }
        }

        public void Wait()
        {
            Wait(Timeout.Infinite, default(CancellationToken));
        }

        public bool Wait(TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), SR.SemaphoreSlim_Wait_TimeoutWrong);
            }

            return Wait((int)totalMilliseconds, default(CancellationToken));
        }

        public bool Wait(int millisecondsTimeout)
        {
            return Wait(millisecondsTimeout, default(CancellationToken));
        }

        public void Wait(CancellationToken cancellationToken)
        {
            Wait(Timeout.Infinite, cancellationToken);
        }

        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), SR.SemaphoreSlim_Wait_TimeoutWrong);
            }

            return Wait((int)totalMilliseconds, cancellationToken);
        }

        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), SR.SemaphoreSlim_Wait_TimeoutWrong);

            CheckDisposed();

            cancellationToken.ThrowIfCancellationRequested();
            // GC.KeepAlive(cancellationToken.WaitHandle);
            if (millisecondsTimeout == -1)
            {
                SpinWaitRelativeSetForCount(1);
                return true;
            }

            Stopwatch timeTrack = new Stopwatch();
            timeTrack.Start();
            var remaining = millisecondsTimeout;
            while (_event.Wait(remaining, cancellationToken))
            {
                // The thread is not allowed here unless there is room in the semaphore
                if (TryEnter())
                {
                    return true;
                }
                remaining = (int)(millisecondsTimeout - timeTrack.ElapsedMilliseconds);
                if (remaining <= 0)
                {
                    break;
                }
            }
            // Time out
            return false;
        }

        private bool TryEnter()
        {
            // Should only be called when there is room in the semaphore
            // No check is done to verify that
            var result = _count + 1;
            if (Interlocked.CompareExchange(ref _count, result, _count) == _count)
            {
                // It may be the case that there is no longer room in the semaphore because we just took one slot
                if (_count == _maxCount)
                {
                    // Cause sync waitets to halt
                    _event.Reset();
                    // It is possible that another thread has just released more slots and called _event.Set() and we have just undone it...
                    // Check if that is the case
                    if (_count < _maxCount)
                    {
                        // Allow sync waiters to proceed
                        _event.Set();
                    }
                }
                return true;
            }
            return false;
        }

        public Task WaitAsync()
        {
            return WaitAsync(Timeout.Infinite, default(CancellationToken));
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            return WaitAsync(Timeout.Infinite, cancellationToken);
        }

        public Task<bool> WaitAsync(int millisecondsTimeout)
        {
            return WaitAsync(millisecondsTimeout, default(CancellationToken));
        }

        public Task<bool> WaitAsync(TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), SR.SemaphoreSlim_Wait_TimeoutWrong);
            }

            return WaitAsync((int)totalMilliseconds, default(CancellationToken));
        }

        public Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (millisecondsTimeout < Timeout.Infinite)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), SR.SemaphoreSlim_Wait_TimeoutWrong);
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<bool>(cancellationToken);

            CheckDisposed();
            var source = new TaskCompletionSource<bool>();

            // Theraot.Threading.Timeout.Launch(() => source.SetResult(false), millisecondsTimeout, cancellationToken);
            if (millisecondsTimeout != Timeout.Infinite)
                new Timer(state => ((TaskCompletionSource<bool>)state).TrySetResult(false), source, millisecondsTimeout, Timeout.Infinite);

            if (cancellationToken.CanBeCanceled)
                cancellationToken.Register(state => source.TrySetCanceled((CancellationToken)state), cancellationToken);

            _asyncWaiters.Enqueue(source);
            return source.Task;
        }

        public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), SR.SemaphoreSlim_Wait_TimeoutWrong);
            }

            return WaitAsync((int)totalMilliseconds, cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            // This is a protected method, the parameter should be kept
            _disposed = true;
            _event.Dispose();
            _asyncWaiters = null;
            _event = null;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null, SR.SemaphoreSlim_Disposed);
            }
        }

        private bool SpinWaitRelativeSetForCount(int value)
        {
            var spinWait = new SpinWait();
            retry:
            var tmpA = _count;
            var tmpB = Interlocked.CompareExchange(ref _count, tmpA + value, tmpA);
            if (tmpB == tmpA)
            {
                return true;
            }
            spinWait.SpinOnce();
            goto retry;
        }

        private bool SpinWaitRelativeExchangeUnlessNegativeForCount(int value, out int lastValue)
        {
            var spinWait = new SpinWait();
            retry:
            lastValue = _count;
            if ((lastValue < 0) || (lastValue < -value))
            {
                return false;
            }
            var result = lastValue + value;
            var tmp = Interlocked.CompareExchange(ref _count, result, lastValue);
            if (tmp == lastValue)
            {
                return true;
            }
            spinWait.SpinOnce();
            goto retry;
        }
    }
}

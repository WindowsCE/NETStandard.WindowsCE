using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

#if NET35_CF
namespace System.Threading
#else
namespace Mock.System.Threading
#endif
{
    public static class Monitor2
    {
        private static readonly Dictionary<object, List<AutoResetEvent>> _waiters
            = new Dictionary<object, List<AutoResetEvent>>();

        public static void Enter(object obj)
            => Monitor.Enter(obj);

        public static void Enter(object obj, ref bool lockTaken)
        {
            if (lockTaken)
                ThrowLockTakenException();

            Monitor.Enter(obj);
            lockTaken = true;
        }

        public static void Exit(object obj)
            => Monitor.Exit(obj);

        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public static bool IsEntered(object obj)
        {
            throw new PlatformNotSupportedException();
        }

        public static void Pulse(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            lock (_waiters)
            {
                if (!_waiters.ContainsKey(obj))
                    return;

                var queue = _waiters[obj];
                if (queue.Count > 0)
                    queue[0].Set();
            }

            Thread.Sleep(0);
        }

        public static void PulseAll(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            int counter = 0;
            lock (_waiters)
            {
                if (!_waiters.ContainsKey(obj))
                    return;

                var queue = _waiters[obj];
                counter = queue.Count;

                for (int i = 0; i < counter; i++)
                    queue[i].Set();
            }

            for (int i = 0; i < counter; i++)
                Thread.Sleep(0);
        }

        public static bool TryEnter(object obj)
            => Monitor.TryEnter(obj);

        public static void TryEnter(object obj, ref bool lockTaken)
        {
            if (lockTaken)
                ThrowLockTakenException();

            lockTaken = Monitor.TryEnter(obj);
        }

        public static bool TryEnter(object obj, int millisecondsTimeout)
        {
            bool lockTaken = false;
            TryEnter(obj, millisecondsTimeout, ref lockTaken);
            return lockTaken;
        }

        public static void TryEnter(object obj, int millisecondsTimeout, ref bool lockTaken)
        {
            if (lockTaken)
                ThrowLockTakenException();

            if (millisecondsTimeout == Timeout.Infinite)
            {
                Enter(obj, ref lockTaken);
                return;
            }

            var timeTrack = new Stopwatch();
            int growThreshold = 250;
            int timeoutMs = 1;
            timeTrack.Start();
            while (timeTrack.ElapsedMilliseconds < millisecondsTimeout)
            {
                if (Monitor.TryEnter(obj))
                {
                    lockTaken = true;
                    return;
                }

                if (timeTrack.ElapsedMilliseconds >= growThreshold)
                {
                    growThreshold += 250;
                    timeoutMs *= 2;
                }

                Thread.Sleep(timeoutMs);
            }
        }

        public static bool TryEnter(object obj, TimeSpan timeout)
            => TryEnter(obj, MillisecondsTimeoutFromTimeSpan(timeout));

        public static void TryEnter(object obj, TimeSpan timeout, ref bool lockTaken)
            => TryEnter(obj, MillisecondsTimeoutFromTimeSpan(timeout), ref lockTaken);

        public static bool Wait(object obj)
            => Wait(obj, Timeout.Infinite);

        public static bool Wait(object obj, int millisecondsTimeout)
        {
            bool objUnlocked = false;
            bool waitersLocked = false;
            AutoResetEvent waitHandle = null;
            bool enqueued = false;
            bool gotSignal = false;
            try
            {
                Monitor.Exit(obj);
                objUnlocked = true;

                // Enqueue a new AutoResetEvent
                Monitor.Enter(_waiters);
                waitersLocked = true;

                List<AutoResetEvent> queue;
                if (_waiters.ContainsKey(obj))
                    queue = _waiters[obj];
                else
                {
                    queue = new List<AutoResetEvent>();
                    _waiters.Add(obj, queue);
                }

                waitHandle = new AutoResetEvent(false);
                queue.Add(waitHandle);
                enqueued = true;

                Monitor.Exit(_waiters);
                waitersLocked = false;
                // ----------------------------

                gotSignal = waitHandle.WaitOne(millisecondsTimeout, false);
            }
            finally
            {
                if (enqueued)
                {
                    if (!waitersLocked)
                    {
                        Monitor.Enter(_waiters);
                        waitersLocked = true;
                    }

                    List<AutoResetEvent> queue;
                    if (_waiters.TryGetValue(obj, out queue))
                    {
                        queue.Remove(waitHandle);

                        // If last handle for object, remove it from list
                        if (queue.Count == 0)
                            _waiters.Remove(obj);
                    }
                }

                if (waitersLocked)
                    Monitor.Exit(_waiters);

                if (objUnlocked)
                    Monitor.Enter(obj);
            }

            return gotSignal;
        }

        public static bool Wait(object obj, TimeSpan timeout)
            => Wait(obj, MillisecondsTimeoutFromTimeSpan(timeout));


        private static void ThrowLockTakenException()
        {
            throw new ArgumentException("The lock is taken already", "lockTaken");
        }

        private static int MillisecondsTimeoutFromTimeSpan(TimeSpan timeout)
        {
            long tm = (long)timeout.TotalMilliseconds;
            if (tm < -1 || tm > (long)int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(timeout));

            return (int)tm;
        }
    }
}

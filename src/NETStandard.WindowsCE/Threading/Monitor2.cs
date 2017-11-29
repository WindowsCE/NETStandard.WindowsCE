using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

#if !NET35_CF
using Mock.System.Collections.Concurrent;
#endif

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

        private static ConcurrentDictionary<object, Condition> s_conditionTable = new ConcurrentDictionary<object, Condition>();
        private static Func<object, Condition> s_createCondition = (o) => new Condition(o);

        private static Condition GetCondition(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            // TODO: Dictionary is leaking obj
            return s_conditionTable.GetOrAdd(obj, s_createCondition);
        }

        public static void Enter(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            Monitor.Enter(obj);
        }

        public static void Enter(object obj, ref bool lockTaken)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (lockTaken)
                throw new ArgumentException(SR.Argument_MustBeFalse, nameof(lockTaken));

            Monitor.Enter(obj);
            lockTaken = true;
        }

        public static void Exit(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            Monitor.Exit(obj);
        }

        [Obsolete(Consts.PlatformNotSupportedDescription, true)]
        public static bool IsEntered(object obj)
        {
            bool lockTaken = false;
            try
            {
                lockTaken = Monitor.TryEnter(obj);
                return lockTaken;
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(obj);
            }
        }

        public static void Pulse(object obj)
        {
            GetCondition(obj).SignalOne();
        }

        public static void PulseAll(object obj)
        {
            GetCondition(obj).SignalAll();
        }

        public static bool TryEnter(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return Monitor.TryEnter(obj);
        }

        public static void TryEnter(object obj, ref bool lockTaken)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (lockTaken)
                throw new ArgumentException(SR.Argument_MustBeFalse, nameof(lockTaken));

            lockTaken = Monitor.TryEnter(obj);
        }

        public static bool TryEnter(object obj, int millisecondsTimeout)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            bool lockTaken = false;
            TryEnter(obj, millisecondsTimeout, ref lockTaken);
            return lockTaken;
        }

        public static void TryEnter(object obj, int millisecondsTimeout, ref bool lockTaken)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (lockTaken)
                throw new ArgumentException(SR.Argument_MustBeFalse, nameof(lockTaken));

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
            => TryEnter(obj, WaitHandle2.ToTimeoutMilliseconds(timeout));

        public static void TryEnter(object obj, TimeSpan timeout, ref bool lockTaken)
            => TryEnter(obj, WaitHandle2.ToTimeoutMilliseconds(timeout), ref lockTaken);

        public static bool Wait(object obj)
            => Wait(obj, Timeout.Infinite);

        public static bool Wait(object obj, int millisecondsTimeout)
        {
            var condition = GetCondition(obj);
            var result = condition.Wait(millisecondsTimeout);

            if (condition.Count() == 0)
                s_conditionTable.TryRemove(obj, out condition);

            return result;
        }

        public static bool Wait(object obj, TimeSpan timeout)
            => Wait(obj, WaitHandle2.ToTimeoutMilliseconds(timeout));

        internal static void Reacquire(object obj, uint recursionCount)
        {
            while (recursionCount > 0)
            {
                Monitor.Enter(obj);
                recursionCount--;
            }
        }

        internal static uint ReleaseAll(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            uint recursionCount = 0;
            while (true)
            {
                try
                {
                    Monitor.Exit(obj);
                    recursionCount++;
                }
#if NET35_CF
                catch (ArgumentException)
#else
                catch (SynchronizationLockException)
#endif
                {
                    return recursionCount;
                }
            }
        }
    }
}

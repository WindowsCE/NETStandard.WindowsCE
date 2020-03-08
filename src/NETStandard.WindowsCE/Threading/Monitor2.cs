using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace System.Threading
{
    public static class Monitor2
    {
        private static ConcurrentDictionary<WeakReference, Condition> s_conditionTable = new ConcurrentDictionary<WeakReference, Condition>(new WeakReferenceComparer());
        private static Func2<WeakReference, Condition> s_createCondition = (o) => new Condition(o);

        private static Condition GetCondition(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var result = s_conditionTable.GetOrAdd(new WeakReference(obj), s_createCondition);
            RemoveAllDeadKeys();
            return result;
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

            RemoveAllDeadKeys();

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
                catch (ArgumentException)
                {
                    return recursionCount;
                }
            }
        }

        private static void RemoveAllDeadKeys()
        {
            Condition condition;
            var matcher = WeakReferenceComparer.DeadMatcher;
            while (s_conditionTable.TryRemove(matcher, out condition))
            {
                // Does nothing
            }
        }

        private sealed class WeakReferenceComparer : IEqualityComparer<WeakReference>
        {
            private static readonly WeakReference DeadMatcherReference = new WeakReference(new object());

            public static WeakReference DeadMatcher
                => DeadMatcherReference;

            public bool Equals(WeakReference x, WeakReference y)
            {
                if (x == DeadMatcherReference)
                {
                    return !y.IsAlive;
                }

                if (y == DeadMatcherReference)
                {
                    return !x.IsAlive;
                }

                var xValue = x.Target;
                if (x.IsAlive)
                {
                    var yValue = y.Target;
                    return y.IsAlive && ReferenceEquals(xValue, yValue);
                }

                return !y.IsAlive;
            }

            public int GetHashCode(WeakReference obj)
            {
                object target = obj.Target;
                if (!obj.IsAlive)
                {
                    return 0;
                }

                return target.GetHashCode();
            }
        }
    }
}

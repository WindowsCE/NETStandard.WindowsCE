namespace System.Threading
{
    public static class Interlocked2
    {
        // Compact Framework runs on processors that cannot handle
        // more than 32 bits, then an ungly global lock is created
        // to emulate an atomic single-operation.
        private static readonly object GlobalLock = new object();

        public static int Add(ref int location1, int value)
        {
            switch (value)
            {
                case 1:
                    return Interlocked.Increment(ref location1);
                case -1:
                    return Interlocked.Decrement(ref location1);
                default:
                    lock (GlobalLock)
                    {
                        var resultValue = location1 + value;
                        location1 = resultValue;
                        return resultValue;
                    }
            }
        }

        public static long Add(ref long location1, long value)
        {
            lock (GlobalLock)
            {
                var resultValue = location1 + value;
                location1 = resultValue;
                return resultValue;
            }
        }

        public static T CompareExchange<T>(ref T location1, T value, T comparand) where T : class
        {
            return Interlocked.CompareExchange(ref location1, value, comparand);
        }

        public static double CompareExchange(ref double location1, double value, double comparand)
        {
            lock (GlobalLock)
            {
                var previousValue = location1;
                if (previousValue == comparand)
                {
                    location1 = value;
                }

                return previousValue;
            }
        }

        public static int CompareExchange(ref int location1, int value, int comparand)
        {
            return Interlocked.CompareExchange(ref location1, value, comparand);
        }

        public static long CompareExchange(ref long location1, long value, long comparand)
        {
            lock (GlobalLock)
            {
                var previousValue = location1;
                if (previousValue == comparand)
                {
                    location1 = value;
                }

                return previousValue;
            }
        }

        public static IntPtr CompareExchange(ref IntPtr location1, IntPtr value, IntPtr comparand)
        {
            lock (GlobalLock)
            {
                var previousValue = location1;
                if (previousValue == comparand)
                {
                    location1 = value;
                }

                return previousValue;
            }
        }

        public static object CompareExchange(ref object location1, object value, object comparand)
        {
            return Interlocked.CompareExchange(ref location1, value, comparand);
        }

        public static unsafe float CompareExchange(ref float location1, float value, float comparand)
        {
            lock (GlobalLock)
            {
                var previousValue = location1;
                if (previousValue == comparand)
                {
                    location1 = value;
                }

                return previousValue;
            }
        }

        public static int Decrement(ref int location)
        {
            return Interlocked.Decrement(ref location);
        }

        public static long Decrement(ref long location)
        {
            lock (GlobalLock)
            {
                var resultValue = location - 1;
                location = resultValue;
                return resultValue;
            }
        }

        public static T Exchange<T>(ref T location1, T value) where T : class
        {
            return Interlocked.Exchange(ref location1, value);
        }

        public static double Exchange(ref double location1, double value)
        {
            lock (GlobalLock)
            {
                var previousValue = location1;
                location1 = value;
                return previousValue;
            }
        }

        public static int Exchange(ref int location1, int value)
        {
            return Interlocked.Exchange(ref location1, value);
        }

        public static long Exchange(ref long location1, long value)
        {
            lock (GlobalLock)
            {
                var previousValue = location1;
                location1 = value;
                return previousValue;
            }
        }

        public static IntPtr Exchange(ref IntPtr location1, IntPtr value)
        {
            lock (GlobalLock)
            {
                var previousValue = location1;
                location1 = value;
                return previousValue;
            }
        }

        public static object Exchange(ref object location1, object value)
        {
            return Interlocked.Exchange(ref location1, value);
        }

        public static float Exchange(ref float location1, float value)
        {
            lock (GlobalLock)
            {
                var previousValue = location1;
                location1 = value;
                return previousValue;
            }
        }

        public static int Increment(ref int location)
        {
            return Interlocked.Increment(ref location);
        }

        public static long Increment(ref long location)
        {
            lock (GlobalLock)
            {
                var resultValue = location + 1;
                location = resultValue;
                return resultValue;
            }
        }

        public static void MemoryBarrier()
        {
            Thread.MemoryBarrier();
        }

        public static long Read(ref long location)
        {
            lock (GlobalLock)
            {
                return location;
            }
        }
    }
}

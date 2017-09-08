using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Threading
{
    /// <summary>
    /// Provides means to wait for multiple handles.
    /// </summary>
    public static class WaitHandle2
    {
        /// <summary>
        /// Indicates that a <see cref="WaitAny(WaitHandle[], int)"/> operation
        /// timed out before any of the wait handles were signaled.
        /// </summary>
        public const int WaitTimeout = 0x102;

        private const uint WaitInfinite = 0xFFFFFFFF;
        private const uint WaitFailed = 0xFFFFFFFF;

        #region WaitAll

        /// <summary>
        /// Waits for all the elements in the specified array to receive a
        /// signal.
        /// </summary>
        /// <param name="waitHandles">
        /// A WaitHandle array containing the objects for which the current
        /// instance will wait.
        /// </param>
        /// <returns>
        /// True when every element in waitHandles has received a signal; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="waitHandles"/> parameter is null, or one or
        /// more of the objects in the <paramref name="waitHandles"/> array
        /// is null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The number of objects in <paramref name="waitHandles"/> is greater
        /// than the system permits.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// waitHandles is an array with no elements.
        /// </exception>
        public static bool WaitAll(WaitHandle[] waitHandles)
            => WaitAll(waitHandles, Timeout.Infinite);

        /// <summary>
        /// Waits for all the elements in the specified array to receive a
        /// signal, using a 32-bit signed integer to specify the time interval.
        /// </summary>
        /// <param name="waitHandles">
        /// A WaitHandle array containing the objects for which the current
        /// instance will wait.
        /// </param>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or <see cref="Timeout.Infinite"/>
        /// to wait indefinitely.
        /// </param>
        /// <returns>
        /// True when every element in waitHandles has received a signal; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="waitHandles"/> parameter is null, or one or
        /// more of the objects in the <paramref name="waitHandles"/> array
        /// is null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The number of objects in <paramref name="waitHandles"/> is greater
        /// than the system permits.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="millisecondsTimeout"/> is a negative number other
        /// than -1, which represents an infinite time-out.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// waitHandles is an array with no elements.
        /// </exception>
        public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout)
        {
            Stopwatch timeTrack = new Stopwatch();
            timeTrack.Start();

            if (waitHandles == null)
                throw new ArgumentNullException("waitHandles");
            if (waitHandles.Length == 0)
                throw new ArgumentException("The waitHandles is empty");
            if (waitHandles.Length >= WaitTimeout)
                throw new NotSupportedException("The waitHandles length is too big");
            if (millisecondsTimeout < Timeout.Infinite)
                throw new ArgumentOutOfRangeException("millisecondsTimeout");

            ValidateHandles(waitHandles, true);

            foreach (var w in waitHandles)
            {
                int remainderTimeMs = millisecondsTimeout - (int)timeTrack.ElapsedMilliseconds;
                if (remainderTimeMs < 0)
                    remainderTimeMs = 0;

                bool isSignaled = w.WaitOne(remainderTimeMs, false);
                if (!isSignaled)
                {
                    timeTrack.Stop();
                    return false;
                }
            }

            timeTrack.Stop();
            return true;
        }

        /// <summary>
        /// Waits for all the elements in the specified array to receive a
        /// signal, using a <see cref="TimeSpan"/> value to specify the time
        /// interval.
        /// </summary>
        /// <param name="waitHandles">
        /// A WaitHandle array containing the objects for which the current
        /// instance will wait.
        /// </param>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> that represents the number of milliseconds
        /// to wait, or a <see cref="TimeSpan"/> that represents -1
        /// milliseconds to wait indefinitely.
        /// </param>
        /// <returns>
        /// True when every element in waitHandles has received a signal; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="waitHandles"/> parameter is null, or one or
        /// more of the objects in the <paramref name="waitHandles"/> array
        /// is null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The number of objects in <paramref name="waitHandles"/> is greater
        /// than the system permits.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="timeout"/> is a negative number other than -1,
        /// which represents an infinite time-out, or
        /// <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// waitHandles is an array with no elements.
        /// </exception>
        public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("timeout");
            }

            return WaitAll(waitHandles, (int)totalMilliseconds);
        }

        #endregion

        #region WaitAny

        /// <summary>
        /// Waits for any of the elements in the specified array to receive a
        /// signal.
        /// </summary>
        /// <param name="waitHandles">
        /// A WaitHandle array containing the objects for which the current
        /// instance will wait.
        /// </param>
        /// <returns>
        /// The array index of the object that satisfied the wait.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="waitHandles"/> parameter is null, or one or
        /// more of the objects in the <paramref name="waitHandles"/> array
        /// is null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The number of objects in <paramref name="waitHandles"/> is greater
        /// than the system permits.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// waitHandles is an array with no elements.
        /// </exception>
        public static int WaitAny(WaitHandle[] waitHandles)
            => WaitAny(waitHandles, Timeout.Infinite);

        /// <summary>
        /// Waits for any of the elements in the specified array to receive a
        /// signal, using a 32-bit signed integer to specify the time interval.
        /// </summary>
        /// <param name="waitHandles">
        /// A WaitHandle array containing the objects for which the current
        /// instance will wait.
        /// </param>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or <see cref="Timeout.Infinite"/>
        /// to wait indefinitely.
        /// </param>
        /// <returns>
        /// The array index of the object that satisfied the wait, or
        /// <see cref="WaitTimeout"/> if no object satisfied the wait and a
        /// time interval equivalent to <paramref name="millisecondsTimeout"/>
        /// has passed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="waitHandles"/> parameter is null, or one or
        /// more of the objects in the <paramref name="waitHandles"/> array
        /// is null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The number of objects in <paramref name="waitHandles"/> is greater
        /// than the system permits.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="millisecondsTimeout"/> is a negative number other
        /// than -1, which represents an infinite time-out.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// waitHandles is an array with no elements.
        /// </exception>
        public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout)
        {
            if (waitHandles == null)
                throw new ArgumentNullException("waitHandles");
            if (waitHandles.Length == 0)
                throw new ArgumentException("The waitHandles is empty");
            if (waitHandles.Length >= WaitTimeout)
                throw new NotSupportedException("The waitHandles length is too big");
            if (millisecondsTimeout < Timeout.Infinite)
                throw new ArgumentOutOfRangeException("millisecondsTimeout");

            ValidateHandles(waitHandles, false);

            uint dwMilliseconds;
            if (millisecondsTimeout == Timeout.Infinite)
                dwMilliseconds = WaitInfinite;
            else
                dwMilliseconds = (uint)millisecondsTimeout;

            IntPtr[] lpHandles = new IntPtr[waitHandles.Length];
            for (int i = 0; i < lpHandles.Length; i++)
            {
#pragma warning disable 0618    // Obsolete
                lpHandles[i] = waitHandles[i].Handle;
#pragma warning restore 0618
            }

            uint result = WaitForMultipleObjects((uint)lpHandles.Length, lpHandles, false, dwMilliseconds);

            if (result == WaitFailed)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new ComponentModel.Win32Exception(lastError);
            }

            return (int)result;
        }

        /// <summary>
        /// Waits for any of the elements in the specified array to receive a
        /// signal, using a 32-bit signed integer to specify the time interval.
        /// </summary>
        /// <param name="waitHandles">
        /// A WaitHandle array containing the objects for which the current
        /// instance will wait.
        /// </param>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> that represents the number of milliseconds
        /// to wait, or a <see cref="TimeSpan"/> that represents -1
        /// milliseconds to wait indefinitely.
        /// </param>
        /// <returns>
        /// The array index of the object that satisfied the wait, or
        /// <see cref="WaitTimeout"/> if no object satisfied the wait and a
        /// time interval equivalent to <paramref name="timeout"/>
        /// has passed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="waitHandles"/> parameter is null, or one or
        /// more of the objects in the <paramref name="waitHandles"/> array
        /// is null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The number of objects in <paramref name="waitHandles"/> is greater
        /// than the system permits.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="timeout"/> is a negative number other than -1,
        /// which represents an infinite time-out, or
        /// <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// waitHandles is an array with no elements.
        /// </exception>
        public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("timeout");
            }

            return WaitAny(waitHandles, (int)totalMilliseconds);
        }

        #endregion

        private static void ValidateHandles(WaitHandle[] waitHandles, bool checkDups)
        {
            Collections.Generic.HashSet<int> dups = null;
            if (checkDups)
                dups = new Collections.Generic.HashSet<int>(waitHandles.Length);

            for (int i = 0; i < waitHandles.Length; i++)
            {
                if (waitHandles[i] == null)
                    throw new ArgumentNullException(nameof(waitHandles));

                if (checkDups)
                {
#pragma warning disable 0618    // Obsolete
                    if (!dups.Add(waitHandles[i].Handle.GetHashCode()))
                        throw new DuplicateWaitObjectException(nameof(waitHandles));
#pragma warning restore 0618
                }
            }

            if (checkDups)
            {
                dups.Clear();
                dups.TrimExcess();
            }
        }

#if NET35_CF
        [DllImport("coredll.dll", EntryPoint = "WaitForMultipleObjects", SetLastError = true)]
        static extern uint WaitForMultipleObjects(uint nCount, IntPtr[] lpHandles, bool fWaitAll, uint dwMilliseconds);
#else
        [DllImport("kernel32.dll", EntryPoint = "WaitForMultipleObjects", SetLastError = true)]
        static extern uint WaitForMultipleObjects(uint nCount, IntPtr[] lpHandles, bool fWaitAll, uint dwMilliseconds);
#endif
    }
}

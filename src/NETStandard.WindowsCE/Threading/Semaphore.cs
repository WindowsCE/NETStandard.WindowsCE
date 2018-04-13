using System;
using System.PInvoke;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CS0618

#if NET35_CF
namespace System.Threading
#else
namespace Mock.System.Threading
#endif
{
    /// <summary>
    /// Limits the number of threads that can access a resource, or a particular type of resource, concurrently.
    /// </summary>
    public sealed class Semaphore : WaitHandle
    {
        public Semaphore(int initialCount, int maximumCount)
            : this(initialCount, maximumCount, null)
        {
        }

        public Semaphore(int initialCount, int maximumCount, string name)
        {
            this.ValidateArgs(initialCount, maximumCount);

            IntPtr hwnd = NativeMethods.CreateSemaphore(IntPtr.Zero, initialCount, maximumCount, name);
            if (hwnd.ToInt32() == 0)
            {
                throw new ApplicationException("could not create semaphore " + Marshal.GetLastWin32Error().ToString() + " " + name);
            }

            this.Handle = hwnd;
        }

        public Semaphore(int initialCount, int maximumCount, string name, out bool createdNew)
        {
            this.ValidateArgs(initialCount, maximumCount);

            IntPtr hwnd = NativeMethods.CreateSemaphore(IntPtr.Zero, initialCount, maximumCount, name);
            Int32 er = 0;
            if (hwnd.ToInt32() == 0)
            {
                er = Marshal.GetLastWin32Error();
                throw new ApplicationException("could not create semaphore " + er.ToString() + " " + name);
            }
            if (er == NativeMethods.ERROR_ALREADY_EXISTS)
            {
                createdNew = false;
            }
            else
            {
                createdNew = true;
            }

            this.Handle = hwnd;
        }

        public static Semaphore OpenExisting(string name)
        {
            ValidateName(name);

            bool created;
            Semaphore s = new Semaphore(0, 0, name, out created);
            if (created)
            {
                //semaphore doesnt already exist
                s.Dispose(true);
                throw new WaitHandleCannotBeOpenedException();
            }
            return s;
        }

        public int Release()
        {
            return this.Release(1);
        }

        public int Release(int releaseCount)
        {
            if (releaseCount < 1)
            {
                throw new ArgumentOutOfRangeException("releaseCount must be greater than 1, not " + releaseCount.ToString());
            }
            int ret;
            if (NativeMethods.ReleaseSemaphore(this.Handle, releaseCount, out ret) == true)
            {
                return ret;
            }

            throw new SemaphoreFullException("The semaphore count is already at the maximum value.");
        }

        public static bool TryOpenExisting(string name, out Semaphore result)
        {
            ValidateName(name);

            bool created;
            Semaphore s = new Semaphore(0, 0, name, out created);
            if (created)
            {
                //semaphore doesnt already exist
                s.Dispose(true);
                result = null;
                return false;
            }

            result = s;
            return true;
        }

        /// <summary>
        /// When overridden in a derived class, blocks the current thread until the current Threading.WaitHandle receives a signal.
        /// </summary>
        /// <returns>true if the current instance receives a signal. if the current instance is never signaled, WaitHandle.WaitOne() never returns.</returns>
        public override bool WaitOne()
        {
            return WaitOne(Timeout.Infinite, false);
        }

        /// <summary>
        /// When overridden in a derived class, blocks the current thread until the current Threading.WaitHandle receives a signal, using 32-bit signed integer to measure the time interval and specifying whether to exit the synchronization domain before the wait.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Threading.Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="notApplicableOnCE">Just pass false</param>
        /// <returns>true if the current instance receives a signal; otherwise, false.</returns>
        public override bool WaitOne(Int32 millisecondsTimeout, bool notApplicableOnCE)
        {
            return (NativeMethods.WaitForSingleObject(this.Handle, millisecondsTimeout) != NativeMethods.WAIT_TIMEOUT);
        }



        /// <summary>
        /// When overridden in a derived class, blocks the current thread until the current instance receives a signal, using a TimeSpan to measure the time interval and specifying whether to exit the synchronization domain before the wait.
        /// </summary>
        /// <param name="timeout">A TimeSpan that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
        /// <param name="notApplicableOnCE">Just pass false</param>
        /// <returns>true if the current instance receives a signal; otherwise, false.</returns>
        public bool WaitOne(TimeSpan timeout, bool notApplicableOnCE)
        {
            return (NativeMethods.WaitForSingleObject(this.Handle, WaitHandle2.ToTimeoutMilliseconds(timeout)) != NativeMethods.WAIT_TIMEOUT);
        }


        /// <summary>
        /// Releases all resources held by the current <see cref="WaitHandle"/>
        /// </summary>
        public override void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool explicitDisposing)
        {
            if (this.Handle != InvalidHandle)
            {
                NativeMethods.CloseHandle(this.Handle);
                this.Handle = InvalidHandle;
            }
            base.Dispose(explicitDisposing);
        }

        private void ValidateArgs(int initialCount, int maximumCount)
        {
            if ((initialCount > maximumCount) || (maximumCount < 1) || (initialCount < 0))
            {
                throw new ArgumentException("RTFM please on what arguments are valid");
            }
        }

        private static void ValidateName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (name.Length < 1)
            {
                throw new ArgumentException("name is a zero-length string.");
            }
            if (name.Length > 260)
            {
                throw new ArgumentException("name is longer than 260 characters.");
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

#if !NET35_CF
using Mock.System.Threading;
#endif

namespace System.Threading
{
    internal sealed class Condition
    {
        private const string SynchronizationObjectDisposed = "The synchronization object was collected by GC.";

        internal class Waiter
        {
            public Waiter next;
            public Waiter prev;
            public AutoResetEvent ev = new AutoResetEvent(false);
            public bool signalled;
        }

        private static readonly LocalDataStoreSlot waiterSlot = Thread.AllocateDataSlot();

        private static Waiter GetWaiterForCurrentThread()
        {
            Waiter waiter = Thread.GetData(waiterSlot) as Waiter;
            if (waiter == null)
            {
                waiter = new Waiter();
                Thread.SetData(waiterSlot, waiter);
            }

            waiter.signalled = false;
            return waiter;
        }

        private WeakReference _lockWeakObject;
        private Waiter _waitersHead;
        private Waiter _waitersTail;

        [Conditional("DEBUG")]
        private void AssertIsInList(Waiter waiter)
        {
            Debug.Assert(_waitersHead != null && _waitersTail != null);
            Debug.Assert((_waitersHead == waiter) == (waiter.prev == null));
            Debug.Assert((_waitersTail == waiter) == (waiter.next == null));

            for (Waiter current = _waitersHead; current != null; current = current.next)
            {
                if (current == waiter)
                    return;
            }

            Debug.Fail("Waiter is not in the waiter list");
        }

        [Conditional("DEBUG")]
        private void AssertIsNotInList(Waiter waiter)
        {
            Debug.Assert(waiter.next == null && waiter.prev == null);
            Debug.Assert((_waitersHead == null) == (_waitersTail == null));

            for (Waiter current = _waitersHead; current != null; current = current.next)
            {
                if (current == waiter)
                    Debug.Fail("Waiter is in the waiter list, but should not be");
            }
        }

        private void AddWaiter(Waiter waiter)
        {
            //Debug.Assert(_lock.IsAcquired);
            AssertIsNotInList(waiter);

            waiter.prev = _waitersTail;
            if (waiter.prev != null)
                waiter.prev.next = waiter;

            _waitersTail = waiter;

            if (_waitersHead == null)
                _waitersHead = waiter;
        }

        private void RemoveWaiter(Waiter waiter)
        {
            //Debug.Assert(_lock.IsAcquired);
            AssertIsInList(waiter);

            if (waiter.next != null)
                waiter.next.prev = waiter.prev;
            else
                _waitersTail = waiter.prev;

            if (waiter.prev != null)
                waiter.prev.next = waiter.next;
            else
                _waitersHead = waiter.next;

            waiter.next = null;
            waiter.prev = null;
        }

        public Condition(WeakReference lockObject)
        {
            if (lockObject == null)
                throw new ArgumentNullException(nameof(lockObject));

            _lockWeakObject = lockObject;
        }

        public bool Wait() => Wait(Timeout.Infinite);

        public bool Wait(TimeSpan timeout) => Wait(WaitHandle2.ToTimeoutMilliseconds(timeout));

        public bool Wait(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);

            //if (!_lock.IsAcquired)
            //    throw new SynchronizationLockException();

            Waiter waiter = GetWaiterForCurrentThread();
            AddWaiter(waiter);

            //uint recursionCount = _lock.ReleaseAll();
            var lockObject = _lockWeakObject.Target;
            if (!_lockWeakObject.IsAlive)
                throw new SynchronizationLockException(SynchronizationObjectDisposed);

            uint recursionCount = Monitor2.ReleaseAll(lockObject);
            bool success = false;
            try
            {
                // Since that IsAcquired is not available ensure that
                // the lock was freed
                if (recursionCount == 0)
                    throw new SynchronizationLockException();

                success = waiter.ev.WaitOne(millisecondsTimeout);
            }
            finally
            {
                //_lock.Reacquire(recursionCount);
                //Debug.Assert(_lock.IsAcquired);
                Monitor2.Reacquire(lockObject, recursionCount);

                if (!waiter.signalled)
                {
                    RemoveWaiter(waiter);
                }
                else if (!success)
                {
                    //
                    // The wait timed out, but we were signalled before we could reacquire the lock.
                    // Since WaitOne timed out, it didn't trigger the auto-reset of the AutoResetEvent.
                    // So, we need to manually reset the event.
                    //
                    waiter.ev.Reset();
                }

                AssertIsNotInList(waiter);
            }

            return waiter.signalled;
        }

        public void SignalAll()
        {
            //if (!_lock.IsAcquired)
            //    throw new SynchronizationLockException();

            object lockObject = null;
            try
            {
                lockObject = EnsureLockAcquisition();

                while (_waitersHead != null)
                {
                    UnsafeSignalOne();
                }
            }
            finally
            {
                UnlockIfAvailable(lockObject);
            }
        }

        public void SignalOne()
        {
            //if (!_lock.IsAcquired)
            //    throw new SynchronizationLockException();

            object lockObject = null;
            try
            {
                lockObject = EnsureLockAcquisition();
                UnsafeSignalOne();
            }
            finally
            {
                UnlockIfAvailable(lockObject);
            }
        }

        private object EnsureLockAcquisition()
        {
            var lockObject = _lockWeakObject.Target;
            if (!_lockWeakObject.IsAlive)
                throw new SynchronizationLockException(SynchronizationObjectDisposed);

            bool lockTaken = Monitor.TryEnter(lockObject);
            if (!lockTaken)
                throw new SynchronizationLockException();

            return lockObject;
        }

        private void UnlockIfAvailable(object lockObject)
        {
            if (lockObject == null)
                return;

            Monitor.Exit(lockObject);
        }

        private void UnsafeSignalOne()
        {
            Waiter waiter = _waitersHead;
            if (waiter != null)
            {
                RemoveWaiter(waiter);
                waiter.signalled = true;
                waiter.ev.Set();
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace Tests
{
    public static class ThreadTestHelpers
    {
        public const int ExpectedTimeoutMilliseconds = 50;
        public const int UnexpectedTimeoutMilliseconds = 1000 * 30;

        public static Thread CreateGuardedThread(out Action waitForThread, Action start)
        {
            Action checkForThreadErrors;
            return CreateGuardedThread(out checkForThreadErrors, out waitForThread, start);
        }

        public static Thread CreateGuardedThread(out Action checkForThreadErrors, out Action waitForThread, Action start)
        {
            Exception backgroundEx = null;
            var t =
                new Thread(() =>
                {
                    try
                    {
                        start();
                    }
                    catch (Exception ex)
                    {
                        backgroundEx = ex;
                        Thread.MemoryBarrier();
                        // TODO: Create a wrapper into Interlocked
                        //Interlocked.MemoryBarrier();
                    }
                });
            Action localCheckForThreadErrors = checkForThreadErrors = // cannot use ref or out parameters in lambda
                () =>
                {
                    Thread.MemoryBarrier();
                    //Interlocked.MemoryBarrier();
                    if (backgroundEx != null)
                    {
                        throw new AggregateException(backgroundEx);
                    }
                };
            waitForThread =
                () =>
                {
                    Assert.IsTrue(t.Join(UnexpectedTimeoutMilliseconds));
                    localCheckForThreadErrors();
                };
            return t;
        }

#if !WindowsCE
        public static Thread CreateGuardedThread(out Action waitForThread, Action<object> start)
        {
            Action checkForThreadErrors;
            return CreateGuardedThread(out checkForThreadErrors, out waitForThread, start);
        }

        public static Thread CreateGuardedThread(out Action checkForThreadErrors, out Action waitForThread, Action<object> start)
        {
            Exception backgroundEx = null;
            var t =
                new Thread(parameter =>
                {
                    try
                    {
                        start(parameter);
                    }
                    catch (Exception ex)
                    {
                        backgroundEx = ex;
                        Thread.MemoryBarrier();
                        //Interlocked.MemoryBarrier();
                    }
                });
            Action localCheckForThreadErrors = checkForThreadErrors = // cannot use ref or out parameters in lambda
                () =>
                {
                    Thread.MemoryBarrier();
                    //Interlocked.MemoryBarrier();
                    if (backgroundEx != null)
                    {
                        throw new AggregateException(backgroundEx);
                    }
                };
            waitForThread =
                () =>
                {
                    Assert.IsTrue(t.Join(UnexpectedTimeoutMilliseconds));
                    localCheckForThreadErrors();
                };
            return t;
        }
#endif

        public static void RunTestInBackgroundThread(Action test)
        {
            Action waitForThread;
            var t = CreateGuardedThread(out waitForThread, test);
            t.IsBackground = true;
            t.Start();
            waitForThread();
        }

        public static void WaitForCondition(Func<bool> condition)
        {
            WaitForConditionWithCustomDelay(condition, () => Thread.Sleep(1));
        }

        public static void WaitForConditionWithoutBlocking(Func<bool> condition)
        {
            //WaitForConditionWithCustomDelay(condition, () => Thread.Yield());
            WaitForConditionWithCustomDelay(condition, () => Thread.Sleep(0));
        }

        public static void WaitForConditionWithCustomDelay(Func<bool> condition, Action delay)
        {
            var startTime = DateTime.Now;
            while (!condition())
            {
                Assert.IsTrue((DateTime.Now - startTime).TotalMilliseconds < UnexpectedTimeoutMilliseconds);
                delay();
            }
        }

        public static void CheckedWait(this WaitHandle wh)
        {
            Assert.IsTrue(wh.WaitOne(UnexpectedTimeoutMilliseconds));
        }

        //public static void CheckedWait(this ManualResetEventSlim e)
        //{
        //    Assert.IsTrue(e.Wait(UnexpectedTimeoutMilliseconds));
        //}
    }
}
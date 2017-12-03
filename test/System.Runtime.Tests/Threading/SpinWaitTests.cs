// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

#if !WindowsCE
using Mock.System;
#endif

namespace Tests.Threading
{
    [TestClass]
    public class SpinWaitTests
    {
        [TestMethod]
        public void RunSpinWaitTests()
        {
            SpinWait spinner = new SpinWait();

            spinner.SpinOnce();
            Assert.AreEqual(spinner.Count, 1);
        }

        [TestMethod]
        public void RunSpinWaitTests_Negative()
        {
            //test SpinUntil
            AssertExtensions.Throws<ArgumentNullException>(
               () => SpinWait.SpinUntil(null));
            // Failure Case:  SpinUntil didn't throw ANE when null condition  passed
            AssertExtensions.Throws<ArgumentOutOfRangeException2>(
               () => SpinWait.SpinUntil(() => true, TimeSpan.MaxValue));
            // Failure Case:  SpinUntil didn't throw AORE when milliseconds > int.Max passed
            AssertExtensions.Throws<ArgumentOutOfRangeException2>(
               () => SpinWait.SpinUntil(() => true, -2));
            // Failure Case:  SpinUntil didn't throw AORE when milliseconds < -1 passed

            Assert.IsFalse(SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(100)),
               "RunSpinWaitTests:  SpinUntil returned true when the condition i always false!");
            Assert.IsTrue(SpinWait.SpinUntil(() => true, 0),
               "RunSpinWaitTests:  SpinUntil returned false when the condition i always true!");
        }
    }
}
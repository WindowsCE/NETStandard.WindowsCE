// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

#if !WindowsCE
using OperationCanceledException = Mock.System.OperationCanceledException;
#endif

namespace Tests
{
    [TestClass]
    public class OperationCanceledExceptionTests
    {
        [TestMethod]
        public void OperationCanceledException_BasicConstructors()
        {
            CancellationToken ct1 = new CancellationTokenSource().Token;
            OperationCanceledException ex1 = new OperationCanceledException(ct1);
            Assert.AreEqual(ct1, ex1.CancellationToken);

            CancellationToken ct2 = new CancellationTokenSource().Token;
            OperationCanceledException ex2 = new OperationCanceledException("message", ct2);
            Assert.AreEqual(ct2, ex2.CancellationToken);

            CancellationToken ct3 = new CancellationTokenSource().Token;
            OperationCanceledException ex3 = new OperationCanceledException("message", new Exception("inner"), ct3);
            Assert.AreEqual(ct3, ex3.CancellationToken);
        }
    }
}
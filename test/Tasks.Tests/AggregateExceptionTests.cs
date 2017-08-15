// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class AggregateExceptionTests
    {
        [TestMethod]
        public void AggregateException_ConstructorBasic()
        {
            AggregateException ex = new AggregateException();
            Assert.AreEqual(ex.InnerExceptions.Count, 0);
            Assert.IsTrue(ex.Message != null, "RunAggregateException_Constructor:  FAILED. Message property is null when the default constructor is used, expected a default message");

            ex = new AggregateException("message");
            Assert.AreEqual(ex.InnerExceptions.Count, 0);
            Assert.IsTrue(ex.Message != null, "RunAggregateException_Constructor:  FAILED. Message property is  null when the default constructor(string) is used");

            ex = new AggregateException("message", new Exception());
            Assert.AreEqual(ex.InnerExceptions.Count, 1);
            Assert.IsTrue(ex.Message != null, "RunAggregateException_Constructor:  FAILED. Message property is  null when the default constructor(string, Exception) is used");
        }

        [TestMethod]
        public void AggregateException_ConstructorInvalidArguments()
        {
            AggregateException ex = new AggregateException();
            AssertExtensions.Throws<ArgumentNullException>(() => new AggregateException("message", (Exception)null));

            AssertExtensions.Throws<ArgumentNullException>(() => new AggregateException("message", (IEnumerable<Exception>)null));

            AssertExtensions.Throws<ArgumentException>(null, () => ex = new AggregateException("message", new[] { new Exception(), null }));
        }

        [TestMethod]
        public void AggregateException_BaseExceptions()
        {
            AggregateException ex = new AggregateException();
            Assert.AreEqual(ex.GetBaseException(), ex);

            Exception[] innerExceptions = new Exception[0];
            ex = new AggregateException(innerExceptions);
            Assert.AreEqual(ex.GetBaseException(), ex);

            innerExceptions = new Exception[1] { new AggregateException() };
            ex = new AggregateException(innerExceptions);
            Assert.AreEqual(ex.GetBaseException(), innerExceptions[0]);

            innerExceptions = new Exception[2] { new AggregateException(), new AggregateException() };
            ex = new AggregateException(innerExceptions);
            Assert.AreEqual(ex.GetBaseException(), ex);
        }

        [TestMethod]
        public void AggregateException_Handle()
        {
            AggregateException ex = new AggregateException();
            ex = new AggregateException(new[] { new ArgumentException(), new ArgumentException(), new ArgumentException() });
            int handledCount = 0;
            ex.Handle((e) =>
            {
                if (e is ArgumentException)
                {
                    handledCount++;
                    return true;
                }
                return false;
            });
            Assert.AreEqual(handledCount, ex.InnerExceptions.Count);
        }

        [TestMethod]
        public void AggregateException_HandleInvalidCases()
        {
            AggregateException ex = new AggregateException();
            AssertExtensions.Throws<ArgumentNullException>(() => ex.Handle(null));

            ex = new AggregateException(new[] { new Exception(), new ArgumentException(), new ArgumentException() });
            int handledCount = 0;
            AssertExtensions.Throws<AggregateException>(
               () => ex.Handle((e) =>
               {
                   if (e is ArgumentException)
                   {
                       handledCount++;
                       return true;
                   }
                   return false;
               }));
        }

        // Validates that flattening (including recursive) works.
        [TestMethod]
        public void AggregateException_Flatten()
        {
            Exception exceptionA = new Exception("A");
            Exception exceptionB = new Exception("B");
            Exception exceptionC = new Exception("C");

            AggregateException aggExceptionBase = new AggregateException(exceptionA, exceptionB, exceptionC);

            // Verify flattening one with another.
            // > Flattening (no recursion)...

            AggregateException flattened1 = aggExceptionBase.Flatten();
            Exception[] expected1 = new Exception[] {
                exceptionA, exceptionB, exceptionC
            };

            if (!expected1.SequenceEqual(flattened1.InnerExceptions))
                Assert.Fail("Unexpected InnerExceptions content");
            //Assert.AreEqual(expected1, flattened1.InnerExceptions);

            // Verify flattening one with another, accounting for recursion.
            AggregateException aggExceptionRecurse = new AggregateException(aggExceptionBase, aggExceptionBase);
            AggregateException flattened2 = aggExceptionRecurse.Flatten();
            Exception[] expected2 = new Exception[] {
                exceptionA, exceptionB, exceptionC, exceptionA, exceptionB, exceptionC,
            };

            if (!expected2.SequenceEqual(flattened2.InnerExceptions))
                Assert.Fail("Unexpected InnerExceptions content");
            //Assert.AreEqual(expected2, flattened2.InnerExceptions);
        }
    }
}
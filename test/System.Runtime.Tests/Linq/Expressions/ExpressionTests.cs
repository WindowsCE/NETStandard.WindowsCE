using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

namespace Tests.Linq.Expressions
{
    [TestClass]
    public class ExpressionTests
    {
        [TestMethod]
        public void BasicExpressionTree()
        {
            var client = new Client();
            var someObj = new SomeClass { Qty = 100 };
            int qty = someObj.Qty * 2;
            double amount = client.Invoke(svc => svc.DoSomeMaths(12, qty));
        }

        class Client
        {
            public double Invoke(Expression<Func<SomeService, double>> expression)
            {
                throw new NotImplementedException();
            }
        }

        class SomeService
        {
            public double DoSomeMaths(int a, int b) => a * b;
        }

        class SomeClass
        {
            public int Qty { get; set; }
        }
    }
}

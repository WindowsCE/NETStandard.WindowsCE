using System;
using System.Reflection;

#if NET35_CF
namespace System.Linq.Expressions.Jvm
#else
namespace Mock.System.Linq.Expressions.Jvm
#endif
{
    public class Interpreter
    {
        private class InternalVoidSubstitute
        {
        }

        private static readonly Type VoidSubstitute = typeof(InternalVoidSubstitute);

        private static MethodInfo[] delegateMap;

        private const int MapSize = 5;

        public LambdaExpression Expression { get; }

        static Interpreter()
        {
            InitDelegateMap();
        }

        private static void InitDelegateMap()
        {
            var mia = typeof(Interpreter).GetMethods(BindingFlags.Instance | BindingFlags.Public);
            delegateMap = new MethodInfo[MapSize];
            foreach (MethodInfo m in mia)
            {
                if (m.Name == "GetDelegate")
                {
                    delegateMap[m.GetGenericArguments().Length - 1] = m;
                }
            }
        }

        public Interpreter(LambdaExpression expression)
        {
            Expression = expression;
        }

        public Delegate CreateDelegate()
        {
            Type[] arr = ExtractGenerecParameters();
            MethodInfo mi = delegateMap[arr.Length - 1];
            MethodInfo mgi = mi.MakeGenericMethod(arr);
            return (Delegate)mgi.Invoke(this, new object[0]);
        }

        public void Validate()
        {
            var validator = new ExpressionValidator(Expression);
            validator.Validate();
        }

        private Type[] ExtractGenerecParameters()
        {
            Type[] arr = new Type[Expression.Parameters.Count + 1];
            Type rt = Expression.GetReturnType();
            if (rt == typeof(void))
            {
                rt = VoidSubstitute;
            }

            arr[Expression.Parameters.Count] = rt;
            for (int i = 0; i < Expression.Parameters.Count; i++)
            {
                arr[i] = Expression.Parameters[i].Type;
            }

            return arr;
        }

        private object Run(object[] arg)
        {
            return ExpressionInterpreter.Interpret(Expression, arg);
        }

        public Delegate GetDelegate<TResult>()
        {
            return typeof(TResult) == VoidSubstitute
                ? (Delegate)new Action(ActionAccessor)
                : new Func<TResult>(FuncAccessor<TResult>);
        }

        public TResult FuncAccessor<TResult>()
        {
            return (TResult)Run(new object[0]);
        }

        public void ActionAccessor()
        {
            Run(new object[0]);
        }

        public Delegate GetDelegate<T, TResult>()
        {
            return typeof(TResult) == VoidSubstitute
                ? (Delegate)new Action<T>(ActionAccessor)
                : new Func<T, TResult>(FuncAccessor<T, TResult>);
        }

        public TResult FuncAccessor<T, TResult>(T arg)
        {
            return (TResult)Run(new object[] { arg });
        }

        public void ActionAccessor<T>(T arg)
        {
            Run(new object[] { arg });
        }

        public Delegate GetDelegate<T1, T2, TResult>()
        {
            return typeof(TResult) == VoidSubstitute
                ? (Delegate)new Action<T1, T2>(ActionAccessor)
                : new Func<T1, T2, TResult>(FuncAccessor<T1, T2, TResult>);
        }

        public TResult FuncAccessor<T1, T2, TResult>(T1 arg1, T2 arg2)
        {
            return (TResult)Run(new object[] { arg1, arg2 });
        }

        public void ActionAccessor<T1, T2>(T1 arg1, T2 arg2)
        {
            Run(new object[] { arg1, arg2 });
        }

        public Delegate GetDelegate<T1, T2, T3, TResult>()
        {
            return typeof(TResult) == VoidSubstitute
                ? (Delegate)new Action<T1, T2, T3>(ActionAccessor)
                : new Func<T1, T2, T3, TResult>(FuncAccessor<T1, T2, T3, TResult>);
        }

        public TResult FuncAccessor<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3)
        {
            return (TResult)Run(new object[] { arg1, arg2, arg3 });
        }

        public void ActionAccessor<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
        {
            Run(new object[] { arg1, arg2, arg3 });
        }

        public Delegate GetDelegate<T1, T2, T3, T4, TResult>()
        {
            return typeof(TResult) == VoidSubstitute
                ? (Delegate)new Action<T1, T2, T3, T4>(ActionAccessor)
                : new Func<T1, T2, T3, T4, TResult>(FuncAccessor<T1, T2, T3, T4, TResult>);
        }

        public TResult FuncAccessor<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return (TResult)Run(new object[] { arg1, arg2, arg3, arg4 });
        }

        public void ActionAccessor<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Run(new object[] { arg1, arg2, arg3, arg4 });
        }
    }
}

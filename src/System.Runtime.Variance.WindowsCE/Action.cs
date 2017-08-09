// Ref: https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Action.cs

namespace System
{
    public delegate void Action2<in T1>(T1 arg1);
    public delegate void Action2<in T1, in T2>(T1 arg1, T2 arg2);
    public delegate void Action2<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Action2<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate TResult Func2<out TResult>();
    public delegate TResult Func2<in T, out TResult>(T arg);
    public delegate TResult Func2<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
    public delegate TResult Func2<in T1, in T2, in T3, out TResult>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Func2<in T1, in T2, in T3, in T4, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
}

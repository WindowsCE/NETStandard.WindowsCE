using System;
using System.Threading;

namespace Tests
{
    static class ParallelTestHelper
    {
        private const int _numRun = 50;

        public static void ParallelStressTest<TSource>(TSource obj, Action<TSource> action)
        {
            ParallelStressTest(obj, action, Environment.ProcessorCount + 2);
        }

        public static void ParallelStressTest<TSource>(TSource obj, Action<TSource> action, int numThread)
        {
            if (action == null)
            {
                return;
            }
            var threads = new Thread[numThread];
            for (var i = 0; i < numThread; i++)
            {
                threads[i] = new Thread(() => action(obj));
                threads[i].Start();
            }

            // Wait for the completion
            for (var i = 0; i < numThread; i++)
            {
                threads[i].Join();
            }
        }

        public static void Repeat(Action action)
        {
            Repeat(action, _numRun);
        }

        public static void Repeat(Action action, int numRun)
        {
            if (action == null)
            {
                return;
            }
            for (var i = 0; i < numRun; i++)
            {
                action();
            }
        }
    }
}

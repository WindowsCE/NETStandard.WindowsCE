// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

#if WindowsCE
using System.Runtime.CompilerServices;
using InternalOCE = System.OperationCanceledException;
#else
using Mock.System;
using InternalOCE = Mock.System.OperationCanceledException;
#endif

namespace Tests.Threading
{
    /// <summary>
    /// Barrier unit tests
    /// </summary>
    [TestClass]
    public class BarrierTests
    {
        /// <summary>
        /// Runs all the unit tests
        /// </summary>
        /// <returns>True if all tests succeeded, false if one or more tests failed</returns>
        [TestMethod]
        public void RunBarrierConstructorTests()
        {
            RunBarrierTest1_ctor(10, null);
            RunBarrierTest1_ctor(0, null);
        }

        [TestMethod]
        public void RunBarrierConstructorTests_NegativeTests()
        {
            RunBarrierTest1_ctor(-1, typeof(ArgumentOutOfRangeException2));
            RunBarrierTest1_ctor(int.MaxValue, typeof(ArgumentOutOfRangeException2));
        }

        /// <summary>
        /// Testing Barrier constructor
        /// </summary>
        /// <param name="initialCount">The initial barrier count</param>
        /// <param name="exceptionType">Type of the exception in case of invalid input, null for valid cases</param>
        /// <returns>True if the test succeeded, false otherwise</returns>
        private static void RunBarrierTest1_ctor(int initialCount, Type exceptionType)
        {
            try
            {
                Barrier b = new Barrier(initialCount);
                Assert.AreEqual(initialCount, b.ParticipantCount);
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(exceptionType);
                AssertExtensions.IsType(exceptionType, ex);
            }
        }

        [TestMethod]
        public void RunBarrierSignalAndWaitTests()
        {
            RunBarrierTest2_SignalAndWait(1, new TimeSpan(0, 0, 0, 0, -1), true, null);
            RunBarrierTest2_SignalAndWait(5, new TimeSpan(0, 0, 0, 0, 100), false, null);
            RunBarrierTest2_SignalAndWait(5, new TimeSpan(0), false, null);
            RunBarrierTest3_SignalAndWait(3);
        }

        [TestMethod]
        public void RunBarrierSignalAndWaitTests_NegativeTests()
        {
            RunBarrierTest2_SignalAndWait(1, new TimeSpan(0, 0, 0, 0, -2), false, typeof(ArgumentOutOfRangeException2));
        }

        /// <summary>
        /// Test SignalAndWait sequential
        /// </summary>
        /// <param name="initialCount">The initial barrier participants</param>
        /// <param name="timeout">SignalAndWait timeout</param>
        /// <param name="result">Expected return value</param>
        /// <param name="exceptionType">Type of the exception in case of invalid input, null for valid cases</param>
        /// <returns>True if the test succeeded, false otherwise</returns>
        private static void RunBarrierTest2_SignalAndWait(int initialCount, TimeSpan timeout, bool result, Type exceptionType)
        {
            Barrier b = new Barrier(initialCount);
            try
            {
                Assert.AreEqual(result, b.SignalAndWait(timeout));
                if (result && b.CurrentPhaseNumber != 1)
                {
                    Assert.AreEqual(1, b.CurrentPhaseNumber);
                    Assert.AreEqual(b.ParticipantCount, b.ParticipantsRemaining);
                }
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(exceptionType);
                AssertExtensions.IsType(exceptionType, ex);
            }
        }

        /// <summary>
        /// Test SignalANdWait parallel
        /// </summary>
        /// <param name="initialCount">Initial barrier count</param>
        /// <returns>True if the test succeeded, false otherwise</returns>
        private static void RunBarrierTest3_SignalAndWait(int initialCount)
        {
            Barrier b = new Barrier(initialCount);
            for (int i = 0; i < initialCount - 1; i++)
            {
                Task.Run(() => b.SignalAndWait());
            }
            b.SignalAndWait();

            Assert.AreEqual(1, b.CurrentPhaseNumber);
            Assert.AreEqual(b.ParticipantCount, b.ParticipantsRemaining);
        }

        [TestMethod]
        public void RunBarrierAddParticipantsTest()
        {
            RunBarrierTest4_AddParticipants(0, 1, null);
            RunBarrierTest4_AddParticipants(5, 3, null);
        }

        [TestMethod]
        public void RunBarrierAddParticipantsTest_NegativeTests()
        {
            RunBarrierTest4_AddParticipants(0, 0, typeof(ArgumentOutOfRangeException2));
            RunBarrierTest4_AddParticipants(2, -1, typeof(ArgumentOutOfRangeException2));
            RunBarrierTest4_AddParticipants(0x00007FFF, 1, typeof(ArgumentOutOfRangeException));
            RunBarrierTest4_AddParticipants(100, int.MaxValue, typeof(ArgumentOutOfRangeException));
        }

        [TestMethod]
        public void TooManyParticipants()
        {
            Barrier b = new Barrier(Int16.MaxValue);
            AssertExtensions.Throws<InvalidOperationException>(() => b.AddParticipant());
        }

        [TestMethod]
        public void RemovingParticipants()
        {
            Barrier b;

            b = new Barrier(1);
            b.RemoveParticipant();
            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => b.RemoveParticipant());

            b = new Barrier(1);
            b.RemoveParticipants(1);
            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => b.RemoveParticipants(1));

            b = new Barrier(1);
            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => b.RemoveParticipants(2));
        }

#if WindowsCE
        private sealed class RemovingWaitingParticipantsStateMachine : IAsyncStateMachine
        {
            private State state;
            private AsyncTaskMethodBuilder builder;
            private Task t;
            private TaskAwaiter u1;
            private Barrier b;

            public RemovingWaitingParticipantsStateMachine(AsyncTaskMethodBuilder builder)
            {
                this.builder = builder;
                state = State.Created;
            }

            private enum State
            {
                Finished = -2,
                Created = -1,
                DelayCallback = 0,
                TCallback = 1,
                TestParticipants = 100,
                Delayed = 101,
                ParticipantSignaled = 102,
                TCompleted = 103
            }

            public void MoveNext()
            {
                State num = (State)state;
                try
                {
                    TaskAwaiter awaiter;
                    TaskAwaiter awaiter2;
                    switch (num)
                    {
                        case State.Created:
                            b = new Barrier(4);
                            t = Task.Run(() => b.SignalAndWait());
                            goto case State.TestParticipants;
                        case State.TestParticipants:
                            if (b.ParticipantsRemaining > 3)
                            {
                                awaiter2 = Task.Delay(100).GetAwaiter();
                                if (awaiter2.IsCompleted)
                                    goto case State.Delayed;

                                num = (state = State.DelayCallback);
                                u1 = awaiter2;
                                var callback2 = this;
                                builder.AwaitOnCompleted(ref awaiter2, ref callback2);
                                return;
                            }

                            goto case State.ParticipantSignaled;
                        case State.DelayCallback:
                            awaiter2 = u1;
                            u1 = default(TaskAwaiter);
                            num = (state = State.Created);
                            goto case State.Delayed;
                        case State.Delayed:
                            awaiter2.GetResult();
                            goto case State.TestParticipants;
                        case State.ParticipantSignaled:
                            b.RemoveParticipants(2);
                            Assert.AreEqual(1, b.ParticipantsRemaining);

                            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => b.RemoveParticipants(20));
                            Assert.AreEqual(1, b.ParticipantsRemaining);

                            AssertExtensions.Throws<InvalidOperationException>(() => b.RemoveParticipants(2));
                            Assert.AreEqual(1, b.ParticipantsRemaining);

                            b.RemoveParticipant();
                            awaiter = t.GetAwaiter();
                            if (awaiter.IsCompleted)
                                goto case State.TCompleted;

                            num = (state = State.TCallback);
                            u1 = awaiter;
                            var callback = this;
                            builder.AwaitOnCompleted(ref awaiter, ref callback);
                            return;
                        case State.TCallback:
                            awaiter = u1;
                            u1 = default(TaskAwaiter);
                            num = (state = State.Created);
                            goto case State.TCompleted;
                        case State.TCompleted:
                            awaiter.GetResult();
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    state = State.Finished;
                    builder.SetException(ex);
                    return;
                }

                state = State.Finished;
                builder.SetResult();
            }


            public void SetStateMachine(IAsyncStateMachine stateMachine)
            {
            }
        }

        [TestMethod]
        public void RemovingWaitingParticipants()
        {
            var builder = AsyncTaskMethodBuilder.Create();
            var sm = new RemovingWaitingParticipantsStateMachine(builder);
            builder.Start(ref sm);
            builder.Task.Wait();
        }
#else
        [TestMethod]
        public async Task RemovingWaitingParticipants()
        {
            Barrier b = new Barrier(4);
            Task t = Task.Run(() =>
            {
                b.SignalAndWait();
            });

            while (b.ParticipantsRemaining > 3)
            {
                await Task.Delay(100);
            }

            b.RemoveParticipants(2); // Legal. Succeeds.

            Assert.AreEqual(1, b.ParticipantsRemaining);

            AssertExtensions.Throws<ArgumentOutOfRangeException>(() => b.RemoveParticipants(20)); // Too few total to remove

            Assert.AreEqual(1, b.ParticipantsRemaining);

            AssertExtensions.Throws<InvalidOperationException>(() => b.RemoveParticipants(2)); // Too few remaining to remove

            Assert.AreEqual(1, b.ParticipantsRemaining);
            b.RemoveParticipant(); // Barrier survives the incorrect removals, and we can still remove correctly.

            await t; // t can now complete.
        }
#endif

        /// <summary>
        /// Test AddParticipants
        /// </summary>
        /// <param name="initialCount">The initial barrier participants count</param>
        /// <param name="participantsToAdd">The participants that will be added</param>
        /// <param name="exceptionType">Type of the exception in case of invalid input, null for valid cases</param>
        /// <returns>True if the test succeeded, false otherwise</returns>
        private static void RunBarrierTest4_AddParticipants(int initialCount, int participantsToAdd, Type exceptionType)
        {
            Barrier b = new Barrier(initialCount);
            try
            {
                Assert.AreEqual(0, b.AddParticipants(participantsToAdd));
                Assert.AreEqual(initialCount + participantsToAdd, b.ParticipantCount);
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(exceptionType);
                AssertExtensions.IsType(exceptionType, ex);
            }
        }

        [TestMethod]
        public void RunBarrierRemoveParticipantsTests()
        {
            RunBarrierTest5_RemoveParticipants(1, 1, null);
            RunBarrierTest5_RemoveParticipants(10, 7, null);
        }

        [TestMethod]
        public void RunBarrierRemoveParticipantsTests_NegativeTests()
        {
            RunBarrierTest5_RemoveParticipants(10, 0, typeof(ArgumentOutOfRangeException2));
            RunBarrierTest5_RemoveParticipants(1, -1, typeof(ArgumentOutOfRangeException2));
            RunBarrierTest5_RemoveParticipants(5, 6, typeof(ArgumentOutOfRangeException));
        }

        /// <summary>
        /// Test RemoveParticipants
        /// </summary>
        /// <param name="initialCount">The initial barrier participants count</param>
        /// <param name="participantsToRemove">The participants that will be added</param>
        /// <param name="exceptionType">Type of the exception in case of invalid input, null for valid cases</param>
        /// <returns>True if the test succeeded, false otherwise</returns>
        private static void RunBarrierTest5_RemoveParticipants(int initialCount, int participantsToRemove, Type exceptionType)
        {
            Barrier b = new Barrier(initialCount);
            try
            {
                b.RemoveParticipants(participantsToRemove);
                Assert.AreEqual(initialCount - participantsToRemove, b.ParticipantCount);
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(exceptionType);
                AssertExtensions.IsType(exceptionType, ex);
            }
        }

        /// <summary>
        /// Test Dispose
        /// </summary>
        /// <returns>True if the test succeeded, false otherwise</returns>
        [TestMethod]
        public void RunBarrierTest6_Dispose()
        {
            Barrier b = new Barrier(1);
            b.Dispose();
            AssertExtensions.Throws<ObjectDisposedException>(() => b.SignalAndWait());
        }

        [TestMethod]
        public void SignalBarrierWithoutParticipants()
        {
            using (Barrier b = new Barrier(0))
            {
                AssertExtensions.Throws<InvalidOperationException>(() => b.SignalAndWait());
            }
        }

        [TestMethod]
        public void RunBarrierTest7a()
        {
            for (int j = 0; j < 100; j++)
            {
                Barrier b = new Barrier(0);
                Action[] actions = new Action[4];
                for (int k = 0; k < 4; k++)
                {
                    actions[k] = (Action)(() =>
                    {
                        for (int i = 0; i < 400; i++)
                        {
                            b.AddParticipant();
                            b.RemoveParticipant();
                        }
                    });
                }

                Task[] tasks = new Task[actions.Length];
                for (int k = 0; k < tasks.Length; k++)
                    tasks[k] = Task.Factory.StartNew((index) => actions[(int)index](), k, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
                Task.WaitAll(tasks);
                Assert.AreEqual(0, b.ParticipantCount);
            }
        }

        /// <summary>
        /// Test the case when the post phase action throws an exception
        /// </summary>
        /// <returns>True if the test succeeded, false otherwise</returns>
        [TestMethod]
        public void RunBarrierTest8_PostPhaseException()
        {
            bool shouldThrow = true;
            int participants = 4;
            Barrier barrier = new Barrier(participants, (b) =>
            {
                if (shouldThrow)
                    throw new InvalidOperationException();
            });
            int succeededCount = 0;

            // Run threads that will expect BarrierPostPhaseException when they call SignalAndWait, and increment the count in the catch block
            // The BarrierPostPhaseException inner exception should be the real exception thrown by the post phase action
            Task[] threads = new Task[participants];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = Task.Run(() =>
                {
                    try
                    {
                        barrier.SignalAndWait();
                    }
                    catch (BarrierPostPhaseException ex)
                    {
                        if (ex.InnerException.GetType().Equals(typeof(InvalidOperationException)))
                            Interlocked.Increment(ref succeededCount);
                    }
                });
            }

            foreach (Task t in threads)
                t.Wait();
            Assert.AreEqual(participants, succeededCount);
            Assert.AreEqual(1, barrier.CurrentPhaseNumber);

            // turn off the exception
            shouldThrow = false;

            // Now run the threads again and they shouldn't got the exception, decrement the count if it got the exception
            threads = new Task[participants];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = Task.Run(() =>
                {
                    try
                    {
                        barrier.SignalAndWait();
                    }
                    catch (BarrierPostPhaseException)
                    {
                        Interlocked.Decrement(ref succeededCount);
                    }
                });
            }
            foreach (Task t in threads)
                t.Wait();
            Assert.AreEqual(participants, succeededCount);
        }

        /// <summary>
        /// Test ithe case when the post phase action throws an exception
        /// </summary>
        /// <returns>True if the test succeeded, false otherwise</returns>
        [TestMethod]
        public void RunBarrierTest9_PostPhaseException()
        {
            Barrier barrier = new Barrier(1, (b) => b.SignalAndWait());
            EnsurePostPhaseThrew(barrier);

            barrier = new Barrier(1, (b) => b.Dispose());
            EnsurePostPhaseThrew(barrier);

            barrier = new Barrier(1, (b) => b.AddParticipant());
            EnsurePostPhaseThrew(barrier);

            barrier = new Barrier(1, (b) => b.RemoveParticipant());
            EnsurePostPhaseThrew(barrier);
        }

        [TestMethod]
        //[OuterLoop]
        public void RunBarrierTest10a()
        {
            // Regression test for Barrier race condition
            for (int j = 0; j < 1000; j++)
            {
                Barrier b = new Barrier(2);
                Task[] tasks = new Task[2];
                var src = new CancellationTokenSource();
                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = Task.Run(() =>
                    {
                        try
                        {
                            if (b.SignalAndWait(-1, src.Token))
                                src.Cancel();
                        }
                        catch (InternalOCE)
                        {
                        }
                    });
                }
                Task.WaitAll(tasks);
            }
        }

        [TestMethod]
        public void RunBarrierTest10b()
        {
            // Regression test for Barrier race condition
            for (int j = 0; j < 10; j++)
            {
                Barrier b = new Barrier(2);
                var t1 = Task.Run(() =>
                {
                    b.SignalAndWait();
                    b.RemoveParticipant();
                    b.SignalAndWait();
                });

                var t2 = Task.Run(() => b.SignalAndWait());
                Task.WaitAll(t1, t2);
                if (j > 0 && j % 1000 == 0)
                    Debug.WriteLine(string.Format(" > Finished {0} iterations", j));
            }
        }

        [TestMethod]
        public void RunBarrierTest10c()
        {
            for (int j = 0; j < 10; j++)
            {
                Task[] tasks = new Task[2];
                Barrier b = new Barrier(3);
                tasks[0] = Task.Run(() => b.SignalAndWait());
                tasks[1] = Task.Run(() => b.SignalAndWait());

                b.SignalAndWait();
                b.Dispose();

                GC.Collect();

                Task.WaitAll(tasks);
            }
        }

        [TestMethod]
        public void PostPhaseException()
        {
            Exception exc = new Exception("inner");

            Assert.IsNotNull(new BarrierPostPhaseException().Message);
            Assert.IsNotNull(new BarrierPostPhaseException((string)null).Message);
            Assert.AreEqual("test", new BarrierPostPhaseException("test").Message);
            Assert.IsNotNull(new BarrierPostPhaseException(exc).Message);
            Assert.AreSame(exc, new BarrierPostPhaseException(exc).InnerException);
            Assert.AreEqual("test", new BarrierPostPhaseException("test", exc).Message);
            Assert.AreSame(exc, new BarrierPostPhaseException("test", exc).InnerException);
        }

        #region Helper Methods

        /// <summary>
        /// Ensures the post phase action throws if Dispose,SignalAndWait and Add/Remove participants called from it.
        /// </summary>
        private static void EnsurePostPhaseThrew(Barrier barrier)
        {
            BarrierPostPhaseException be = AssertExtensions.Throws<BarrierPostPhaseException>(() => barrier.SignalAndWait());
            AssertExtensions.IsType<InvalidOperationException>(be.InnerException);
        }

        #endregion
    }
}
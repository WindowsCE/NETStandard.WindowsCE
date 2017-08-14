#if !NET35_CF
using Mock.System;
#endif

namespace System.Threading.Tasks
{
    /// <summary>
    /// Provides support for creating and scheduling <see cref="Task"/> objects.
    /// </summary>
    public sealed class TaskFactory
    {
        /// <summary>
        /// Initializes a <see cref="TaskFactory"/> instance with the default configuration.
        /// </summary>
        public TaskFactory() { }

        #region StartNew

        /// <summary>
        /// Creates and starts a task.
        /// </summary>
        /// <param name="action">The action delegate to execute asynchronously.</param>
        /// <returns>The started task.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="action"/> argument is null.</exception>
        public Task StartNew(Action action)
            => StartNew(action, default(CancellationToken));

        public Task StartNew(Action action, CancellationToken cancellationToken)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Task task;
            if (cancellationToken.CanBeCanceled)
                task = new Task(action, cancellationToken);
            else
                task = new Task(action);

            task.Start();
            return task;
        }

        /// <summary>
        /// Creates and starts a <see cref="Task"/>.
        /// </summary>
        /// <param name="action">The action delegate to execute asynchronously.</param>
        /// <param name="state">An object containing data to be used by the <paramref name="action"/> delegate.</param>
        /// <returns>The started <see cref="Task"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="action"/> argument is null.</exception>
        public Task StartNew(Action<object> action, object state)
            => StartNew(action, state, default(CancellationToken));

        public Task StartNew(Action<object> action, object state, CancellationToken cancellationToken)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Task task;
            if (cancellationToken.CanBeCanceled)
                task = new Task(action, state, cancellationToken);
            else
                task = new Task(action, state);

            task.Start();
            return task;
        }

        /// <summary>
        /// Creates and starts a <see cref="Task{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result available through the <see cref="Task{TResult}"/>.</typeparam>
        /// <param name="function">
        /// A function delegate that returns the future result to be available
        /// through the <see cref="Task{TResult}"/>.
        /// </param>
        /// <returns>The started <see cref="Task{TResult}"/>.</returns>
        /// <exception cref="ArgumentNullException">The exception that is thrown when the <paramref name="function"/> argument is null.</exception>
        public Task<TResult> StartNew<TResult>(Func<TResult> function)
        {
            if (function == null)
                throw new ArgumentNullException("function");

            var task = new Task<TResult>(function);
            task.Start();
            return task;
        }

        public Task<TResult> StartNew<TResult>(Func<TResult> function, CancellationToken cancellationToken)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            Task<TResult> task;
            if (cancellationToken.CanBeCanceled)
                task = new Task<TResult>(function, cancellationToken);
            else
                task = new Task<TResult>(function);

            task.Start();
            return task;
        }

        /// <summary>
        /// Creates and starts a <see cref="Task{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the result available through the <see cref="Task{TResult}"/>.
        /// </typeparam>
        /// <param name="function">
        /// A function delegate that returns the future result to be available
        /// through the <see cref="Task{TResult}"/>.
        /// </param>
        /// <param name="state">
        /// An object containing data to be used by the <paramref name="function"/> delegate.
        /// </param>
        /// <returns>The started <see cref="Task{TResult}"/>.</returns>
        /// <exception cref="ArgumentNullException">The exception that is thrown when the <paramref name="function"/> argument is null.</exception>
        public Task<TResult> StartNew<TResult>(Func<object, TResult> function, object state)
            => StartNew(function, state, default(CancellationToken));

        public Task<TResult> StartNew<TResult>(Func<object, TResult> function, object state, CancellationToken cancellationToken)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            Task<TResult> task;
            if (cancellationToken.CanBeCanceled)
                task = new Task<TResult>(function, state, cancellationToken);
            else
                task = new Task<TResult>(function, state);

            task.Start();
            return task;
        }

        #endregion

        #region FromAsync Task

        public Task FromAsync(
            Func<AsyncCallback, object, IAsyncResult> beginMethod,
            Action<IAsyncResult> endMethod,
            object state
            )
        {
            if (beginMethod == null)
                throw new ArgumentNullException(nameof(beginMethod));
            if (endMethod == null)
                throw new ArgumentNullException(nameof(endMethod));

            Task wrapper = new Task(state);
            beginMethod(CreateBeginCallback(wrapper, endMethod), state);

            return wrapper;
        }

        public Task FromAsync(
            IAsyncResult asyncResult,
            Action<IAsyncResult> endMethod
            )
        {
            if (asyncResult == null)
                throw new ArgumentNullException(nameof(asyncResult));
            if (endMethod == null)
                throw new ArgumentNullException(nameof(endMethod));

            return StartNew(() =>
            {
                if (!asyncResult.AsyncWaitHandle.WaitOne())
                    throw new InvalidOperationException("Could not await for signal");

                endMethod(asyncResult);
            });
        }

        public Task FromAsync<TArg1>(
            Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod,
            Action<IAsyncResult> endMethod,
            TArg1 arg1,
            object state
            )
        {
            if (beginMethod == null)
                throw new ArgumentNullException(nameof(beginMethod));
            if (endMethod == null)
                throw new ArgumentNullException(nameof(endMethod));

            Task wrapper = new Task(state);
            beginMethod(arg1, CreateBeginCallback(wrapper, endMethod), state);

            return wrapper;
        }

        public Task FromAsync<TArg1, TArg2>(
            Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod,
            Action<IAsyncResult> endMethod,
            TArg1 arg1,
            TArg2 arg2,
            object state
            )
        {
            if (beginMethod == null)
                throw new ArgumentNullException(nameof(beginMethod));
            if (endMethod == null)
                throw new ArgumentNullException(nameof(endMethod));

            Task wrapper = new Task(state);
            beginMethod(arg1, arg2, CreateBeginCallback(wrapper, endMethod), state);

            return wrapper;
        }

        public Task FromAsync<TArg1, TArg2, TArg3>(
            Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod,
            Action<IAsyncResult> endMethod,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3,
            object state
            )
        {
            if (beginMethod == null)
                throw new ArgumentNullException(nameof(beginMethod));
            if (endMethod == null)
                throw new ArgumentNullException(nameof(endMethod));

            Task wrapper = new Task(state);
            beginMethod(arg1, arg2, arg3, CreateBeginCallback(wrapper, endMethod), state);

            return wrapper;
        }

        private AsyncCallback CreateBeginCallback(Task wrapper, Action<IAsyncResult> endMethod)
        {
            return ar =>
            {
                bool ok;
                try
                {
                    endMethod(ar);
                    ok = wrapper.TrySetCompleted();
                }
                catch (Exception ex)
                {
                    ok = wrapper.TrySetException(ex);
                }

                if (!ok)
                {
                    throw new InvalidOperationException(
                        "An attempt was made to transition a task to a final " +
                        "state when it had already completed");
                }
            };
        }

        #endregion

        #region FromAsync Task<TResult>

        public Task<TResult> FromAsync<TResult>(
            Func<AsyncCallback, object, IAsyncResult> beginMethod,
            Func<IAsyncResult, TResult> endMethod,
            object state
            )
        {
            if (beginMethod == null)
                throw new ArgumentNullException(nameof(beginMethod));
            if (endMethod == null)
                throw new ArgumentNullException(nameof(endMethod));

            Task<TResult> wrapper = new Task<TResult>(state);
            beginMethod(CreateBeginCallback(wrapper, endMethod), state);

            return wrapper;
        }

        public Task<TResult> FromAsync<TResult>(
            IAsyncResult asyncResult,
            Func<IAsyncResult, TResult> endMethod
            )
        {
            if (asyncResult == null)
                throw new ArgumentNullException(nameof(asyncResult));
            if (endMethod == null)
                throw new ArgumentNullException(nameof(endMethod));

            return StartNew(() =>
            {
                if (!asyncResult.AsyncWaitHandle.WaitOne())
                    throw new InvalidOperationException("Could not await for signal");

                return endMethod(asyncResult);
            });
        }

        public Task<TResult> FromAsync<TArg1, TResult>(
            Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod,
            Func<IAsyncResult, TResult> endMethod,
            TArg1 arg1,
            object state
            )
        {
            if (beginMethod == null)
                throw new ArgumentNullException(nameof(beginMethod));
            if (endMethod == null)
                throw new ArgumentNullException(nameof(endMethod));

            Task<TResult> wrapper = new Task<TResult>(state);
            beginMethod(arg1, CreateBeginCallback(wrapper, endMethod), state);

            return wrapper;
        }

        public Task<TResult> FromAsync<TArg1, TArg2, TResult>(
            Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod,
            Func<IAsyncResult, TResult> endMethod,
            TArg1 arg1,
            TArg2 arg2,
            object state
            )
        {
            if (beginMethod == null)
                throw new ArgumentNullException(nameof(beginMethod));
            if (endMethod == null)
                throw new ArgumentNullException(nameof(endMethod));

            Task<TResult> wrapper = new Task<TResult>(state);
            beginMethod(arg1, arg2, CreateBeginCallback(wrapper, endMethod), state);

            return wrapper;
        }

        public Task<TResult> FromAsync<TArg1, TArg2, TArg3, TResult>(
            Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod,
            Func<IAsyncResult, TResult> endMethod,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3,
            object state
            )
        {
            if (beginMethod == null)
                throw new ArgumentNullException(nameof(beginMethod));
            if (endMethod == null)
                throw new ArgumentNullException(nameof(endMethod));

            Task<TResult> wrapper = new Task<TResult>(state);
            beginMethod(arg1, arg2, arg3, CreateBeginCallback(wrapper, endMethod), state);

            return wrapper;
        }

        private AsyncCallback CreateBeginCallback<TResult>(Task<TResult> wrapper, Func<IAsyncResult, TResult> endMethod)
        {
            return ar =>
            {
                bool ok;
                try
                {
                    var result = endMethod(ar);
                    ok = wrapper.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    ok = wrapper.TrySetException(ex);
                }

                if (!ok)
                {
                    throw new InvalidOperationException(
                        "An attempt was made to transition a task to a final " +
                        "state when it had already completed");
                }
            };
        }

        #endregion
    }
}

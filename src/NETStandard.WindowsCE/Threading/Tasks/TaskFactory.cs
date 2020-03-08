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

        public TaskFactory(TaskScheduler scheduler) { }

        #region StartNew Task

        /// <summary>
        /// Creates and starts a task.
        /// </summary>
        /// <param name="action">The action delegate to execute asynchronously.</param>
        /// <returns>The started task.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="action"/> argument is null.</exception>
        public Task StartNew(Action action)
            => StartNew(action, default, TaskCreationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task">Task</see>.
        /// </summary>
        /// <param name="action">The action delegate to execute asynchronously.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task">Task</see>.</returns>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref name="action"/> 
        /// argument is null.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task StartNew(Action action, CancellationToken cancellationToken)
            => StartNew(action, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task">Task</see>.
        /// </summary>
        /// <param name="action">The action delegate to execute asynchronously.</param>
        /// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the
        /// created
        /// <see cref="T:System.Threading.Tasks.Task">Task.</see></param>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task">Task</see>.</returns>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="action"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument specifies an invalid TaskCreationOptions
        /// value.</exception>
        public Task StartNew(Action action, TaskCreationOptions creationOptions)
            => StartNew(action, default, creationOptions, TaskScheduler.Default);

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task">Task</see>.
        /// </summary>
        /// <param name="action">The action delegate to execute asynchronously.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new <see cref="Task"/></param>
        /// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the
        /// created
        /// <see cref="T:System.Threading.Tasks.Task">Task.</see></param>
        /// <param name="scheduler">The <see
        /// cref="T:System.Threading.Tasks.TaskScheduler">TaskScheduler</see>
        /// that is used to schedule the created <see
        /// cref="T:System.Threading.Tasks.Task">Task</see>.</param>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task">Task</see>.</returns>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="action"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="scheduler"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument specifies an invalid TaskCreationOptions
        /// value.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task StartNew(
            Action action,
            CancellationToken cancellationToken,
            TaskCreationOptions creationOptions,
            TaskScheduler scheduler)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var task = new Task(action, cancellationToken, creationOptions);
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
            => StartNew(action, state, default, TaskCreationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task">Task</see>.
        /// </summary>
        /// <param name="action">The action delegate to execute asynchronously.</param>
        /// <param name="state">An object containing data to be used by the <paramref name="action"/>
        /// delegate.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new <see cref="Task"/></param>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task">Task</see>.</returns>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="action"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task StartNew(Action<object> action, object state, CancellationToken cancellationToken)
            => StartNew(action, state, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task">Task</see>.
        /// </summary>
        /// <param name="action">The action delegate to execute asynchronously.</param>
        /// <param name="state">An object containing data to be used by the <paramref name="action"/>
        /// delegate.</param>
        /// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the
        /// created
        /// <see cref="T:System.Threading.Tasks.Task">Task.</see></param>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task">Task</see>.</returns>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="action"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument specifies an invalid TaskCreationOptions
        /// value.</exception>
        public Task StartNew(Action<Object> action, Object state, TaskCreationOptions creationOptions)
            => StartNew(action, state, default, creationOptions, TaskScheduler.Default);

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task">Task</see>.
        /// </summary>
        /// <param name="action">The action delegate to execute asynchronously.</param>
        /// <param name="state">An object containing data to be used by the <paramref name="action"/>
        /// delegate.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the
        /// created
        /// <see cref="T:System.Threading.Tasks.Task">Task.</see></param>
        /// <param name="scheduler">The <see
        /// cref="T:System.Threading.Tasks.TaskScheduler">TaskScheduler</see>
        /// that is used to schedule the created <see
        /// cref="T:System.Threading.Tasks.Task">Task</see>.</param>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task">Task</see>.</returns>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="action"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="scheduler"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument specifies an invalid TaskCreationOptions
        /// value.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task StartNew(
            Action<Object> action,
            Object state,
            CancellationToken cancellationToken,
            TaskCreationOptions creationOptions,
            TaskScheduler scheduler)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var task = new Task(action, state, cancellationToken, creationOptions);
            task.Start();
            return task;
        }

        #endregion

        #region StartNew Task<TResult>

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
        public Task<TResult> StartNew<TResult>(Func2<TResult> function)
            => StartNew(function, default, TaskCreationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result available through the
        /// <see cref="T:System.Threading.Tasks.Task{TResult}">Task</see>.
        /// </typeparam>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new <see cref="Task"/></param>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task<TResult> StartNew<TResult>(Func2<TResult> function, CancellationToken cancellationToken)
            => StartNew(function, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result available through the
        /// <see cref="T:System.Threading.Tasks.Task{TResult}">Task</see>.
        /// </typeparam>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the
        /// created
        /// <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument specifies an invalid TaskCreationOptions
        /// value.</exception>
        public Task<TResult> StartNew<TResult>(Func2<TResult> function, TaskCreationOptions creationOptions)
            => StartNew(function, default, creationOptions, TaskScheduler.Default);

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result available through the
        /// <see cref="T:System.Threading.Tasks.Task{TResult}">Task</see>.
        /// </typeparam>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the
        /// created
        /// <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="scheduler">The <see
        /// cref="T:System.Threading.Tasks.TaskScheduler">TaskScheduler</see>
        /// that is used to schedule the created <see cref="T:System.Threading.Tasks.Task{TResult}">
        /// Task{TResult}</see>.</param>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="scheduler"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument specifies an invalid TaskCreationOptions
        /// value.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task<TResult> StartNew<TResult>(
            Func2<TResult> function,
            CancellationToken cancellationToken,
            TaskCreationOptions creationOptions,
            TaskScheduler scheduler)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            var task = new Task<TResult>(function, cancellationToken, creationOptions);
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
        public Task<TResult> StartNew<TResult>(Func2<object, TResult> function, object state)
            => StartNew(function, state, default, TaskCreationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result available through the
        /// <see cref="T:System.Threading.Tasks.Task{TResult}">Task</see>.
        /// </typeparam>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="state">An object containing data to be used by the <paramref name="function"/>
        /// delegate.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new <see cref="Task"/></param>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task<TResult> StartNew<TResult>(Func2<object, TResult> function, object state, CancellationToken cancellationToken)
            => StartNew(function, state, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result available through the
        /// <see cref="T:System.Threading.Tasks.Task{TResult}">Task</see>.
        /// </typeparam>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="state">An object containing data to be used by the <paramref name="function"/>
        /// delegate.</param>
        /// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the
        /// created
        /// <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument specifies an invalid TaskCreationOptions
        /// value.</exception>
        public Task<TResult> StartNew<TResult>(Func2<Object, TResult> function, Object state, TaskCreationOptions creationOptions)
            => StartNew(function, state, default, creationOptions, TaskScheduler.Default);

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result available through the
        /// <see cref="T:System.Threading.Tasks.Task{TResult}">Task</see>.
        /// </typeparam>
        /// <param name="function">A function delegate that returns the future result to be available through
        /// the <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="state">An object containing data to be used by the <paramref name="function"/>
        /// delegate.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <param name="creationOptions">A TaskCreationOptions value that controls the behavior of the
        /// created
        /// <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <param name="scheduler">The <see
        /// cref="T:System.Threading.Tasks.TaskScheduler">TaskScheduler</see>
        /// that is used to schedule the created <see cref="T:System.Threading.Tasks.Task{TResult}">
        /// Task{TResult}</see>.</param>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task{TResult}"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="function"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">The exception that is thrown when the <paramref
        /// name="scheduler"/>
        /// argument is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when the
        /// <paramref name="creationOptions"/> argument specifies an invalid TaskCreationOptions
        /// value.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task<TResult> StartNew<TResult>(
            Func2<Object, TResult> function,
            Object state,
            CancellationToken cancellationToken,
            TaskCreationOptions creationOptions,
            TaskScheduler scheduler)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            var task = new Task<TResult>(function, state, cancellationToken, creationOptions);
            task.Start();
            return task;
        }

        #endregion

        #region FromAsync Task

        public Task FromAsync(
            Func2<AsyncCallback, object, IAsyncResult> beginMethod,
            Action2<IAsyncResult> endMethod,
            object state)
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
            Action2<IAsyncResult> endMethod)
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
            Func2<TArg1, AsyncCallback, object, IAsyncResult> beginMethod,
            Action2<IAsyncResult> endMethod,
            TArg1 arg1,
            object state)
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
            Func2<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod,
            Action2<IAsyncResult> endMethod,
            TArg1 arg1,
            TArg2 arg2,
            object state)
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
            Action2<IAsyncResult> endMethod,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3,
            object state)
        {
            if (beginMethod == null)
                throw new ArgumentNullException(nameof(beginMethod));
            if (endMethod == null)
                throw new ArgumentNullException(nameof(endMethod));

            Task wrapper = new Task(state);
            beginMethod(arg1, arg2, arg3, CreateBeginCallback(wrapper, endMethod), state);

            return wrapper;
        }

        private AsyncCallback CreateBeginCallback(Task wrapper, Action2<IAsyncResult> endMethod)
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
            Func2<AsyncCallback, object, IAsyncResult> beginMethod,
            Func2<IAsyncResult, TResult> endMethod,
            object state)
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
            Func2<IAsyncResult, TResult> endMethod)
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
            Func2<TArg1, AsyncCallback, object, IAsyncResult> beginMethod,
            Func2<IAsyncResult, TResult> endMethod,
            TArg1 arg1,
            object state)
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
            Func2<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod,
            Func2<IAsyncResult, TResult> endMethod,
            TArg1 arg1,
            TArg2 arg2,
            object state)
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
            Func2<IAsyncResult, TResult> endMethod,
            TArg1 arg1,
            TArg2 arg2,
            TArg3 arg3,
            object state)
        {
            if (beginMethod == null)
                throw new ArgumentNullException(nameof(beginMethod));
            if (endMethod == null)
                throw new ArgumentNullException(nameof(endMethod));

            Task<TResult> wrapper = new Task<TResult>(state);
            beginMethod(arg1, arg2, arg3, CreateBeginCallback(wrapper, endMethod), state);

            return wrapper;
        }

        private AsyncCallback CreateBeginCallback<TResult>(Task<TResult> wrapper, Func2<IAsyncResult, TResult> endMethod)
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

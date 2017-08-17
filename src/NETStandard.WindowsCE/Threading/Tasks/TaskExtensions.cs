// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if NET35_CF
using InternalOCE = System.OperationCanceledException;
#else
using InternalOCE = Mock.System.OperationCanceledException;
#endif

namespace System.Threading.Tasks
{
    /// <summary>Provides a set of static methods for working with specific kinds of <see cref="Task"/> instances.</summary>
    public static class TaskExtensions
    {
        /// <summary>Creates a proxy <see cref="Task"/> that represents the asynchronous operation of a <see cref="Task{Task}"/>.</summary>
        /// <param name="task">The <see cref="Task{Task}"/> to unwrap.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation of the provided <see cref="Task{Task}"/>.</returns>
        public static Task Unwrap(this Task<Task> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            // If the task hasn't completed or was faulted/canceled, wrap it in an unwrap promise. Otherwise,
            // it completed successfully.  Return its inner task to avoid unnecessary wrapping, or if the inner
            // task is null, return a canceled task to match the same semantics as CreateUnwrapPromise.
            if (task.IsCompletedSuccessfully)
                return task.Result ?? Task.FromCanceled(new CancellationToken(true));
            else
                return CreateUnwrapPromise(task);
        }

        /// <summary>
        /// Creates a proxy <see cref="Task{TResult}"/> that represents the
        /// asynchronous operation of a <see cref="Task{TResult}"/> inside
        /// another <see cref="Task{TResult}"/>.
        /// </summary>
        /// <param name="task">The <see cref="Task{TResult}"/> inside another <see cref="Task{TResult}"/> to unwrap.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that represents the asynchronous
        /// operation of the provided <see cref="Task{TResult}"/> inside
        /// another <see cref="Task{TResult}"/>.
        /// </returns>
        public static Task<TResult> Unwrap<TResult>(this Task<Task<TResult>> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            // If the task hasn't completed or was faulted/canceled, wrap it in an unwrap promise. Otherwise,
            // it completed successfully.  Return its inner task to avoid unnecessary wrapping, or if the inner
            // task is null, return a canceled task to match the same semantics as CreateUnwrapPromise.
            if (task.IsCompletedSuccessfully)
                return task.Result ?? Task.FromCanceled<TResult>(new CancellationToken(true));
            else
                return CreateUnwrapPromise(task);
        }

        private static async Task CreateUnwrapPromise(Task<Task> task)
        {
            var innerTask = await task.ConfigureAwait(false);
            if (innerTask == null)
                throw new InternalOCE(new CancellationToken(true));

            await innerTask.ConfigureAwait(false);
        }

        private static async Task<TResult> CreateUnwrapPromise<TResult>(Task<Task<TResult>> task)
        {
            var innerTask = await task.ConfigureAwait(false);
            if (innerTask == null)
                throw new InternalOCE(new CancellationToken(true));

            return await innerTask.ConfigureAwait(false);
        }
    }
}
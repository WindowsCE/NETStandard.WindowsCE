using System.Diagnostics;

#if NET35_CF
using InternalOCE = System.OperationCanceledException;
#else
using InternalOCE = Mock.System.OperationCanceledException;
#endif

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents an asynchronous operation that produces a result at some time in the future.
    /// </summary>
    /// <typeparam name="TResult">
    /// The type of the result produced by this <see cref="Task{TResult}"/>.
    /// </typeparam>
    [DebuggerDisplay("Id = {Id}, Status = {Status}, Method = {DebuggerDisplayMethodDescription}, Result = {DebuggerDisplayResultDescription}")]
    public class Task<TResult> : Task
    {
        private TResult _result;

        #region Property

        /// <summary>
        /// Gets the result value of this <see cref="Task{TResult}"/>.
        /// </summary>
        /// <remarks>
        /// The get accessor for this property ensures that the asynchronous operation is complete before
        /// returning. Once the result of the computation is available, it is stored and will be returned
        /// immediately on later calls to <see cref="Result"/>.
        /// </remarks>
        /// <exception cref="AggregateException">An exception was thrown during the execution of the <see cref="Task{TResult}"/>.</exception>
        public TResult Result
        {
            get
            {
                Wait();
                return _result;
            }
        }

        // Debugger support
        private string DebuggerDisplayResultDescription
            => IsCompletedSuccessfully ? "" + _result : SR.TaskT_DebuggerNoResult;

        // Debugger support
        private string DebuggerDisplayMethodDescription
            => m_contingentProperties?.m_action?.Method.ToString() ?? "{null}";

        #endregion

        #region Constructors

        /// <summary>
        /// Internal constructor to create an empty task.
        /// </summary>
        internal Task()
            : base()
        { }

        /// <summary>
        /// Internal constructor to create an empty task.
        /// </summary>
        /// <param name="state">An object representing data to be used by the action.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> that will be assigned to the new task.</param>
        internal Task(object state, CancellationToken cancellationToken = default)
            : base(state, cancellationToken)
        { }

        /// <summary>
        /// Internal constructor to create an already-completed task.
        /// </summary>
        internal Task(TResult result, Exception ex, CancellationToken cancellationToken = default(CancellationToken))
            : base(ex, cancellationToken)
        {
            _result = result;
        }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified function.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        public Task(Func2<TResult> function)
            : this(function, null, default(CancellationToken), TaskCreationOptions.None, null)
        { }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified function.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to be assigned to this task.</param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task(Func2<TResult> function, CancellationToken cancellationToken)
            : this(function, null, cancellationToken, TaskCreationOptions.None, null)
        { }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified function and creation options.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="creationOptions">
        /// The <see cref="System.Threading.Tasks.TaskCreationOptions">TaskCreationOptions</see> used to
        /// customize the task's behavior.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="creationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskCreationOptions"/>.
        /// </exception>
        public Task(Func2<TResult> function, TaskCreationOptions creationOptions)
            : this(function, null, default(CancellationToken), creationOptions, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified function and creation options.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <param name="creationOptions">
        /// The <see cref="System.Threading.Tasks.TaskCreationOptions">TaskCreationOptions</see> used to
        /// customize the task's behavior.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="creationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskCreationOptions"/>.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task(Func2<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
            : this(function, null, cancellationToken, creationOptions, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified function and state.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="state">An object representing data to be used by the action.</param>
        /// <exception cref="ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        public Task(Func2<object, TResult> function, object state)
            : base(function, state, default(CancellationToken), TaskCreationOptions.None, null)
        { }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified action, state, and options.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="state">An object representing data to be used by the function.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to be assigned to the new task.</param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task(Func2<object, TResult> function, object state, CancellationToken cancellationToken)
            : base(function, state, cancellationToken, TaskCreationOptions.None, null)
        { }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified action, state, and options.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="state">An object representing data to be used by the function.</param>
        /// <param name="creationOptions">
        /// The <see cref="System.Threading.Tasks.TaskCreationOptions">TaskCreationOptions</see> used to
        /// customize the task's behavior.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="creationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskCreationOptions"/>.
        /// </exception>
        public Task(Func2<object, TResult> function, object state, TaskCreationOptions creationOptions)
            : this(function, state, default(CancellationToken), creationOptions, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Task{TResult}"/> with the specified action, state, and options.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed,
        /// the task's <see cref="Result"/> property will be set to return the result value of the function.
        /// </param>
        /// <param name="state">An object representing data to be used by the function.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to be assigned to the new task.</param>
        /// <param name="creationOptions">
        /// The <see cref="System.Threading.Tasks.TaskCreationOptions">TaskCreationOptions</see> used to
        /// customize the task's behavior.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="function"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="creationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskCreationOptions"/>.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task(Func2<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
            : this(function, state, cancellationToken, creationOptions, null)
        {
        }

        /// <summary>
        /// Internal constructor to allow creation of continue tasks.
        /// </summary>
        internal Task(Delegate function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, Task continueSource)
            : base(function, state, cancellationToken, creationOptions, continueSource)
        { }

        #endregion

        #region Helper methods

        internal bool TrySetResult(TResult result)
        {
            return TrySetCompleted(() => _result = result);
        }

        #endregion

        #region Task thread execution

        /// <summary>
        /// Unbox task action and execute it.
        /// </summary>
        protected override void InnerInvoke()
        {
            var cp = m_contingentProperties;
            if (cp == null)
                throw new InvalidOperationException("Should not try to execute null actions");
            var uncastAction = cp.m_action;
            var parent = cp.m_parent;

            switch (uncastAction)
            {
                case Func2<TResult> func0:
                    _result = func0();
                    break;
                case Func2<object, TResult> funcObj:
                    _result = funcObj(m_stateObject);
                    break;
                case Func2<Task, TResult> funcTask:
                    _result = funcTask(parent);
                    break;
                case Func2<Task, object, TResult> funcTaskObj:
                    _result = funcTaskObj(parent, m_stateObject);
                    break;
                default:
                    throw new InvalidOperationException("Unexpected action type");
            }
        }

        #endregion

        #region Await support

        /// <summary>
        /// Creates an awaiter used to await this <see cref="Task{TResult}"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        public new Runtime.CompilerServices.TaskAwaiter<TResult> GetAwaiter()
        {
            return new Runtime.CompilerServices.TaskAwaiter<TResult>(this);
        }

        /// <summary>
        /// Configures an awaiter used to await this <see cref="Task"/>.
        /// </summary>
        /// <param name="continueOnCapturedContext">
        /// true to attempt to marshal the continuation back to the original
        /// context captured; otherwise, false.
        /// </param>
        /// <returns>A new awaiter instance.</returns>
        public new Runtime.CompilerServices.ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
        {
            return new Runtime.CompilerServices.ConfiguredTaskAwaitable<TResult>(
                this, continueOnCapturedContext);
        }

        #endregion

        #region Continuation Methods

        public Task ContinueWith(Action2<Task<TResult>, object> continuationAction, object state)
        {
            return ContinueWith(continuationAction, state, TaskScheduler.Current, default, TaskContinuationOptions.None);
        }

        public Task ContinueWith(Action2<Task<TResult>, object> continuationAction, object state, CancellationToken cancellationToken)
        {
            return ContinueWith(continuationAction, state, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None);
        }

        public Task ContinueWith(Action2<Task<TResult>, object> continuationAction, object state, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            return ContinueWith(continuationAction, state, scheduler, cancellationToken, continuationOptions);
        }

        public Task ContinueWith(Action2<Task<TResult>, object> continuationAction, object state, TaskContinuationOptions continuationOptions)
        {
            return ContinueWith(continuationAction, state, TaskScheduler.Current, default, continuationOptions);
        }

        public Task ContinueWith(Action2<Task<TResult>, object> continuationAction, object state, TaskScheduler scheduler)
        {
            return ContinueWith(continuationAction, state, scheduler, default, TaskContinuationOptions.None);
        }

        public Task ContinueWith(Action2<Task<TResult>> continuationAction)
        {
            return ContinueWith(continuationAction, TaskScheduler.Current, default, TaskContinuationOptions.None);
        }

        public Task ContinueWith(Action2<Task<TResult>> continuationAction, CancellationToken cancellationToken)
        {
            return ContinueWith(continuationAction, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None);
        }

        public Task ContinueWith(Action2<Task<TResult>> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            return ContinueWith(continuationAction, scheduler, cancellationToken, continuationOptions);
        }

        public Task ContinueWith(Action2<Task<TResult>> continuationAction, TaskContinuationOptions continuationOptions)
        {
            return ContinueWith(continuationAction, TaskScheduler.Current, default, continuationOptions);
        }

        public Task ContinueWith(Action2<Task<TResult>> continuationAction, TaskScheduler scheduler)
        {
            return ContinueWith(continuationAction, scheduler, default, TaskContinuationOptions.None);
        }

        public Task<TNewResult> ContinueWith<TNewResult>(Func2<Task<TResult>, object, TNewResult> continuationFunction, object state)
        {
            return ContinueWith<TNewResult>(continuationFunction, state, TaskScheduler.Current, default, TaskContinuationOptions.None);
        }

        public Task<TNewResult> ContinueWith<TNewResult>(Func2<Task<TResult>, object, TNewResult> continuationFunction, object state, CancellationToken cancellationToken)
        {
            return ContinueWith<TNewResult>(continuationFunction, state, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None);
        }

        public Task<TNewResult> ContinueWith<TNewResult>(Func2<Task<TResult>, object, TNewResult> continuationFunction, object state, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            return ContinueWith<TNewResult>(continuationFunction, state, scheduler, cancellationToken, continuationOptions);
        }

        public Task<TNewResult> ContinueWith<TNewResult>(Func2<Task<TResult>, object, TNewResult> continuationFunction, object state, TaskContinuationOptions continuationOptions)
        {
            return ContinueWith<TNewResult>(continuationFunction, state, TaskScheduler.Current, default, continuationOptions);
        }

        public Task<TNewResult> ContinueWith<TNewResult>(Func2<Task<TResult>, object, TNewResult> continuationFunction, object state, TaskScheduler scheduler)
        {
            return ContinueWith<TNewResult>(continuationFunction, state, scheduler, default, TaskContinuationOptions.None);
        }

        public Task<TNewResult> ContinueWith<TNewResult>(Func2<Task<TResult>, TNewResult> continuationFunction)
        {
            return ContinueWith<TNewResult>(continuationFunction, TaskScheduler.Current, default, TaskContinuationOptions.None);
        }

        public Task<TNewResult> ContinueWith<TNewResult>(Func2<Task<TResult>, TNewResult> continuationFunction, CancellationToken cancellationToken)
        {
            return ContinueWith<TNewResult>(continuationFunction, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None);
        }

        public Task<TNewResult> ContinueWith<TNewResult>(Func2<Task<TResult>, TNewResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            return ContinueWith<TNewResult>(continuationFunction, scheduler, cancellationToken, continuationOptions);
        }

        public Task<TNewResult> ContinueWith<TNewResult>(Func2<Task<TResult>, TNewResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            return ContinueWith<TNewResult>(continuationFunction, TaskScheduler.Current, default, continuationOptions);
        }

        public Task<TNewResult> ContinueWith<TNewResult>(Func2<Task<TResult>, TNewResult> continuationFunction, TaskScheduler scheduler)
        {
            return ContinueWith<TNewResult>(continuationFunction, scheduler, default, TaskContinuationOptions.None);
        }

        internal Task ContinueWith(
            Action2<Task<TResult>> continuationAction,
            TaskScheduler scheduler,
            CancellationToken cancellationToken,
            TaskContinuationOptions continuationOptions)
        {
            if (continuationAction == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationAction);
            }

            CreationOptionsFromContinuationOptions(
                continuationOptions,
                out TaskCreationOptions creationOptions,
                out InternalTaskOptions _);

            void ActionWrapper(Task task) => continuationAction((Task<TResult>)task);
            return new Task((Action2<Task>)ActionWrapper, null, cancellationToken, creationOptions, this);
        }

        internal Task ContinueWith(
            Action2<Task<TResult>, object> continuationAction,
            object state,
            TaskScheduler scheduler,
            CancellationToken cancellationToken,
            TaskContinuationOptions continuationOptions)
        {
            if (continuationAction == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationAction);
            }

            CreationOptionsFromContinuationOptions(
                continuationOptions,
                out TaskCreationOptions creationOptions,
                out InternalTaskOptions _);

            void ActionWrapper(Task task, object s) => continuationAction((Task<TResult>)task, s);
            return new Task((Action2<Task, object>)ActionWrapper, state, cancellationToken, creationOptions, this);
        }

        internal Task<TNewResult> ContinueWith<TNewResult>(
            Func2<Task<TResult>, TNewResult> continuationFunction,
            TaskScheduler scheduler,
            CancellationToken cancellationToken,
            TaskContinuationOptions continuationOptions)
        {
            if (continuationFunction == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);
            }

            CreationOptionsFromContinuationOptions(
                continuationOptions,
                out TaskCreationOptions creationOptions,
                out InternalTaskOptions _);

            TNewResult FunctionWrapper(Task task) => continuationFunction((Task<TResult>)task);
            return new Task<TNewResult>((Func2<Task, TNewResult>)FunctionWrapper, null, cancellationToken, creationOptions, this);
        }

        internal Task<TNewResult> ContinueWith<TNewResult>(
            Func2<Task<TResult>, object, TNewResult> continuationFunction,
            object state,
            TaskScheduler scheduler,
            CancellationToken cancellationToken,
            TaskContinuationOptions continuationOptions)
        {
            if (continuationFunction == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);
            }

            CreationOptionsFromContinuationOptions(
                continuationOptions,
                out TaskCreationOptions creationOptions,
                out InternalTaskOptions _);

            TNewResult FunctionWrapper(Task task, object o) => continuationFunction((Task<TResult>)task, o);
            return new Task<TNewResult>((Func2<Task, object, TNewResult>)FunctionWrapper, state, cancellationToken, creationOptions, this);
        }

        #endregion
    }
}

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
        internal Task(object state)
            : base(state)
        { }

        /// <summary>
        /// Internal constructor to create an already-completed task.
        /// </summary>
        internal Task(TResult result, Exception ex)
            : base(ex)
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
        public Task(Func<TResult> function)
            : base(function, null, default(CancellationToken), null)
        { }

        public Task(Func<TResult> function, CancellationToken cancellationToken)
            : base(function, null, cancellationToken, null)
        { }

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
        public Task(Func<object, TResult> function, object state)
            : base(function, state, default(CancellationToken), null)
        { }

        public Task(Func<object, TResult> function, object state, CancellationToken cancellationToken)
            : base(function, state, cancellationToken, null)
        { }

        /// <summary>
        /// Internal constructor to allow creation of continue tasks.
        /// </summary>
        internal Task(Delegate function, object state, CancellationToken cancellationToken, Task continueSource)
            : base(function, state, cancellationToken, continueSource)
        { }

        #endregion

        #region Helper methods

        internal bool TrySetResult(TResult result)
        {
            _result = result;
            return TrySetCompleted();
        }

        #endregion

        #region Task thread execution

        /// <summary>
        /// Unbox task action and execute it.
        /// </summary>
        protected override void ExecuteTaskAction()
        {
            var cp = m_contingentProperties;
            if (cp == null)
                throw new InvalidOperationException("Should not try to execute null actions");
            var uncastAction = cp.m_action;
            var parent = cp.m_parent;

            if (uncastAction is Func<TResult>)
            {
                var userWork = (Func<TResult>)uncastAction;
                _result = userWork();
            }
            else if (uncastAction is Func<object, TResult>)
            {
                var userWork = (Func<object, TResult>)uncastAction;
                _result = userWork(m_stateObject);
            }
            else if (uncastAction is Func<Task, TResult>)
            {
                Func<Task, TResult> userWork = (Func<Task, TResult>)uncastAction;
                _result = userWork(parent);
            }
            else if (uncastAction is Func<Task, object, TResult>)
            {
                Func<Task, object, TResult> userWork = (Func<Task, object, TResult>)uncastAction;
                _result = userWork(parent, m_stateObject);
            }
            else
            {
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
    }
}

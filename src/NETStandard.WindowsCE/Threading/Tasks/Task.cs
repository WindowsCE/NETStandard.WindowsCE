using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#if NET35_CF
using System.Runtime.ExceptionServices;
using InternalOCE = System.OperationCanceledException;
#else
using Mock.System.Runtime.ExceptionServices;
using InternalOCE = Mock.System.OperationCanceledException;
#endif

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents an asynchronous operation.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, Status = {Status}, Method = {DebuggerDisplayMethodDescription}")]
    public class Task : IAsyncResult, IDisposable
    {
        private static Task _completedTask; // A task that's already been completed successfully.
        private static readonly TaskFactory _defaultFactory = new TaskFactory();
        private static int _taskIdCounter;  // static counter used to generate unique task IDs

        /// <summary>
        /// A state object that can be optionally supplied, passed to action.
        /// </summary>
        protected readonly object m_stateObject;

        /// <summary>
        /// A set of exceptions occurred when trying to execute current task.
        /// </summary>
        protected readonly List<Exception> m_exceptions = new List<Exception>();

        protected CancellationToken m_cancellationToken;
        protected ContingentProperties m_contingentProperties;
        // this task's unique ID. initialized only if it is ever requested
        private int _taskId;
        // Unified flags for Task
        private protected volatile int _stateFlags;

        private Action<object> _completedCallback;
        private readonly object _lockObj = new object();

        // State constants for _stateFlags;
        // The bits of _stateFlags are allocated as follows:
        //   0x40000000 - TaskBase state flag
        //   0x3FFF0000 - Task state flags
        //   0x0000FF00 - internal TaskCreationOptions flags
        //   0x000000FF - publicly exposed TaskCreationOptions flags
        //
        // See TaskCreationOptions for bit values associated with TaskCreationOptions
        //
        private const int OptionsMask = 0xFFFF; // signifies the Options portion of _stateFlags  bin: 0000 0000 0000 0000 1111 1111 1111 1111
        internal const int TASK_STATE_STARTED = 0x10000;                                       //bin: 0000 0000 0000 0001 0000 0000 0000 0000
        internal const int TASK_STATE_DELEGATE_INVOKED = 0x20000;                              //bin: 0000 0000 0000 0010 0000 0000 0000 0000
        internal const int TASK_STATE_DISPOSED = 0x40000;                                      //bin: 0000 0000 0000 0100 0000 0000 0000 0000
        internal const int TASK_STATE_FAULTED = 0x200000;                                      //bin: 0000 0000 0010 0000 0000 0000 0000 0000
        internal const int TASK_STATE_CANCELED = 0x400000;                                     //bin: 0000 0000 0100 0000 0000 0000 0000 0000
        internal const int TASK_STATE_RAN_TO_COMPLETION = 0x1000000;                           //bin: 0000 0001 0000 0000 0000 0000 0000 0000
        internal const int TASK_STATE_WAITINGFORACTIVATION = 0x2000000;                        //bin: 0000 0010 0000 0000 0000 0000 0000 0000

        // A mask for all of the final states a task may be in
        private const int TASK_STATE_COMPLETED_MASK = TASK_STATE_CANCELED | TASK_STATE_FAULTED | TASK_STATE_RAN_TO_COMPLETION;

        #region Properties

        internal CancellationToken CancellationToken
            => m_cancellationToken;

        private ManualResetEventSlim CompletedEvent
        {
            get
            {
                var contingentProperties = EnsureContigentPropertiesInitialized();
                if (contingentProperties.m_taskCompletedEvent == null)
                {
                    bool isCompleted = IsCompleted;
                    ManualResetEventSlim mre = new ManualResetEventSlim(isCompleted);
                    if (Interlocked.CompareExchange(ref contingentProperties.m_taskCompletedEvent, mre, null) != null)
                        mre.Dispose();
                    else if (!isCompleted && IsCompleted)
                        mre.Set();
                }

                return contingentProperties.m_taskCompletedEvent;
            }
        }

        /// <summary>
        /// Gets a task that's already been completed successfully.
        /// </summary>
        /// <remarks>
        /// May not always return the same instance.
        /// </remarks>
        public static Task CompletedTask
        {
            get
            {
                var completedTask = _completedTask;
                if (completedTask == null)
                    _completedTask = completedTask = new Task((Exception)null); // lazy initialization
                return completedTask;
            }
        }

        /// <summary>
        /// Gets the <see cref="T:System.AggregateException">Exception</see> that caused the <see
        /// cref="Task">Task</see> to end prematurely. If the <see
        /// cref="Task">Task</see> completed successfully or has not yet thrown any
        /// exceptions, this will return null.
        /// </summary>
        /// <remarks>
        /// Tasks that throw unhandled exceptions store the resulting exception and propagate it wrapped in a
        /// <see cref="System.AggregateException"/> in calls to <see cref="Wait()">Wait</see>
        /// or in accesses to the <see cref="Exception"/> property.  Any exceptions not observed by the time
        /// the Task instance is garbage collected will be propagated on the finalizer thread.
        /// </remarks>
        public AggregateException Exception
        {
            get
            {
                if (!IsFaulted)
                    return null;

                return new AggregateException(m_exceptions);
            }
        }

        /// <summary>
        /// Gets a unique ID for this <see cref="Task">Task</see> instance.
        /// </summary>
        /// <remarks>
        /// Task IDs are assigned on-demand and do not necessarily represent the order in the which Task
        /// instances were created.
        /// </remarks>
        public int Id
        {
            get
            {
                if (_taskId == 0)
                {
                    int newId = NewId();
                    Interlocked.CompareExchange(ref _taskId, newId, 0);
                }

                return _taskId;
            }
        }

        public bool IsCanceled
            => (_stateFlags & TASK_STATE_COMPLETED_MASK) == TASK_STATE_CANCELED;

        /// <summary>
        /// Gets whether this <see cref="Task">Task</see> has completed.
        /// </summary>
        /// <remarks>
        /// <see cref="IsCompleted"/> will return true when the Task is in one of the three
        /// final states: <see cref="TaskStatus.RanToCompletion">RanToCompletion</see>,
        /// <see cref="TaskStatus.Faulted">Faulted</see>, or
        /// <see cref="TaskStatus.Canceled">Canceled</see>.
        /// </remarks>
        public bool IsCompleted
        {
            get
            {
                // enable in-lining of IsCompletedMethod by "cast"ing away the volatility
                int stateFlags = _stateFlags;
                return IsCompletedMethod(stateFlags);
            }
        }

        public bool IsCompletedSuccessfully
        {
            get { return (_stateFlags & TASK_STATE_COMPLETED_MASK) == TASK_STATE_RAN_TO_COMPLETION; }
        }

        /// <summary>
        /// Gets the <see cref="TaskCreationOptions"/> used
        /// to create this task.
        /// </summary>
        public TaskCreationOptions CreationOptions
        {
            get { return Options & (TaskCreationOptions)(~InternalTaskOptions.InternalOptionsMask); }
        }

        /// <summary>
        /// Gets whether the <see cref="Task"/> completed due to an unhandled exception.
        /// </summary>
        /// <remarks>
        /// If <see cref="IsFaulted"/> is true, the Task's <see cref="Status"/> will be equal to
        /// <see cref="TaskStatus.Faulted">TaskStatus.Faulted</see>, and its
        /// <see cref="Exception"/> property will be non-null.
        /// </remarks>
        public bool IsFaulted
        {
            get
            {
                // Faulted is "king" -- if that bit is present (regardless of other bits), we are faulted.
                return ((_stateFlags & TASK_STATE_FAULTED) != 0);
            }
        }

        /// <summary>
        /// Provides access to factory methods for creating
        /// <see cref="Task"/> and <see cref="Task{TResult}"/>
        /// instances.
        /// </summary>
        public static TaskFactory Factory { get { return _defaultFactory; } }

        /// <summary>
        /// Gets the <see cref="T:System.Threading.Tasks.TaskStatus">TaskStatus</see> of this Task. 
        /// </summary>
        public TaskStatus Status
        {
            get
            {
                TaskStatus rval;

                // get a cached copy of the state flags.  This should help us
                // to get a consistent view of the flags if they are changing during the
                // execution of this method.
                int sf = _stateFlags;

                if ((sf & TASK_STATE_FAULTED) != 0) rval = TaskStatus.Faulted;
                else if ((sf & TASK_STATE_CANCELED) != 0) rval = TaskStatus.Canceled;
                else if ((sf & TASK_STATE_RAN_TO_COMPLETION) != 0) rval = TaskStatus.RanToCompletion;
                //else if ((sf & TASK_STATE_WAITING_ON_CHILDREN) != 0) rval = TaskStatus.WaitingForChildrenToComplete;
                else if ((sf & TASK_STATE_DELEGATE_INVOKED) != 0) rval = TaskStatus.Running;
                else if ((sf & TASK_STATE_STARTED) != 0) rval = TaskStatus.WaitingToRun;
                else if ((sf & TASK_STATE_WAITINGFORACTIVATION) != 0) rval = TaskStatus.WaitingForActivation;
                else rval = TaskStatus.Created;

                return rval;
            }
        }

        // Debugger support
        private string DebuggerDisplayMethodDescription
            => m_contingentProperties?.m_action?.Method.ToString() ?? "{null}";

        #endregion

        #region Constructors and Destructor

        /// <summary>
        /// Internal constructor to create an empty task.
        /// </summary>
        internal Task()
        {
            _stateFlags = TASK_STATE_WAITINGFORACTIVATION;
        }

        /// <summary>
        /// Internal constructor to create an empty task.
        /// </summary>
        /// <param name="state">An object representing data to be used by the action.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> that will be assigned to the new task.</param>
        internal Task(object state, CancellationToken cancellationToken = default)
        {
            _stateFlags = TASK_STATE_WAITINGFORACTIVATION;
            m_stateObject = state;
            m_cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Internal constructor to create an already-completed task.
        /// </summary>
        internal Task(Exception ex, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ex == null)
            {
                if (!cancellationToken.IsCancellationRequested)
                    _stateFlags = TASK_STATE_RAN_TO_COMPLETION;
                else
                {
                    _stateFlags = TASK_STATE_CANCELED;
                    m_cancellationToken = cancellationToken;
                }
            }
            else if (ex is InternalOCE)
            {
                _stateFlags = TASK_STATE_CANCELED;
                m_cancellationToken = ((InternalOCE)ex).CancellationToken;
            }
            else
            {
                _stateFlags = TASK_STATE_FAULTED;
                m_exceptions.Add(ex);
            }
        }

        /// <summary>
        /// Internal constructor to allow creation of continue tasks.
        /// </summary>
        /// <param name="action">The delegate that represents the code to execute in the Task.</param>
        /// <param name="state">An object representing data to be used by the action.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> that will be assigned to the new task.</param>
        /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the Task's behavior.</param>
        /// <param name="continueSource">The task that run before current one.</param>
        internal Task(Delegate action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, Task continueSource)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            // Check for validity of options
            if ((creationOptions &
                    ~(TaskCreationOptions.AttachedToParent |
                      TaskCreationOptions.LongRunning |
                      TaskCreationOptions.DenyChildAttach |
                      TaskCreationOptions.HideScheduler |
                      TaskCreationOptions.PreferFairness |
                      TaskCreationOptions.RunContinuationsAsynchronously)) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(creationOptions));
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.creationOptions);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                _stateFlags = TASK_STATE_CANCELED | (int)creationOptions;
                m_cancellationToken = cancellationToken;
                return;
            }

            _stateFlags = (int)creationOptions;
            m_contingentProperties = new ContingentProperties(action, continueSource);
            m_stateObject = state;

            m_cancellationToken = cancellationToken;
            if (cancellationToken.CanBeCanceled)
            {
                m_contingentProperties.m_cancelRegistration = cancellationToken.Register(s =>
                {
                    Task task = s as Task;
                    task.TrySetCanceled(task.m_cancellationToken);
                }, this, false);
            }

            // Auto-start continuation tasks
            if (continueSource != null)
                StartContinuation(continueSource);
        }

        /// <summary>
        /// Initializes a new <see cref="Task"/> with the specified action.
        /// </summary>
        /// <param name="action">The delegate that represents the code to execute in the Task.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="action"/> argument is null.</exception>
        public Task(Action action)
            : this(action, null, default(CancellationToken), TaskCreationOptions.None, null)
        { }

        /// <summary>
        /// Initializes a new <see cref="Task"/> with the specified action and <see cref="Threading.CancellationToken">CancellationToken</see>.
        /// </summary>
        /// <param name="action">The delegate that represents the code to execute in the Task.</param>
        /// <param name="cancellationToken">The <see cref="Threading.CancellationToken">CancellationToken</see>
        /// that will be assigned to the new Task.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="action"/> argument is null.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task(Action action, CancellationToken cancellationToken)
            : this(action, null, cancellationToken, TaskCreationOptions.None, null)
        { }

        /// <summary>
        /// Initializes a new <see cref="Task"/> with the specified action and creation options.
        /// </summary>
        /// <param name="action">The delegate that represents the code to execute in the task.</param>
        /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the Task's behavior.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="action"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="creationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskCreationOptions"/>.
        /// </exception>
        public Task(Action action, TaskCreationOptions creationOptions)
            : this(action, null, default(CancellationToken), creationOptions, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Task"/> with the specified action and creation options.
        /// </summary>
        /// <param name="action">The delegate that represents the code to execute in the task.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <param name="creationOptions">
        /// The <see cref="System.Threading.Tasks.TaskCreationOptions">TaskCreationOptions</see> used to
        /// customize the Task's behavior.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="action"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="creationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskCreationOptions"/>.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
            : this(action, null, cancellationToken, creationOptions, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Task"/> with the specified action and state.
        /// </summary>
        /// <param name="action">The delegate that represents the code to execute in the task.</param>
        /// <param name="state">An object representing data to be used by the action.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="action"/> argument is null.
        /// </exception>
        public Task(Action<object> action, object state)
            : this(action, state, default(CancellationToken), TaskCreationOptions.None, null)
        { }

        /// <summary>
        /// Initializes a new <see cref="Task"/> with the specified action, state, and options.
        /// </summary>
        /// <param name="action">The delegate that represents the code to execute in the task.</param>
        /// <param name="state">An object representing data to be used by the action.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="action"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task(Action<object> action, object state, CancellationToken cancellationToken)
            : this(action, state, cancellationToken, TaskCreationOptions.None, null)
        { }

        /// <summary>
        /// Initializes a new <see cref="Task"/> with the specified action, state, and options.
        /// </summary>
        /// <param name="action">The delegate that represents the code to execute in the task.</param>
        /// <param name="state">An object representing data to be used by the action.</param>
        /// <param name="creationOptions">
        /// The <see cref="System.Threading.Tasks.TaskCreationOptions">TaskCreationOptions</see> used to
        /// customize the Task's behavior.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="action"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="creationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskCreationOptions"/>.
        /// </exception>
        public Task(Action<object> action, object state, TaskCreationOptions creationOptions)
            : this(action, state, default(CancellationToken), creationOptions, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Task"/> with the specified action, state, and options.
        /// </summary>
        /// <param name="action">The delegate that represents the code to execute in the task.</param>
        /// <param name="state">An object representing data to be used by the action.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
        /// <param name="creationOptions">
        /// The <see cref="System.Threading.Tasks.TaskCreationOptions">TaskCreationOptions</see> used to
        /// customize the Task's behavior.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="action"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="creationOptions"/> argument specifies an invalid value for <see
        /// cref="T:System.Threading.Tasks.TaskCreationOptions"/>.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The provided <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// has already been disposed.
        /// </exception>
        public Task(Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
            : this(action, state, cancellationToken, creationOptions, null)
        {
        }

        #endregion

        #region Helper methods

        // Internal property to process TaskCreationOptions access and mutation.
        internal TaskCreationOptions Options => OptionsMethod(_stateFlags);

        // Similar to Options property, but allows for the use of a cached flags value rather than
        // a read of the volatile m_stateFlags field.
        internal static TaskCreationOptions OptionsMethod(int flags)
        {
            Diagnostics.Debug.Assert((OptionsMask & 1) == 1, "OptionsMask needs a shift in Options.get");
            return (TaskCreationOptions)(flags & OptionsMask);
        }

        // Atomically OR-in newBits to _stateFlags, while making sure that
        // no illegalBits are set.  Returns true on success, false on failure.
        private bool AtomicStateUpdate(int newBits, int illegalBits)
        {
            SpinWait sw = new SpinWait();
            do
            {
                int oldFlags = _stateFlags;
                if ((oldFlags & illegalBits) != 0) return false;
                if (Interlocked.CompareExchange(ref _stateFlags, oldFlags | newBits, oldFlags) == oldFlags)
                {
                    return true;
                }
                sw.SpinOnce();
            } while (true);
        }

        private ContingentProperties EnsureContigentPropertiesInitialized()
        {
            if (m_contingentProperties == null)
            {
                var cp = new ContingentProperties(null, null);
                if (Interlocked.CompareExchange(ref m_contingentProperties, cp, null) != null)
                    cp = null;
            }

            return m_contingentProperties;
        }

        // Atomically mark a Task as started while making sure that it is not canceled.
        private bool MarkStarted()
        {
            return AtomicStateUpdate(TASK_STATE_STARTED, TASK_STATE_CANCELED | TASK_STATE_STARTED);
        }

        internal bool TrySetCanceled(CancellationToken cancellationToken)
        {
            if (!AtomicStateUpdate(TASK_STATE_STARTED | TASK_STATE_CANCELED, TASK_STATE_COMPLETED_MASK))
                return false;

            m_cancellationToken = cancellationToken;
            SendCompletedSignal();
            return true;
        }

        // Atomically mark a Task as completed while making sure that it is not already completed.
        internal bool TrySetCompleted(Action action = null)
        {
            if (!AtomicStateUpdate(TASK_STATE_STARTED | TASK_STATE_RAN_TO_COMPLETION, TASK_STATE_COMPLETED_MASK))
                return false;
            
            action?.Invoke();
            SendCompletedSignal();
            return true;
        }

        internal bool TrySetException(Exception e)
        {
            if (!AtomicStateUpdate(TASK_STATE_FAULTED, TASK_STATE_COMPLETED_MASK))
                return false;

            AggregateException agg = e as AggregateException;
            if (agg != null)
                m_exceptions.AddRange(agg.InnerExceptions);
            else
                m_exceptions.Add(e);

            SendCompletedSignal();
            return true;
        }

        private static int NewId()
        {
            int newId = 0;
            // We need to repeat if Interlocked.Increment wraps around and returns 0.
            // Otherwise next time this task's Id is queried it will get a new value
            do
            {
                newId = Interlocked.Increment(ref _taskIdCounter);
            }
            while (newId == 0);
            return newId;
        }

        /// <summary>
        /// Throws an exception when called more than once.
        /// </summary>
        protected void EnsureStartOnce()
        {
            if (!MarkStarted())
                throw new InvalidOperationException("Cannot start a task that is already completed");
        }

        // Similar to IsCompleted property, but allows for the use of a cached flags value
        // rather than reading the volatile m_stateFlags field.
        private static bool IsCompletedMethod(int flags)
        {
            return (flags & TASK_STATE_COMPLETED_MASK) != 0;
        }

        // For use in InternalWait -- marginally faster than (Task.Status == TaskStatus.RanToCompletion)
        internal bool IsRanToCompletion
        {
            get { return (_stateFlags & TASK_STATE_COMPLETED_MASK) == TASK_STATE_RAN_TO_COMPLETION; }
        }

        // Send signal to completed event handler and execute callback
        private void SendCompletedSignal()
        {
            // TODO: Avoid initialization race
            var contingentProperties = m_contingentProperties;
            if (contingentProperties != null)
                contingentProperties.SetCompleted();

            // Execute wait callback if any
            lock (_lockObj)
            {
                _completedCallback?.Invoke(null);
                _completedCallback = null;
            }
        }

        /// <summary>
        /// Converts TaskContinuationOptions to TaskCreationOptions, and also does
        /// some validity checking along the way.
        /// </summary>
        /// <param name="continuationOptions">Incoming TaskContinuationOptions</param>
        /// <param name="creationOptions">Outgoing TaskCreationOptions</param>
        /// <param name="internalOptions">Outgoing InternalTaskOptions</param>
        internal static void CreationOptionsFromContinuationOptions(
            TaskContinuationOptions continuationOptions,
            out TaskCreationOptions creationOptions,
            out InternalTaskOptions internalOptions)
        {
            // This is used a couple of times below
            const TaskContinuationOptions NotOnAnything =
                TaskContinuationOptions.NotOnCanceled |
                TaskContinuationOptions.NotOnFaulted |
                TaskContinuationOptions.NotOnRanToCompletion;

            const TaskContinuationOptions CreationOptionsMask =
                TaskContinuationOptions.PreferFairness |
                TaskContinuationOptions.LongRunning |
                TaskContinuationOptions.DenyChildAttach |
                TaskContinuationOptions.HideScheduler |
                TaskContinuationOptions.AttachedToParent |
                TaskContinuationOptions.RunContinuationsAsynchronously;

            // Check that LongRunning and ExecuteSynchronously are not specified together
            const TaskContinuationOptions IllegalMask = TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.LongRunning;
            if ((continuationOptions & IllegalMask) == IllegalMask)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.continuationOptions, ExceptionResource.Task_ContinueWith_ESandLR);
            }

            // Check that no illegal options were specified
            if ((continuationOptions &
                ~(CreationOptionsMask | NotOnAnything |
                    TaskContinuationOptions.LazyCancellation | TaskContinuationOptions.ExecuteSynchronously)) != 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.continuationOptions);
            }

            // Check that we didn't specify "not on anything"
            if ((continuationOptions & NotOnAnything) == NotOnAnything)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.continuationOptions, ExceptionResource.Task_ContinueWith_NotOnAnything);
            }

            // This passes over all but LazyCancellation, which has no representation in TaskCreationOptions
            creationOptions = (TaskCreationOptions)(continuationOptions & CreationOptionsMask);

            // internalOptions has at least ContinuationTask and possibly LazyCancellation
            internalOptions = (continuationOptions & TaskContinuationOptions.LazyCancellation) != 0 ?
                InternalTaskOptions.ContinuationTask | InternalTaskOptions.LazyCancellation :
                InternalTaskOptions.ContinuationTask;
        }

        #endregion

        #region Start method

        /// <summary>
        /// Starts the <see cref="Task"/>, scheduling it for execution to the current <see
        /// cref="System.Threading.ThreadPool">ThreadPool</see>.
        /// </summary>
        /// <remarks>
        /// A task may only be started and run only once.  Any attempts to schedule a task a second time
        /// will result in an exception.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="Task"/> is already been started.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Task"/> could not be enqueued for execution.
        /// </exception>
        public void Start()
        {
            EnsureStartOnce();
            AtomicStateUpdate(TASK_STATE_WAITINGFORACTIVATION, TASK_STATE_COMPLETED_MASK);

            bool isLongRunning = (_stateFlags & (int)TaskCreationOptions.LongRunning) > 0;
            if (!isLongRunning)
            {
                if (!ThreadPool.QueueUserWorkItem(TaskStartAction))
                    throw new NotSupportedException("Could not enqueue task for execution");
            }
            else
            {
                new Thread((ThreadStart)TaskStartAction).Start();
            }
        }

        private void StartContinuation(Task source)
        {
            EnsureStartOnce();
            AtomicStateUpdate(TASK_STATE_WAITINGFORACTIVATION, TASK_STATE_COMPLETED_MASK);

            // TODO: Long running continuation should not run on ThreadPool
            if (source.EnqueueContinuation(TaskStartAction))
                return;

            bool isLongRunning = (_stateFlags & (int)TaskCreationOptions.LongRunning) > 0;
            if (!isLongRunning)
            {
                if (!ThreadPool.QueueUserWorkItem(TaskStartAction))
                    throw new NotSupportedException("Could not enqueue task for execution");
            }
            else
            {
                new Thread((ThreadStart)TaskStartAction).Start();
            }
        }

        internal bool EnqueueContinuation(Action<object> callback)
        {
            lock (_lockObj)
            {
                if (IsCompletedMethod(_stateFlags))
                    return false;

                // Enqueue to execute when Task is finalizing
                _completedCallback += callback;
                return true;
            }
        }

        #endregion

        #region Task thread execution

        /// <summary>
        /// Executes the action designed for current task.
        /// </summary>
        private void TaskStartAction()
        {
            TaskStartAction(null);
        }

        /// <summary>
        /// Executes the action designed for current task.
        /// </summary>
        /// <param name="stateObject">Ignored.</param>
        protected void TaskStartAction(object stateObject)
        {
            try
            {
                if (!AtomicStateUpdate(TASK_STATE_DELEGATE_INVOKED, TASK_STATE_DELEGATE_INVOKED | TASK_STATE_COMPLETED_MASK))
                    return;

                // Execute provided action
                InnerInvoke();
            }
            catch (InternalOCE ex)
            when (ex.CancellationToken == m_cancellationToken)
            {
                AtomicStateUpdate(TASK_STATE_CANCELED, TASK_STATE_COMPLETED_MASK);
            }
            catch (AggregateException ex)
            {
                AtomicStateUpdate(TASK_STATE_FAULTED, TASK_STATE_COMPLETED_MASK);
                m_exceptions.AddRange(ex.InnerExceptions);
            }
            catch (Exception ex)
            {
                AtomicStateUpdate(TASK_STATE_FAULTED, TASK_STATE_COMPLETED_MASK);
                m_exceptions.Add(ex);
            }
            finally
            {
                AtomicStateUpdate(TASK_STATE_RAN_TO_COMPLETION, TASK_STATE_COMPLETED_MASK);
                SendCompletedSignal();
            }
        }

        /// <summary>
        /// Unbox task action and execute it.
        /// </summary>
        protected virtual void InnerInvoke()
        {
            var cp = m_contingentProperties;
            if (cp == null)
                throw new InvalidOperationException("Should not try to execute null actions");
            var uncastAction = cp.m_action;
            var parent = cp.m_parent;

            switch (uncastAction)
            {
                case Action action0:
                    action0();
                    break;
                case Action<object> actionObj:
                    actionObj(m_stateObject);
                    break;
                case Action2<Task> actionTask:
                    actionTask(parent);
                    break;
                case Action2<Task, object> actionTaskObj:
                    actionTaskObj(parent, m_stateObject);
                    break;
                default:
                    throw new InvalidOperationException("Unexpected action type");
            }
        }

        #endregion

        #region Synchronous

        /// <summary>
        /// Runs the <see cref="Task"/> synchronously on the current <see cref="Thread"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="Task"/> is already been started.
        /// </exception>
        public void RunSynchronously()
        {
            EnsureStartOnce();

            TaskStartAction(null);
        }

        #endregion

        #region Wait methods

        /// <summary>
        /// Waits for the <see cref="Task"/> to complete execution.
        /// </summary>
        /// <exception cref="AggregateException">
        /// An exception was thrown during the execution of the <see cref="Task"/>.
        /// </exception>
        public void Wait()
        {
            Wait(Timeout.Infinite, default(CancellationToken));
        }

        /// <summary>
        /// Waits for the <see cref="Task"/> to complete execution
        /// within a specified time interval.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> that represents the number of milliseconds to
        /// wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait
        /// indefinitely.
        /// </param>
        /// <returns>
        /// true if the <see cref="Task"/> completed execution within
        /// the allotted time; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// timeout is a negative number other than -1 milliseconds, which
        /// represents an infinite time-out -or- timeout is greater than
        /// <see cref="int.MaxValue"/>.
        /// </exception>
        /// <exception cref="AggregateException">
        /// An exception was thrown during the execution of the <see cref="Task"/>.
        /// </exception>
        public bool Wait(TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }

            return Wait((int)totalMilliseconds, default(CancellationToken));
        }

        /// <summary>
        /// Waits for the <see cref="Task"/> to complete execution.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
        /// </param>
        /// <exception cref="T:System.OperationCanceledException">
        /// The <paramref name="cancellationToken"/> was canceled.
        /// </exception>
        /// <exception cref="T:System.AggregateException">
        /// The <see cref="Task"/> was canceled -or- an exception was thrown during the execution of the <see
        /// cref="Task"/>.
        /// </exception>
        public void Wait(CancellationToken cancellationToken)
        {
            Wait(Timeout.Infinite, cancellationToken);
        }

        /// <summary>
        /// Waits for the System.Threading.Tasks.Task to complete execution
        /// within a specified number of milliseconds.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or
        /// System.Threading.Timeout.Infinite (-1) to wait indefinitely.
        /// </param>
        /// <returns>
        /// true if the System.Threading.Tasks.Task completed execution within
        /// the allotted time; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// millisecondsTimeout is a negative number other than -1, which
        /// represents an infinite time-out.
        /// </exception>
        /// <exception cref="AggregateException">
        /// An exception was thrown during the execution of the
        /// System.Threading.Tasks.Task.
        /// </exception>
        public bool Wait(int millisecondsTimeout)
        {
            return Wait(millisecondsTimeout, default(CancellationToken));
        }

        /// <summary>
        /// Waits for the <see cref="Task"/> to complete execution.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or <see cref="System.Threading.Timeout.Infinite"/> (-1) to
        /// wait indefinitely.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// true if the <see cref="Task"/> completed execution within the allotted time; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.AggregateException">
        /// The <see cref="Task"/> was canceled -or- an exception was thrown during the execution of the <see
        /// cref="Task"/>.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="millisecondsTimeout"/> is a negative number other than -1, which represents an
        /// infinite time-out.
        /// </exception>
        /// <exception cref="T:System.OperationCanceledException">
        /// The <paramref name="cancellationToken"/> was canceled.
        /// </exception>
        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
            }

            bool success = CompletedEvent.Wait(millisecondsTimeout, cancellationToken);

            switch (Status)
            {
                case TaskStatus.Canceled:
                    throw new AggregateException(new TaskCanceledException(this));
                case TaskStatus.Faulted:
                    ExceptionDispatchInfo.Capture(this.Exception).Throw();
                    break;
            }

            return success;
        }

        #endregion

        #region Await Support

        /// <summary>
        /// Gets an awaiter to await this <see cref="Task"/>.
        /// </summary>
        /// <returns>A new awaiter instance.</returns>
        public Runtime.CompilerServices.TaskAwaiter GetAwaiter()
        {
            return new Runtime.CompilerServices.TaskAwaiter(this);
        }

        /// <summary>
        /// Configures an awaiter used to await this <see cref="Task"/>.
        /// </summary>
        /// <param name="continueOnCapturedContext">
        /// true to attempt to marshal the continuation back to the original
        /// context captured; otherwise, false.
        /// </param>
        /// <returns>A new awaiter instance.</returns>
        public Runtime.CompilerServices.ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext)
        {
            return new Runtime.CompilerServices.ConfiguredTaskAwaitable(
                this, continueOnCapturedContext);
        }

        /// <summary>
        /// Creates an awaitable that asynchronously yields back to the current context when awaited.
        /// </summary>
        /// <returns>
        /// A context that, when awaited, will asynchronously transition back into the current context at the 
        /// time of the await. If the current SynchronizationContext is non-null, that is treated as the current context.
        /// Otherwise, TaskScheduler.Current is treated as the current context.
        /// </returns>
        public static Runtime.CompilerServices.YieldAwaitable Yield()
        {
            return new Runtime.CompilerServices.YieldAwaitable();
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Disposes the <see cref="Task"/>, releasing all of its unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                AtomicStateUpdate(TASK_STATE_DISPOSED, 0);
                return;
            }

            if ((_stateFlags & TASK_STATE_COMPLETED_MASK) == 0)
            {
                throw new InvalidOperationException(
                    "A task may only be disposed if it has completed its execution");
            }

            m_contingentProperties?.Dispose();
            AtomicStateUpdate(TASK_STATE_DISPOSED, 0);
        }

        #endregion

        #region IAsyncResult Members

        /// <summary>
        /// Gets the state object supplied when the <see cref="Task">Task</see> was created,
        /// or null if none was supplied.
        /// </summary>
        public object AsyncState
        {
            get { return m_stateObject; }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle"/> that can be used to wait for the task to
        /// complete.
        /// </summary>
        /// <remarks>
        /// Using the wait functionality provided by <see cref="Wait()"/>
        /// should be preferred over using <see cref="IAsyncResult.AsyncWaitHandle"/> for similar
        /// functionality.
        /// </remarks>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <see cref="Task"/> has been disposed.
        /// </exception>
        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get
            {
                bool isDisposed = (_stateFlags & TASK_STATE_DISPOSED) != 0;
                if (isDisposed)
                    throw new ObjectDisposedException("Task");

                return CompletedEvent.WaitHandle;
            }
        }

        /// <summary>
        /// Gets an indication of whether the asynchronous operation completed synchronously.
        /// </summary>
        /// <value>true if the asynchronous operation completed synchronously; otherwise, false.</value>
        bool IAsyncResult.CompletedSynchronously
        {
            get { return false; }
        }

        #endregion

        #region Continuation Methods

        public Task ContinueWith(Action2<Task, object> continuationAction, object state)
        {
            return InternalContinueWith(continuationAction, state, TaskScheduler.Current, default, TaskContinuationOptions.None);
        }

        public Task ContinueWith(Action2<Task, object> continuationAction, object state, CancellationToken cancellationToken)
        {
            return InternalContinueWith(continuationAction, state, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None);
        }

        public Task ContinueWith(Action2<Task, object> continuationAction, object state, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            return InternalContinueWith(continuationAction, state, scheduler, cancellationToken, continuationOptions);
        }

        public Task ContinueWith(Action2<Task, object> continuationAction, object state, TaskContinuationOptions continuationOptions)
        {
            return InternalContinueWith(continuationAction, state, TaskScheduler.Current, default, continuationOptions);
        }

        public Task ContinueWith(Action2<Task, object> continuationAction, object state, TaskScheduler scheduler)
        {
            return InternalContinueWith(continuationAction, state, scheduler, default, TaskContinuationOptions.None);
        }

        public Task ContinueWith(Action2<Task> continuationAction)
        {
            return InternalContinueWith(continuationAction, null, TaskScheduler.Current, default, TaskContinuationOptions.None);
        }

        public Task ContinueWith(Action2<Task> continuationAction, CancellationToken cancellationToken)
        {
            return InternalContinueWith(continuationAction, null, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None);
        }

        public Task ContinueWith(Action2<Task> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            return InternalContinueWith(continuationAction, null, scheduler, cancellationToken, continuationOptions);
        }

        public Task ContinueWith(Action2<Task> continuationAction, TaskContinuationOptions continuationOptions)
        {
            return InternalContinueWith(continuationAction, null, TaskScheduler.Current, default, continuationOptions);
        }

        public Task ContinueWith(Action2<Task> continuationAction, TaskScheduler scheduler)
        {
            return InternalContinueWith(continuationAction, null, scheduler, default, TaskContinuationOptions.None);
        }

        public Task<TResult> ContinueWith<TResult>(Func2<Task, object, TResult> continuationFunction, object state)
        {
            return InternalContinueWith<TResult>(continuationFunction, state, TaskScheduler.Current, default, TaskContinuationOptions.None);
        }

        public Task<TResult> ContinueWith<TResult>(Func2<Task, object, TResult> continuationFunction, object state, CancellationToken cancellationToken)
        {
            return InternalContinueWith<TResult>(continuationFunction, state, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None);
        }

        public Task<TResult> ContinueWith<TResult>(Func2<Task, object, TResult> continuationFunction, object state, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            return InternalContinueWith<TResult>(continuationFunction, state, scheduler, cancellationToken, continuationOptions);
        }

        public Task<TResult> ContinueWith<TResult>(Func2<Task, object, TResult> continuationFunction, object state, TaskContinuationOptions continuationOptions)
        {
            return InternalContinueWith<TResult>(continuationFunction, state, TaskScheduler.Current, default, continuationOptions);
        }

        public Task<TResult> ContinueWith<TResult>(Func2<Task, object, TResult> continuationFunction, object state, TaskScheduler scheduler)
        {
            return InternalContinueWith<TResult>(continuationFunction, state, scheduler, default, TaskContinuationOptions.None);
        }

        public Task<TResult> ContinueWith<TResult>(Func2<Task, TResult> continuationFunction)
        {
            return InternalContinueWith<TResult>(continuationFunction, null, TaskScheduler.Current, default, TaskContinuationOptions.None);
        }

        public Task<TResult> ContinueWith<TResult>(Func2<Task, TResult> continuationFunction, CancellationToken cancellationToken)
        {
            return InternalContinueWith<TResult>(continuationFunction, null, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None);
        }

        public Task<TResult> ContinueWith<TResult>(Func2<Task, TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            return InternalContinueWith<TResult>(continuationFunction, null, TaskScheduler.Current, cancellationToken, continuationOptions);
        }

        public Task<TResult> ContinueWith<TResult>(Func2<Task, TResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            return InternalContinueWith<TResult>(continuationFunction, null, TaskScheduler.Current, default, continuationOptions);
        }

        public Task<TResult> ContinueWith<TResult>(Func2<Task, TResult> continuationFunction, TaskScheduler scheduler)
        {
            return InternalContinueWith<TResult>(continuationFunction, null, scheduler, default, TaskContinuationOptions.None);
        }

        private Task InternalContinueWith(
            Delegate continuationAction,
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

            return new Task(continuationAction, state, cancellationToken, creationOptions, this);
        }

        private Task<TResult> InternalContinueWith<TResult>(
            Delegate continuationFunction,
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

            return new Task<TResult>(continuationFunction, state, cancellationToken, creationOptions, this);
        }

        #endregion

        #region Static Wait methods

        /// <summary>
        /// Waits for all of the provided <see cref="Task"/> objects to complete execution.
        /// </summary>
        /// <param name="tasks">
        /// An array of <see cref="Task"/> instances on which to wait.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument contains a null element.
        /// </exception>
        /// <exception cref="T:System.AggregateException">
        /// At least one of the <see cref="Task"/> instances was canceled -or- an exception was thrown during
        /// the execution of at least one of the <see cref="Task"/> instances.
        /// </exception>
        public static void WaitAll(params Task[] tasks)
        {
            WaitAll(tasks, Timeout.Infinite, default(CancellationToken));
        }

        public static void WaitAll(Task[] tasks, CancellationToken cancellationToken)
        {
            WaitAll(tasks, Timeout.Infinite, default(CancellationToken));
        }

        /// <summary>
        /// Waits for all of the provided <see cref="Task"/> objects to complete execution.
        /// </summary>
        /// <returns>
        /// true if all of the <see cref="Task"/> instances completed execution within the allotted time;
        /// otherwise, false.
        /// </returns>
        /// <param name="tasks">
        /// An array of <see cref="Task"/> instances on which to wait.
        /// </param>
        /// <param name="timeout">
        /// A <see cref="System.TimeSpan"/> that represents the number of milliseconds to wait, or a <see
        /// cref="System.TimeSpan"/> that represents -1 milliseconds to wait indefinitely.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> argument contains a null element.
        /// </exception>
        /// <exception cref="T:System.AggregateException">
        /// At least one of the <see cref="Task"/> instances was canceled -or- an exception was thrown during
        /// the execution of at least one of the <see cref="Task"/> instances.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="timeout"/> is a negative number other than -1 milliseconds, which represents an
        /// infinite time-out -or- timeout is greater than
        /// <see cref="int.MaxValue"/>.
        /// </exception>
        public static bool WaitAll(Task[] tasks, TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > Int32.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }

            return WaitAll(tasks, (int)totalMilliseconds, default(CancellationToken));
        }

        /// <summary>
        /// Waits for all of the provided <see cref="Task"/> objects to complete execution.
        /// </summary>
        /// <returns>
        /// true if all of the <see cref="Task"/> instances completed execution within the allotted time;
        /// otherwise, false.
        /// </returns>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or <see cref="System.Threading.Timeout.Infinite"/> (-1) to
        /// wait indefinitely.</param>
        /// <param name="tasks">An array of <see cref="Task"/> instances on which to wait.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> argument contains a null element.
        /// </exception>
        /// <exception cref="T:System.AggregateException">
        /// At least one of the <see cref="Task"/> instances was canceled -or- an exception was thrown during
        /// the execution of at least one of the <see cref="Task"/> instances.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="millisecondsTimeout"/> is a negative number other than -1, which represents an
        /// infinite time-out.
        /// </exception>
        public static bool WaitAll(Task[] tasks, int millisecondsTimeout)
        {
            return WaitAll(tasks, millisecondsTimeout, default(CancellationToken));
        }

        public static bool WaitAll(Task[] tasks, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

            cancellationToken.ThrowIfCancellationRequested();

            using (var countdown = new CountdownEvent(tasks.Length))
            {
                for (int i = 0; i < tasks.Length; i++)
                {
                    if (!tasks[i].EnqueueContinuation(s => countdown.Signal()))
                        countdown.Signal();
                }

                if (!countdown.Wait(millisecondsTimeout, cancellationToken))
                    return false;
            }

            var exceptions = new List<System.Exception>();
            for (int i = 0; i < tasks.Length; i++)
            {
                Task task = tasks[i];
                if (task.Status == TaskStatus.Faulted)
                    exceptions.AddRange(task.m_exceptions);
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions).Flatten();

            return true;
        }

        /// <summary>
        /// Waits for any of the provided <see cref="Task"/> objects to complete execution.
        /// </summary>
        /// <param name="tasks">
        /// An array of <see cref="Task"/> instances on which to wait.
        /// </param>
        /// <returns>The index of the completed task in the <paramref name="tasks"/> array argument.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> argument contains a null element.
        /// </exception>
        public static int WaitAny(params Task[] tasks)
        {
            return WaitAny(tasks, Timeout.Infinite, default(CancellationToken));
        }

        public static int WaitAny(Task[] tasks, CancellationToken cancellationToken)
        {
            return WaitAny(tasks, Timeout.Infinite, cancellationToken);
        }

        /// <summary>
        /// Waits for any of the provided <see cref="Task"/> objects to complete execution.
        /// </summary>
        /// <param name="tasks">
        /// An array of <see cref="Task"/> instances on which to wait.
        /// </param>
        /// <param name="timeout">
        /// A <see cref="System.TimeSpan"/> that represents the number of milliseconds to wait, or a <see
        /// cref="System.TimeSpan"/> that represents -1 milliseconds to wait indefinitely.
        /// </param>
        /// <returns>
        /// The index of the completed task in the <paramref name="tasks"/> array argument, or -1 if the
        /// timeout occurred.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> argument contains a null element.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="timeout"/> is a negative number other than -1 milliseconds, which represents an
        /// infinite time-out -or- timeout is greater than
        /// <see cref="int.MaxValue"/>.
        /// </exception>
        public static int WaitAny(Task[] tasks, TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > Int32.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }

            return WaitAny(tasks, (int)totalMilliseconds, default(CancellationToken));
        }

        /// <summary>
        /// Waits for any of the provided <see cref="Task"/> objects to complete execution.
        /// </summary>
        /// <param name="tasks">
        /// An array of <see cref="Task"/> instances on which to wait.
        /// </param>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or <see cref="System.Threading.Timeout.Infinite"/> (-1) to
        /// wait indefinitely.
        /// </param>
        /// <returns>
        /// The index of the completed task in the <paramref name="tasks"/> array argument, or -1 if the
        /// timeout occurred.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> argument contains a null element.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="millisecondsTimeout"/> is a negative number other than -1, which represents an
        /// infinite time-out.
        /// </exception>
        public static int WaitAny(Task[] tasks, int millisecondsTimeout)
        {
            return WaitAny(tasks, millisecondsTimeout, default(CancellationToken));
        }

        public static int WaitAny(Task[] tasks, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

            cancellationToken.ThrowIfCancellationRequested();

            int completedIndex;
            using (var waitDone = new ManualResetEventSlim(false))
            {
                completedIndex = -1;

                for (int i = 0; i < tasks.Length; i++)
                {
                    int currentIndex = i;
                    void TaskSetCompleted(object s)
                    {
                        int originalValue = Interlocked.CompareExchange(ref completedIndex, currentIndex, -1);
                        if (originalValue != -1)
                            return;

                        waitDone.Set();
                    }

                    if (!tasks[i].EnqueueContinuation(TaskSetCompleted))
                        waitDone.Set();

                    if (waitDone.IsSet)
                        break;
                }

                if (!waitDone.Wait(millisecondsTimeout, cancellationToken))
                    return -1;
            }

            Task completedTask = tasks[completedIndex];
            return completedTask.Status != TaskStatus.Faulted
                ? completedIndex
                : throw completedTask.Exception;
        }

        #endregion

        #region FromResult / FromException / FromCanceled

        /// <summary>
        /// Creates a <see cref="Task{TResult}"/> that's completed
        /// successfully with the specified result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="result">The result to store into the completed task.</param>
        /// <returns>The successfully completed task.</returns>
        public static Task<TResult> FromResult<TResult>(TResult result)
        {
            return new Task<TResult>(result, null);
        }

        /// <summary>Creates a <see cref="Task{TResult}"/> that's completed exceptionally with the specified exception.</summary>
        /// <param name="exception">The exception with which to complete the task.</param>
        /// <returns>The faulted task.</returns>
        public static Task FromException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return new Task(exception);
        }

        /// <summary>Creates a <see cref="Task{TResult}"/> that's completed exceptionally with the specified exception.</summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="exception">The exception with which to complete the task.</param>
        /// <returns>The faulted task.</returns>
        public static Task<TResult> FromException<TResult>(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return new Task<TResult>(default(TResult), exception);
        }

        /// <summary>Creates a <see cref="Task"/> that's completed due to cancellation with the specified token.</summary>
        /// <param name="cancellationToken">The token with which to complete the task.</param>
        /// <returns>The canceled task.</returns>
        public static Task FromCanceled(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                throw new ArgumentOutOfRangeException(nameof(cancellationToken));

            return new Task((Exception)null, cancellationToken);
        }

        /// <summary>Creates a <see cref="Task{TResult}"/> that's completed due to cancellation with the specified token.</summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="cancellationToken">The token with which to complete the task.</param>
        /// <returns>The canceled task.</returns>
        public static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                throw new ArgumentOutOfRangeException(nameof(cancellationToken));

            return new Task<TResult>(default(TResult), (Exception)null, cancellationToken);
        }

        #endregion

        #region Run methods

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a Task handle for that work.
        /// </summary>
        /// <param name="action">The work to execute asynchronously</param>
        /// <returns>A Task that represents the work queued to execute in the ThreadPool.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="action"/> parameter was null.
        /// </exception>
        public static Task Run(Action action)
        {
            return Factory.StartNew(action);
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a proxy for the
        /// Task returned by <paramref name="function"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A Task that represents a proxy for the Task returned by <paramref name="function"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="function"/> parameter was null.
        /// </exception>
        public static Task Run(Func2<Task> function)
        {
            return Factory.StartNew(function).Unwrap();
        }

        public static Task Run(Action action, CancellationToken cancellationToken)
        {
            return Factory.StartNew(action, cancellationToken);
        }

        public static Task Run(Func2<Task> function, CancellationToken cancellationToken)
        {
            return Factory.StartNew(function, cancellationToken).Unwrap();
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a Task(TResult) handle for that work.
        /// </summary>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A Task(TResult) that represents the work queued to execute in the ThreadPool.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="function"/> parameter was null.
        /// </exception>
        public static Task<TResult> Run<TResult>(Func2<TResult> function)
        {
            return Factory.StartNew(function);
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a proxy for the
        /// Task(TResult) returned by <paramref name="function"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the proxy Task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <returns>A Task(TResult) that represents a proxy for the Task(TResult) returned by <paramref name="function"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="function"/> parameter was null.
        /// </exception>
        public static Task<TResult> Run<TResult>(Func2<Task<TResult>> function)
        {
            return Factory.StartNew(function).Unwrap();
        }

        public static Task<TResult> Run<TResult>(Func2<Task<TResult>> function, CancellationToken cancellationToken)
        {
            return Factory.StartNew(function, cancellationToken).Unwrap();
        }

        public static Task<TResult> Run<TResult>(Func2<TResult> function, CancellationToken cancellationToken)
        {
            return Factory.StartNew(function, cancellationToken);
        }

        #endregion

        #region Delay methods

        /// <summary>
        /// Creates a Task that will complete after a time delay.
        /// </summary>
        /// <param name="delay">The time span to wait before completing the returned Task</param>
        /// <returns>A Task that represents the time delay</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="delay"/> is less than -1 or greater than Int32.MaxValue.
        /// </exception>
        /// <remarks>
        /// After the specified time delay, the Task is completed in RanToCompletion state.
        /// </remarks>
        public static Task Delay(TimeSpan delay)
        {
            return Delay(delay, default(CancellationToken));
        }

        /// <summary>
        /// Creates a Task that will complete after a time delay.
        /// </summary>
        /// <param name="delay">The time span to wait before completing the returned Task</param>
        /// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned task.</param>
        /// <returns>A Task that represents the time delay</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="delay"/> is less than -1 or greater than Int32.MaxValue.
        /// </exception>
        /// <remarks>
        /// After the specified time delay, the Task is completed in RanToCompletion state.
        /// </remarks>
        public static Task Delay(TimeSpan delay, CancellationToken cancellationToken)
        {
            long totalMilliseconds = (long)delay.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(delay));
            }

            return Delay((int)totalMilliseconds);
        }

        /// <summary>
        /// Creates a Task that will complete after a time delay.
        /// </summary>
        /// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned Task</param>
        /// <returns>A Task that represents the time delay</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="millisecondsDelay"/> is less than -1.
        /// </exception>
        /// <remarks>
        /// After the specified time delay, the Task is completed in RanToCompletion state.
        /// </remarks>
        public static Task Delay(int millisecondsDelay)
        {
            return Delay(millisecondsDelay, default(CancellationToken));
        }

        /// <summary>
        /// Creates a Task that will complete after a time delay.
        /// </summary>
        /// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned Task</param>
        /// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned task.</param>
        /// <returns>A Task that represents the time delay</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="millisecondsDelay"/> is less than -1.
        /// </exception>
        /// <remarks>
        /// After the specified time delay, the Task is completed in RanToCompletion state.
        /// </remarks>
        public static Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
        {
            if (millisecondsDelay < -1)
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
            if (cancellationToken.IsCancellationRequested)
                return new Task((Exception)null, cancellationToken);
            if (millisecondsDelay == 0)
                return CompletedTask;

            var promise = new DelayPromise(cancellationToken);
            promise.Start(millisecondsDelay);

            return promise;
        }

        #endregion

        #region WhenAll

        /// <summary>
        /// Creates a task that will complete when all of the supplied tasks have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument was null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> collection contained a null task.
        /// </exception>
        public static Task WhenAll(IEnumerable<Task> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            // Take a more efficient path if tasks is actually an array
            return WhenAll(tasks as Task[] ?? tasks.ToArray());
        }

        /// <summary>
        /// Creates a task that will complete when all of the supplied tasks have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument was null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> array contained a null task.
        /// </exception>
        public static Task WhenAll(params Task[] tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            int length = tasks.Length;
            if (length == 0)
                return new Task((Exception)null);

            var tasksCopy = new Task[length];
            for (int i = 0; i < length; i++)
            {
                tasksCopy[i] = tasks[i] ?? throw new ArgumentException(SR.Task_MultiTaskContinuation_NullTask, nameof(tasks));
            }

            return new WhenAllPromise(tasksCopy);
        }

        /// <summary>
        /// Creates a task that will complete when all of the supplied tasks have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument was null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> collection contained a null task.
        /// </exception>       
        public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            // Take a more efficient path if tasks is actually an array
            return WhenAll(tasks as Task<TResult>[] ?? tasks.ToArray());
        }

        /// <summary>
        /// Creates a task that will complete when all of the supplied tasks have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument was null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> array contained a null task.
        /// </exception>
        public static Task<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            int length = tasks.Length;
            if (length == 0)
                return new Task<TResult[]>(new TResult[0], null);

            Task<TResult>[] tasksCopy = new Task<TResult>[length];
            for (int i = 0; i < length; i++)
            {
                tasksCopy[i] = tasks[i] ?? throw new ArgumentException(SR.Task_MultiTaskContinuation_NullTask, nameof(tasks));
            }

            return new WhenAllPromise<TResult>(tasksCopy);
        }

        #endregion

        #region WhenAny

        /// <summary>
        /// Creates a task that will complete when any of the supplied tasks have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of one of the supplied tasks.  The return Task's Result is the task that completed.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument was null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> collection contained a null task, or was empty.
        /// </exception>
        public static Task<Task> WhenAny(IEnumerable<Task> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            // Take a more efficient path if tasks is actually an array
            return WhenAny(tasks as Task[] ?? tasks.ToArray());
        }

        /// <summary>
        /// Creates a task that will complete when any of the supplied tasks have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of one of the supplied tasks.  The return Task's Result is the task that completed.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument was null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> array contained a null task, or was empty.
        /// </exception>
        public static Task<Task> WhenAny(params Task[] tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            if (tasks.Length == 0)
                throw new ArgumentException(SR.Task_MultiTaskContinuation_EmptyTaskList, nameof(tasks));

            var promise = new WhenAnyPromise();
            promise.Start(tasks);
            return promise;
        }

        /// <summary>
        /// Creates a task that will complete when any of the supplied tasks have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of one of the supplied tasks.  The return Task's Result is the task that completed.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument was null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> collection contained a null task, or was empty.
        /// </exception>
        public static Task<Task<TResult>> WhenAny<TResult>(IEnumerable<Task<TResult>> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            // Take a more efficient path if tasks is actually an array
            return WhenAny(tasks as Task<TResult>[] ?? tasks.ToArray());
        }

        /// <summary>
        /// Creates a task that will complete when any of the supplied tasks have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of one of the supplied tasks.  The return Task's Result is the task that completed.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument was null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> array contained a null task, or was empty.
        /// </exception>
        public static Task<Task<TResult>> WhenAny<TResult>(params Task<TResult>[] tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            if (tasks.Length == 0)
                throw new ArgumentException(SR.Task_MultiTaskContinuation_EmptyTaskList, nameof(tasks));

            var promise = new WhenAnyPromise<TResult>();
            promise.Start(tasks);
            return promise;
        }

        #endregion

        #region Contingency

        protected sealed class ContingentProperties : IDisposable
        {
            /// <summary>
            /// The body of the task. Might be <see cref="Action"/>,
            /// <see cref="Action{T}"/>, <see cref="Func{TResult}"/> or
            /// <see cref="Func{T, TResult}"/>.
            /// </summary>
            public readonly Delegate m_action;

            /// <summary>
            /// A thread-safe event which notifies that current task is completed its execution.
            /// </summary>
            public ManualResetEventSlim m_taskCompletedEvent;

            public CancellationTokenRegistration m_cancelRegistration;

            public readonly Task m_parent;

            public ContingentProperties(Delegate action, Task parent)
            {
                m_action = action;
                m_parent = parent;

                // TODO: Should be lazy initialized
                //m_taskCompletedEvent = new ManualResetEventSlim();
            }

            public void Dispose()
            {
                m_taskCompletedEvent?.Dispose();
                m_cancelRegistration.Dispose();
            }

            /// <summary>
            /// Sets the internal completion event.
            /// </summary>
            internal void SetCompleted()
            {
                var mres = m_taskCompletedEvent;
                if (mres != null) mres.Set();
            }
        }

        #endregion
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** 
**
** Implementation details of CLR Contracts.
**
===========================================================*/
#define DEBUG // The behavior of this contract library should be consistent regardless of build type.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;

#if !NET35_CF
using Mock.System;
#endif

namespace System.Diagnostics.Contracts
{
    public static partial class Contract
    {
        #region Private Methods

        /// <summary>
        /// This method is used internally to trigger a failure indicating to the "programmer" that he is using the interface incorrectly.
        /// It is NEVER used to indicate failure of actual contracts at runtime.
        /// </summary>
        private static void AssertMustUseRewriter(ContractFailureKind kind, string contractKind)
        {
            // For better diagnostics, report which assembly is at fault.  Walk up stack and
            // find the first non-mscorlib assembly.
            Assembly thisAssembly = typeof(Contract).Assembly;  // In case we refactor mscorlib, use Contract class instead of Object.

            // TODO: Should implement StackTrace?
            //StackTrace stack = new StackTrace();
            //Assembly probablyNotRewritten = null;
            //for (int i = 0; i < stack.FrameCount; i++)
            //{
            //    Assembly caller = stack.GetFrame(i).GetMethod().DeclaringType.Assembly;
            //    if (caller != thisAssembly)
            //    {
            //        probablyNotRewritten = caller;
            //        break;
            //    }
            //}

            //if (probablyNotRewritten == null)
            //    probablyNotRewritten = thisAssembly;
            //string simpleName = probablyNotRewritten.GetName().Name;

            string simpleName = thisAssembly.GetName().Name;
            System.Runtime.CompilerServices.ContractHelper.TriggerFailure(kind, SR.Format(SR.MustUseCCRewrite, contractKind, simpleName), null, null, null);
        }

        #endregion Private Methods

        #region Failure Behavior

        /// <summary>
        /// Without contract rewriting, failing Assert/Assumes end up calling this method.
        /// Code going through the contract rewriter never calls this method. Instead, the rewriter produced failures call
        /// System.Runtime.CompilerServices.ContractHelper.RaiseContractFailedEvent, followed by 
        /// System.Runtime.CompilerServices.ContractHelper.TriggerFailure.
        /// </summary>
        [DebuggerNonUserCode]
        private static void ReportFailure(ContractFailureKind failureKind, string userMessage, string conditionText, Exception innerException)
        {
            if (failureKind < ContractFailureKind.Precondition || failureKind > ContractFailureKind.Assume)
                throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, failureKind), nameof(failureKind));

            // displayMessage == null means: yes we handled it. Otherwise it is the localized failure message
            string displayMessage = System.Runtime.CompilerServices.ContractHelper.RaiseContractFailedEvent(failureKind, userMessage, conditionText, innerException);

            if (displayMessage == null) return;

            System.Runtime.CompilerServices.ContractHelper.TriggerFailure(failureKind, displayMessage, userMessage, conditionText, innerException);
        }

        /// <summary>
        /// Allows a managed application environment such as an interactive interpreter (IronPython)
        /// to be notified of contract failures and 
        /// potentially "handle" them, either by throwing a particular exception type, etc.  If any of the
        /// event handlers sets the Cancel flag in the ContractFailedEventArgs, then the Contract class will
        /// not pop up an assert dialog box or trigger escalation policy.  Hooking this event requires 
        /// full trust, because it will inform you of bugs in the appdomain and because the event handler
        /// could allow you to continue execution.
        /// </summary>
        public static event EventHandler<ContractFailedEventArgs> ContractFailed
        {
            add
            {
                System.Runtime.CompilerServices.ContractHelper.InternalContractFailed += value;
            }
            remove
            {
                System.Runtime.CompilerServices.ContractHelper.InternalContractFailed -= value;
            }
        }
        #endregion FailureBehavior
    }

    public sealed class ContractFailedEventArgs : EventArgs
    {
        internal Exception thrownDuringHandler;

        public ContractFailedEventArgs(ContractFailureKind failureKind, string message, string condition, Exception originalException)
        {
            Debug.Assert(originalException == null || failureKind == ContractFailureKind.PostconditionOnException);
            FailureKind = failureKind;
            Message = message;
            Condition = condition;
            OriginalException = originalException;
        }

        public string Message { get; }
        public string Condition { get; }
        public ContractFailureKind FailureKind { get; }
        public Exception OriginalException { get; }

        // Whether the event handler "handles" this contract failure, or to fail via escalation policy.
        public bool Handled { get; private set; }

        public void SetHandled()
        {
            Handled = true;
        }

        public bool Unwind { get; private set; }

        public void SetUnwind()
        {
            Unwind = true;
        }
    }

    [Serializable]
    //[System.Runtime.CompilerServices.TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
#if CORERT
    public // On CoreRT this must be public to support binary serialization with type forwarding.
#else
    internal
#endif
    sealed class ContractException : Exception2
    {
        public ContractFailureKind Kind { get; }
        public string Failure => this.Message;
        public string UserMessage { get; }
        public string Condition { get; }

        // Called by COM Interop, if we see COR_E_CODECONTRACTFAILED as an HRESULT.
        private ContractException()
        {
            HResult = System.Runtime.CompilerServices.ContractHelper.COR_E_CODECONTRACTFAILED;
        }

        public ContractException(ContractFailureKind kind, string failure, string userMessage, string condition, Exception innerException)
            : base(failure, innerException)
        {
            HResult = System.Runtime.CompilerServices.ContractHelper.COR_E_CODECONTRACTFAILED;
            Kind = kind;
            UserMessage = userMessage;
            Condition = condition;
        }

        private ContractException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            Kind = (ContractFailureKind)info.GetInt32("Kind");
            UserMessage = info.GetString("UserMessage");
            Condition = info.GetString("Condition");
        }


        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Kind", Kind);
            info.AddValue("UserMessage", UserMessage);
            info.AddValue("Condition", Condition);
        }
    }
}


namespace System.Runtime.CompilerServices
{
    public static partial class ContractHelper
    {
        #region Private fields

        private static volatile EventHandler<ContractFailedEventArgs> contractFailedEvent;
        private static readonly object lockObject = new object();

        internal const int COR_E_CODECONTRACTFAILED = unchecked((int)0x80131542);

        #endregion

        /// <summary>
        /// Allows a managed application environment such as an interactive interpreter (IronPython) or a
        /// web browser host (Jolt hosting Silverlight in IE) to be notified of contract failures and 
        /// potentially "handle" them, either by throwing a particular exception type, etc.  If any of the
        /// event handlers sets the Cancel flag in the ContractFailedEventArgs, then the Contract class will
        /// not pop up an assert dialog box or trigger escalation policy.  Hooking this event requires 
        /// full trust.
        /// </summary>
        internal static event EventHandler<ContractFailedEventArgs> InternalContractFailed
        {
            add
            {
                // Eagerly prepare each event handler _marked with a reliability contract_, to 
                // attempt to reduce out of memory exceptions while reporting contract violations.
                // This only works if the new handler obeys the constraints placed on 
                // constrained execution regions.  Eagerly preparing non-reliable event handlers
                // would be a perf hit and wouldn't significantly improve reliability.
                // UE: Please mention reliable event handlers should also be marked with the 
                // PrePrepareMethodAttribute to avoid CER eager preparation work when ngen'ed.
                //System.Runtime.CompilerServices.RuntimeHelpers.PrepareContractedDelegate(value);
                lock (lockObject)
                {
                    contractFailedEvent += value;
                }
            }
            remove
            {
                lock (lockObject)
                {
                    contractFailedEvent -= value;
                }
            }
        }

        /// <summary>
        /// Rewriter will call this method on a contract failure to allow listeners to be notified.
        /// The method should not perform any failure (assert/throw) itself.
        /// This method has 3 functions:
        /// 1. Call any contract hooks (such as listeners to Contract failed events)
        /// 2. Determine if the listeners deem the failure as handled (then resultFailureMessage should be set to null)
        /// 3. Produce a localized resultFailureMessage used in advertising the failure subsequently.
        /// </summary>
        // On exit: null if the event was handled and should not trigger a failure.
        //          Otherwise, returns the localized failure message
        [System.Diagnostics.DebuggerNonUserCode]
        public static string RaiseContractFailedEvent(ContractFailureKind failureKind, string userMessage, string conditionText, Exception innerException)
        {
            if (failureKind < ContractFailureKind.Precondition || failureKind > ContractFailureKind.Assume)
                throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, failureKind), nameof(failureKind));

            string returnValue;
            string displayMessage = "contract failed.";  // Incomplete, but in case of OOM during resource lookup...
            ContractFailedEventArgs eventArgs = null;  // In case of OOM.

            try
            {
                displayMessage = GetDisplayMessage(failureKind, userMessage, conditionText);
                EventHandler<ContractFailedEventArgs> contractFailedEventLocal = contractFailedEvent;
                if (contractFailedEventLocal != null)
                {
                    eventArgs = new ContractFailedEventArgs(failureKind, displayMessage, conditionText, innerException);
                    foreach (EventHandler<ContractFailedEventArgs> handler in contractFailedEventLocal.GetInvocationList())
                    {
                        try
                        {
                            handler(null, eventArgs);
                        }
                        catch (Exception e)
                        {
                            eventArgs.thrownDuringHandler = e;
                            eventArgs.SetUnwind();
                        }
                    }
                    if (eventArgs.Unwind)
                    {
                        // unwind
                        if (innerException == null) { innerException = eventArgs.thrownDuringHandler; }
                        throw new ContractException(failureKind, displayMessage, userMessage, conditionText, innerException);
                    }
                }
            }
            finally
            {
                if (eventArgs != null && eventArgs.Handled)
                {
                    returnValue = null; // handled
                }
                else
                {
                    returnValue = displayMessage;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Rewriter calls this method to get the default failure behavior.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        public static void TriggerFailure(ContractFailureKind kind, string displayMessage, string userMessage, string conditionText, Exception innerException)
        {
            if (string.IsNullOrEmpty(displayMessage))
            {
                displayMessage = GetDisplayMessage(kind, userMessage, conditionText);
            }

            //System.Diagnostics.Debug.ContractFailure(false, displayMessage, string.Empty, GetResourceNameForFailure(kind));
            //throw new ContractException(kind, displayMessage, userMessage, conditionText, innerException);
            Debug.Assert(false, displayMessage, string.Empty);
        }

        private static string GetResourceNameForFailure(ContractFailureKind failureKind)
        {
            string resourceName = null;
            switch (failureKind)
            {
                case ContractFailureKind.Assert:
                    resourceName = "AssertionFailed";
                    break;

                case ContractFailureKind.Assume:
                    resourceName = "AssumptionFailed";
                    break;

                case ContractFailureKind.Precondition:
                    resourceName = "PreconditionFailed";
                    break;

                case ContractFailureKind.Postcondition:
                    resourceName = "PostconditionFailed";
                    break;

                case ContractFailureKind.Invariant:
                    resourceName = "InvariantFailed";
                    break;

                case ContractFailureKind.PostconditionOnException:
                    resourceName = "PostconditionOnExceptionFailed";
                    break;

                default:
                    Debug.Fail("Unreachable code");
                    resourceName = "AssumptionFailed";
                    break;
            }
            return resourceName;
        }

        private static string GetDisplayMessage(ContractFailureKind failureKind, string userMessage, string conditionText)
        {
            string failureMessage;
            // Well-formatted English messages will take one of four forms.  A sentence ending in
            // either a period or a colon, the condition string, then the message tacked 
            // on to the end with two spaces in front.
            // Note that both the conditionText and userMessage may be null.  Also, 
            // on Silverlight we may not be able to look up a friendly string for the
            // error message.  Let's leverage Silverlight's default error message there. 
            if (!string.IsNullOrEmpty(conditionText))
            {
                string resourceName = GetResourceNameForFailure(failureKind);
                resourceName += "_Cnd";
                failureMessage = SR.Format(SR.GetResourceString(resourceName), conditionText);
            }
            else
            {
                failureMessage = "";
            }

            // Now add in the user message, if present.
            return !string.IsNullOrEmpty(userMessage)
                ? failureMessage + "  " + userMessage
                : failureMessage;
        }
    }
}  // namespace System.Runtime.CompilerServices

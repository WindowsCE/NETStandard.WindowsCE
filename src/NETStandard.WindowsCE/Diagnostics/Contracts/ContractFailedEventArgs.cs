// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Diagnostics.Contracts
{
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
}
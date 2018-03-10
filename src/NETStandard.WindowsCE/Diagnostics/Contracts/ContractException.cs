// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if !NET35_CF
using Mock.System;
#endif

namespace System.Diagnostics.Contracts
{
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
        public string Failure => Message;
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
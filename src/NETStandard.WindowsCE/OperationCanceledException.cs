using System;
using System.Threading;
using NativeOperationCanceledException = System.OperationCanceledException;

namespace System
{
    public class OperationCanceledException
        : SystemException
    {
        private readonly CancellationToken token;

        public OperationCanceledException() { }

        public OperationCanceledException(CancellationToken cancellationToken)
        {
            token = cancellationToken;
        }

        public OperationCanceledException(string message)
            : base(message)
        { }

        public OperationCanceledException(string message, CancellationToken cancellationToken)
            : base(message)
        {
            token = cancellationToken;
        }

        public OperationCanceledException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public OperationCanceledException(string message, Exception innerException, CancellationToken cancellationToken)
            : base(message, innerException)
        {
            token = cancellationToken;
        }

        public CancellationToken CancellationToken
            => token;
    }
}

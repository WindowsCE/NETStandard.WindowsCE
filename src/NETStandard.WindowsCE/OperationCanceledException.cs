using System;
using System.Threading;
using NativeOperationCanceledException = System.OperationCanceledException;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public class OperationCanceledException
#if NET35_CF
        : SystemException
#else
        : NativeOperationCanceledException
#endif
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

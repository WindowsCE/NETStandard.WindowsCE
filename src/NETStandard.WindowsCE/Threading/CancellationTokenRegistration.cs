namespace System.Threading
{
    public struct CancellationTokenRegistration : IDisposable, IEquatable<CancellationTokenRegistration>
    {
        private readonly int _id;
        private CancellationTokenSource _source;

        internal CancellationTokenRegistration(int id, CancellationTokenSource source)
        {
            _id = id;
            _source = source;
        }

        public static bool operator !=(CancellationTokenRegistration left, CancellationTokenRegistration right)
        {
            return !left.Equals(right);
        }

        public static bool operator ==(CancellationTokenRegistration left, CancellationTokenRegistration right)
        {
            return left.Equals(right);
        }

        public void Dispose()
        {
            var source = _source;
            if (!ReferenceEquals(source, null))
            {
                if (source.RemoveCallback(this))
                {
                    _source = null;
                }
            }
        }

        public bool Equals(CancellationTokenRegistration other)
        {
            return _id == other._id && _source == other._source;
        }

        public override bool Equals(object obj)
        {
            return (obj is CancellationTokenRegistration) && Equals((CancellationTokenRegistration)obj);
        }

        public override int GetHashCode()
        {
            return _id;
        }
    }
}

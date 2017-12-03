namespace System.Threading
{
    public static class Timeout2
    {
        public static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, Timeout.Infinite);

        public const int Infinite = Timeout.Infinite;
    }
}

using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace System
{
    public static class Environment2
    {
        private static int processorCountException = 0;

        public static int CurrentManagedThreadId
        {
            [MethodImpl((MethodImplOptions)MethodImplOptions2.AggressiveInlining)]
            get { return Thread.CurrentThread.ManagedThreadId; }
        }

        public static bool HasShutdownStarted
            => false;

        public static string NewLine
            => Environment.NewLine;

        public static OperatingSystem OSVersion
            => Environment.OSVersion;

        public static int ProcessorCount
        {
            get
            {
                if (processorCountException > 0)
                    return 1;

                try
                {
                    PInvoke.SystemInfo systemInfo;
                    PInvoke.NativeMethods.GetSystemInfo(out systemInfo);
                    return systemInfo.NumberOfProcessors;
                }
                catch
                {
                    Interlocked.Increment(ref processorCountException);
                    return 1;
                }
            }
        }

        public static int TickCount
            => Environment.TickCount;

        public static Version Version
            => Environment.Version;

        public static void FailFast(string message)
            => FailFast(message, null);

        public static void FailFast(string message, Exception exception)
        {
            var output = new StringBuilder();
            string timestamp = string.Format("[{0}] ", DateTime.Now.ToString("o"));
            if (!string.IsNullOrEmpty(message))
                output.Append(timestamp).AppendLine(message);
            if (exception != null)
                output.Append(timestamp).AppendLine(exception.ToString());

            using (var reader = File.AppendText("error.log"))
            {
                reader.Write(output.ToString());
                reader.Flush();
            }
        }

        public static string GetFolderPath(Environment.SpecialFolder folder)
            => Environment.GetFolderPath(folder);
    }
}

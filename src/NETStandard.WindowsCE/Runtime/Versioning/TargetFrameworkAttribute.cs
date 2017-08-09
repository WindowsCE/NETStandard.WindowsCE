using System;

#if NET35_CF
namespace System.Runtime.Versioning
#else
namespace Mock.System.Runtime.Versioning
#endif
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class TargetFrameworkAttribute : Attribute
    {
        private readonly string _frameworkName;

        public TargetFrameworkAttribute(string frameworkName)
        {
            if (frameworkName == null)
                throw new ArgumentNullException(nameof(frameworkName));

            _frameworkName = frameworkName;
        }

        public string FrameworkDisplayName { get; set; }

        public string FrameworkName
            => _frameworkName;
    }
}

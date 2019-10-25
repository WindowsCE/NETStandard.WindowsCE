using System;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

#if !NET35_CF
using Mock.System.Runtime.Versioning;
#endif

[assembly: CLSCompliant(true)]
[assembly: TargetFramework(Consts.TargetFramework)]
[assembly: InternalsVisibleTo("Tasks.Tests, PublicKey=" + Consts.SkarllotPublicKey)]

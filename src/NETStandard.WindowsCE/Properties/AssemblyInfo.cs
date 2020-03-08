using System;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

[assembly: CLSCompliant(true)]
[assembly: TargetFramework(Consts.TargetFramework)]
[assembly: InternalsVisibleTo("Tasks.Tests, PublicKey=" + Consts.SkarllotPublicKey)]

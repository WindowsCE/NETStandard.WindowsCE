using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

#if !NET35_CF
using Mock.System.Runtime.Versioning;
#endif

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany(Consts.Company)]
[assembly: AssemblyCopyright(Consts.Copyright)]
[assembly: AssemblyProduct(Consts.Product)]
[assembly: AssemblyDescription(Consts.Description)]
[assembly: CLSCompliant(true)]
[assembly: TargetFramework(Consts.TargetFramework)]

[assembly: AssemblyVersion(Consts.AssemblyVersion)]
[assembly: AssemblyInformationalVersion(Consts.ProductVersion)]
[assembly: AssemblyFileVersion(Consts.FileVersion)]

[assembly: AssemblyConfiguration(Consts.Configuration)]
[assembly: InternalsVisibleTo("Tasks.Tests, PublicKey=" + Consts.SkarllotPublicKey)]

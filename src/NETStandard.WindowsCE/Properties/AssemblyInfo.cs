using System;
using System.Reflection;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany("Fabrício Godoy")]
[assembly: AssemblyCopyright("© Fabrício Godoy. All rights reserved.")]
[assembly: AssemblyProduct("NETStandard.WindowsCE")]
[assembly: AssemblyDescription("A set of standard .NET APIs that are prescribed to be used and supported together")]
[assembly: CLSCompliant(true)]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]
[assembly: AssemblyFileVersion("1.0.0")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Retail")]
#endif

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

#if NET35_CF
namespace System.Globalization
#else
namespace Mock.System.Globalization
#endif
{
    [Flags]
    public enum TimeSpanStyles
    {
        None = 0x00000000,
        AssumeNegative = 0x00000001,
    }
}
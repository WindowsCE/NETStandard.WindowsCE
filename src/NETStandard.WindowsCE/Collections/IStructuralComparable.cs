// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;

#if NET35_CF
namespace System.Collections
#else
namespace Mock.System.Collections
#endif
{
    public interface IStructuralComparable
    {
        int CompareTo(object other, IComparer comparer);
    }
}
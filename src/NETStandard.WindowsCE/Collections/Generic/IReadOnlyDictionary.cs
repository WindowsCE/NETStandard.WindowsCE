// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** Interface:  IReadOnlyDictionary<TKey, TValue>
** 
** 
**
** Purpose: Base interface for read-only generic dictionaries.
** 
===========================================================*/

using System.Collections.Generic;

#if NET35_CF
namespace System.Collections.Generic
#else
namespace Mock.System.Collections.Generic
#endif
{
    /// <summary>
    /// Provides a read-only view of a generic dictionary.
    /// </summary>
    public interface IReadOnlyDictionary<TKey, TValue>
        : IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    {
        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue value);

        TValue this[TKey key] { get; }
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }
    }
}

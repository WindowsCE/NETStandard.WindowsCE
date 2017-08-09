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

namespace System.Collections.Generic
{
    /// <summary>
    /// Provides a read-only view of a generic dictionary.
    /// </summary>
    public interface IReadOnlyDictionary2<TKey, TValue>
        : IReadOnlyCollection2<KeyValuePair<TKey, TValue>>
    {
        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue value);

        TValue this[TKey key] { get; }
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }
    }
}

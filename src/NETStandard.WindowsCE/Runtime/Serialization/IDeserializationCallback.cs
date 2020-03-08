// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** Interface: IDeserializationEventListener
**
**
** Purpose: Implemented by any class that wants to indicate that
**          it wishes to receive deserialization events.
**
**
===========================================================*/
namespace System.Runtime.Serialization
{
    /// <summary>
    /// Indicates that a class is to be notified when deserialization of the
    /// entire object graph has been completed.
    /// </summary>
    public interface IDeserializationCallback
    {
        /// <summary>
        /// Runs when the entire object graph has been deserialized.
        /// </summary>
        /// <param name="sender">
        /// <para>The object that initiated the callback.</para>
        /// <para>The functionality for this parameter is not currently implemented.</para>
        /// </param>
        void OnDeserialization(object sender);
    }
}

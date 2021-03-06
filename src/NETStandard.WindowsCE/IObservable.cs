﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** 
** 
**
**
** Purpose: Interface for exposing an Observable in the 
** Observer pattern
**
**
===========================================================*/

using System;

namespace System
{
    /// <summary>
    /// Interface for exposing an Observable in the
    /// Observer pattern.
    /// </summary>
    public interface IObservable<out T>
    {
        IDisposable Subscribe(IObserver<T> observer);
    }

}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

#if NET35_CF
namespace System.Threading
#else
namespace Mock.System.Threading
#endif
{
    //
    // Methods for accessing memory with volatile semantics.  These are preferred over Thread.VolatileRead
    // and Thread.VolatileWrite, as these are implemented more efficiently.
    //
    // (We cannot change the implementations of Thread.VolatileRead/VolatileWrite without breaking code
    // that relies on their overly-strong ordering guarantees.)
    //
    // The actual implementations of these methods are typically supplied by the VM at JIT-time, because C# does
    // not allow us to express a volatile read/write from/to a byref arg.
    // See getILIntrinsicImplementationForVolatile() in jitinterface.cpp.
    //
    /// <summary>
    /// Methods for accessing memory with volatile semantics.
    /// </summary>
    public static class Volatile
    {
        public static bool Read(ref bool location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [CLSCompliant(false)]
        public static sbyte Read(ref sbyte location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        public static byte Read(ref byte location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        public static short Read(ref short location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [CLSCompliant(false)]
        public static ushort Read(ref ushort location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        public static int Read(ref int location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [CLSCompliant(false)]
        public static uint Read(ref uint location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        public static long Read(ref long location)
        {
            var value = location;
            Thread.MemoryBarrier();
            value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [CLSCompliant(false)]
        public static ulong Read(ref ulong location)
        {
            var value = location;
            Thread.MemoryBarrier();
            value = location;
            Thread.MemoryBarrier();
            return value;
        }

        public static IntPtr Read(ref IntPtr location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [CLSCompliant(false)]
        public static UIntPtr Read(ref UIntPtr location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        public static float Read(ref float location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        public static double Read(ref double location)
        {
            var value = location;
            Thread.MemoryBarrier();
            value = location;
            Thread.MemoryBarrier();
            return value;
        }

        public static T Read<T>(ref T location) where T : class
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }




        public static void Write(ref bool location, bool value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [CLSCompliant(false)]
        public static void Write(ref sbyte location, sbyte value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        public static void Write(ref byte location, byte value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        public static void Write(ref short location, short value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [CLSCompliant(false)]
        public static void Write(ref ushort location, ushort value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        public static void Write(ref int location, int value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [CLSCompliant(false)]
        public static void Write(ref uint location, uint value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        public static void Write(ref long location, long value)
        {
            Thread.MemoryBarrier();
            location = value;
            Thread.MemoryBarrier();
            location = value;
        }

        [CLSCompliant(false)]
        public static void Write(ref ulong location, ulong value)
        {
            Thread.MemoryBarrier();
            location = value;
            Thread.MemoryBarrier();
            location = value;
        }

        public static void Write(ref IntPtr location, IntPtr value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [CLSCompliant(false)]
        public static void Write(ref UIntPtr location, UIntPtr value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        public static void Write(ref float location, float value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        public static void Write(ref double location, double value)
        {
            Thread.MemoryBarrier();
            location = value;
            Thread.MemoryBarrier();
            location = value;
        }

        public static void Write<T>(ref T location, T value) where T : class
        {
            Thread.MemoryBarrier();
            location = value;
        }
    }
}
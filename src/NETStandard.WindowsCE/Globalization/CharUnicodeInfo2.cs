// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

////////////////////////////////////////////////////////////////////////////
//
//
//  Purpose:  This class implements a set of methods for retrieving
//            character type information.  Character type information is
//            independent of culture and region.
//
//
////////////////////////////////////////////////////////////////////////////

using System;

#if NET35_CF
namespace System.Globalization
#else
namespace Mock.System.Globalization
#endif
{
    internal class CharUnicodeInfo2
    {
        //--------------------------------------------------------------------//
        //                        Internal Information                        //
        //--------------------------------------------------------------------//

        //
        // Native methods to access the Unicode category data tables in charinfo.nlp.
        //
        internal const char HIGH_SURROGATE_START = '\ud800';
        internal const char HIGH_SURROGATE_END = '\udbff';
        internal const char LOW_SURROGATE_START = '\udc00';
        internal const char LOW_SURROGATE_END = '\udfff';
    }
}

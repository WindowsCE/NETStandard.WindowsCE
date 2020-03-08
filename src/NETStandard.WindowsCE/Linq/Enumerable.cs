using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ***** Regexes *****
// - No parameters
// public static ([A-z0-9<,>]+\??) (\w+)(<[A-z, ]+>)?\(this IEnumerable<([A-z0-9?]+)> source\);
// public static $1 $2$3(this IEnumerable<$4> source)\r\n            => Enumerable.$2(source);\r\n
//
// - Single parameter
// public static ([A-z0-9<,>]+\??) (\w+)(<[A-z, ]+>)\(this IEnumerable<([A-z0-9?]+)> source, ([A-z0-9<,>? ]+) (\w+)\);
// public static $1 $2$3(this IEnumerable<$4> source, $5 $6)\r\n            => Enumerable.$2(source, $6);\r\n

namespace System.Linq
{
    public static partial class Enumerable2
    {
    }
}

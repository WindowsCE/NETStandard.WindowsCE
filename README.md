# NETStandard.WindowsCE
A library that implements the API of .NET Standard specification on Microsoft Windows CE platform.

## Getting Started
Install the [NuGet package](https://www.nuget.org/packages/NETStandard.WindowsCE/)

## Extending existing classes

The naming of classes that extends existing types are the same but appended '2' digit, as `Activator2` and `Int322`.
Instance methods are implemented using _extension methods_ and class methods as static method.

```csharp
using System;

#if !WindowsCE
using static System.Int32;
#else
using static System.Int322;
#endif

namespace Tests
{
    static class Program
    {
        static void Main(string[])
        {
            int result;
            if (!TryParse("123", NumberStyles.None, null, out result))
                System.Console.WriteLine("Could not parse provided string");
            else
                System.Console.WriteLine("The provided string could be parsed");
            
            const string text = "Lorem ipsum dolor sit amet";
            string croppedText = text.Remove(11);
            System.Console.WriteLine("The text after processing: {0}", croppedText);
        }
    }
}
```

## Not Supported API

The API that is not supported on Microsoft Windows CE platform throw `PlatformNotSupportedException` exception when called
and are marked as deprecated using `ObsoleteAttribute` attribute.

![VS showing deprecated message for not supported API](docs/not-supported.png)

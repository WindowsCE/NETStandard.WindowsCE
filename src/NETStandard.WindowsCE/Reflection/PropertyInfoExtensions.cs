using System.ComponentModel;

#nullable enable

namespace System.Reflection
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PropertyInfoExtensions
    {
        public static object? GetValue(this PropertyInfo propertyInfo, object? obj)
        {
            return propertyInfo.GetValue(obj, null);
        }
    }
}

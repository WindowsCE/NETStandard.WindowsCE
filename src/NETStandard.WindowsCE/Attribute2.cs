#nullable enable

namespace System
{
    public class Attribute2
        : Attribute
    {
        public virtual object TypeId => GetType();

        public virtual bool IsDefaultAttribute() => false;
    }
}

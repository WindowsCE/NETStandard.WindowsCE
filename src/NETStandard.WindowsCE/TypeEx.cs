using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class TypeEx
    {
        public static readonly char Delimiter = Type.Delimiter;
        public static readonly Type[] EmptyTypes = new Type[0];
        public static readonly object Missing = Type.Missing;

        public static Binder DefaultBinder
            => Type.DefaultBinder;

        public static Type GetType(string typeName, bool throwOnError, bool ignoreCase)
            => Type.GetType(typeName, throwOnError, ignoreCase);
        public static Type GetType(string typeName, bool throwOnError)
            => Type.GetType(typeName, throwOnError);
        public static Type GetType(string typeName)
            => Type.GetType(typeName);
        public static Type GetTypeFromCLSID(Guid clsid, bool throwOnError)
            => Type.GetTypeFromCLSID(clsid, throwOnError);
        public static Type GetTypeFromCLSID(Guid clsid)
            => Type.GetTypeFromCLSID(clsid);
        public static Type GetTypeFromHandle(RuntimeTypeHandle handle)
            => Type.GetTypeFromHandle(handle);
        public static Type GetTypeFromProgID(string progID, bool throwOnError)
            => Type.GetTypeFromProgID(progID, throwOnError);
        public static Type GetTypeFromProgID(string progID)
            => Type.GetTypeFromProgID(progID);
        public static TypeCode GetTypeCode(Type type)
            => Type.GetTypeCode(type);

        public static Type GetInterface(this Type t, string name)
            => GetInterface(t, name, false);

        public static Type GetInterface(this Type t, string name, bool ignoreCase)
        {
            StringComparison cmp = ignoreCase ?
                StringComparison.OrdinalIgnoreCase :
                StringComparison.Ordinal;


            Type[] ifaces = t.GetInterfaces();
            for (int i = 0; i < ifaces.Length; i++)
            {
                if (ifaces[i].Name.Equals(name, cmp))
                    return ifaces[i];
            }

            return null;
        }

        public static MemberInfo[] GetMember(this Type t, string name, MemberTypes type, BindingFlags bindingAttr)
        {
            MemberInfo[] mInfos = t.GetMember(name, bindingAttr);
            if (mInfos.Length == 0)
                return mInfos;

            List<MemberInfo> fMInfos = null;
            for (int i = 0; i < mInfos.Length; i++)
            {
                var mTypes = mInfos[i].MemberType;
                if ((type & mTypes) != mTypes)
                {
                    if (fMInfos == null)
                    {
                        fMInfos = new List<MemberInfo>(mInfos.Length);
                        fMInfos.AddRange(mInfos.Take(i));
                    }
                }
                else if (fMInfos != null)
                {
                    fMInfos.Add(mInfos[i]);
                }
            }

            if (fMInfos == null)
                return mInfos;
            else
                return fMInfos.ToArray();
        }

        public static bool IsSerializable(this Type t)
        {
            return (t.Attributes & TypeAttributes.Serializable)
                == TypeAttributes.Serializable;
        }

        public static Type MakeArrayType(this Type t)
            => MakeArrayType(t, 1);

        public static Type MakeArrayType(this Type t, int rank)
        {
            if (rank <= 0)
                throw new IndexOutOfRangeException();

            string strRank = '[' + new string(',', rank - 1) + ']';
            string fName = t.FullName;
            string qName = t.AssemblyQualifiedName;
            qName = qName.Replace(fName, fName + strRank);

            Type arrayType = Type.GetType(qName, true, false);
            if (!arrayType.IsArray)
                throw new InvalidOperationException("Could not make a valid array type");

            return arrayType;
        }

        public static Type MakeByRefType(this Type t)
        {
            string fName = t.FullName;
            string qName = t.AssemblyQualifiedName;
            qName = qName.Replace(fName, fName + '&');

            Type byrefType = Type.GetType(qName, true, false);
            if (!byrefType.IsByRef)
                throw new InvalidOperationException("Could not make a valid by reference type");

            return byrefType;
        }

        public static Type MakePointerType(this Type t)
        {
            string fName = t.FullName;
            string qName = t.AssemblyQualifiedName;
            qName = qName.Replace(fName, fName + '*');

            Type ptrType = Type.GetType(qName, true, false);
            if (!ptrType.IsPointer)
                throw new InvalidOperationException("Could not make a valid pointer type");

            return ptrType;
        }
    }
}

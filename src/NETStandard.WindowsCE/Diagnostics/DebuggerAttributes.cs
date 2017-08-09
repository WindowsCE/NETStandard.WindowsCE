// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
**
**
** Purpose: Attributes for debugger
**
**
===========================================================*/

using System;
using System.Runtime.InteropServices;

#if NET35_CF
namespace System.Diagnostics
#else
namespace Mock.System.Diagnostics
#endif
{
    //  DebuggerBrowsableState states are defined as follows:
    //      Never       never show this element
    //      Expanded    expansion of the class is done, so that all visible internal members are shown
    //      Collapsed   expansion of the class is not performed. Internal visible members are hidden
    //      RootHidden  The target element itself should not be shown, but should instead be 
    //                  automatically expanded to have its members displayed.
    //  Default value is collapsed

    //  Please also change the code which validates DebuggerBrowsableState variable (in this file)
    //  if you change this enum.
    [ComVisible(true)]
    public enum DebuggerBrowsableState
    {
        Never = 0,
        //Expanded is not supported in this release
        //Expanded = 1, 
        Collapsed = 2,
        RootHidden = 3
    }


    // the one currently supported with the csee.dat 
    // (mcee.dat, autoexp.dat) file. 
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    [ComVisible(true)]
    public sealed class DebuggerBrowsableAttribute : Attribute
    {
        private readonly DebuggerBrowsableState _state;

        public DebuggerBrowsableAttribute(DebuggerBrowsableState state)
        {
            if (state < DebuggerBrowsableState.Never || state > DebuggerBrowsableState.RootHidden)
                throw new ArgumentOutOfRangeException(nameof(state));

            _state = state;
        }
        public DebuggerBrowsableState State
        {
            get { return _state; }
        }
    }


    // DebuggerTypeProxyAttribute
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    [ComVisible(true)]
    public sealed class DebuggerTypeProxyAttribute : Attribute
    {
        private readonly string _typeName;
        private string _targetName;
        private Type _target;

        public DebuggerTypeProxyAttribute(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            _typeName = type.AssemblyQualifiedName;
        }

        public DebuggerTypeProxyAttribute(string typeName)
        {
            _typeName = typeName;
        }
        public string ProxyTypeName
        {
            get { return _typeName; }
        }

        public Type Target
        {
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _targetName = value.AssemblyQualifiedName;
                _target = value;
            }

            get { return _target; }
        }

        public string TargetTypeName
        {
            get { return _targetName; }
            set { _targetName = value; }

        }
    }

    // This attribute is used to control what is displayed for the given class or field 
    // in the data windows in the debugger.  The single argument to this attribute is
    // the string that will be displayed in the value column for instances of the type.  
    // This string can include text between { and } which can be either a field, 
    // property or method (as will be documented in mscorlib).  In the C# case, 
    // a general expression will be allowed which only has implicit access to the this pointer 
    // for the current instance of the target type. The expression will be limited, 
    // however: there is no access to aliases, locals, or pointers. 
    // In addition, attributes on properties referenced in the expression are not processed.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Assembly, AllowMultiple = true)]
    [ComVisible(true)]
    public sealed class DebuggerDisplayAttribute : Attribute
    {
        private string _name;
        private readonly string _value;
        private string _type;
        private string _targetName;
        private Type _target;

        public DebuggerDisplayAttribute(string value)
        {
            if (value == null)
            {
                _value = "";
            }
            else
            {
                _value = value;
            }
            _name = "";
            _type = "";
        }

        public string Value
        {
            get { return _value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public Type Target
        {
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _targetName = value.AssemblyQualifiedName;
                _target = value;
            }
            get { return _target; }
        }

        public string TargetTypeName
        {
            get { return _targetName; }
            set { _targetName = value; }

        }
    }
}


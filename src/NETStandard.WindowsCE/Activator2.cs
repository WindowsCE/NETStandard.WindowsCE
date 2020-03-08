using System;
using System.Globalization;
using System.Reflection;

#nullable enable

namespace System
{
    public static class Activator2
    {
        private const BindingFlags ConstructorDefault = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;
        private static readonly Type NullableType = typeof(Nullable<>);

        public static object? CreateInstance(Type type)
            => Activator.CreateInstance(type);

        public static object? CreateInstance(Type type, bool nonPublic) =>
            CreateInstance(type, ConstructorDefault | BindingFlags.NonPublic, null, null, null, null);

        public static object? CreateInstance(Type type, BindingFlags bindingAttr, Binder? binder, object?[]? args, CultureInfo? culture) =>
            CreateInstance(type, bindingAttr, binder, args, culture, null);

        public static object? CreateInstance(Type type, params object?[]? args) =>
            CreateInstance(type, ConstructorDefault, null, args, null, null);

        public static object? CreateInstance(Type type, object?[]? args, object?[]? activationAttributes) =>
            CreateInstance(type, ConstructorDefault, null, args, null, activationAttributes);

        public static object? CreateInstance(Type type, BindingFlags bindingAttr, Binder? binder, object?[]? args, CultureInfo? culture, object?[]? activationAttributes)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // If they didn't specify a lookup, then we will provide the default lookup.
            const int LookupMask = 0x000000FF;
            if ((bindingAttr & (BindingFlags)LookupMask) == 0)
                bindingAttr |= ConstructorDefault;

            if (activationAttributes?.Length > 0)
                throw new PlatformNotSupportedException(SR.NotSupported_ActivAttr);

            Type?[] argsType = args != null ? new Type?[args.Length] : Type2.EmptyTypes;
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                    argsType[i] = args[i]?.GetType();
            }

            ConstructorInfo? foundMatch = null;
            bool foundMatchIsVarArgs = false;
            ParameterInfo[]? foundMatchParameters = null;
            foreach (var ctor in type.GetConstructors(bindingAttr))
            {
                var parameters = ctor.GetParameters();

                bool isVarArgs = false;
                if (parameters.Length > 0)
                {
                    isVarArgs = parameters[parameters.Length - 1]
                        .GetCustomAttributes(typeof(ParamArrayAttribute), false)
                        .Length > 0;
                }

                bool isMatch;
                if (!isVarArgs)
                    isMatch = ParametersMatch(parameters, argsType);
                else
                    isMatch = VariableParametersMatch(parameters, argsType);

                if (isMatch)
                {
                    if (foundMatch != null)
                        throw new AmbiguousMatchException("Ambiguous match found calling constructor");

                    foundMatch = ctor;
                    foundMatchIsVarArgs = isVarArgs;
                    foundMatchParameters = isVarArgs ? parameters : null;
                }
            }

            if (foundMatch == null)
                throw new MissingMethodException("No matching constructor was found for specified parameters");

            if (!foundMatchIsVarArgs)
                return foundMatch.Invoke(bindingAttr, binder, args, culture);
            else
                return foundMatch.Invoke(bindingAttr, binder, FlattenVarArgs(args, foundMatchParameters!), culture);
        }

        private static bool ParametersMatch(ParameterInfo[] parameters, Type?[] argsType)
        {
            if (parameters.Length != argsType.Length)
                return false;

            for (int i = 0; i < argsType.Length; i++)
            {
                if (!IsTypeAssignableFrom(parameters[i].ParameterType, argsType[i]))
                    return false;
            }

            return true;
        }

        private static bool VariableParametersMatch(ParameterInfo[] parameters, Type?[] argsType)
        {
            if (argsType.Length < parameters.Length - 1)
                return false;

            for (int i = 0; i < parameters.Length - 1; i++)
            {
                if (!IsTypeAssignableFrom(parameters[i].ParameterType, argsType[i]))
                    return false;
            }

            if (argsType.Length == parameters.Length - 1)
                return true;

            var varArgType = parameters[parameters.Length - 1].ParameterType;
            // Reject unexpected ParamArrayAttribute
            if (!varArgType.IsArray)
                return false;

            var varArgElementType = varArgType.GetElementType();
            for (int i = parameters.Length - 1; i < argsType.Length; i++)
            {
                if (!IsTypeAssignableFrom(varArgElementType, argsType[i]))
                    return false;
            }

            return true;
        }

        private static bool IsTypeAssignableFrom(Type refType, Type? instanceType)
        {
            if (refType == instanceType)
                return true;

            // Check null instances and Nullable structs
            if (instanceType == null)
            {
                if (!refType.IsValueType)
                    return true;
                if (!refType.IsGenericType)
                    return false;
                if (refType.GetGenericTypeDefinition() != NullableType)
                    return false;
            }

            if (refType.IsAssignableFrom(instanceType))
                return true;

            if (!refType.IsPrimitive && typeof(decimal) != refType)
                return false;

            // Primitive widening
            var refTypeCode = Type.GetTypeCode(refType);
            var instanceTypeCode = Type.GetTypeCode(instanceType);

            switch (refTypeCode)
            {
                case TypeCode.Boolean:
                    return false;
                case TypeCode.Char:
                    return false;
                case TypeCode.SByte:
                    return false;
                case TypeCode.Byte:
                    return instanceTypeCode == TypeCode.SByte;
                case TypeCode.Int16:
                    return instanceTypeCode == TypeCode.SByte ||
                        instanceTypeCode == TypeCode.Byte;
                case TypeCode.UInt16:
                    return instanceTypeCode == TypeCode.SByte ||
                        instanceTypeCode == TypeCode.Byte;
                case TypeCode.Int32:
                    return instanceTypeCode >= TypeCode.SByte &&
                        instanceTypeCode <= TypeCode.UInt16;
                case TypeCode.UInt32:
                    return instanceTypeCode >= TypeCode.SByte &&
                        instanceTypeCode <= TypeCode.UInt16;
                case TypeCode.Int64:
                    return instanceTypeCode >= TypeCode.SByte &&
                        instanceTypeCode <= TypeCode.UInt32;
                case TypeCode.UInt64:
                    return instanceTypeCode >= TypeCode.SByte &&
                        instanceTypeCode <= TypeCode.UInt32;
                case TypeCode.Single:
                    return instanceTypeCode >= TypeCode.SByte &&
                        instanceTypeCode <= TypeCode.UInt64;
                case TypeCode.Double:
                    return instanceTypeCode >= TypeCode.SByte &&
                        instanceTypeCode <= TypeCode.Single;
                case TypeCode.Decimal:
                    return instanceTypeCode >= TypeCode.SByte &&
                        instanceTypeCode <= TypeCode.Double;
                default:
                    return false;
            }
        }

        private static object?[]? FlattenVarArgs(object?[]? args, ParameterInfo[] parameters)
        {
            if (args == null)
                return null;

            var result = new object[parameters.Length];
            Array.Copy(args, result, result.Length - 1);
            int varArgsIndex = parameters.Length - 1;
            var varArgElementType = parameters[parameters.Length - 1]
                .ParameterType
                .GetElementType();

            int varArgsLength = args.Length - (parameters.Length - 1);
            if (varArgsLength == -1)
                varArgsLength = 0;

            var varArgs = Array.CreateInstance(varArgElementType, varArgsLength);
            result[varArgsIndex] = varArgs;

            if (varArgsLength > 0)
                Array.Copy(args, varArgsIndex, varArgs, 0, varArgsLength);

            return result;
        }

        public static T CreateInstance<T>()
            => Activator.CreateInstance<T>();
    }
}

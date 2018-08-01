using System;
using System.Globalization;
using System.Linq;
using SystemMath = System.Math;

#if NET35_CF
namespace System.Linq.Expressions.Jvm
#else
namespace Mock.System.Linq.Expressions.Jvm
#endif
{
    internal class Math
    {
        private static readonly Type NullableType = typeof(Nullable<>);

        public static object Evaluate(object a, object b, Type t, ExpressionType et)
        {
            TypeCode tc = Type.GetTypeCode(t);
            if (tc != TypeCode.Object)
                return Evaluate(a, b, tc, et);

            return t.GetGenericTypeDefinition() == NullableType
                ? EvaluateNullable(a, b, Type.GetTypeCode(t.GetGenericArguments()[0]), et)
                : throw new NotImplementedException($"Expression with Node type {t.FullName} for type {tc}");
        }

        public static object EvaluateNullable(object a, object b, TypeCode tc, ExpressionType et)
        {
            object o = null;
            if (a == null || b == null)
            {
                if (tc != TypeCode.Boolean)
                {
                    return null;
                }

                switch (et)
                {
                    case ExpressionType.And:
                        o = And(a, b);
                        break;
                    case ExpressionType.Or:
                        o = Or(a, b);
                        break;
                    case ExpressionType.ExclusiveOr:
                        o = ExclusiveOr(a, b);
                        break;
                }
            }
            else
            {
                o = Evaluate(a, b, tc, et);
            }

            return Convert2Nullable(o, tc);
        }

        private static object ExclusiveOr(object a, object b)
        {
            return a != null && b != null
                ? (bool)a ^ (bool)b
                : (object)null;
        }

        public static object Or(object a, object b)
        {
            if (a == null)
            {
                return b != null && (bool)b
                    ? true
                    : (object)null;
            }

            if (b == null)
            {
                return (bool)a
                    ? true
                    : (object)null;
            }

            return (bool)a || (bool)b;
        }

        public static object And(object a, object b)
        {
            if (a == null)
            {
                return b != null && !(bool)b
                    ? false
                    : (object)null;
            }

            if (b == null)
            {
                return !(bool)a
                    ? false
                    : (object)null;
            }

            return (bool)a && (bool)b;
        }

        private static object Convert2Nullable(object o, TypeCode tc)
        {
            if (o == null)
            {
                return null;
            }

            switch (tc)
            {
                case TypeCode.Char:
                    return (char)o;
                case TypeCode.Byte:
                    return (byte)o;
                case TypeCode.Decimal:
                    return (decimal)o;
                case TypeCode.Double:
                    return (double)o;
                case TypeCode.Int16:
                    return (short)o;
                case TypeCode.Int32:
                    return (int)o;
                case TypeCode.Int64:
                    return (long)o;
                case TypeCode.UInt16:
                    return (ushort)o;
                case TypeCode.UInt32:
                    return (uint)o;
                case TypeCode.SByte:
                    return (sbyte)o;
                case TypeCode.Single:
                    return (float)o;
                case TypeCode.Boolean:
                    return (bool)o;
            }

            throw new NotImplementedException($"No Convert2Nullable defined for type {tc} ");
        }

        public static object Evaluate(object a, object b, TypeCode tc, ExpressionType et)
        {
            switch (tc)
            {
                case TypeCode.Boolean:
                    return Evaluate(Convert.ToBoolean(a), Convert.ToBoolean(b), et);
                case TypeCode.Char:
                    return Evaluate(Convert.ToChar(a), Convert.ToChar(b), et);
                case TypeCode.Byte:
                    return unchecked((byte)Evaluate(Convert.ToByte(a), Convert.ToByte(b), et));
                case TypeCode.Decimal:
                    return Evaluate(Convert.ToDecimal(a), Convert.ToDecimal(b), et);
                case TypeCode.Double:
                    return Evaluate(Convert.ToDouble(a), Convert.ToDouble(b), et);
                case TypeCode.Int16:
                    return unchecked((short)Evaluate(Convert.ToInt16(a), Convert.ToInt16(b), et));
                case TypeCode.Int32:
                    return Evaluate(Convert.ToInt32(a), Convert.ToInt32(b), et);
                case TypeCode.Int64:
                    return Evaluate(Convert.ToInt64(a), Convert.ToInt64(b), et);
                case TypeCode.UInt16:
                    return unchecked((ushort)Evaluate(Convert.ToUInt16(a), Convert.ToUInt16(b), et));
                case TypeCode.UInt32:
                    return Evaluate(Convert.ToUInt32(a), Convert.ToUInt32(b), et);
                case TypeCode.UInt64:
                    return Evaluate(Convert.ToUInt64(a), Convert.ToUInt64(b), et);
                case TypeCode.SByte:
                    return unchecked((sbyte)Evaluate(Convert.ToSByte(a), Convert.ToSByte(b), et));
                case TypeCode.Single:
                    return Evaluate(Convert.ToSingle(a), Convert.ToSingle(b), et);

            }

            throw new NotImplementedException($"Expression with Node type {et} for type {tc}");
        }

        public static object NegeteChecked(object a, TypeCode tc)
        {
            switch (tc)
            {
                case TypeCode.Char:
                    return checked(-Convert.ToChar(a));
                case TypeCode.Byte:
                    return checked(-Convert.ToByte(a));
                case TypeCode.Decimal:
                    return -Convert.ToDecimal(a);
                case TypeCode.Double:
                    return -Convert.ToDouble(a);
                case TypeCode.Int16:
                    return checked(-Convert.ToInt16(a));
                case TypeCode.Int32:
                    return checked(-Convert.ToInt32(a));
                case TypeCode.Int64:
                    return checked(-Convert.ToInt64(a));
                case TypeCode.UInt16:
                    return checked(-Convert.ToUInt16(a));
                case TypeCode.UInt32:
                    return checked(-Convert.ToUInt32(a));
                case TypeCode.SByte:
                    return checked(-Convert.ToSByte(a));
                case TypeCode.Single:
                    return -Convert.ToSingle(a);
            }

            throw new NotImplementedException($"No NegeteChecked defined for type {tc} ");

        }

        private static object CreateInstance(Type type, params object[] arguments)
        {
            var argumentsTypes = arguments.Select(argument => argument.GetType()).ToArray();
            return type.GetConstructor(argumentsTypes).Invoke(arguments);
        }

        public static object ConvertToTypeChecked(object a, Type fromType, Type toType)
        {
            if (toType.IsNullable() && toType.GetNotNullableType() == fromType)
                return a == null ? null : CreateInstance(toType, a);

            if (a == null)
            {
                if (!toType.IsValueType)
                    return null;

                if (fromType.IsNullable())
                    throw new InvalidOperationException("Nullable object must have a value");
            }

            if (IsType(toType, a))
            {
                return a;
            }

            if (Expression.IsPrimitiveConversion(fromType, toType))
                return Convert.ChangeType(a, toType, CultureInfo.CurrentCulture);

            throw new NotImplementedException($"No Convert defined for type {toType} ");
        }

        public static object ConvertToTypeUnchecked(object a, Type fromType, Type toType)
        {
            if (toType.IsNullable() && toType.GetNotNullableType() == fromType)
                return a == null ? null : CreateInstance(toType, a);

            if (a == null)
            {
                if (!toType.IsValueType)
                    return null;

                if (fromType.IsNullable())
                    throw new InvalidOperationException("Nullable object must have a value");
            }

            if (IsType(toType, a))
            {
                return a;
            }

            if (Expression.IsPrimitiveConversion(fromType, toType))
                return Conversion.ConvertPrimitiveUnChecked(fromType, toType, a);

            throw new NotImplementedException($"No Convert defined for type {toType} ");
        }

        public static bool IsType(Type t, object o)
        {
            return t.IsInstanceOfType(o);
        }

        public static object Negete(object a, TypeCode tc)
        {
            switch (tc)
            {
                case TypeCode.Char:
                    return unchecked(-Convert.ToChar(a));
                case TypeCode.Byte:
                    return unchecked(-Convert.ToByte(a));
                case TypeCode.Decimal:
                    return -Convert.ToDecimal(a);
                case TypeCode.Double:
                    return -Convert.ToDouble(a);
                case TypeCode.Int16:
                    return unchecked(-Convert.ToInt16(a));
                case TypeCode.Int32:
                    return unchecked(-Convert.ToInt32(a));
                case TypeCode.Int64:
                    return unchecked(-Convert.ToInt64(a));
                case TypeCode.UInt16:
                    return unchecked(-Convert.ToUInt16(a));
                case TypeCode.UInt32:
                    return unchecked(-Convert.ToUInt32(a));
                case TypeCode.SByte:
                    return unchecked(-Convert.ToSByte(a));
                case TypeCode.Single:
                    return -Convert.ToSingle(a);
            }

            throw new NotImplementedException($"No Negete defined for type {tc} ");
        }

        public static object RightShift(object a, int n, TypeCode tc)
        {
            switch (tc)
            {
                case TypeCode.Int16:
                    return Convert.ToInt16(a) >> n;
                case TypeCode.Int32:
                    return Convert.ToInt32(a) >> n;
                case TypeCode.Int64:
                    return Convert.ToInt64(a) >> n;
                case TypeCode.UInt16:
                    return Convert.ToUInt16(a) >> n;
                case TypeCode.UInt32:
                    return Convert.ToUInt32(a) >> n;
                case TypeCode.UInt64:
                    return Convert.ToUInt64(a) >> n;
            }

            throw new NotImplementedException($"No right shift defined for type {tc} ");
        }

        public static object LeftShift(object a, int n, TypeCode tc)
        {
            switch (tc)
            {
                case TypeCode.Int16:
                    return Convert.ToInt16(a) << n;
                case TypeCode.Int32:
                    return Convert.ToInt32(a) << n;
                case TypeCode.Int64:
                    return Convert.ToInt64(a) << n;
                case TypeCode.UInt16:
                    return Convert.ToUInt16(a) << n;
                case TypeCode.UInt32:
                    return Convert.ToUInt32(a) << n;
                case TypeCode.UInt64:
                    return Convert.ToUInt64(a) << n;
            }

            throw new NotImplementedException($"No right shift defined for type {tc} ");
        }

        private static decimal Evaluate(decimal a, decimal b, ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.Add:
                    return a + b;
                case ExpressionType.AddChecked:
                    return a + b;
                case ExpressionType.Subtract:
                    return a - b;
                case ExpressionType.SubtractChecked:
                    return a - b;
                case ExpressionType.Multiply:
                    return a * b;
                case ExpressionType.MultiplyChecked:
                    return a * b;
                case ExpressionType.Divide:
                    return a / b;
                case ExpressionType.Modulo:
                    return a % b;
            }

            throw new NotImplementedException($"Expression with Node type {et}");
        }

        private static double Evaluate(double a, double b, ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.Add:
                    return a + b;
                case ExpressionType.AddChecked:
                    return a + b;
                case ExpressionType.Subtract:
                    return a - b;
                case ExpressionType.SubtractChecked:
                    return a - b;
                case ExpressionType.Multiply:
                    return a * b;
                case ExpressionType.MultiplyChecked:
                    return a * b;
                case ExpressionType.Divide:
                    return a / b;
                case ExpressionType.Modulo:
                    return a % b;
                case ExpressionType.Power:
                    return SystemMath.Pow(a, b);
            }

            throw new NotImplementedException($"Expression with Node type {et}");
        }

        private static int Evaluate(short a, short b, ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.Add:
                    return unchecked(a + b);
                case ExpressionType.AddChecked:
                    return checked(a + b);
                case ExpressionType.Subtract:
                    return unchecked(a - b);
                case ExpressionType.SubtractChecked:
                    return checked(a - b);
                case ExpressionType.Multiply:
                    return unchecked(a * b);
                case ExpressionType.MultiplyChecked:
                    return checked(a * b);
                case ExpressionType.Divide:
                    return a / b;
                case ExpressionType.Modulo:
                    return a % b;
                case ExpressionType.ExclusiveOr:
                    return a ^ b;
                case ExpressionType.And:
                    return a & b;
                case ExpressionType.Or:
                    return a | b;
            }

            throw new NotImplementedException($"Expression with Node type {et}");
        }

        private static int Evaluate(int a, int b, ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.Add:
                    return unchecked(a + b);
                case ExpressionType.AddChecked:
                    return checked(a + b);
                case ExpressionType.Subtract:
                    return unchecked(a - b);
                case ExpressionType.SubtractChecked:
                    return checked(a - b);
                case ExpressionType.Multiply:
                    return unchecked(a * b);
                case ExpressionType.MultiplyChecked:
                    return checked(a * b);
                case ExpressionType.Divide:
                    return a / b;
                case ExpressionType.Modulo:
                    return a % b;
                case ExpressionType.ExclusiveOr:
                    return a ^ b;
                case ExpressionType.And:
                    return a & b;
                case ExpressionType.Or:
                    return a | b;
            }

            throw new NotImplementedException($"Expression with Node type {et}");
        }

        private static long Evaluate(long a, long b, ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.Add:
                    return unchecked(a + b);
                case ExpressionType.AddChecked:
                    return checked(a + b);
                case ExpressionType.Subtract:
                    return unchecked(a - b);
                case ExpressionType.SubtractChecked:
                    return checked(a - b);
                case ExpressionType.Multiply:
                    return unchecked(a * b);
                case ExpressionType.MultiplyChecked:
                    return checked(a * b);
                case ExpressionType.Divide:
                    return a / b;
                case ExpressionType.Modulo:
                    return a % b;
                case ExpressionType.ExclusiveOr:
                    return a ^ b;
                case ExpressionType.And:
                    return a & b;
                case ExpressionType.Or:
                    return a | b;
            }

            throw new NotImplementedException($"Expression with Node type {et}");
        }

        private static int Evaluate(ushort a, ushort b, ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.Add:
                    return unchecked(a + b);
                case ExpressionType.AddChecked:
                    return checked(a + b);
                case ExpressionType.Subtract:
                    return unchecked(a - b);
                case ExpressionType.SubtractChecked:
                    return checked((ushort)(a - b));
                case ExpressionType.Multiply:
                    return unchecked(a * b);
                case ExpressionType.MultiplyChecked:
                    return checked(a * b);
                case ExpressionType.Divide:
                    return a / b;
                case ExpressionType.Modulo:
                    return a % b;
                case ExpressionType.ExclusiveOr:
                    return a ^ b;
                case ExpressionType.And:
                    return a & b;
                case ExpressionType.Or:
                    return a | b;
            }

            throw new NotImplementedException($"Expression with Node type {et}");
        }

        private static uint Evaluate(uint a, uint b, ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.Add:
                    return unchecked(a + b);
                case ExpressionType.AddChecked:
                    return checked(a + b);
                case ExpressionType.Subtract:
                    return unchecked(a - b);
                case ExpressionType.SubtractChecked:
                    return checked(a - b);
                case ExpressionType.Multiply:
                    return unchecked(a * b);
                case ExpressionType.MultiplyChecked:
                    return checked(a * b);
                case ExpressionType.Divide:
                    return a / b;
                case ExpressionType.Modulo:
                    return a % b;
                case ExpressionType.ExclusiveOr:
                    return a ^ b;
                case ExpressionType.And:
                    return a & b;
                case ExpressionType.Or:
                    return a | b;
            }

            throw new NotImplementedException($"Expression with Node type {et}");
        }

        private static ulong Evaluate(ulong a, ulong b, ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.Add:
                    return unchecked(a + b);
                case ExpressionType.AddChecked:
                    return checked(a + b);
                case ExpressionType.Subtract:
                    return unchecked(a - b);
                case ExpressionType.SubtractChecked:
                    return checked(a - b);
                case ExpressionType.Multiply:
                    return unchecked(a * b);
                case ExpressionType.MultiplyChecked:
                    return checked(a * b);
                case ExpressionType.Divide:
                    return a / b;
                case ExpressionType.Modulo:
                    return a % b;
                case ExpressionType.ExclusiveOr:
                    return a ^ b;
                case ExpressionType.And:
                    return a & b;
                case ExpressionType.Or:
                    return a | b;
            }

            throw new NotImplementedException($"Expression with Node type {et}");
        }

        private static object Evaluate(char a, char b, ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.ExclusiveOr:
                    return a ^ b;
                case ExpressionType.And:
                    return a & b;
                case ExpressionType.Or:
                    return a | b;
            }

            throw new NotImplementedException($"Expression with Node type {et}");

        }

        private static int Evaluate(sbyte a, sbyte b, ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.Add:
                    return unchecked(a + b);
                case ExpressionType.AddChecked:
                    return checked(a + b);
                case ExpressionType.Subtract:
                    return unchecked(a - b);
                case ExpressionType.SubtractChecked:
                    return checked(a - b);
                case ExpressionType.Multiply:
                    return unchecked(a * b);
                case ExpressionType.MultiplyChecked:
                    return checked(a * b);
                case ExpressionType.Divide:
                    return a / b;
                case ExpressionType.Modulo:
                    return a % b;
                case ExpressionType.ExclusiveOr:
                    return a ^ b;
                case ExpressionType.And:
                    return a & b;
                case ExpressionType.Or:
                    return a | b;
            }

            throw new NotImplementedException($"Expression with Node type {et}");
        }

        private static int Evaluate(byte a, byte b, ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.Add:
                    return unchecked(a + b);
                case ExpressionType.AddChecked:
                    return checked(a + b);
                case ExpressionType.Subtract:
                    return unchecked(a - b);
                case ExpressionType.SubtractChecked:
                    return checked(a - b);
                case ExpressionType.Multiply:
                    return unchecked(a * b);
                case ExpressionType.MultiplyChecked:
                    return checked(a * b);
                case ExpressionType.Divide:
                    return a / b;
                case ExpressionType.Modulo:
                    return a % b;
                case ExpressionType.ExclusiveOr:
                    return a ^ b;
                case ExpressionType.And:
                    return a & b;
                case ExpressionType.Or:
                    return a | b;
            }

            throw new NotImplementedException($"Expression with Node type {et}");
        }

        private static float Evaluate(float a, float b, ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.Add:
                    return a + b;
                case ExpressionType.AddChecked:
                    return a + b;
                case ExpressionType.Subtract:
                    return a - b;
                case ExpressionType.SubtractChecked:
                    return a - b;
                case ExpressionType.Multiply:
                    return a * b;
                case ExpressionType.MultiplyChecked:
                    return a * b;
                case ExpressionType.Divide:
                    return a / b;
                case ExpressionType.Modulo:
                    return a % b;
            }

            throw new NotImplementedException($"Expression with Node type {et}");
        }

        private static bool Evaluate(bool a, bool b, ExpressionType et)
        {
            switch (et)
            {
                case ExpressionType.ExclusiveOr:
                    return a ^ b;
                case ExpressionType.And:
                    return a & b;
                case ExpressionType.Or:
                    return a | b;
            }

            throw new NotImplementedException($"Expression with Node type {et}");
        }
    }
}

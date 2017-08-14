// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mock.System.Runtime.Serialization;
using System;
using System.Reflection;
using SerializationException = System.Runtime.Serialization.SerializationException;

namespace Tests.Runtime.Serialization
{
    [TestClass]
    public class SerializationInfoTests
    {
        [TestMethod]
        public void SerializationInfo_AddGet()
        {
            var value = new Serializable();
            var si = new SerializationInfo(typeof(Serializable), new FormatterConverter());
            var sc = new StreamingContext();
            value.GetObjectData(si, sc);

            Assert.AreEqual(typeof(Serializable), si.ObjectType);
            Assert.AreEqual(typeof(Serializable).FullName, si.FullTypeName);
            // TODO: Implement AssemblyName property
            // Assert.AreEqual(typeof(Serializable).Assembly.FullName, si.AssemblyName);

            Assert.AreEqual(15, si.MemberCount);

            Assert.AreEqual(true, si.GetBoolean("bool"));
            Assert.AreEqual("hello", si.GetString("string"));
            Assert.AreEqual('a', si.GetChar("char"));

            Assert.AreEqual(byte.MaxValue, si.GetByte("byte"));

            Assert.AreEqual(decimal.MaxValue, si.GetDecimal("decimal"));
            Assert.AreEqual(double.MaxValue, si.GetDouble("double"));
            Assert.AreEqual(short.MaxValue, si.GetInt16("short"));
            Assert.AreEqual(int.MaxValue, si.GetInt32("int"));
            Assert.AreEqual(long.MaxValue, si.GetInt64("long"));
            Assert.AreEqual(sbyte.MaxValue, si.GetSByte("sbyte"));
            Assert.AreEqual(float.MaxValue, si.GetSingle("float"));
            Assert.AreEqual(ushort.MaxValue, si.GetUInt16("ushort"));
            Assert.AreEqual(uint.MaxValue, si.GetUInt32("uint"));
            Assert.AreEqual(ulong.MaxValue, si.GetUInt64("ulong"));
            Assert.AreEqual(DateTime.MaxValue, si.GetDateTime("datetime"));
        }

        [TestMethod]
        public void SerializationInfo_Enumerate()
        {
            var value = new Serializable();
            var si = new SerializationInfo(typeof(Serializable), new FormatterConverter());
            var sc = new StreamingContext();
            value.GetObjectData(si, sc);

            int items = 0;
            foreach (SerializationEntry entry in si)
            {
                items++;
                switch (entry.Name)
                {
                    case "int":
                        Assert.AreEqual(int.MaxValue, (int)entry.Value);
                        Assert.AreEqual(typeof(int), entry.ObjectType);
                        break;
                    case "string":
                        Assert.AreEqual("hello", (string)entry.Value);
                        Assert.AreEqual(typeof(string), entry.ObjectType);
                        break;
                    case "bool":
                        Assert.AreEqual(true, (bool)entry.Value);
                        Assert.AreEqual(typeof(bool), entry.ObjectType);
                        break;
                }
            }

            Assert.AreEqual(si.MemberCount, items);
        }

        [TestMethod]
        public void SerializationInfo_NegativeAddValueTwice()
        {
            var si = new SerializationInfo(typeof(Serializable), new FormatterConverter());
            si.AddValue("bool", true);
            AssertExtensions.Throws<SerializationException>(() => si.AddValue("bool", true));
        }

        [TestMethod]
        public void SerializationInfo_NegativeValueNotFound()
        {
            var si = new SerializationInfo(typeof(Serializable), new FormatterConverter());
            si.AddValue("a", 1);
            AssertExtensions.Throws<SerializationException>(() => si.GetInt32("b"));
        }
    }

    [Serializable]
    internal class Serializable : ISerializable
    {
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("string", "hello");
            info.AddValue("bool", true);
            info.AddValue("char", 'a');
            info.AddValue("byte", byte.MaxValue);
            info.AddValue("decimal", decimal.MaxValue);
            info.AddValue("double", double.MaxValue);
            info.AddValue("short", short.MaxValue);
            info.AddValue("int", int.MaxValue);
            info.AddValue("long", long.MaxValue);
            info.AddValue("sbyte", sbyte.MaxValue);
            info.AddValue("float", float.MaxValue);
            info.AddValue("ushort", ushort.MaxValue);
            info.AddValue("uint", uint.MaxValue);
            info.AddValue("ulong", ulong.MaxValue);
            info.AddValue("datetime", DateTime.MaxValue);
        }
    }
}
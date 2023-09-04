// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace SomaSim.SION
{
    public static class Test
    {
#pragma warning disable 0414 // warning: the private field 'field' is assigned but its value is never used

        public enum TestEnum
        {
            Zero = 0,
            One = 1,
            FortyTwo = 42
        }

        public class TestClassOne
        {
            // these primitive fields will be serialized
            public int PublicFieldInt = 0;
            public string PublicFieldString = "";

            // these public properties will be serialized
            public int PublicProperty { get; set; }
            public int PublicGetterSetter { get { return _PrivateField; } set { _PrivateField = value; } }

            // these private properties will not be serialized
            private int _PrivateField = 0;
            protected int _ProtectedField = 0;
            int _UnspecifiedField = 0;

            // these public properties will not get serialized
            public int _MissingSetter { get { return 0; } }

            //public string NPrivateSetter { get; private set; } // unfortunately right now this gets serialized. todo fixme
        }

        public struct TestStruct
        {
            public int x;
            public int y;
            public string id;
        }

        public interface IClass { }

        public abstract class AbstractClass : IClass { }

        public class ClassA : AbstractClass
        {
            public int fielda;
        }

        public class ClassB : AbstractClass
        {
            public string fieldb;
        }

        public class TestClassGenerics
        {
            public Dictionary<int, string> IntToString;
            public Dictionary<TestEnum, string> EnumToString;
            public Dictionary<string, TestStruct> StringToStruct;
            public Dictionary<int, IClass> IntToInterface;
            public List<TestStruct> ListOfStructs;
            public List<IClass> ListOfInterfaces;
            public List<AbstractClass> ListOfAbstracts;
            public List<ClassA> ListOfConcretes;
            public List<string> ListOfPrimitives;
        }

        public class TestClassNonGenerics
        {
            public int[] ArrayOfInts;
            public Hashtable Hashtable;
            public ArrayList ArrayList;
        }

        public struct TestLabel
        {
            public string Value;
            public int Hash;

            public TestLabel (string value) {
                Value = value;
                Hash = value.GetHashCode();
            }

            public static object Serialize (TestLabel label, Serializer s) {
                return label.Value;
            }

            public static TestLabel Deserialize (object value, Serializer s) {
                return value is string ? new TestLabel((string) value) : new TestLabel();
            }
        }


#pragma warning restore 0414
    }

    [TestClass]
    public class SerializerTests
    {
        public static Serializer s;

        static SerializerTests () {
            s = new Serializer();
        }

        private void CheckSerializedTypeAndValue (object source, Type expectedType, object expectedValue) {
            object serialized = s.Serialize(source);
            Assert.IsTrue(expectedValue == null ? serialized == null : expectedValue.Equals(serialized));
            Assert.IsTrue(expectedType == serialized.GetType());
        }

        [TestMethod]
        public void TestSerializeNumbers () {
            // make sure all integers are upcast to 64-bit long or ulong
            CheckSerializedTypeAndValue((byte) 42, typeof(ulong), 42UL);
            CheckSerializedTypeAndValue((ushort) 42, typeof(ulong), 42UL);
            CheckSerializedTypeAndValue((uint) 42, typeof(ulong), 42UL);
            CheckSerializedTypeAndValue((ulong) 42, typeof(ulong), 42UL);
            CheckSerializedTypeAndValue((sbyte) 42, typeof(long), 42L);
            CheckSerializedTypeAndValue((short) 42, typeof(long), 42L);
            CheckSerializedTypeAndValue((int) 42, typeof(long), 42L);
            CheckSerializedTypeAndValue((long) 42, typeof(long), 42L);

            // make sure all floating points are upcast to double precision
            CheckSerializedTypeAndValue(1.0f, typeof(double), 1.0);
            CheckSerializedTypeAndValue(1.0, typeof(double), 1.0);
        }

        [TestMethod]
        public void TestSerializeOtherPrimitives () {
            // null be null
            Assert.IsNull(s.Serialize(null));

            // booleans are serialized as is
            CheckSerializedTypeAndValue(true, typeof(bool), true);
            CheckSerializedTypeAndValue(false, typeof(bool), false);

            // characters and strings are serialized as strings
            CheckSerializedTypeAndValue("foo", typeof(string), "foo");
            CheckSerializedTypeAndValue('x', typeof(string), "x");

            // enums are serialized as uint64, or as a string, depending on settings
            var previousSetting = s.Options.EnumSerialization;

            s.Options.EnumSerialization = EnumSerializationOption.SerializeAsNumber;
            CheckSerializedTypeAndValue(Test.TestEnum.Zero, typeof(long), 0L);
            CheckSerializedTypeAndValue(Test.TestEnum.One, typeof(long), 1L);
            CheckSerializedTypeAndValue(Test.TestEnum.FortyTwo, typeof(long), 42L);

            s.Options.EnumSerialization = EnumSerializationOption.SerializeAsSimpleName;
            CheckSerializedTypeAndValue(Test.TestEnum.Zero, typeof(string), "zero");
            CheckSerializedTypeAndValue(Test.TestEnum.One, typeof(string), "one");
            CheckSerializedTypeAndValue(Test.TestEnum.FortyTwo, typeof(string), "fortytwo");

            s.Options.EnumSerialization = previousSetting;

            // date time are converted to their binary representation (specially encoded int64)
            DateTime time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long binary = time.ToBinary();
            CheckSerializedTypeAndValue(time, typeof(long), binary);
        }

        [TestMethod]
        public void TestSerializeClassInstances () {

            // try serialize everything

            bool previous = s.Options.SkipDefaultValuesDuringSerialization;
            s.Options.SkipDefaultValuesDuringSerialization = false;
            {
                object source = new Test.TestClassOne() { };
                object expected = new Hashtable() {
                    { "#type", "SomaSim.SION.Test+TestClassOne" },
                    { "PublicFieldInt", 0L }, { "PublicFieldString", "" }, { "PublicGetterSetter", 0L }, { "PublicProperty", 0L }
                };

                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source, true), expected));
                s.Options.SkipDefaultValuesDuringSerialization = previous;
            }

            // try serialize just non-default fields

            s.Options.SkipDefaultValuesDuringSerialization = true;
            {
                // all fields are set, all will be serialized 

                object source = new Test.TestClassOne() {
                    PublicFieldInt = 42,
                    PublicFieldString = "fortytwo",
                    PublicGetterSetter = 42,
                    PublicProperty = 42
                };
                object expected = new Hashtable() {
                    { "#type", "SomaSim.SION.Test+TestClassOne" },
                    { "PublicFieldInt", 42L }, { "PublicFieldString", "fortytwo" }, { "PublicGetterSetter", 42L }, { "PublicProperty", 42L }
                };

                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source, true), expected));
            }

            {
                // some fields are not set, they will be skipped

                object source = new Test.TestClassOne() {
                    PublicGetterSetter = 42,
                    PublicProperty = 42
                };
                object expected = new Hashtable() {
                    { "#type", "SomaSim.SION.Test+TestClassOne" },
                    { "PublicGetterSetter", 42L }, { "PublicProperty", 42L }
                };

                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source, true), expected));
            }

            s.Options.SkipDefaultValuesDuringSerialization = previous;
        }

        [TestMethod]
        public void TestSerializeDictionaryOfPrimitives () {

            // test generic / typed

            {
                var source = new Dictionary<string, int>() {
                    { "foo", 1 },
                    { "bar", 2 } };
                Hashtable expected = new Hashtable() {
                    { "foo", 1L },
                    { "bar", 2L } }; // because serializer converts up to longs
                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), expected));
            }

            {
                var source = new Dictionary<sbyte, List<uint>>() {
                    { 1, new List<uint>() { 1, 2 } },
                    { 3, new List<uint>() { 3, 4 } } };
                Hashtable expected = new Hashtable() {
                    { 1L, new ArrayList() { 1UL, 2UL } },
                    { 3L, new ArrayList() { 3UL, 4UL } } };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), expected));
            }

            // test untyped

            {
                Hashtable source = new Hashtable() {
                    { "foo", new ArrayList() { 1, (sbyte)2 } },
                    { 'x', new ArrayList() { (uint)3, (ushort)4 } } };
                Hashtable expected = new Hashtable() {
                    { "foo", new ArrayList() { 1L, 2L } },
                    { "x", new ArrayList() { 3UL, 4UL } } };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), expected));
            }

            {
                Test.TestClassNonGenerics source = new Test.TestClassNonGenerics() {
                    Hashtable = new Hashtable() { { "one", 1 } }
                };
                Hashtable expected = new Hashtable() {
                    { "Hashtable", new Hashtable() { { "one", 1L } } } };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), expected));
            }
        }

        [TestMethod]
        public void TestSerializeDictionaryOfClassInstances () {
            {
                Test.TestClassGenerics source = new Test.TestClassGenerics() {
                    IntToString = new Dictionary<int, string>() { { 1, "one" }, { 2, "two" } },
                    EnumToString = new Dictionary<Test.TestEnum, string>() {
                        { Test.TestEnum.FortyTwo, "fortytwo" },
                        { Test.TestEnum.One, "one" } },
                    StringToStruct = new Dictionary<string, Test.TestStruct>() {
                        { "testone", new Test.TestStruct() { id = "one", x = 1, y = 1 } },
                        { "testtwo", new Test.TestStruct() { id = "two", x = 2, y = 2 } }
                    },
                    IntToInterface = new Dictionary<int, Test.IClass>() {
                        { 1, new Test.ClassA() { fielda = 1 } },
                        { 2, new Test.ClassB() { fieldb = "two" } }
                    }
                };

                var expected = new Hashtable() {
                    { "IntToString", new Hashtable() { { 1L, "one" }, { 2L, "two" } } },
                    { "EnumToString", new Hashtable() { { 42L, "fortytwo" }, { 1L, "one" } } },
                    { "StringToStruct", new Hashtable() {
                        { "testone", new Hashtable() { { "id", "one" }, { "x", 1L }, { "y", 1L } } },
                        { "testtwo", new Hashtable() { { "id", "two" }, { "x", 2L }, { "y", 2L } } }
                    } },
                    { "IntToInterface", new Hashtable() {
                        { 1L, new Hashtable() { { "#type", "SomaSim.SION.Test+ClassA" }, { "fielda", 1L } } },
                        { 2L, new Hashtable() { { "#type", "SomaSim.SION.Test+ClassB" }, { "fieldb", "two" } } }
                    } }
                };

                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), expected));
            }
        }

        [TestMethod]
        public void TestSerializeEnumerableOfPrimitives () {

            // test generic / typed

            {
                short[] source = new short[] { 1, 2, 3 };
                ArrayList expected = new ArrayList() { 1L, 2L, 3L };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), expected));
            }

            {
                List<short> source = new List<short>() { 1, 2, 3 };
                ArrayList expected = new ArrayList() { 1L, 2L, 3L };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), expected));
            }

            {
                List<char[]> source = new List<char[]>() { new char[] { 'f', 'o', 'o' } };
                ArrayList expected = new ArrayList() { new ArrayList() { "f", "o", "o" } };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), expected));
            }

            // test untyped

            {
                ArrayList source = new ArrayList() { "foo", 'x', 123, (byte) 42, new List<bool>() { true, false } };
                ArrayList expected = new ArrayList() { "foo", "x", 123L, 42UL, new ArrayList() { true, false } };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), expected));
            }

            {
                Test.TestClassNonGenerics source = new Test.TestClassNonGenerics() {
                    ArrayList = new ArrayList() { "one", 1 },
                    ArrayOfInts = new int[] { 1, 2 }
                };
                Hashtable expected = new Hashtable() {
                    { "ArrayList", new ArrayList() { "one", 1L } },
                    { "ArrayOfInts", new ArrayList() { 1L, 2L } }
                };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), expected));
            }
        }

        [TestMethod]
        public void TestSerializeEnumerableOfClassInstances () {

            {
                Test.TestClassGenerics source = new Test.TestClassGenerics() {
                    ListOfStructs = new List<Test.TestStruct>() {
                        new Test.TestStruct() { id = "one", x = 1, y = 1 },
                        new Test.TestStruct() { id = "two", x = 2, y = 2 },
                    },
                    ListOfInterfaces = new List<Test.IClass>() {
                        new Test.ClassA() { fielda = 1 },
                        new Test.ClassB() { fieldb = "two" }
                    },
                    ListOfAbstracts = new List<Test.AbstractClass>() {
                        new Test.ClassA() { fielda = 1 },
                        new Test.ClassB() { fieldb = "two" }
                    },
                    ListOfConcretes = new List<Test.ClassA>() {
                        new Test.ClassA() { fielda = 1 }
                    },
                    ListOfPrimitives = new List<string>() {
                        "foo", "bar"
                    }
                };

                var expected = new Hashtable() {
                    { "ListOfStructs", new ArrayList() {
                        new Hashtable() { { "id", "one" }, { "x", 1L }, { "y", 1L } },
                        new Hashtable() { { "id", "two" }, { "x", 2L }, { "y", 2L } }
                    } },
                    { "ListOfInterfaces", new ArrayList() {
                        new Hashtable() { { "#type", "SomaSim.SION.Test+ClassA" }, { "fielda", 1L } },
                        new Hashtable() { { "#type", "SomaSim.SION.Test+ClassB" }, { "fieldb", "two" } }
                    } },
                    { "ListOfAbstracts", new ArrayList() {
                        new Hashtable() { { "#type", "SomaSim.SION.Test+ClassA" }, { "fielda", 1L } },
                        new Hashtable() { { "#type", "SomaSim.SION.Test+ClassB" }, { "fieldb", "two" } }
                    } },
                    { "ListOfConcretes", new ArrayList() {
                        new Hashtable() { { "fielda", 1L } }
                    } },
                    { "ListOfPrimitives", new ArrayList() { "foo", "bar" } }
                };

                Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), expected));
            }
        }

        [TestMethod]
        public void TestSerializeWithImplicitNamespaces () {

            Test.TestClassGenerics source = new Test.TestClassGenerics() {
                ListOfInterfaces = new List<Test.IClass>() {
                    new Test.ClassA() { fielda = 1 },
                    new Test.ClassB() { fieldb = "two" }
                }
            };

            var resultExplicit = new Hashtable() {
                { "ListOfInterfaces", new ArrayList() {
                    new Hashtable() { { "#type", "SomaSim.SION.Test+ClassA" }, { "fielda", 1L } },
                    new Hashtable() { { "#type", "SomaSim.SION.Test+ClassB" }, { "fieldb", "two" } }
                } } };

            var resultImplicit = new Hashtable() {
                { "ListOfInterfaces", new ArrayList() {
                    new Hashtable() { { "#type", "ClassA" }, { "fielda", 1L } },
                    new Hashtable() { { "#type", "ClassB" }, { "fieldb", "two" } }
                } } };

            Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), resultExplicit));

            s.AddImplicitNamespace("SomaSim.SION.Test", false);
            Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), resultImplicit));

            s.RemoveImplicitNamespace("SomaSim.SION.Test");
            Assert.IsTrue(DeepCompare.DeepEquals(s.Serialize(source), resultExplicit));
        }



        //
        // test deserialization

        private void CheckDeserializedTypeAndValue (object source, Type desiredType, Type expectedType, object expectedValue) {
            object deserialized = s.Deserialize(source, desiredType);
            Assert.IsTrue(expectedValue == null ? deserialized == null : expectedValue.Equals(deserialized));
            Assert.IsTrue(expectedType == deserialized.GetType());
        }

        [TestMethod]
        public void TestDeserializeNumbers () {
            // check deserialization of plain primitives with casting
            CheckDeserializedTypeAndValue(42UL, typeof(ulong), typeof(ulong), 42UL);
            CheckDeserializedTypeAndValue(42UL, typeof(uint), typeof(uint), 42U);
            CheckDeserializedTypeAndValue(42UL, typeof(ushort), typeof(ushort), (ushort) 42);
            CheckDeserializedTypeAndValue(42UL, typeof(byte), typeof(byte), (byte) 42);

            CheckDeserializedTypeAndValue(42L, typeof(long), typeof(long), 42L);
            CheckDeserializedTypeAndValue(42L, typeof(int), typeof(int), 42);
            CheckDeserializedTypeAndValue(42L, typeof(short), typeof(short), (short) 42);
            CheckDeserializedTypeAndValue(42L, typeof(sbyte), typeof(sbyte), (sbyte) 42);

            CheckDeserializedTypeAndValue(42.0, typeof(float), typeof(float), 42.0f);
            CheckDeserializedTypeAndValue(42.0, typeof(double), typeof(double), 42.0);
        }

        [TestMethod]
        public void TestDeserializeOtherPrimitives () {
            // null is null
            Assert.IsNull(s.Deserialize(null, null));

            // make sure plain primitives come through as is when not forced into a type
            CheckDeserializedTypeAndValue((ulong) 42, null, typeof(ulong), 42UL);
            CheckDeserializedTypeAndValue((long) 42, null, typeof(long), 42L);
            CheckDeserializedTypeAndValue((double) 42, null, typeof(double), 42.0);
            CheckDeserializedTypeAndValue(true, null, typeof(bool), true);
            CheckDeserializedTypeAndValue("foo", null, typeof(string), "foo");

            // booleans are serialized as is
            CheckDeserializedTypeAndValue(true, typeof(bool), typeof(bool), true);
            CheckDeserializedTypeAndValue(false, typeof(bool), typeof(bool), false);

            // characters and strings as serialized as strings
            CheckDeserializedTypeAndValue("x", typeof(char), typeof(char), 'x');
            CheckDeserializedTypeAndValue("x", typeof(string), typeof(string), "x");
            CheckDeserializedTypeAndValue("x", null, typeof(string), "x");

            // enums should deserialize correctly from either number or string
            CheckDeserializedTypeAndValue(42L, typeof(Test.TestEnum), typeof(Test.TestEnum), Test.TestEnum.FortyTwo);
            CheckDeserializedTypeAndValue(42UL, typeof(Test.TestEnum), typeof(Test.TestEnum), Test.TestEnum.FortyTwo);
            CheckDeserializedTypeAndValue("fortytwo", typeof(Test.TestEnum), typeof(Test.TestEnum), Test.TestEnum.FortyTwo);
            CheckDeserializedTypeAndValue("FortyTwo", typeof(Test.TestEnum), typeof(Test.TestEnum), Test.TestEnum.FortyTwo);
            CheckDeserializedTypeAndValue("forty-two", typeof(Test.TestEnum), typeof(Test.TestEnum), Test.TestEnum.FortyTwo);
            CheckDeserializedTypeAndValue("f-o-rty-t-w-o", typeof(Test.TestEnum), typeof(Test.TestEnum), Test.TestEnum.FortyTwo);
        }


        [TestMethod]
        public void TestDeserializeClassInstances () {

            {
                // try deserialize with type embedded in the source object

                object source = new Hashtable() {
                    { "#type", "SomaSim.SION.Test+TestClassOne" },
                    { "PublicFieldInt", 42L },
                    { "PublicFieldString", "fortytwo" },
                    { "PublicGetterSetter", 42L },
                    { "PublicProperty", 42L }
                };

                object expected = new Test.TestClassOne() {
                    PublicFieldInt = 42,
                    PublicFieldString = "fortytwo",
                    PublicGetterSetter = 42,
                    PublicProperty = 42
                };

                Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize(source), expected));
            }

            {
                // try deserialize with type specified explicitly

                object source = new Hashtable() {
                    { "PublicFieldInt", 42L },
                    { "PublicFieldString", "fortytwo" },
                    { "PublicGetterSetter", 42L },
                    { "PublicProperty", 42L }
                };

                object expected = new Test.TestClassOne() {
                    PublicFieldInt = 42,
                    PublicFieldString = "fortytwo",
                    PublicGetterSetter = 42,
                    PublicProperty = 42
                };

                Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize<Test.TestClassOne>(source), expected));
            }
        }


        [TestMethod]
        public void TestDeserializeDictionaryOfPrimitives () {

            // test generic / typed

            {
                Hashtable source = new Hashtable() {
                    { "foo", 1L },
                    { "bar", 2L } };
                Dictionary<string, int> expected = new Dictionary<string, int>() {
                    { "foo", 1 },
                    { "bar", 2 } };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize(source, expected.GetType()), expected));
            }

            {
                Hashtable source = new Hashtable() {
                    { 1L, new ArrayList() { 1UL, 2UL } },
                    { 3L, new ArrayList() { 3UL, 4UL } } };
                Dictionary<sbyte, List<uint>> expected = new Dictionary<sbyte, List<uint>>() {
                    { 1, new List<uint>() { 1, 2 } },
                    { 3, new List<uint>() { 3, 4 } } };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize(source, expected.GetType()), expected));
            }

            // test untyped

            {
                Hashtable source = new Hashtable() {
                    { "foo", new ArrayList() { 1L, 2L } },
                    { "x", new ArrayList() { 3UL, 4UL } } };
                Hashtable expected = new Hashtable() {
                    { "foo", new ArrayList() { 1L, 2L } },
                    { "x", new ArrayList() { 3UL, 4UL } } };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize(source), expected)); // no expected type specified
            }

            {
                Hashtable source = new Hashtable() {
                    { "Hashtable", new Hashtable() { { "one", 1L } } } };
                Test.TestClassNonGenerics expected = new Test.TestClassNonGenerics() {
                    Hashtable = new Hashtable() { { "one", 1L } }
                };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize(source, expected.GetType()), expected));
            }
        }

        [TestMethod]
        public void TestDeserializeDictionaryOfClassInstances () {

            {
                var source = new Hashtable() {
                    { "IntToString", new Hashtable() { { 1L, "one" }, { 2L, "two" } } },
                    { "EnumToString", new Hashtable() { { 42L, "fortytwo" }, { 1L, "one" } } },
                    { "StringToStruct", new Hashtable() {
                        { "testone", new Hashtable() { { "id", "one" }, { "x", 1L }, { "y", 1L } } },
                        { "testtwo", new Hashtable() { { "id", "two" }, { "x", 2L }, { "y", 2L } } }
                    } }
                };

                Test.TestClassGenerics expected = new Test.TestClassGenerics() {
                    IntToString = new Dictionary<int, string>() {
                        { 1, "one" },
                        { 2, "two" } },
                    EnumToString = new Dictionary<Test.TestEnum, string>() {
                        { Test.TestEnum.FortyTwo, "fortytwo" },
                        { Test.TestEnum.One, "one" } },
                    StringToStruct = new Dictionary<string, Test.TestStruct>() {
                        { "testone", new Test.TestStruct() { id = "one", x = 1, y = 1 } },
                        { "testtwo", new Test.TestStruct() { id = "two", x = 2, y = 2 } }
                    }
                };

                Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize<Test.TestClassGenerics>(source), expected));
            }
        }

        [TestMethod]
        public void TestDeserializeEnumerableOfPrimitives () {

            // test generic / typed

            {
                ArrayList source = new ArrayList() { 1L, 2L, 3L };
                short[] expected = new short[] { 1, 2, 3 };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize(source, expected.GetType()), expected));
            }

            {
                ArrayList source = new ArrayList() { 1L, 2L, 3L };
                List<short> expected = new List<short>() { 1, 2, 3 };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize(source, expected.GetType()), expected));
            }

            {
                ArrayList source = new ArrayList() { new ArrayList() { "f", "o", "o" } };
                List<char[]> expected = new List<char[]>() { new char[] { 'f', 'o', 'o' } };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize(source, expected.GetType()), expected));
            }

            // test untyped

            {
                ArrayList source = new ArrayList() { "foo", "x", 123L, 42UL, new ArrayList() { true, false } };
                ArrayList expected = new ArrayList() { "foo", "x", 123L, 42UL, new ArrayList() { true, false } };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize(source), expected)); // no expected type specified
            }

            {
                Hashtable source = new Hashtable() {
                    { "Hashtable", new Hashtable() { { 1L, "one" } } },
                    { "ArrayList", new ArrayList() { "one", 1L } },
                    { "ArrayOfInts", new ArrayList() { 1L, 2L } }
                };
                Test.TestClassNonGenerics expected = new Test.TestClassNonGenerics() {
                    Hashtable = new Hashtable() { { 1L, "one" } },
                    ArrayList = new ArrayList() { "one", 1L },
                    ArrayOfInts = new int[] { 1, 2 }
                };
                Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize(source, expected.GetType()), expected));
            }
        }

        [TestMethod]
        public void TestDeserializeEnumerableOfClassInstances () {
            {
                var source = new Hashtable() {
                    { "ListOfStructs", new ArrayList() {
                        new Hashtable() { { "id", "one" }, { "x", 1L }, { "y", 1L } },
                        new Hashtable() { { "id", "two" }, { "x", 2L }, { "y", 2L } }
                    } },
                    { "ListOfInterfaces", new ArrayList() {
                        new Hashtable() { { "#type", "SomaSim.SION.Test+ClassA" }, { "fielda", 1L } },
                        new Hashtable() { { "#type", "SomaSim.SION.Test+ClassB" }, { "fieldb", "two" } }
                    } },
                    { "ListOfAbstracts", new ArrayList() {
                        new Hashtable() { { "#type", "SomaSim.SION.Test+ClassA" }, { "fielda", 1L } },
                        new Hashtable() { { "#type", "SomaSim.SION.Test+ClassB" }, { "fieldb", "two" } }
                    } },
                    { "ListOfConcretes", new ArrayList() {
                        new Hashtable() { { "fielda", 1L } }
                    } }
                };

                Test.TestClassGenerics expected = new Test.TestClassGenerics() {
                    ListOfStructs = new List<Test.TestStruct>() {
                        new Test.TestStruct() { id = "one", x = 1, y = 1 },
                        new Test.TestStruct() { id = "two", x = 2, y = 2 },
                    },
                    ListOfInterfaces = new List<Test.IClass>() {
                        new Test.ClassA() { fielda = 1 },
                        new Test.ClassB() { fieldb = "two" }
                    },
                    ListOfAbstracts = new List<Test.AbstractClass>() {
                        new Test.ClassA() { fielda = 1 },
                        new Test.ClassB() { fieldb = "two" }
                    },
                    ListOfConcretes = new List<Test.ClassA>() {
                        new Test.ClassA() { fielda = 1 }
                    }
                };

                Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize<Test.TestClassGenerics>(source), expected));
            }
        }

        [TestMethod]
        public void TestDeserializeWithImplicitNamespaces () {
            var sourceExplicit = new Hashtable() {
                { "ListOfInterfaces", new ArrayList() {
                    new Hashtable() { { "#type", "SomaSim.SION.Test+ClassA" }, { "fielda", 1L } },
                    new Hashtable() { { "#type", "SomaSim.SION.Test+ClassB" }, { "fieldb", "two" } }
                } },
            };

            var sourceImplicit = new Hashtable() {
                { "ListOfInterfaces", new ArrayList() {
                    new Hashtable() { { "#type", "ClassA" }, { "fielda", 1L } },
                    new Hashtable() { { "#type", "ClassB" }, { "fieldb", "two" } }
                } },
            };

            Test.TestClassGenerics expected = new Test.TestClassGenerics() {
                ListOfInterfaces = new List<Test.IClass>() {
                    new Test.ClassA() { fielda = 1 },
                    new Test.ClassB() { fieldb = "two" }
                },
            };

            Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize<Test.TestClassGenerics>(sourceExplicit), expected));

            s.AddImplicitNamespace("SomaSim.SION.Test", false);
            Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize<Test.TestClassGenerics>(sourceImplicit), expected));

            s.RemoveImplicitNamespace("SomaSim.SION.Test");
            Assert.IsTrue(DeepCompare.DeepEquals(s.Deserialize<Test.TestClassGenerics>(sourceExplicit), expected));
        }

        [TestMethod]
        public void TestTypedLabelDeserialize () {

            Serializer s = new Serializer();

            s.AddCustomSerializer(Test.TestLabel.Serialize, Test.TestLabel.Deserialize);

            var orig = new Test.TestLabel("foo");
            var serialized = s.Serialize(orig);
            Assert.IsTrue(serialized is string && (string) serialized == "foo");

            var foo1 = s.Deserialize<Test.TestLabel>("foo");
            Assert.IsNotNull(foo1);
            Assert.IsTrue(foo1.Value == "foo");
            Assert.IsTrue(foo1.Hash == "foo".GetHashCode());

            var foo2 = s.Deserialize<Test.TestLabel>("foo");
            Assert.IsTrue(foo1.Equals(foo2));        // they evaluate to the same struct
            Assert.IsTrue(foo1.Value == foo2.Value); // because they have the same string and
            Assert.IsTrue(foo1.Hash == foo2.Hash);   // they have the same hash. however,

            var bar1 = s.Deserialize<Test.TestLabel>("bar");
            Assert.IsFalse(foo1.Equals(bar1));        // these are not the same,
            Assert.IsFalse(foo1.Value == bar1.Value); // either by string value
            Assert.IsFalse(foo1.Hash == bar1.Hash);   // or by hash

            s.RemoveCustomSerializer<Test.TestLabel>();

            bar1.Hash = foo1.Hash;                   // let's hack our label instance and show that
            Assert.IsTrue(foo1.Hash == bar1.Hash);   // even though the hashes are the same (to simulate a hash collision)
            Assert.IsFalse(foo1.Value == bar1.Value); // the labels are still not equal, although now the test takes longer
        }
    }
}

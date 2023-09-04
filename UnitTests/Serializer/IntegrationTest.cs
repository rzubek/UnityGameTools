// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System.Collections;
using System.Collections.Generic;

namespace SomaSim.SION
{
    [TestClass]
    public class IntegrationTests
    {
        #region Test Instances

        Test.TestClassOne test1 = new Test.TestClassOne() {
            PublicFieldInt = 42,
            PublicFieldString = "hello, \"world\" ",
            PublicProperty = 1,
            PublicGetterSetter = 2
        };

        Test.TestClassOne test2 = new Test.TestClassOne() {
            PublicFieldString = "hello", // other fields left as defaults
        };

        Test.TestClassGenerics testGenerics = new Test.TestClassGenerics() {
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
                "foo", "bar", "this is \"cool\" isn't it"
            },
            EnumToString = new Dictionary<Test.TestEnum, string>() {
                { Test.TestEnum.Zero, "Zero" },
                { Test.TestEnum.One, "One" },
                { Test.TestEnum.FortyTwo, "FortyTwo" }
            },
            IntToString = new Dictionary<int, string>() {
                { 0, "Zero" }, { 1, "One" } },
            IntToInterface = new Dictionary<int, Test.IClass>() {
                { 1, new Test.ClassA() { fielda = 1 } },
                { 2, new Test.ClassB() { fieldb = "two" } }
            },
            StringToStruct = new Dictionary<string, Test.TestStruct>() {
                { "one", new Test.TestStruct() { id = "one", x = 1, y = 1 } },
                { "two", new Test.TestStruct() { id = "two", x = 2, y = 2 } },
            }
        };

        #endregion

        private void TestPrintAndParseBack<T> (T value, Serializer s) {
            var serialized = s.Serialize(value);
            var stringified = SION.Print(serialized);
            object parsed = SION.Parse(stringified);
            T deserialized = s.Deserialize<T>(parsed);

            Assert.IsTrue(DeepCompare.DeepEquals(deserialized, value));
        }

        [TestMethod]
        public void TestIntegrationAndSkippingDefaults () {
            Serializer s = new Serializer();

            TestPrintAndParseBack(test1, s);
            TestPrintAndParseBack(test2, s);

            s.Options.SkipDefaultValuesDuringSerialization = false;
            TestPrintAndParseBack(test2, s);
            s.Options.SkipDefaultValuesDuringSerialization = true;

            TestPrintAndParseBack(testGenerics, s);
        }

        [TestMethod]
        public void TestIntegrationInjectingSpuriousData () {
            string spuriousKey = null;
            Serializer s = new Serializer();
            s.Options.OnSpuriousDataCallback = (key, type) => { spuriousKey = key; };

            Hashtable serialized = s.Serialize(test1) as Hashtable;
            serialized["SomeSpuriousKey"] = "SpuriousValue";
            var stringified = SION.Print(serialized);
            object parsed = SION.Parse(stringified);
            var deserialized = s.Deserialize<Test.TestClassOne>(parsed);

            Assert.IsTrue(DeepCompare.DeepEquals(deserialized, test1));
            Assert.IsTrue(spuriousKey == "SomeSpuriousKey");
        }

        [TestMethod]
        public void TestIntegrationInvalidType () {
            string unknownType = null;
            Serializer s = new Serializer();
            s.Options.OnUnknownTypeCallback = (type) => { unknownType = type; };

            Hashtable serialized = s.Serialize(test1) as Hashtable;
            serialized[s.Options.SpecialTypeToken] = "$$$blah";
            var stringified = SION.Print(serialized);
            object parsed = SION.Parse(stringified);
            var deserialized = s.Deserialize(parsed);

            Assert.IsTrue(deserialized is Hashtable);
            Assert.IsTrue(unknownType == "$$$blah");
        }

        [TestMethod]
        public void TestClone () {
            Serializer s = new Serializer();
            Assert.IsTrue(DeepCompare.DeepEquals(s.Clone(test1), test1));
            Assert.IsTrue(DeepCompare.DeepEquals(s.Clone(test2), test2));
            Assert.IsTrue(DeepCompare.DeepEquals(s.Clone(testGenerics), testGenerics));
        }
    }

}

using System;
using System.Collections;
using System.Collections.Generic;

namespace SomaSim.Serializer
{
    [TestClass]
    public class SerializerTest
    {
        private string json = @"
        {
            ""ti"": 1,
            ""td"": 3.141,
            ""tstr"": ""This is a test string"",
            ""te"": ""valueofone"",

            ""tHashtable"": {
                ""foo"": ""bar"",
                ""x"": 1,
                ""instance"": {
                    ""#type"": ""SomaSim.Serializer.SerializerTest+TestData"",
                    ""name"": ""foo""
                }
            },
            ""tTypedDictionary"": {
			    ""a"": 1,
			    ""b"": 2,
                ""c"": 3.1
		    },
            ""tTypedDictOfData"": {
                ""foo"": { ""name"": ""wheat"", ""buy"": 10 }
            }            

            ""tArrayList"": [
                1, 2, ""foo"", true
            ],
		    ""tTypedList"": [
			    { ""name"": ""wheat"",  ""buy"": 10, ""sell"": true },
			    { ""name"": ""barley"", ""buy"": 11, ""sell"": false },
			    { ""#type"": ""SomaSim.Serializer.SerializerTest+TestSubData"",
                  ""name"": ""cotton"", ""buy"": 12, ""sell"": true, ""subname"": ""fancy"" }
            ],

            ""tStruct"": {
                ""i"": 123
            },

            ""tTestRecursive"": {
                ""ti"": 4
            },
            ""tTestRecursiveExplicit"": {
                ""#type"": ""SomaSim.Serializer.SerializerTest+Test"",
                ""ti"": 42
            }
        }
        ";

        public class Test
        {
            public int ti;
            public double td;
            public string tstr;
            public TestEnum te;

            public ArrayList tArrayList;
            public List<TestData> tTypedList;

            public Hashtable tHashtable;
            public Dictionary<string, double> tTypedDictionary;
            public Dictionary<string, TestData> tTypedDictOfData;

            public object tTestRecursiveExplicit; // instance of test

            public Test tTestRecursive;
            public Test tTestRecursiveEmpty;

            public TestStruct tStruct;
            public TestStruct tStructEmpty;
        }

        public struct TestStruct
        {
            public int i;
            public int j;
        }

        public class TestData
        {
            public string name;
            public int buy;
            public bool sell;
        }

        public class TestSubData : TestData
        {
            public string subname;
        }

        public enum TestEnum
        {
            ValueOfZero = 0,
            ValueOfOne = 1
        }

        [TestMethod]
        public void TestDeserialization () {
            Serializer serializer = new Serializer();
            serializer.Initialize();

            Hashtable jsonobj = JSON.JsonDecode(json) as Hashtable;
            Test target = serializer.Deserialize<Test>(jsonobj);

            Assert.IsTrue(target.ti == 1);
            Assert.IsTrue(target.td == 3.141);
            Assert.IsTrue(target.tstr == "This is a test string");
            Assert.IsTrue(target.te == TestEnum.ValueOfOne);

            Assert.IsTrue(target.tHashtable.Keys.Count == 3);
            Assert.IsTrue(target.tHashtable["foo"] as String == "bar");
            Assert.IsTrue(target.tHashtable["instance"] is TestData);
            Assert.IsTrue((target.tHashtable["instance"] as TestData).name == "foo");

            Assert.IsTrue(target.tTypedDictionary.Keys.Count == 3);
            Assert.IsTrue(target.tTypedDictionary["a"] == 1);
            Assert.IsTrue(target.tTypedDictionary["b"] == 2);
            Assert.IsTrue(target.tTypedDictionary["c"] == 3.1);

            Assert.IsTrue(target.tTypedDictOfData.Keys.Count == 1);
            Assert.IsTrue(target.tTypedDictOfData["foo"].name == "wheat");

            Assert.IsTrue(target.tArrayList.Count == 4);
            Assert.IsTrue(target.tArrayList[2] as String == "foo");

            Assert.IsTrue(target.tTypedList.Count == 3);
            Assert.IsTrue(target.tTypedList[0].name == "wheat");
            Assert.IsTrue(target.tTypedList[0].buy == 10);
            Assert.IsTrue(target.tTypedList[0].sell);

            Assert.IsTrue(target.tStruct.i == 123);
            Assert.IsTrue(target.tStruct.j == 0);

            Assert.IsTrue(target.tTestRecursive.ti == 4);

            Assert.IsTrue(target.tTestRecursiveExplicit is Test);
            Assert.IsTrue((target.tTestRecursiveExplicit as Test).ti == 42);

            serializer.Release();
        }

        [TestMethod]
        public void TestSerialization () {
            foreach (bool skipDefaults in new bool[] { true, false }) {
                Serializer serializer = new Serializer();
                serializer.Initialize();
                serializer.SkipDefaultsDuringSerialization = skipDefaults;

                Test source = new Test();
                source.tstr = "Test String";
                source.ti = 1;
                source.td = 3.141;
                source.te = TestEnum.ValueOfOne;

                source.tHashtable = new Hashtable { { 1, 2 }, { "foo", new TestData { name = "bar" } } };
                source.tTypedDictionary = new Dictionary<string, double> { { "foo", 1 }, { "bar", 2 } };
                source.tTypedDictOfData = new Dictionary<string, TestData> { { "foo", new TestData { name = "wheat" } } };

                source.tArrayList = new ArrayList { 1, 2, "foo", true, new TestData { name = "wheat" } };
                source.tTypedList = new List<TestData> { new TestData { name = "wheat" }, new TestSubData { name = "bread", subname = "wheat" } };

                source.tTestRecursive = new Test();
                source.tTestRecursive.tstr = "Hello World!";

                source.tStruct.i = 42;

                object result = serializer.Serialize(source);
                Assert.IsTrue(result is Hashtable);
                Assert.IsTrue(((Hashtable)result).Keys.Count == (skipDefaults ? 11 : 14));
                Assert.IsTrue((string)((Hashtable)result)["tstr"] == "Test String");
                Assert.IsTrue((int)((Hashtable)result)["ti"] == 1);
                Assert.IsTrue((double)((Hashtable)result)["td"] == 3.141);
                Assert.IsTrue((int)((Hashtable)result)["te"] == 1); // enum gets serialized as its numeric value

                string json = JSON.JsonEncode(result);
                Assert.IsNotNull(json);

                serializer.Release();
            }
        }

        [TestMethod]
        public void TestSerializationDelta () {
            Serializer serializer = new Serializer();
            serializer.Initialize();
            serializer.SkipDefaultsDuringSerialization = true;

            Test source = new Test();
            source.tstr = "Test String";
            source.tTypedList = new List<TestData> { new TestData { name = "wheat" }, new TestData { name = "bread" } };
            source.tTestRecursive = new Test();
            source.tTestRecursive.tstr = "Hello World!";
            source.tTypedDictOfData = new Dictionary<string, TestData> { { "foo", new TestData { name = "wheat" } } };

            Hashtable serialized = serializer.Serialize(source) as Hashtable;

            Assert.IsTrue(serialized["tstr"] as String == "Test String");
            Assert.IsTrue(serialized["td"] == null);
            Assert.IsTrue(serialized["ti"] == null);
            Assert.IsTrue(serialized["te"] == null);
            Assert.IsTrue((serialized["tTestRecursive"] as Hashtable)["tstr"] as String == "Hello World!");
            Assert.IsTrue(((serialized["tTypedDictOfData"] as Hashtable)["foo"] as Hashtable)["name"] as string == "wheat");

            Test result = serializer.Deserialize<Test>(serialized);

            Assert.IsTrue(source.tstr == result.tstr);
            Assert.IsTrue(source.td == result.td);
            Assert.IsTrue(source.tTestRecursive.tstr == result.tTestRecursive.tstr);
            Assert.IsTrue(source.tTypedDictOfData["foo"].name == result.tTypedDictOfData["foo"].name);

            serializer.Release();
        }

        [TestMethod]
        public void TestSerializationDeltaAndJSON () {
            Serializer serializer = new Serializer();
            serializer.Initialize();
            serializer.SkipDefaultsDuringSerialization = true;

            Test source = new Test();
            source.tstr = "Test String";
            source.ti = 1;
            source.td = 3.141;
            source.tTypedList = new List<TestData> { new TestData { name = "wheat" }, new TestData { name = "bread" } };
            source.tTestRecursive = new Test();
            source.tTestRecursive.tstr = "Hello World!";
            source.tTypedDictOfData = new Dictionary<string, TestData> { { "foo", new TestData { name = "wheat" } } };

            Hashtable serialized = serializer.Serialize(source) as Hashtable;
            Assert.IsNotNull(serialized);

            string json = JSON.JsonEncode(serialized);
            Assert.IsNotNull(json);

            object unjson = JSON.JsonDecode(json);
            Assert.IsNotNull(unjson);

            Test result = serializer.Deserialize<Test>(unjson);
            Assert.IsNotNull(result);

            Assert.IsTrue(source.tstr == result.tstr);
            Assert.IsTrue(source.td == result.td);
            Assert.IsTrue(source.ti == result.ti);
            Assert.IsTrue(source.tTestRecursive.tstr == result.tTestRecursive.tstr);
            Assert.IsTrue(source.tTypedDictOfData["foo"].name == result.tTypedDictOfData["foo"].name);
            Assert.IsTrue(source.tTypedDictOfData["foo"].buy == result.tTypedDictOfData["foo"].buy);

            serializer.Release();
        }

        [TestMethod]
        public void TestClone () {
            foreach (bool skipDefaults in new bool[] { true, false }) {
                Serializer serializer = new Serializer();
                serializer.Initialize();
                serializer.SkipDefaultsDuringSerialization = skipDefaults;

                Test source = new Test();
                source.tstr = "Test String";
                source.ti = 1;
                source.td = 3.141;
                source.te = TestEnum.ValueOfOne;

                source.tHashtable = new Hashtable { { 1, 2 }, { "foo", new TestData { name = "bar" } } };
                source.tTypedDictionary = new Dictionary<string, double> { { "foo", 1 }, { "bar", 2 } };
                source.tTypedDictOfData = new Dictionary<string, TestData> { { "foo", new TestData { name = "wheat" } } };

                source.tArrayList = new ArrayList { 1, 2, "foo", true, new TestData { name = "wheat" } };
                source.tTypedList = new List<TestData> { new TestData { name = "wheat" }, new TestData { name = "bread" } };

                source.tTestRecursive = new Test();
                source.tTestRecursive.tstr = "Hello World!";

                Test target = serializer.Clone(source);
                Assert.IsTrue(target != source);
                Assert.IsTrue(source.tstr == target.tstr);
                Assert.IsTrue(source.ti == target.ti);
                Assert.IsTrue(source.td == target.td);
                Assert.IsTrue(source.te == target.te);
                
                Assert.IsTrue(source.tHashtable != target.tHashtable);
                Assert.IsTrue(source.tHashtable.Count == target.tHashtable.Count);
                Assert.IsTrue((int)(source.tHashtable[1]) == (int)(target.tHashtable[1]));
                Assert.IsTrue(((TestData)(source.tHashtable["foo"])).name == ((TestData)(target.tHashtable["foo"])).name);

                Assert.IsTrue(source.tArrayList.Count == target.tArrayList.Count);
                Assert.IsTrue(source.tTypedList.Count == target.tTypedList.Count);
                Assert.IsTrue(source.tTypedList[0].name == target.tTypedList[0].name);

                serializer.Release();
            }
        }

        [TestMethod]
        public void TestDeserializingImplicitNamespaces () {
            string test = @"
                {
                    ""tHashtable"": {
                        ""foo"": ""bar"",
                        ""x"": 1,
                        ""instance"": {
                            ""#type"": ""TestData"",
                            ""name"": ""foo""
                        }
                    }
                }";

            Serializer serializer = new Serializer();
            serializer.Initialize();

            serializer.AddImplicitNamespace("SomaSim.Serializer.SerializerTest", false); // false because it's an enclosing class, not a namespace!

            Hashtable jsonobj = JSON.JsonDecode(test) as Hashtable;
            Test target = serializer.Deserialize<Test>(jsonobj);

            Assert.IsTrue(target.tHashtable != null);
            Assert.IsTrue(target.tHashtable["instance"] != null);
            Assert.IsTrue(target.tHashtable["instance"] is TestData);
            Assert.IsTrue((target.tHashtable["instance"] as TestData).name == "foo");
        }

        [TestMethod]
        public void TestShorthandDeserializer () {
            string test = @"
                {
                    ""tHashtable"": {
                        ""instance"": ""_ test-data name foo buy 42 sell true ""
                    }
                }";

            Serializer serializer = new Serializer();
            serializer.Initialize();

            serializer.AddImplicitNamespace("SomaSim.Serializer.SerializerTest", false);

            Hashtable jsonobj = JSON.JsonDecode(test) as Hashtable;
            Test target = serializer.Deserialize<Test>(jsonobj);

            Assert.IsTrue(target.tHashtable != null);
            Assert.IsTrue(target.tHashtable["instance"] != null);
            Assert.IsTrue(target.tHashtable["instance"] is TestData);
            Assert.IsTrue((target.tHashtable["instance"] as TestData).name == "foo");
            Assert.IsTrue((target.tHashtable["instance"] as TestData).buy == 42);
            Assert.IsTrue((target.tHashtable["instance"] as TestData).sell);
        }
    }
}

// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System.Collections;

namespace SomaSim.SION
{
    [TestClass]
    public class HashtableMergerTest
    {
        [TestMethod]
        public void TestHashtableMerger () {
            object parent = JSON.JsonDecode(@"
                {
                    ""Base"": ""base"",
                    ""Name"": ""test"",
                    ""Sprite"": {
                        ""FrameOffset"": { ""x"": 0, ""y"": 0 },
                        ""FrameSize"": { ""x"": 32, ""y"": 32 },
                        ""FrameCount"": 3,
                        ""FramesPerSecond"": 5,
                        ""FrameTypes"": [ 1, 3, 5 ]
                    }
                }
            ");
            object child = JSON.JsonDecode(@"
                {
                    ""Name"": ""test-two"",
                    ""Sprite"": {
                        ""FrameOffset"": { ""x"": 10 },
                        ""FramesPerSecond"": 6,
                        ""FrameTypes"": [ 2, 4 ],
                        ""NewField"": true
                    }
                }
            ");

            var result = HashtableMerger.Merge(parent, child, false);
            Hashtable r = result as Hashtable;
            Assert.IsNotNull(r);

            Assert.IsTrue(r["Name"] as string == ("test-two"));
            Assert.IsTrue(r["Base"] as string == "base");

            Hashtable s = r["Sprite"] as Hashtable;
            Assert.IsNotNull(s);

            Assert.IsTrue(s["FrameOffset"] is Hashtable);
            Assert.IsTrue((s["FrameOffset"] as Hashtable)["x"] is double);
            Assert.IsTrue((double) (s["FrameOffset"] as Hashtable)["x"] == 10.0);
            Assert.IsTrue((double) (s["FrameOffset"] as Hashtable)["y"] == 0);

            Assert.IsTrue((double) (s["FrameSize"] as Hashtable)["x"] == 32);
            Assert.IsTrue((double) (s["FrameSize"] as Hashtable)["y"] == 32);

            Assert.IsTrue((double) s["FrameCount"] == 3);
            Assert.IsTrue((double) s["FramesPerSecond"] == 6);
            Assert.IsTrue((bool) s["NewField"] == true);

            Assert.IsTrue(s["FrameTypes"] is ArrayList);
            Assert.IsTrue((s["FrameTypes"] as ArrayList).Count == 2);
        }


        [TestMethod]
        public void TestHashtableMergerArrayListSettings () {
            object parent = JSON.JsonDecode(@"{ ""Test"": [ 1, 2, 3 ] }");
            object child = JSON.JsonDecode(@"{ ""Test"": [ 4, 5 ] }");

            // first test replacing parent with child
            var result = HashtableMerger.Merge(parent, child, false);
            ArrayList arr = ((result as Hashtable)["Test"]) as ArrayList;
            Assert.IsNotNull(arr);
            Assert.IsTrue(arr.Count == 2);
            Assert.IsTrue((double) arr[0] == 4);
            Assert.IsTrue((double) arr[1] == 5);

            // second test merging parent with child
            result = HashtableMerger.Merge(parent, child, true);
            arr = ((result as Hashtable)["Test"]) as ArrayList;
            Assert.IsNotNull(arr);
            Assert.IsTrue(arr.Count == 5);
            Assert.IsTrue((double) arr[0] == 1);
            Assert.IsTrue((double) arr[1] == 2);
            Assert.IsTrue((double) arr[2] == 3);
            Assert.IsTrue((double) arr[3] == 4);
            Assert.IsTrue((double) arr[4] == 5);

        }
    }
}

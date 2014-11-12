using System;
using System.Collections;

namespace SomaSim.Serializer
{
    [TestClass]
    public class JSONMergerTest
    {
        [TestMethod]
        public void TestMerger () {
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

            var result = JSONMerger.Merge(parent, child);
            Hashtable r = result as Hashtable;
            Assert.IsNotNull(r);

            Assert.IsTrue(r["Name"] as string == ("test-two"));
            Assert.IsTrue(r["Base"] as string == "base");

            Hashtable s = r["Sprite"] as Hashtable;
            Assert.IsNotNull(s);

            Assert.IsTrue(s["FrameOffset"] is Hashtable);
            Assert.IsTrue((s["FrameOffset"] as Hashtable)["x"] is double);
            Assert.IsTrue((double)(s["FrameOffset"] as Hashtable)["x"] == 10.0);
            Assert.IsTrue((double)(s["FrameOffset"] as Hashtable)["y"] == 0);

            Assert.IsTrue((double)(s["FrameSize"] as Hashtable)["x"] == 32);
            Assert.IsTrue((double)(s["FrameSize"] as Hashtable)["y"] == 32);

            Assert.IsTrue((double)s["FrameCount"] == 3);
            Assert.IsTrue((double)s["FramesPerSecond"] == 6);
            Assert.IsTrue((bool)s["NewField"] == true);

            Assert.IsTrue(s["FrameTypes"] is ArrayList);
            Assert.IsTrue((s["FrameTypes"] as ArrayList).Count == 2);
        }
    }
}

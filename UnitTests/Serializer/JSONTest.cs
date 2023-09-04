// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System.Collections;

namespace SomaSim.SION
{
    [TestClass]
    public class JSONTest
    {
        private string json1 = @"
        {
		    ""multi"": {
			    ""x"": [    0,  0.5, 0.5, 1 ],
			    ""y"": [ 0.05, 0.15,   1, 1 ]
		    },
		    ""baseprices"": [
			    { ""name"": ""wheat"",  		""buy"": 10,	""sell"": true },
			    { ""name"": ""barley"",  		""buy"": 11,	""sell"": false },
			    { ""name"": ""cottom"",  		""buy"": 12,	""sell"": true }
            ]
        }
        ";

        [TestMethod]
        public void TestDecode () {
            object result = JSON.JsonDecode(json1);
            Assert.IsTrue(result is Hashtable);
            Assert.IsTrue((result as Hashtable)["multi"] is Hashtable);
            Assert.IsTrue(((result as Hashtable)["multi"] as Hashtable)["x"] is ArrayList);
            Assert.IsTrue((result as Hashtable)["baseprices"] is ArrayList);
            Assert.IsTrue(((result as Hashtable)["baseprices"] as ArrayList).Count == 3);
        }

        [TestMethod]
        public void TestDecodeEncode () {
            object result = JSON.JsonDecode(json1);
            string encoded = JSON.JsonEncode(result);
            Assert.IsNotNull(encoded);
        }
    }
}

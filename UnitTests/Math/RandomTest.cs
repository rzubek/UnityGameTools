﻿// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

namespace SomaSim.Util
{
    [TestClass]
    public class RandomTest
    {
        [TestMethod]
        public void TestTinyMT () {
            IRandom rand = new TinyMT();

            // these magic constants courtesy of the original C implementation test suite

            rand.Init(1);
            Assert.IsTrue(rand.Generate() == 2545341989);
            Assert.IsTrue(rand.Generate() == 981918433);
            Assert.IsTrue(rand.Generate() == 3715302833);
            Assert.IsTrue(rand.Generate() == 2387538352);
            Assert.IsTrue(rand.Generate() == 3591001365);
        }

        [TestMethod]
        public void TestTinyMTState () {
            TinyMT mt1 = new TinyMT();
            TinyMT mt2 = new TinyMT();

            mt1.Init(1);
            Assert.IsTrue(mt1.Generate() == 2545341989);
            Assert.IsTrue(mt1.Generate() == 981918433);

            // copy state to a brand new rng, and make sure it picks up where the first one left off
            mt1.s.CopyTo(mt2.s, 0);
            Assert.IsTrue(mt2.Generate() == 3715302833 && mt1.Generate() == 3715302833);
        }

        [TestMethod]
        public void TestIRandomExtensionMethods () {
            IRandom rand = new TinyMT();

            rand.Init(1); // 2545341989 will be the first number
            Assert.IsTrue(rand.Generate(0, 5) == 4); // 2545341989 % 5 = 4

            rand.Init(1);
            Assert.IsTrue(rand.Generate(1, 6) == 5); // 2545341989 % 5 + 1 = 5

            rand.Init(1);
            Assert.IsTrue(rand.PickElement(new uint[] { 0, 1, 2, 3, 4 }) == 4); // 2545341989 % 5 = 4, so 4th element

            rand.Init(1);
            var roll = rand.DieRoll(10);
            Assert.IsTrue(roll == 9); // last digit of 2545341989 

            RandomRange range = new RandomRange(0, 10, 3);
            rand.Init(1);
            // first random number: 2545341989 % 10 = 9
            // second random number: 981918433 % 10 = 3
            // third random number: 3715302833 % 10 = 3
            // total: 15, over three passes: result = 5
            Assert.IsTrue(rand.Generate(range) == 5);
        }

        [TestMethod]
        public void TestHashUtil () {
            // test consecutive inputs producing non-consecutive hashes
            uint a = HashUtil.Hash(1), b = HashUtil.Hash(2), c = HashUtil.Hash(3);
            Assert.IsTrue(a == 2298633409);
            Assert.IsTrue(b == 479680206);
            Assert.IsTrue(c == 3674312685);

            // test string hashing, also well-distributed
            // and remains the same across .net versions and platforms
            uint s = HashUtil.Hash("Hello1"), t = HashUtil.Hash("Hello2");
            Assert.IsTrue(s == 925636315);
            Assert.IsTrue(t == 625902542);
        }
    }
}

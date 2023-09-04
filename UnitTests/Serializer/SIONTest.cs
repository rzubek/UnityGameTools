// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections;

namespace SomaSim.SION
{
    [TestClass]
    public class TestSION
    {
        [TestMethod]
        public void TestSIONParsePrimitives () {

            // booleans and null

            object t = SION.Parse("#true");
            Assert.IsTrue(t is Boolean && ((Boolean) t) == true);

            object f = SION.Parse("#false");
            Assert.IsTrue(f is Boolean && ((Boolean) f) == false);

            object n = SION.Parse("#null");
            Assert.IsTrue(n == null);

            Assert.IsTrue(((string) SION.Parse("#unknowntoken")) == "#unknowntoken");

            // numbers

            Assert.IsTrue((double) (SION.Parse("0")) == 0);
            Assert.IsTrue((double) (SION.Parse("1")) == 1);
            Assert.IsTrue((double) (SION.Parse("1.1")) == 1.1);
            Assert.IsTrue((double) (SION.Parse("-1")) == -1);
            Assert.IsTrue((double) (SION.Parse("+1")) == 1);

            AssertParseFails("+text");
            AssertParseFails("-text");
            AssertParseFails("-+1");
            AssertParseFails(".5");

            // simple strings

            Assert.IsTrue((string) (SION.Parse("HelloWorld")) == "HelloWorld");
            Assert.IsTrue((string) (SION.Parse("_Hello")) == "_Hello");
            Assert.IsTrue((string) (SION.Parse("string_foo-bar")) == "string_foo-bar");
            Assert.IsTrue((string) (SION.Parse("a123")) == "a123");

            AssertParseFails("-hello");
            Assert.IsTrue((double) (SION.Parse("31337haxxor")) == 31337.0); // two values, first a number, second a string
            Assert.IsTrue((string) (SION.Parse("Hello World")) == "Hello"); // these are actually two separate simple strings!

            Assert.IsTrue(SION.Parse("s-1") is string);
            Assert.IsTrue((string) SION.Parse("s-1") == "s-1");
            Assert.IsTrue(SION.Parse("s-10") is string);
            Assert.IsTrue((string) SION.Parse("s-10") == "s-10");

            // quoted strings

            // hard to read because we need to convert those strings into C# compatible format first... sigh...
            Assert.IsTrue((string) (SION.Parse("\"Hello\"")) == "Hello");
            Assert.IsTrue((string) (SION.Parse("\"Hello \\\"Bob\\\"\"")) == "Hello \"Bob\"");
            Assert.IsTrue((string) (SION.Parse(@"""c:\\windows""")) == @"c:\windows");
            Assert.IsTrue((string) (SION.Parse("'c:\\windows \"test\"'")) == "c:\\windows \"test\"");
            Assert.IsTrue((string) (SION.Parse("\"foo\\r\\nbar\"")) == "foo\r\nbar");

            AssertParseFails("\"Hello"); // unclosed quote
            AssertParseFails("\"Hello'"); // closed incorrectly
            Assert.IsTrue((string) (SION.Parse("'O\\'Hare'")) == "O\\"); // fail because can't escape inside single-quote
        }

        [TestMethod]
        public void TestSIONParseCollections () {

            // lists

            AssertEqual(SION.Parse("[foo bar \"baz baz\"]"), new ArrayList() { "foo", "bar", "baz baz" });
            AssertEqual(SION.Parse(" [foo  #null  [1   2  #true]] "), new ArrayList() { "foo", null, new ArrayList() { 1.0, 2.0, true } });

            AssertParseFails("[ foo bar }"); // not closed

            // dictionaries

            AssertEqual(SION.Parse("{name Dennis age 37 old #false}"),
                        new Hashtable() { { "name", "Dennis" }, { "age", 37.0 }, { "old", false } });

            AssertParseFails("{ foo bar ]"); // not closed
            AssertParseFails("{ foo bar baz }"); // missing value for key "baz"

            // lists of dictionaries

            AssertEqual(SION.Parse("[{name Dennis age 37} {name \"King Arthur\" age 100}]"),
                new ArrayList() {
                    new Hashtable() { { "name", "Dennis" }, { "age", 37.0 } },
                    new Hashtable() { { "name", "King Arthur" }, { "age", 100.0 } }
                });

            // dictionaries of lists 

            AssertEqual(SION.Parse("{keys [foo bar baz] values [1 2 3]}"),
                new Hashtable() {
                    { "keys", new ArrayList() { "foo", "bar", "baz" } },
                    { "values", new ArrayList() { 1.0, 2.0, 3.0 } }
                });

            // comments

            AssertEqual(SION.Parse("{name Dennis ; this is a comment until end of line... \n  age 37 old #false}"),
                        new Hashtable() { { "name", "Dennis" }, { "age", 37.0 }, { "old", false } });


        }

        [TestMethod]
        public void TestSIONPrintPrimitives () {
            int digits = SION.PrintSettings.MaxFloatDoubleDecimalDigits;
            SION.PrintSettings.MaxFloatDoubleDecimalDigits = -1;

            Assert.IsTrue(SION.Print(null) == "#null");
            Assert.IsTrue(SION.Print(true) == "#true");
            Assert.IsTrue(SION.Print(false) == "#false");
            Assert.IsTrue(SION.Print((double) 42) == "42");
            Assert.IsTrue(SION.Print((double) 1.123456) == "1.123456");
            Assert.IsTrue(SION.Print((float) 0.0001f) == "0.0001");
            Assert.IsTrue(SION.Print((float) 1.0e-20) == "0");
            Assert.IsTrue(SION.Print((float) 1.0e20) == "100000000000000000000");
            Assert.IsTrue(SION.Print(42) == "42");
            Assert.IsTrue(SION.Print(42L) == "42");
            Assert.IsTrue(SION.Print("foo-bar_baz-123") == "foo-bar_baz-123"); // simple string
            Assert.IsTrue(SION.Print("foo\r\nbar") == "\"foo\\r\\nbar\"");
            Assert.IsTrue(SION.Print(@"c:\windows") == "\"c:\\windows\"");
            Assert.IsTrue(SION.Print("hello, \"world\"") == "\"hello, \\\"world\\\"\"");

            SION.PrintSettings.MaxFloatDoubleDecimalDigits = digits;
        }

        [TestMethod]
        public void TestSIONPrintMaxFloatDigits () {
            int digits = SION.PrintSettings.MaxFloatDoubleDecimalDigits;

            SION.PrintSettings.MaxFloatDoubleDecimalDigits = 10;
            Assert.IsTrue(SION.Print((double) 1.123456) == "1.123456");
            Assert.IsTrue(SION.Print((float) 1.123456f) == "1.123456");

            SION.PrintSettings.MaxFloatDoubleDecimalDigits = 5;
            Assert.IsTrue(SION.Print((double) 1.123456) == "1.12345");
            Assert.IsTrue(SION.Print((float) 1.123456f) == "1.12345");

            SION.PrintSettings.MaxFloatDoubleDecimalDigits = 3;
            Assert.IsTrue(SION.Print((double) 1.123456) == "1.123");
            Assert.IsTrue(SION.Print((float) 1.123456f) == "1.123");

            SION.PrintSettings.MaxFloatDoubleDecimalDigits = 1;
            Assert.IsTrue(SION.Print((double) 1.123456) == "1.1");
            Assert.IsTrue(SION.Print((float) 1.123456f) == "1.1");

            SION.PrintSettings.MaxFloatDoubleDecimalDigits = 0;
            Assert.IsTrue(SION.Print((double) 1.123456) == "1");
            Assert.IsTrue(SION.Print((float) 1.123456f) == "1");

            SION.PrintSettings.MaxFloatDoubleDecimalDigits = 3; // make sure we don't lose digits before decimal point
            Assert.IsTrue(SION.Print((double) int.MaxValue) == "2147483647");
            SION.PrintSettings.MaxFloatDoubleDecimalDigits = 0;
            Assert.IsTrue(SION.Print((double) int.MaxValue) == "2147483647");
            SION.PrintSettings.MaxFloatDoubleDecimalDigits = -1;
            Assert.IsTrue(SION.Print((double) int.MaxValue) == "2147483647");

            SION.PrintSettings.MaxFloatDoubleDecimalDigits = digits;
        }

        [TestMethod]
        public void TestSIONPrintDataScructures () {

            // simple list

            IList list = new ArrayList() { "foo", "bar\n", 1, 2.1, true, null };
            Assert.IsTrue(SION.Print(list) == "[foo \"bar\\n\" 1 2.1 #true #null]");

            // simple dictionary

            IDictionary dict = new Hashtable() { { "foo", 1 }, { "bar", 2 } };
            string dictprinted = SION.Print(dict);
            Assert.IsTrue(dictprinted == "{foo 1 bar 2}" || dictprinted == "{bar 2 foo 1}");

            /*
                [{foo bar
                    1 2}
                  {names [Alice Bob]
                    values [42 42]}]
             */
        }

        [TestMethod]
        public void TestPrintAndReparse () {

            IList list = new ArrayList() {
                new Hashtable() { { "foo", "bar" }, { "value", 1.0 }, { "visible", true } },
                new Hashtable() { { "names", new ArrayList() { "Alice", "Bob", "Carol" } },
                                  { "values", new ArrayList() { 1.0, 2.0, 3.0 } } }
            };

            string printed = SION.Print(list);

            /* printed ==
                 [{
                    foo bar
                    visible #true
                    value 1}
                  {
                    names [Alice Bob Carol]
                    values [1 2 3]}]
             */

            object parsed = SION.Parse(printed);

            AssertEqual(list, parsed);
        }

        private void AssertEqual (object left, object right) {
            if (left == null) {
                Assert.IsTrue(left == right); // trivially true
            } else if (left is ArrayList && right is ArrayList) {
                AssertLists(left as ArrayList, right as ArrayList);
            } else if (left is Hashtable && right is Hashtable) {
                AssertHashtables(left as Hashtable, right as Hashtable);
            } else {
                Assert.IsTrue(left.Equals(right));
            }
        }

        private void AssertHashtables (Hashtable left, Hashtable right) {
            Assert.IsTrue(left.Count == right.Count);
            var lkeys = left.Keys;
            foreach (var lkey in lkeys) {
                Assert.IsTrue(right.ContainsKey(lkey));
                var lval = left[lkey];
                var rval = right[lkey];
                AssertEqual(lval, rval);
            }
        }

        private void AssertLists (ArrayList left, ArrayList right) {
            Assert.IsTrue(left.Count == right.Count);
            for (int i = 0; i < left.Count; i++) {
                AssertEqual(left[i], right[i]);
            }
        }

        private void AssertParseFails (string text) {
            bool caught = false;

#pragma warning disable 0219 // results not being used
            try {
                object results = SION.Parse(text);
            } catch (SIONException e) {
                caught = (e != null);
            }
#pragma warning restore 0219

            Assert.IsTrue(caught);
        }
    }
}

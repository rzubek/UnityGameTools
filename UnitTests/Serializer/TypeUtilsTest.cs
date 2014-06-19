using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SomaSim.Serializer
{
    public interface FooBar { };
    public class Foo : FooBar { };
    public class Bar : FooBar { };
    public class Baz { };
    public struct Struct { };

    public class TestClass
    {
        public Foo foo;
        public Bar bar;
        public Baz baz;
        public double d;
        public int i;
        public Struct s;
        public TestClass tc;

        public Foo fooFunction() { return null; }
        public Baz bazFunction() { return null; }
    }

    [TestClass]
    public class TypeUtilsTest
    {
        [TestMethod]
        public void TestGetFieldsByType()
        {
            var tc = new TestClass();
            var fields = TypeUtils.GetFieldsByType<FooBar>(tc);
            Assert.IsTrue(fields.Count == 2); // just foo and bar
        }

        [TestMethod]
        public void TestMakeAndRemoveFieldsByType()
        {
            var tc = new TestClass();
            
            TypeUtils.MakeFieldInstances<FooBar>(tc);
            Assert.IsInstanceOfType(tc.foo, typeof(Foo));
            Assert.IsInstanceOfType(tc.bar, typeof(Bar));
            Assert.IsNull(tc.baz);

            tc.baz = new Baz();
            TypeUtils.RemoveFieldInstances<FooBar>(tc);
            Assert.IsNull(tc.foo);
            Assert.IsNull(tc.bar);
            Assert.IsNotNull(tc.baz);
        }
    }
}

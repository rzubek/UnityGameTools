using System;

namespace SomaSim.Serializer
{
    public interface FooBar { };
    public class Foo : FooBar { };
    public class Bar : FooBar { };
    public class Baz { };
    public struct Struct { };

    public class Data
    {
        public Foo foo;
        public Bar bar;
        public Baz baz;
        public double d;
        public int i;
        public Struct s;
        public Data tc;

        public Foo fooFunction () { return null; }
        public Baz bazFunction () { return null; }
    }

    [TestClass]
    public class TypeUtilsTest
    {
        [TestMethod]
        public void TestGetFieldsByType () {
            var tc = new Data();
            var fields = TypeUtils.GetMembersByType<FooBar>(tc);
            Assert.IsTrue(fields.Count == 2); // just foo and bar

            var fields2 = TypeUtils.GetMembersByType(typeof(FooBar), tc);
            Assert.IsTrue(fields2.Count == 2); // same
        }

        [TestMethod]
        public void TestSetValueByType () {
            var tc = new Data();
            Assert.IsNull(tc.bar);
            TypeUtils.SetValueByType(tc, new Bar());
            Assert.IsNotNull(tc.bar);
            Assert.IsNull(tc.foo);
            Assert.IsNull(tc.baz);
        }

        [TestMethod]
        public void TestMakeAndRemoveFieldsByType () {
            var tc = new Data();

            TypeUtils.MakeMemberInstances<FooBar>(tc);
            Assert.IsInstanceOfType(tc.foo, typeof(Foo));
            Assert.IsInstanceOfType(tc.bar, typeof(Bar));
            Assert.IsNull(tc.baz);

            tc.baz = new Baz();
            TypeUtils.RemoveMemberInstances<FooBar>(tc);
            Assert.IsNull(tc.foo);
            Assert.IsNull(tc.bar);
            Assert.IsNotNull(tc.baz);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TestClass : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestMethod : Attribute
    {
    }

    public class UnitTestException : Exception {
        public UnitTestException () : base() {}
        public UnitTestException (string message) : base(message) {}
        public UnitTestException (string message, Exception exception) : base(message, exception) {}
    }

    public class Assert
    {
        public static void IsTrue (bool value) {
            if (!value) { throw new UnitTestException("Assert.IsTrue failed"); }
        }

        public static void IsFalse (bool value) {
            if (value) { throw new UnitTestException("Assert.IsFalse failed"); }
        }

        public static void IsNull (object value) {
            if (value != null) { throw new UnitTestException("Assert.IsNull failed"); }
        }

        public static void IsNotNull (object value) {
            if (value == null) { throw new UnitTestException("Assert.IsNotNull failed"); }
        }
    
        public static void IsInstanceOfType (object value, Type type) {
            var valueType = (value != null) ? value.GetType() : null;
            if (valueType != type) { throw new UnitTestException("Assert.IsInstanceOfType failed, expected " + type + ", got " + valueType); }
        }
    }
}

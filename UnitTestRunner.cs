using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SomaSim
{
    public class UnitTestRunner : MonoBehaviour
    {
#if UNITY_EDITOR
        public void Start () {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            RunUnitTests();
            timer.Stop();
            Debug.Log("Unit tests ran in " + timer.Elapsed.TotalSeconds + " seconds");
        }

        private void RunUnitTests () {
            foreach (Type testClass in GetTestClasses()) {
                var instance = Activator.CreateInstance(testClass);
                RunTestMethods(instance);
            }
        }

        private void RunTestMethods (object testInstance) {
            foreach (MethodInfo method in testInstance.GetType().GetMethods()) {
                if (method.GetCustomAttributes(typeof(TestMethod), true).Length > 0) {
                    method.Invoke(testInstance, null);
                }
            }
        }

        private static IEnumerable<Type> GetTestClasses () {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type type in assembly.GetTypes()) {
                    if (type.GetCustomAttributes(typeof(TestClass), true).Length > 0) {
                        yield return type;
                    }
                }
            }
        }
    }
#endif
}

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
            int count = RunUnitTests();
            timer.Stop();
            Debug.Log("Unit tests: " + count + " tests ran in " + timer.Elapsed.TotalSeconds + " seconds");
        }

        private int RunUnitTests () {
            int sum = 0;
            foreach (Type testClass in GetTestClasses()) {
                sum += RunTestMethods(Activator.CreateInstance(testClass));
            }
            return sum;
        }

        private int RunTestMethods (object testInstance) {
            int sum = 0;
            foreach (MethodInfo method in testInstance.GetType().GetMethods()) {
                if (method.GetCustomAttributes(typeof(TestMethod), true).Length > 0) {
                    sum++;

                    try {
                        method.Invoke(testInstance, null);
                    } catch (UnitTestException e) {
                        Debug.LogError("UNIT TEST FAILURE in " + method.ToString() + "\n" + e.Message + "\n" + e.StackTrace);
                    } catch (Exception e) {
                        Debug.LogError("UNIT TEST ERROR in " + method.ToString() + "\n" + e.InnerException);
                    }
                }
            }
            return sum;
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
#endif
    }
}

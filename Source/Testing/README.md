Unit Tests
===

Unit test harness that works in two steps:
1.  mark up unit test classes and methods with \[TestClass\] and \[TestMethod\] respectively, 
2.  add the [UnitTestRunner](UnitTestRunner.cs) component to some kind of a global game object that exists at start time.

When started up in the editor, UnitTestRunner will 
1.  scan all assemblies for classes annotated as *TestClass*
2.  create an instance of each such class, then
3.  run each method annotated as *TestMethod*
4.  finally, report any failures and run time

UnitTests only get run in the editor.

[UnityTestUtils](UnityTestUtils.cs) provides a simple Assert class for testing inside unit tests, eg. 

    var result = FunctionToBeTested();
    Assert.IsNotNull(result);


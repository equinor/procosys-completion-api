using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectCommandTests;
using Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;
using Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

/// <summary>
/// This unit test don't test any business logic.
/// Just a helper for developer to remember to add Unit Test for AccessValidator when implementing
/// a new IIsProjectCommand, IIsPunchItemCommand or IIsPunchItemQuery
/// </summary>
[TestClass]
public class AccessValidatorTestCoverageTests
{
    [TestMethod]
    public void Each_IIsProjectCommand_ShouldHaveUnitTest_ForAccessValidator()
    {
        var classes =
            TestHelper.GetClassesImplementingInterface(
                "Equinor.ProCoSys.Completion.Command",
                typeof(IIsProjectCommand));
        var testClassList
            = TestHelper.GetTestsWhichInheritsBaseClass(
                Assembly.GetExecutingAssembly(),
                typeof(AccessValidatorForIIsProjectCommandTests<>));

        AssertAllHasUnitTest(classes, testClassList);
    }

    [TestMethod]
    public void Each_IIsPunchItemCommand_ShouldHaveUnitTest_ForAccessValidator()
    {
        var classes =
            TestHelper.GetClassesImplementingInterface(
                "Equinor.ProCoSys.Completion.Command",
                typeof(IIsPunchItemCommand));

        var testClassList
            = TestHelper.GetTestsWhichInheritsBaseClass(
                Assembly.GetExecutingAssembly(),
                typeof(AccessValidatorForIIsPunchItemCommandTests<>));

        AssertAllHasUnitTest(classes, testClassList);
    }

    [TestMethod]
    public void Each_IIsPunchItemQuery_ShouldHaveUnitTest_ForAccessValidator()
    {
        var classes =
            TestHelper.GetClassesImplementingInterface(
                "Equinor.ProCoSys.Completion.Query",
                typeof(IIsPunchItemQuery));

        var testClassList
            = TestHelper.GetTestsWhichInheritsBaseClass(
                Assembly.GetExecutingAssembly(),
                typeof(AccessValidatorForIIsPunchItemQueryTests<>));

        AssertAllHasUnitTest(classes, testClassList);
    }

    private static void AssertAllHasUnitTest(List<Type> classes, List<Type> testClassList)
    {
        Assert.AreNotEqual(0, classes.Count);
        
        var missingTests = new List<string>();
        var genericArguments = TestHelper.GetGenericArgumentsOfBaseClasses(testClassList);
        foreach (var c in classes)
        {
            var classHasTest = genericArguments.Any(g => g.FullName == c.FullName);
            if (!classHasTest)
            {
                missingTests.Add(c.FullName);
            }
        }

        Assert.AreEqual(0, missingTests.Count,
            $"{missingTests.Count} tests are missing: {string.Join(",", missingTests)}");
    }
}

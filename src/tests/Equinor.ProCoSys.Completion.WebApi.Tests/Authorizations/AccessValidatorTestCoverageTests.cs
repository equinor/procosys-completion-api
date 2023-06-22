using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Query.PunchQueries;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectCommandTests;
using Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;
using Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

/// <summary>
/// This unit test don't test any business logic.
/// Just a helper for developer to remember to add Unit Test for AccessValidator when implementing
/// a new IIsProjectCommand, IIsPunchCommand or IIsPunchQuery
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
    public void Each_IIsPunchCommand_ShouldHaveUnitTest_ForAccessValidator()
    {
        var classes =
            TestHelper.GetClassesImplementingInterface(
                "Equinor.ProCoSys.Completion.Command",
                typeof(IIsPunchCommand));

        var testClassList
            = TestHelper.GetTestsWhichInheritsBaseClass(
                Assembly.GetExecutingAssembly(),
                typeof(AccessValidatorForIIsPunchCommandTests<>));

        AssertAllHasUnitTest(classes, testClassList);
    }

    [TestMethod]
    public void Each_IIsPunchQuery_ShouldHaveUnitTest_ForAccessValidator()
    {
        var classes =
            TestHelper.GetClassesImplementingInterface(
                "Equinor.ProCoSys.Completion.Query",
                typeof(IIsPunchQuery));

        var testClassList
            = TestHelper.GetTestsWhichInheritsBaseClass(
                Assembly.GetExecutingAssembly(),
                typeof(AccessValidatorForIIsPunchQueryTests<>));

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

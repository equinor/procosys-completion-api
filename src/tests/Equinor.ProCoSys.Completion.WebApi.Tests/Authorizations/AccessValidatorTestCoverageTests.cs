using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

/// <summary>
/// This unit test don't test any business logic.
/// Just a helper for developer to remember to add Unit Test for AccessValidator when implementing
/// a new MediatR command which need access check
/// </summary>
[TestClass]
public class AccessValidatorTestCoverageTests
{
    [TestMethod]
    public void Each_Command_NeedProjectAccess_ShouldHaveUnitTest_ForAccessValidator()
    {
        var classes =
            TestHelper.GetClassesInheritsBaseClass(
                "Equinor.ProCoSys.Completion.Command",
                typeof(INeedProjectAccess));
        var testClassList
            = TestHelper.GetTestsWhichInheritsBaseClass(
                Assembly.GetExecutingAssembly(),
                typeof(AccessValidatorTestBase));

        AssertAllHasUnitTest(classes, testClassList);
    }

    [TestMethod]
    public void Each_Query_NeedProjectAccess_ShouldHaveUnitTest_ForAccessValidator()
    {
        var classes =
            TestHelper.GetClassesInheritsBaseClass(
                "Equinor.ProCoSys.Completion.Query",
                typeof(INeedProjectAccess));
        var testClassList
            = TestHelper.GetTestsWhichInheritsBaseClass(
                Assembly.GetExecutingAssembly(),
                typeof(AccessValidatorTestBase));

        AssertAllHasUnitTest(classes, testClassList);
    }

    [TestMethod]
    public void Each_Command_NeedRestrictionAccessCheck_ShouldHaveUnitTest_ForAccessValidator()
    {
        var classes =
            TestHelper.GetClassesInheritsBaseClass(
                "Equinor.ProCoSys.Completion.Command",
                typeof(ICanHaveRestrictionsViaCheckList));
        var testClassList
            = TestHelper.GetTestsWhichInheritsBaseClass(
                Assembly.GetExecutingAssembly(),
                typeof(AccessValidatorTestBase));

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
                missingTests.Add(c.FullName!);
            }
        }

        Assert.AreEqual(0, missingTests.Count,
            $"{missingTests.Count} tests are missing: {string.Join(",", missingTests)}");
    }
}

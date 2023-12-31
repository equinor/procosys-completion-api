﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests;

/// <summary>
/// This unit test don't test any business logic.
/// Just a helper for developer to remember to add ContractTestBase Unit Test when implementing a new IIntegrationEvent
/// </summary>
[TestClass]
public class ContractNotBreachedTestCoverageTests
{
    [TestMethod]
    public void Each_IIntegrationEvent_ShouldHaveUnitTest_ForContractBreach()
    {
        var interfaces =
            TestHelper.GetInterfacesImplementingInterface(
                "Equinor.ProCoSys.Completion.MessageContracts",
                typeof(IIntegrationEvent));
        var testClassList
            = TestHelper.GetTestsWhichInheritsBaseClass(Assembly.GetExecutingAssembly(), typeof(ContractTestBase<>));

        AssertAllHasUnitTest(interfaces, testClassList);
    }

    private static void AssertAllHasUnitTest(List<Type> interfaces, List<Type> testClasses)
    {
        Assert.AreNotEqual(0, interfaces.Count);
        
        var missingTests = new List<string>();
        var genericArguments = TestHelper.GetGenericArgumentsOfBaseClasses(testClasses);
        foreach (var i in interfaces)
        {
            var classHasTest = genericArguments.Any(g => g.FullName == i.FullName);
            if (!classHasTest)
            {
                missingTests.Add(i.FullName!);
            }
        }

        Assert.AreEqual(0, missingTests.Count,
            $"{missingTests.Count} tests are missing: {string.Join(",", missingTests)}");
    }
}

using System;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

[TestClass]
public abstract class TestBase
{
    private readonly RowVersionInputValidator _rowVersionValidator = new();
        
    [AssemblyCleanup]
    public static void AssemblyCleanup() => TestFactory.Instance.Dispose();

    public void AssertRowVersionChange(string oldRowVersion, string newRowVersion)
    {
        AssertValidRowVersion(oldRowVersion);
        AssertValidRowVersion(newRowVersion);
        Assert.AreNotEqual(oldRowVersion, newRowVersion);
    }

    public void AssertValidGuidAndRowVersion(GuidAndRowVersion guidAndRowVersion)
    {
        Assert.IsNotNull(guidAndRowVersion);
        AssertValidRowVersion(guidAndRowVersion.RowVersion);
        Assert.AreNotEqual(Guid.Empty, guidAndRowVersion.Guid);
    }

    public void AssertValidRowVersion(string rowVersion)
        => Assert.IsTrue(_rowVersionValidator.IsValid(rowVersion));

    protected void AssertCreatedBy(UserType userType, PersonDto personDto)
    {
        var profile = TestFactory.Instance.GetTestProfile(userType);
        AssertUser(profile, personDto);
    }

    protected void AssertUser(TestProfile profile, PersonDto personDto)
    {
        Assert.IsNotNull(personDto);
        Assert.AreEqual(profile.FirstName, personDto.FirstName);
        Assert.AreEqual(profile.LastName, personDto.LastName);
    }
}

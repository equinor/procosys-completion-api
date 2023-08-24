using Equinor.ProCoSys.Completion.TieImport.Infrastructure;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Tests;

[TestClass]
public class TIObjectExtensionsTests
{
    [TestMethod]
    public void GetLogFriendlyStringForTieLog_ShouldHaveClassificationPrefix_WhenClassificationIsNotNull()
    {
        var tiObject = new TIObject {Classification = "A valid classification"};

        var result = tiObject.GetLogFriendlyStringForTieLog();

        Assert.IsTrue(result.Contains("A valid classification"));
    }

    [TestMethod]
    public void GetLogFriendlyStringForTieLog_ShouldHaveObjectNamePrefix_WhenObjectNameIsNotNull()
    {
        var tiObject = new TIObject { ObjectName = "A valid object name" };

        var result = tiObject.GetLogFriendlyStringForTieLog();

        Assert.IsTrue(result.Contains("A valid object name"));
    }

    [TestMethod]
    public void GetLogFriendlyStringForTieLog_ShouldHaveNamePrefix_WhenObjectNameIsNullAndNameAttributeExists()
    {
        var tiObject = new TIObject { Attributes = new List<TIAttribute>{new() {Name = "Name", Value = "Value of name attribute"}} };

        var result = tiObject.GetLogFriendlyStringForTieLog();

        Assert.IsTrue(result.Contains("Value of name attribute"));
    }
}

using System;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class PropertyHelperTests
{
    private readonly PropertyHelper _propertyHelper = new(Substitute.For<ILogger<PropertyHelper>>());

    [TestMethod]
    public void NonUserProperty_ShouldReturnNull_WhenValueDisplayType_NotUser()
    {
        // Act
        var result = _propertyHelper.TryGetPropertyValueAsUser("S", ValueDisplayType.StringAsText);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void UserPropertyAsJSon_ShouldReturnUserObject_WhenCorrectValueDisplayType()
    {
        var oid = Guid.NewGuid();
        var fn = "Peter Pan";
        var json = $"{{\"oid\": \"{oid}\",\"fullName\": \"{fn}\"}}";

        // Act
        var result = _propertyHelper.TryGetPropertyValueAsUser(json, ValueDisplayType.UserAsLinkToAddressBook);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(oid, result.Oid);
        Assert.AreEqual(fn, result.FullName);
    }

    [TestMethod]
    public void UserPropertyAsJSon_ShouldReturnNull_WhenValueDisplayType_NotUser()
    {
        var oid = Guid.NewGuid();
        var fn = "Peter Pan";
        var json = $"{{\"oid\": \"{oid}\",\"fullName\": \"{fn}\"}}";

        // Act
        var result = _propertyHelper.TryGetPropertyValueAsUser(json, ValueDisplayType.StringAsText);
        //Assert.ThrowsException<Exception>(() => _dut.Clear(_person));
        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void UserPropertyAsNonJSon_ShouldThrowException() =>
        // Act and Assert
        Assert.ThrowsException<Exception>(
            () => _propertyHelper.TryGetPropertyValueAsUser("", ValueDisplayType.UserAsContactCard));
}

﻿using System;
using System.Text.Json;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class UserPropertyHelperTests
{
    private readonly UserPropertyHelper _dut = new(Substitute.For<ILogger<UserPropertyHelper>>());

    [TestMethod]
    public void GetPropertyValueAsUser_ShouldReturnNull_WhenValueDisplayType_NotUser()
    {
        // Arrange
        var oid = Guid.NewGuid();
        var fn = "Peter Pan";
        var json = $"{{\"oid\": \"{oid}\",\"fullName\": \"{fn}\"}}";

        // Act
        var result = _dut.GetPropertyValueAsUser(json, ValueDisplayType.StringAsText);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetPropertyValueAsUser_ShouldReturnUserObject_WhenCorrectJsonAndValueDisplayType()
    {
        // Arrange
        var oid = Guid.NewGuid();
        var fn = "Peter Pan";
        var json = $"{{\"oid\": \"{oid}\",\"fullName\": \"{fn}\"}}";

        // Act
        var result = _dut.GetPropertyValueAsUser(json, ValueDisplayType.UserAsLinkToAddressBook);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(oid, result.Oid);
        Assert.AreEqual(fn, result.FullName);
    }

    [TestMethod]
    public void GetPropertyValueAsUser_ShouldThrowJsonException_WhenNotValidJson() =>
        // Act and Assert
        Assert.ThrowsException<JsonException>(
            () => _dut.GetPropertyValueAsUser("", ValueDisplayType.UserAsContactCard));
}

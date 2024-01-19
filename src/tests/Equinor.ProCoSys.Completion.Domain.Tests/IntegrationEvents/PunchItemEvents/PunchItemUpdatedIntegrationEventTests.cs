using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.IntegrationEvents.PunchItemEvents;

[TestClass]
public class PunchItemUpdatedIntegrationEventTests : PunchItemIntegrationEventTestsBase
{
    [TestInitialize]
    public void Setup() => _punchItem.SetModified(_person);

    [TestMethod]
    public void Constructor_ShouldSetCorrectProperties_WhenPunchItemHasRequiredPropertiesOnly()
    {
        // Act
        var integrationEvent = new PunchItemUpdatedIntegrationEvent(_punchItem);

        // Assert
        Assert.AreEqual(_punchItem.Plant, integrationEvent.Plant);
        Assert.AreEqual(_punchItem.Guid, integrationEvent.Guid);
        AssertModifiedBy(_punchItem, integrationEvent);
        AssertRequiredProperties(_punchItem, integrationEvent);
        AssertOptionalPropertiesIsNull(integrationEvent);
        AssertNotCleared(integrationEvent);
        AssertNotRejected(integrationEvent);
        AssertNotVerified(integrationEvent);
    }

    [TestMethod]
    public void Constructor_ShouldSetCorrectProperties_WhenPunchItemHasAllProperties()
    {
        // Arrange
        FillOptionalProperties(_punchItem);
        
        // Act
        var integrationEvent = new PunchItemUpdatedIntegrationEvent(_punchItem);

        // Assert
        Assert.AreEqual(_punchItem.Plant, integrationEvent.Plant);
        Assert.AreEqual(_punchItem.Guid, integrationEvent.Guid);
        AssertModifiedBy(_punchItem, integrationEvent);
        AssertRequiredProperties(_punchItem, integrationEvent);
        AssertOptionalProperties(_punchItem, integrationEvent);
        AssertNotCleared(integrationEvent);
        AssertNotRejected(integrationEvent);
        AssertNotVerified(integrationEvent);
    }

    [TestMethod]
    public void Constructor_ShouldSetCorrectProperties_WhenPunchItemIsCleared()
    {
        // Arrange
        _punchItem.Clear(_person);

        // Act
        var integrationEvent = new PunchItemUpdatedIntegrationEvent(_punchItem);

        // Assert
        Assert.AreEqual(_punchItem.Plant, integrationEvent.Plant);
        Assert.AreEqual(_punchItem.Guid, integrationEvent.Guid);
        AssertModifiedBy(_punchItem, integrationEvent);
        AssertRequiredProperties(_punchItem, integrationEvent);
        AssertOptionalPropertiesIsNull(integrationEvent);
        AssertIsCleared(_punchItem, _person, integrationEvent);
        AssertNotRejected(integrationEvent);
        AssertNotVerified(integrationEvent);
    }

    [TestMethod]
    public void Constructor_ShouldSetCorrectProperties_WhenPunchItemIsRejected()
    {
        // Arrange
        _punchItem.Clear(_person);
        _punchItem.Reject(_person);

        // Act
        var integrationEvent = new PunchItemUpdatedIntegrationEvent(_punchItem);

        // Assert
        Assert.AreEqual(_punchItem.Plant, integrationEvent.Plant);
        Assert.AreEqual(_punchItem.Guid, integrationEvent.Guid);
        AssertModifiedBy(_punchItem, integrationEvent);
        AssertRequiredProperties(_punchItem, integrationEvent);
        AssertOptionalPropertiesIsNull(integrationEvent);
        AssertNotCleared(integrationEvent);
        AssertIsRejected(_punchItem, _person, integrationEvent);
        AssertNotVerified(integrationEvent);
    }

    [TestMethod]
    public void Constructor_ShouldSetCorrectProperties_WhenPunchItemIsVerified()
    {
        // Arrange
        _punchItem.Clear(_person);
        _punchItem.Verify(_person);

        // Act
        var integrationEvent = new PunchItemUpdatedIntegrationEvent(_punchItem);

        // Assert
        Assert.AreEqual(_punchItem.Plant, integrationEvent.Plant);
        Assert.AreEqual(_punchItem.Guid, integrationEvent.Guid);
        AssertModifiedBy(_punchItem, integrationEvent);
        AssertRequiredProperties(_punchItem, integrationEvent);
        AssertOptionalPropertiesIsNull(integrationEvent);
        AssertIsCleared(_punchItem, _person, integrationEvent);
        AssertNotRejected(integrationEvent);
        AssertIsVerified(_punchItem, _person, integrationEvent);
    }
}

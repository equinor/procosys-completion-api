using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.IntegrationEvents.PunchItemEvents;

[TestClass]
public class PunchItemCreatedIntegrationEventTests : PunchItemIntegrationEventTestsBase
{
    [TestMethod]
    public void Constructor_ShouldSetCorrectProperties_WhenPunchItemHasRequiredPropertiesOnly()
    {
        // Act
        var integrationEvent = new PunchItemCreatedIntegrationEvent(_punchItem);

        // Assert
        Assert.AreEqual(_punchItem.Plant, integrationEvent.Plant);
        Assert.AreEqual(_punchItem.Guid, integrationEvent.Guid);
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
        var integrationEvent = new PunchItemCreatedIntegrationEvent(_punchItem);

        // Assert
        Assert.AreEqual(_punchItem.Plant, integrationEvent.Plant);
        Assert.AreEqual(_punchItem.Guid, integrationEvent.Guid);
        AssertRequiredProperties(_punchItem, integrationEvent);
        AssertOptionalProperties(_punchItem, integrationEvent);
        AssertNotCleared(integrationEvent);
        AssertNotRejected(integrationEvent);
        AssertNotVerified(integrationEvent);
    }
}

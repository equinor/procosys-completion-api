using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.IntegrationEvents.PunchItemEvents;

[TestClass]
public class PunchItemDeletedIntegrationEventTests : PunchItemIntegrationEventTestsBase
{
    [TestInitialize]
    public void Setup() => _punchItem.SetModified(_person);

    [TestMethod]
    public void Constructor_ShouldSetCorrectProperties_WhenPunchItemHasRequiredPropertiesOnly()
    {
        // Act
        var integrationEvent = new PunchItemDeletedIntegrationEvent(_punchItem);

        // Assert
        Assert.AreEqual(_punchItem.Plant, integrationEvent.Plant);
        Assert.AreEqual(_punchItem.Guid, integrationEvent.Guid);
        Assert.AreEqual(_punchItem.CheckListGuid, integrationEvent.ParentGuid);
        AssertDeletedBy(_punchItem, integrationEvent);
    }
}

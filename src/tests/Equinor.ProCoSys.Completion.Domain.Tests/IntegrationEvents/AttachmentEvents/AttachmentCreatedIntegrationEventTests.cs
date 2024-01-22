using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.IntegrationEvents.AttachmentEvents;

[TestClass]
public class AttachmentCreatedIntegrationEventTests : AttachmentIntegrationEventTestsBase
{
    [TestMethod]
    public void Constructor_ShouldSetCorrectProperties()
    {
        // Act
        var integrationEvent = new AttachmentCreatedIntegrationEvent(_attachment, "PlantX");

        // Assert
        Assert.AreEqual("PlantX", integrationEvent.Plant);
        Assert.AreEqual(_attachment.Guid, integrationEvent.Guid);
        Assert.AreEqual(_attachment.ParentGuid, integrationEvent.ParentGuid);
        Assert.AreEqual(_attachment.ParentType, integrationEvent.ParentType);
        Assert.AreEqual(_attachment.FileName, integrationEvent.FileName);
        Assert.AreEqual(_attachment.CreatedAtUtc, integrationEvent.CreatedAtUtc);
        Assert.AreEqual(_attachment.CreatedBy.Guid, integrationEvent.CreatedBy.Oid);
        Assert.AreEqual(_attachment.CreatedBy.GetFullName(), integrationEvent.CreatedBy.FullName);
    }
}

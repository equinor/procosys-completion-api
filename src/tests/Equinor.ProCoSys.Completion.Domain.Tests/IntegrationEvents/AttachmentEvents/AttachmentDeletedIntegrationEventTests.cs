using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.IntegrationEvents.AttachmentEvents;

[TestClass]
public class AttachmentDeletedIntegrationEventTests : AttachmentIntegrationEventTestsBase
{
    [TestMethod]
    public void Constructor_ShouldSetCorrectProperties()
    {
        // Act
        var integrationEvent = new AttachmentDeletedIntegrationEvent(_attachment, "PlantX");

        // Assert
        Assert.AreEqual("PlantX", integrationEvent.Plant);
        Assert.AreEqual(_attachment.Guid, integrationEvent.Guid);
        Assert.AreEqual(_attachment.ParentGuid, integrationEvent.ParentGuid);

        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... use ModifiedBy/ModifiedAtUtc which is set when saving a deletion
        Assert.AreEqual(_attachment.ModifiedAtUtc, integrationEvent.DeletedAtUtc);
        Assert.AreEqual(_attachment.ModifiedBy!.Guid, integrationEvent.DeletedBy.Oid);
        Assert.AreEqual(_attachment.ModifiedBy!.GetFullName(), integrationEvent.DeletedBy.FullName);
    }
}

using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.IntegrationEvents.AttachmentEvents;

[TestClass]
public class AttachmentUpdatedIntegrationEventTests : AttachmentIntegrationEventTestsBase
{
    [TestMethod]
    public void Constructor_ShouldSetCorrectProperties()
    {
        // Act
        var integrationEvent = new AttachmentUpdatedIntegrationEvent(_attachment, "PlantX");

        // Assert
        Assert.AreEqual("PlantX", integrationEvent.Plant);
        Assert.AreEqual(_attachment.Guid, integrationEvent.Guid);
        Assert.AreEqual(_attachment.ParentGuid, integrationEvent.ParentGuid);
        Assert.AreEqual(_attachment.ParentType, integrationEvent.ParentType);
        Assert.AreEqual(_attachment.FileName, integrationEvent.FileName);
        Assert.AreEqual(_attachment.RevisionNumber, integrationEvent.RevisionNumber);
        Assert.AreEqual(_attachment.Description, integrationEvent.Description);
        Assert.AreEqual(_attachment.ModifiedAtUtc, integrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_attachment.ModifiedBy!.Guid, integrationEvent.ModifiedBy.Oid);
        Assert.AreEqual(_attachment.ModifiedBy!.GetFullName(), integrationEvent.ModifiedBy.FullName);
        AssertSameLabels(_attachment.GetOrderedNonVoidedLabels().ToList(), integrationEvent.Labels);
    }

    private void AssertSameLabels(List<Label> labelList1, List<string> labelList2)
    {
        Assert.AreEqual(labelList1.Count, labelList2.Count);
        for (var i = 0; i < labelList1.Count; i++)
        {
            Assert.AreEqual(labelList1.ElementAt(i).Text, labelList2.ElementAt(i));
        }
    }
}

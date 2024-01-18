using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.IntegrationEvents.AttachmentEvents;

[TestClass]
public class AttachmentIntegrationEventTestsBase
{
    protected Attachment _attachment;
 
    [TestInitialize]
    public void SetupBase()
    {
        _attachment = new Attachment(nameof(PunchItem), Guid.NewGuid(), "PCS$PLANT", "file.txt");
        _attachment.UpdateLabels(new List<Label> { new("A"), new("B") });
    }
}

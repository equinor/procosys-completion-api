using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.IntegrationEvents.AttachmentEvents;

[TestClass]
public class AttachmentIntegrationEventTestsBase : TestsBase
{
    protected Attachment _attachment;
 
    [TestInitialize]
    public void SetupBase()
    {
        _attachment = new Attachment("Proj", nameof(PunchItem), Guid.NewGuid(), "file.txt");
        _attachment.UpdateLabels(new List<Label> { new("A"), new("B") });
        _attachment.SetCreated(_person);
        _attachment.SetModified(_person);
    }
}

using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.AttachmentAggregate;

[TestClass]
public class AttachmentLabelTests : IHaveLabelsTests
{
    private Attachment _dut;

    protected override IHaveLabels GetEntityWithLabels() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new Attachment("Proj", "X", Guid.NewGuid(), "a.txt");
}

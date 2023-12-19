using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.CommentAggregate;

[TestClass]
public class CommentLabelTests : IHaveLabelsTests
{
    private Comment _dut;

    protected override IHaveLabels GetEntityWithLabels() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new Comment("X", Guid.NewGuid(), "a.txt");
}

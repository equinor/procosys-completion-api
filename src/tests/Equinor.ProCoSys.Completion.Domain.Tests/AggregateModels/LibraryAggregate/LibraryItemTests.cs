using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.LibraryAggregate;

[TestClass]
public class LibraryItemTests : IModificationAuditableTests
{
    private LibraryItem _dut;
    private readonly string _testPlant = "PlantA";
    private readonly string _code = "X";
    private readonly Guid _guid = Guid.NewGuid();
    private readonly string _description = "X Desc";
    private readonly LibraryType _type = LibraryType.COMPLETION_ORGANIZATION;

    protected override ICreationAuditable GetCreationAuditable() => _dut;
    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new LibraryItem(_testPlant, _guid, _code, _description, _type);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_code, _dut.Code);
        Assert.AreEqual(_description, _dut.Description);
        Assert.AreEqual(_type, _dut.Type);
        Assert.AreEqual(_guid, _dut.Guid);
    }
}

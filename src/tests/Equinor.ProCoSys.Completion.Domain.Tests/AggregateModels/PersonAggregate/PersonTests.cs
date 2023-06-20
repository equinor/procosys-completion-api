using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.PersonAggregate;

[TestClass]
public class PersonTests : IModificationAuditableTests
{
    private Person _dut;
    private readonly Guid _oid = Guid.NewGuid();

    protected override ICreationAuditable GetCreationAuditable() => _dut;
    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new Person(_oid, "FirstName", "LastName", "UserName", "EmailAddress");

    [TestMethod]
    public void Constructor_SetsProperties()
    {
        Assert.AreEqual(_oid, _dut.Guid);
        Assert.AreEqual("FirstName", _dut.FirstName);
        Assert.AreEqual("LastName", _dut.LastName);
        Assert.AreEqual("UserName", _dut.UserName);
        Assert.AreEqual("EmailAddress", _dut.Email);
    }
}

using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.PersonAggregate;

[TestClass]
public class PersonTests
{
    private Person _dut;
    private readonly Guid _oid = Guid.NewGuid();

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
        Assert.AreEqual("FirstName LastName", _dut.GetFullName());
    }
}

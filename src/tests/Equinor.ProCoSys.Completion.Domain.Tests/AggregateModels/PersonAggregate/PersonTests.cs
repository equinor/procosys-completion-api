using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.PersonAggregate;

[TestClass]
public class PersonTests
{
    private readonly Guid _oid = Guid.NewGuid();

    [TestMethod]
    public void Constructor_SetsProperties()
    {
        var dut = new Person(_oid, "FirstName", "LastName", "UserName", "EmailAddress");
        Assert.AreEqual(_oid, dut.Guid);
        Assert.AreEqual("FirstName", dut.FirstName);
        Assert.AreEqual("LastName", dut.LastName);
        Assert.AreEqual("UserName", dut.UserName);
        Assert.AreEqual("EmailAddress", dut.Email);
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenFirstNameNotGiven() =>
        Assert.ThrowsException<ArgumentNullException>(() =>
            new Person(_oid, null!, "LastName", "UserName", "EmailAddress"));

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenLastNameNotGiven() =>
        Assert.ThrowsException<ArgumentNullException>(() =>
            new Person(_oid, "FirstName", null!, "UserName", "EmailAddress"));

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenUserNameNotGiven() =>
        Assert.ThrowsException<ArgumentNullException>(() =>
            new Person(_oid, "FirstName", "LastName", null!, "EmailAddress"));

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenMailNotGiven() =>
        Assert.ThrowsException<ArgumentNullException>(() =>
            new Person(_oid, "FirstName", "LastName", "UserName", null!));
}

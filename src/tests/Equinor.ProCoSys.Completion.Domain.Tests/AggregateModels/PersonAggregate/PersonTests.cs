﻿using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.PersonAggregate;

[TestClass]
public class PersonTests
{
    private Person _dut;
    private readonly Guid _oid = Guid.NewGuid();

    [TestInitialize]
    public void Setup() => _dut = new Person(_oid, "FirstName", "LastName", "UserName", "EmailAddress", true);

    [TestMethod]
    public void Constructor_SetsProperties()
    {
        Assert.AreEqual(_oid, _dut.Guid);
        Assert.AreEqual("FirstName", _dut.FirstName);
        Assert.AreEqual("LastName", _dut.LastName);
        Assert.AreEqual("UserName", _dut.UserName);
        Assert.AreEqual("EmailAddress", _dut.Email);
        Assert.AreEqual("FirstName LastName", _dut.GetFullName());
        Assert.IsTrue(_dut.Superuser);
    }

    [TestMethod]
    public void ProCoSys4LastUpdated_ShouldSetProCoSys4LastUpdated_WhenSetProCoSys4LastUpdated()
    {
        var lastUpdated = DateTime.Now;
        _dut.SetProCoSys4LastUpdated(lastUpdated);
        Assert.AreEqual(_dut.ProCoSys4LastUpdated, lastUpdated);
    }

}

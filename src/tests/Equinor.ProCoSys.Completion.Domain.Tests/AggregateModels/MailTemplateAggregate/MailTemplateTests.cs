﻿using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.MailTemplateAggregate;

[TestClass]
public class MailTemplateTests : IModificationAuditableTests
{
    private MailTemplate _dut;
    private readonly string _code = "X";
    private readonly string _subject = "X Subject";
    private readonly string _body = "X Body";

    protected override ICreationAuditable GetCreationAuditable() => _dut;
    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new MailTemplate(_code, _subject, _body);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_code, _dut.Code);
        Assert.AreEqual(_subject, _dut.Subject);
        Assert.AreEqual(_body, _dut.Body);
    }
}
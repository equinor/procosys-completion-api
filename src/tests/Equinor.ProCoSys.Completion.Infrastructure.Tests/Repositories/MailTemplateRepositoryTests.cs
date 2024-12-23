﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class MailTemplateRepositoryTests : RepositoryTestBase<MailTemplate>
{
    private readonly string _codeA = "A";
    private readonly string _codeB = "B";
    private readonly string _codeC = "C";
    private readonly string _codeVoided = "V";
    private readonly string _plantP = "P";
    private MailTemplate _globalMailTemplateA;
    private MailTemplate _mailTemplateBInPlantP;
    private MailTemplate _globalMailTemplateC;
    private MailTemplate _mailTemplateCInPlantP;

    protected override EntityRepository<MailTemplate> GetDut()
        => new MailTemplateRepository(_contextHelper.ContextMock);

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var mailTemplate = new MailTemplate("c", "s", "b");
        mailTemplate.SetProtectedIdForTesting(_knownId);

        var mailTemplates = new List<MailTemplate> { mailTemplate };

        _dbSetMock = mailTemplates.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .MailTemplates
            .Returns(_dbSetMock);
    }

    protected override MailTemplate GetNewEntity() => new("c|", "s", "b");

    [TestMethod]
    public async Task GetNonVoidedByCode_ShouldGetGlobalMailTemplate_WhenPlantSpecificDoNotExists()
    {
        // Arrange
        var dut = ArrangeRepository();

        // Act
        var result = await dut.GetNonVoidedByCodeAsync(_plantP, _codeA, default);

        // Assert
        Assert.AreEqual(_globalMailTemplateA, result);
        Assert.IsTrue(result.IsGlobal());
    }

    [TestMethod]
    public async Task GetNonVoidedByCode_ShouldGetPlantSpecificMailTemplate_WhenGlobalDoNotExists()
    {
        // Arrange
        var dut = ArrangeRepository();

        // Act
        var result = await dut.GetNonVoidedByCodeAsync(_plantP, _codeB, default);

        // Assert
        Assert.AreEqual(_mailTemplateBInPlantP, result);
        Assert.IsFalse(result.IsGlobal());
    }

    [TestMethod]
    public async Task GetNonVoidedByCode_ShouldGetPlantSpecificMailTemplate_WhenBothGlobalAndPlantSpecificExists()
    {
        // Arrange
        var dut = ArrangeRepository();

        // Act
        var result = await dut.GetNonVoidedByCodeAsync(_plantP, _codeC, default);

        // Assert
        Assert.AreEqual(_mailTemplateCInPlantP, result);
        Assert.IsFalse(result.IsGlobal());
    }

    [TestMethod]
    public async Task GetNonVoidedByCode_ShouldThrowEntityNotFoundException_WhenGettingVoidedMailTemplate()
    {
        // Arrange
        var dut = ArrangeRepository();

        // Act and Assert
        await Assert.ThrowsExceptionAsync<EntityNotFoundException>(()
            => dut.GetNonVoidedByCodeAsync(_plantP, _codeVoided, default));
    }

    [TestMethod]
    public async Task GetNonVoidedByCode_ShouldThrowEntityNotFoundException_WhenGettingUnknownMailTemplate()
    {
        // Arrange
        var dut = ArrangeRepository();

        // Act and Assert
        await Assert.ThrowsExceptionAsync<EntityNotFoundException>(()
            => dut.GetNonVoidedByCodeAsync(_plantP, Guid.NewGuid().ToString(), default));
    }

    private MailTemplateRepository ArrangeRepository()
    {
        _globalMailTemplateA = new MailTemplate(_codeA, "s", "b");
        _globalMailTemplateA.SetProtectedIdForTesting(_knownId);

        _mailTemplateBInPlantP = new MailTemplate(_codeB, "s", "b") { Plant = _plantP };
        _globalMailTemplateC = new MailTemplate(_codeC, "s", "b");
        _mailTemplateCInPlantP = new MailTemplate(_codeC, "s", "b") { Plant = _plantP };

        var voidedMailTemplate = new MailTemplate(_codeVoided, "s", "b") { IsVoided = true };

        var mailTemplates = new List<MailTemplate>
        {
            _globalMailTemplateA,
            _mailTemplateBInPlantP,
            _globalMailTemplateC,
            _mailTemplateCInPlantP,
            voidedMailTemplate
        };

        _dbSetMock = mailTemplates.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .MailTemplates
            .Returns(_dbSetMock);

        return new MailTemplateRepository(_contextHelper.ContextMock);
    }
}

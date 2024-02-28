using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Query.MailTemplateQueries.GetAllMailTemplates;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.MailTemplateQueries.GetAllMailTemplates;

[TestClass]
public class GetAllMailTemplatesQueryHandlerTests : ReadOnlyTestsBase
{
    private readonly GetAllMailTemplatesQuery _query = new ();

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_WhenNoMailTemplatesExists()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var dut = new GetAllMailTemplatesQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(0, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectNumberOfMailTemplates()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        Add4UnorderedGlobalMailTemplatesInclusiveAVoidedMailTemplate(context);

        var dut = new GetAllMailTemplatesQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        Assert.AreEqual(4, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectOrderedMailTemplates()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        Add4UnorderedGlobalMailTemplatesInclusiveAVoidedMailTemplate(context);

        var dut = new GetAllMailTemplatesQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        Assert.AreEqual(MailTemplateCodeA, result.Data.ElementAt(0).Code);
        Assert.AreEqual(MailTemplateCodeB, result.Data.ElementAt(1).Code);
        Assert.AreEqual(MailTemplateCodeC, result.Data.ElementAt(2).Code);
        Assert.AreEqual(MailTemplateCodeVoided, result.Data.ElementAt(3).Code);
    }

    [TestMethod]
    public async Task Handler_ShouldReturnBothVoidedAndNonVoidedMailTemplates()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        Add4UnorderedGlobalMailTemplatesInclusiveAVoidedMailTemplate(context);

        var dut = new GetAllMailTemplatesQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        var voidedMailTemplate = result.Data.SingleOrDefault(dto => dto.IsVoided);
        Assert.IsNotNull(voidedMailTemplate);
        Assert.AreEqual(MailTemplateCodeVoided, voidedMailTemplate.Code);

        var nonVoidedMailTemplates = result.Data.Where(dto => !dto.IsVoided).ToList();
        Assert.AreEqual(3, nonVoidedMailTemplates.Count);
    }

    [TestMethod]
    public async Task Handler_ShouldReturnBothGlobalAndPlantSpecificMailTemplates()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        Add4UnorderedGlobalMailTemplatesInclusiveAVoidedMailTemplate(context);
        Add4UnorderedMailTemplatesForPlantInclusiveAVoidedMailTemplate(context, TestPlantA);

        var dut = new GetAllMailTemplatesQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        var voidedMailTemplates = result.Data.Where(dto => dto.IsVoided).ToList();
        Assert.AreEqual(2, voidedMailTemplates.Count);

        var nonVoidedMailTemplates = result.Data.Where(dto => !dto.IsVoided).ToList();
        Assert.AreEqual(6, nonVoidedMailTemplates.Count);

        var globalMailTemplates = result.Data.Where(dto => dto.IsGlobal).ToList();
        Assert.AreEqual(4, globalMailTemplates.Count);

        var plantSpecificMailTemplates = result.Data.Where(dto => !dto.IsGlobal).ToList();
        Assert.AreEqual(4, plantSpecificMailTemplates.Count);

        Assert2MailTemplatesWithSameCode(result.Data.ToList(), MailTemplateCodeA);
        Assert2MailTemplatesWithSameCode(result.Data.ToList(), MailTemplateCodeB);
        Assert2MailTemplatesWithSameCode(result.Data.ToList(), MailTemplateCodeC);
        Assert2MailTemplatesWithSameCode(result.Data.ToList(), MailTemplateCodeVoided);
    }

    [TestMethod]
    public async Task Handler_ShouldOrderGlobalMailTemplatesBeforePlantSpecificMailTemplates()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        Add4UnorderedGlobalMailTemplatesInclusiveAVoidedMailTemplate(context);
        Add4UnorderedMailTemplatesForPlantInclusiveAVoidedMailTemplate(context, TestPlantA);

        var dut = new GetAllMailTemplatesQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        AssertCodeAndIsGlobal(MailTemplateCodeA, true, result.Data.ElementAt(0));
        AssertCodeAndIsGlobal(MailTemplateCodeB, true, result.Data.ElementAt(1));
        AssertCodeAndIsGlobal(MailTemplateCodeC, true, result.Data.ElementAt(2));
        AssertCodeAndIsGlobal(MailTemplateCodeVoided, true, result.Data.ElementAt(3));

        AssertCodeAndIsGlobal(MailTemplateCodeA, false, result.Data.ElementAt(0+4));
        AssertCodeAndIsGlobal(MailTemplateCodeB, false, result.Data.ElementAt(1+4));
        AssertCodeAndIsGlobal(MailTemplateCodeC, false, result.Data.ElementAt(2+4));
        AssertCodeAndIsGlobal(MailTemplateCodeVoided, false, result.Data.ElementAt(3+4));
    }

    private void AssertCodeAndIsGlobal(string code, bool isGlobal, MailTemplateDto dto)
    {
        Assert.AreEqual(code, dto.Code);
        Assert.AreEqual(isGlobal, dto.IsGlobal);
    }

    private void Assert2MailTemplatesWithSameCode(IList<MailTemplateDto> dtos, string code)
    {
        var mailTemplates = dtos.Where(dto => dto.Code == code).ToList();
        Assert.AreEqual(2, mailTemplates.Count);
        Assert.IsTrue(mailTemplates.Any(mt => mt.IsGlobal));
        Assert.IsTrue(mailTemplates.Any(mt => !mt.IsGlobal));
    }
}

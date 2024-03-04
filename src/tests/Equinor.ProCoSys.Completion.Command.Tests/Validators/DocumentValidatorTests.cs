using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Command.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.Validators;

[TestClass]
public class DocumentValidatorTests : ReadOnlyTestsBase
{
    private readonly string _testPlant = TestPlantA;
    private Document _nonVoidedDocument = null!;
    private Document _voidedDocument = null!;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        _nonVoidedDocument = new Document(_testPlant, Guid.NewGuid(), "D1");
        _voidedDocument = new Document(_testPlant, Guid.NewGuid(), "D2") { IsVoided = true };
        context.Documents.Add(_nonVoidedDocument);
        context.Documents.Add(_voidedDocument);

        context.SaveChangesAsync().Wait();
    }

    #region ExistsAsync
    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenDocumentExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new DocumentValidator(context);

        // Act
        var result = await dut.ExistsAsync(_nonVoidedDocument.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnFalse_WhenDocumentNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new DocumentValidator(context);

        // Act
        var result = await dut.ExistsAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region IsVoided
    [TestMethod]
    public async Task IsVoided_ShouldReturnTrue_WhenDocumentIsVoided()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new DocumentValidator(context);

        // Act
        var result = await dut.IsVoidedAsync(_voidedDocument.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsVoided_ShouldReturnFalse_WhenDocumentIsNotVoided()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new DocumentValidator(context);

        // Act
        var result = await dut.IsVoidedAsync(_nonVoidedDocument.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsVoided_ShouldReturnFalse_WhenDocumentNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new DocumentValidator(context);

        // Act
        var result = await dut.IsVoidedAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}

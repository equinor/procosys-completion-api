using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Command.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Equinor.ProCoSys.Completion.Command.Tests.Validators;

[TestClass]
public class LibraryItemValidatorTests : ReadOnlyTestsBase
{

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    { }

    #region ExistsAsync
    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenLibraryItemExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new LibraryItemValidator(context);
        var clearingByOrg = context.Library.Single(l => l.Id == _clearingByOrgId[TestPlantA]);

        // Act
        var result = await dut.ExistsAsync(clearingByOrg.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnFalse_WhenLibraryItemNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new LibraryItemValidator(context);

        // Act
        var result = await dut.ExistsAsync(Guid.NewGuid(), default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region IsVoided
    [TestMethod]
    public async Task IsVoided_ShouldReturnTrue_WhenLibraryItemIsVoided()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new LibraryItemValidator(context);
        var voidedOrg = context.Library.Single(l => l.Id == _voidedOrgId[TestPlantA]);

        // Act
        var result = await dut.IsVoidedAsync(voidedOrg.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsVoided_ShouldReturnFalse_WhenLibraryItemIsNotVoided()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new LibraryItemValidator(context);
        var clearingByOrg = context.Library.Single(l => l.Id == _clearingByOrgId[TestPlantA]);

        // Act
        var result = await dut.IsVoidedAsync(clearingByOrg.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsVoided_ShouldReturnFalse_WhenLibraryItemNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new LibraryItemValidator(context);

        // Act
        var result = await dut.IsVoidedAsync(Guid.NewGuid(), default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}

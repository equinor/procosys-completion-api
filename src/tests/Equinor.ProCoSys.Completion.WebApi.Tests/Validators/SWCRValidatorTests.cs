using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.WebApi.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Validators;

[TestClass]
public class SWCRValidatorTests : ReadOnlyTestsBase
{
    private SWCR _nonVoidedSWCR = null!;
    private SWCR _voidedSWCR = null!;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        _nonVoidedSWCR = new SWCR(TestPlantA, Guid.NewGuid(), 1);
        _voidedSWCR = new SWCR(TestPlantA, Guid.NewGuid(), 2) { IsVoided = true };
        context.SWCRs.Add(_nonVoidedSWCR);
        context.SWCRs.Add(_voidedSWCR);

        context.SaveChangesAsync().Wait();
    }

    #region ExistsAsync
    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenSWCRExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new SWCRValidator(context);

        // Act
        var result = await dut.ExistsAsync(_nonVoidedSWCR.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnFalse_WhenSWCRNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new SWCRValidator(context);

        // Act
        var result = await dut.ExistsAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region IsVoided
    [TestMethod]
    public async Task IsVoided_ShouldReturnTrue_WhenSWCRIsVoided()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new SWCRValidator(context);

        // Act
        var result = await dut.IsVoidedAsync(_voidedSWCR.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsVoided_ShouldReturnFalse_WhenSWCRIsNotVoided()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new SWCRValidator(context);

        // Act
        var result = await dut.IsVoidedAsync(_nonVoidedSWCR.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsVoided_ShouldReturnFalse_WhenSWCRNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new SWCRValidator(context);

        // Act
        var result = await dut.IsVoidedAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}

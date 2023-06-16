using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

namespace Equinor.ProCoSys.Completion.Command.Tests.Validators;

[TestClass]
public class PunchValidatorTests : ReadOnlyTestsBase
{
    private Guid _nonVoidedPunchGuid;
    private Guid _voidedPunchGuid;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            
        var punch1 = new Punch(TestPlantA, _projectA, "Punch 1");
        var punch2 = new Punch(TestPlantA, _projectA, "Punch 2") { IsVoided = true };
        context.Punches.Add(punch1);
        context.Punches.Add(punch2);
        context.SaveChangesAsync().Wait();
        _nonVoidedPunchGuid = punch1.Guid;
        _voidedPunchGuid = punch2.Guid;
    }

    #region PunchExists
    [TestMethod]
    public async Task PunchExists_ShouldReturnTrue_WhenPunchExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);            
        var dut = new PunchValidator(context);

        // Act
        var result = await dut.PunchExistsAsync(_nonVoidedPunchGuid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task PunchExists_ShouldReturnFalse_WhenPunchNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);    
        var dut = new PunchValidator(context);

        // Act
        var result = await dut.PunchExistsAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region PunchIsVoided
    [TestMethod]
    public async Task PunchIsVoided_ShouldReturnTrue_WhenPunchIsVoided()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);    
        var dut = new PunchValidator(context);

        // Act
        var result = await dut.PunchIsVoidedAsync(_voidedPunchGuid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task PunchIsVoided_ShouldReturnFalse_WhenPunchIsNotVoided()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchValidator(context);

        // Act
        var result = await dut.PunchIsVoidedAsync(_nonVoidedPunchGuid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task PunchIsVoided_ShouldReturnFalse_WhenPunchNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);  
        var dut = new PunchValidator(context);

        // Act
        var result = await dut.PunchIsVoidedAsync(Guid.NewGuid(), default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}

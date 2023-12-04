using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Command.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.Validators;

[TestClass]
public class LabelValidatorTests : ReadOnlyTestsBase
{
    private Label _nonVoidedLabel = null!;
    private Label _voidedLabel = null!;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        _nonVoidedLabel = new Label("D1");
        _voidedLabel = new Label("D2") { IsVoided = true };
        context.Labels.Add(_nonVoidedLabel);
        context.Labels.Add(_voidedLabel);

        context.SaveChangesAsync().Wait();
    }

    #region ExistsAsync
    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenLabelExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new LabelValidator(context);

        // Act
        var result = await dut.ExistsAsync(_nonVoidedLabel.Text, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnFalse_WhenLabelNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new LabelValidator(context);

        // Act
        var result = await dut.ExistsAsync(Guid.NewGuid().ToString(), default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region IsVoided
    [TestMethod]
    public async Task IsVoided_ShouldReturnTrue_WhenLabelIsVoided()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new LabelValidator(context);

        // Act
        var result = await dut.IsVoidedAsync(_voidedLabel.Text, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsVoided_ShouldReturnFalse_WhenLabelIsNotVoided()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new LabelValidator(context);

        // Act
        var result = await dut.IsVoidedAsync(_nonVoidedLabel.Text, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsVoided_ShouldReturnFalse_WhenLabelNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new LabelValidator(context);

        // Act
        var result = await dut.IsVoidedAsync(Guid.NewGuid().ToString(), default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}

using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Command.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.Validators;

[TestClass]
public class LabelEntityValidatorTests : ReadOnlyTestsBase
{
    private LabelEntity _labelEntity = null!;
    private readonly EntityTypeWithLabel _existingEntityTypeWithLabels = EntityTypeWithLabel.PunchComment;
    private readonly EntityTypeWithLabel _nonExistingEntityTypeWithLabels = EntityTypeWithLabel.PunchPicture;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        _labelEntity = new LabelEntity(_existingEntityTypeWithLabels);
        context.LabelEntities.Add(_labelEntity);

        context.SaveChangesAsync().Wait();
    }

    #region ExistsAsync
    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenLabelEntityExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new LabelEntityValidator(context);

        // Act
        var result = await dut.ExistsAsync(_existingEntityTypeWithLabels, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnFalse_WhenLabelEntityNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new LabelEntityValidator(context);

        // Act
        var result = await dut.ExistsAsync(_nonExistingEntityTypeWithLabels, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}

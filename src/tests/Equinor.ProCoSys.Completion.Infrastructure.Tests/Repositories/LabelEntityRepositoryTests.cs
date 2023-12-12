using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class LabelEntityRepositoryTests : RepositoryTestBase<LabelEntity>
{
    private readonly EntityTypeWithLabel _knownEntityType = EntityTypeWithLabel.PunchComment;
    private readonly EntityTypeWithLabel _unknownEntityType = EntityTypeWithLabel.PunchPicture;

    protected override EntityRepository<LabelEntity> GetDut()
        => new LabelEntityRepository(_contextHelper.ContextMock);

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var labelEntity = new LabelEntity(_knownEntityType);
        labelEntity.SetProtectedIdForTesting(_knownId);

        var labelEntities = new List<LabelEntity> { labelEntity };

        _dbSetMock = labelEntities.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .LabelEntities
            .Returns(_dbSetMock);
    }

    protected override LabelEntity GetNewEntity() => new(EntityTypeWithLabel.PunchComment);

    [TestMethod]
    public async Task ExistsAsync_KnownEntityType_ShouldReturnTrue()
    {
        // Arrange
        var dut = new LabelEntityRepository(_contextHelper.ContextMock);

        // Act
        var result = await dut.ExistsAsync(_knownEntityType, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_UnknownEntityType_ShouldReturnFalse()
    {
        // Arrange
        var dut = new LabelEntityRepository(_contextHelper.ContextMock);

        // Act
        var result = await dut.ExistsAsync(_unknownEntityType, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task GetByTypeAsync_KnownEntityType_ShouldReturnEntityType()
    {
        // Arrange
        var dut = new LabelEntityRepository(_contextHelper.ContextMock);

        // Act
        var result = await dut.GetByTypeAsync(_knownEntityType, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_knownEntityType, result.EntityType);
    }

    [TestMethod]
    public async Task GetByTypeAsync_UnknownLabel_ShouldThrowException()
    {
        // Arrange
        var dut = new LabelEntityRepository(_contextHelper.ContextMock);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => dut.GetByTypeAsync(_unknownEntityType, default));
    }
}

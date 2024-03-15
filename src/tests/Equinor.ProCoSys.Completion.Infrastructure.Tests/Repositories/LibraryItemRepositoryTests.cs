using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class LibraryItemRepositoryTests : EntityWithGuidRepositoryTestBase<LibraryItem>
{
    private LibraryType _knownLibraryType;

    protected override EntityWithGuidRepository<LibraryItem> GetDut()
        => new LibraryItemRepository(_contextHelper.ContextMock);

    protected override void SetupRepositoryWithOneKnownItem()
    {
        _knownGuid = Guid.NewGuid();
        _knownLibraryType = LibraryType.COMPLETION_ORGANIZATION;
        var libraryItem = new LibraryItem(TestPlant, _knownGuid, "A", "A Desc", _knownLibraryType);
        libraryItem.SetProtectedIdForTesting(_knownId);
        var library = new List<LibraryItem> { libraryItem };

        _dbSetMock = library.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Library
            .Returns(_dbSetMock);
    }

    protected override LibraryItem GetNewEntity() => new(TestPlant, Guid.NewGuid(), "B", "B Desc", LibraryType.COMPLETION_ORGANIZATION);

    [TestMethod]
    public async Task GetByGuidAndTypeAsync_ShouldReturnItem_WhenKnownGuidAndType()
    {
        // Arrange
        var dut = new LibraryItemRepository(_contextHelper.ContextMock);
        
        // Act
        var result = await dut.GetByGuidAndTypeAsync(_knownGuid, _knownLibraryType, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_knownGuid, result.Guid);
        Assert.AreEqual(_knownLibraryType, result.Type);
    }

    [TestMethod]
    public async Task GetByGuidAndTypeAsync_ShouldThrowEntityNotFoundException_WhenUnknownGuid()
    {
        // Arrange
        var dut = new LibraryItemRepository(_contextHelper.ContextMock);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<EntityNotFoundException<LibraryItem>>(()
            => dut.GetByGuidAndTypeAsync(Guid.NewGuid(), _knownLibraryType, default));
    }

    [TestMethod]
    public async Task GetByGuidAndTypeAsync_ShouldThrowEntityNotFoundException_WhenUnknownType()
    {
        // Arrange
        var dut = new LibraryItemRepository(_contextHelper.ContextMock);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<EntityNotFoundException<LibraryItem>>(()
            => dut.GetByGuidAndTypeAsync(_knownGuid, LibraryType.PUNCHLIST_SORTING, default));
    }
}

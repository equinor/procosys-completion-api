using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;
using Equinor.ProCoSys.Completion.Query.LibraryItemQueries.GetLibraryItems;

namespace Equinor.ProCoSys.Completion.Query.Tests.LibraryItemQueries.GetLibraryItems;

[TestClass]
public class GetLibraryItemsQueryHandlerTests : ReadOnlyTestsBase
{
    private readonly LibraryType _sortingType = LibraryType.PUNCHLIST_SORTING;
    private LibraryItem _libraryItemA;
    private LibraryItem _libraryItemB;
    private LibraryItem _libraryItemC;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        _libraryItemA = new LibraryItem(TestPlantA, Guid.NewGuid(), "A", "A Desc", _sortingType);
        _libraryItemB = new LibraryItem(TestPlantA, Guid.NewGuid(), "B", "B Desc", _sortingType);
        _libraryItemC = new LibraryItem(TestPlantA, Guid.NewGuid(), "C", "C Desc", _sortingType);
        var otherLibraryItem = new LibraryItem(TestPlantA, Guid.NewGuid(), "O", "O Desc", LibraryType.PUNCHLIST_PRIORITY);

        context.Library.Add(_libraryItemC);
        context.Library.Add(_libraryItemA);
        context.Library.Add(_libraryItemB);
        context.Library.Add(otherLibraryItem);

        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_IfNoneFound()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var query = new GetLibraryItemsQuery(LibraryType.PUNCHLIST_TYPE);
        var dut = new GetLibraryItemsQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(0, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectNumberOfLibraryItems()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var query = new GetLibraryItemsQuery(_sortingType);
        var dut = new GetLibraryItemsQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(3, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectOrderedLibraryItems()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var query = new GetLibraryItemsQuery(_sortingType);
        var dut = new GetLibraryItemsQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        AssertLibraryItem(result.Data.ElementAt(0), _libraryItemA);
        AssertLibraryItem(result.Data.ElementAt(1), _libraryItemB);
        AssertLibraryItem(result.Data.ElementAt(2), _libraryItemC);
    }

    private void AssertLibraryItem(LibraryItemDto libraryItemDto, LibraryItem libraryItem)
    {
        Assert.AreEqual(libraryItem.Code, libraryItemDto.Code);
        Assert.AreEqual(libraryItem.Description, libraryItemDto.Description);
    }
}

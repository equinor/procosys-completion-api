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
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Query.Tests.LibraryItemQueries.GetLibraryItems;

[TestClass]
public class GetLibraryItemsQueryHandlerTests : ReadOnlyTestsBase
{
    // perform these tests in other plant than TestPlantA since ReadOnlyTestsBase creates LibraryItem's there
    private readonly LibraryType _sortingType = LibraryType.PUNCHLIST_SORTING;
    private LibraryItem _punchListSortingLibraryItemA;
    private LibraryItem _punchListSortingLibraryItemB;
    private LibraryItem _punchListSortingLibraryItemC;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        _plantProviderMock.Plant.Returns(TestPlantB);
        
        using var context = new CompletionContext(dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);

        _punchListSortingLibraryItemA = new LibraryItem(TestPlantB, Guid.NewGuid(), "A", "A Desc", _sortingType);
        _punchListSortingLibraryItemB = new LibraryItem(TestPlantB, Guid.NewGuid(), "B", "B Desc", _sortingType);
        _punchListSortingLibraryItemC = new LibraryItem(TestPlantB, Guid.NewGuid(), "C", "C Desc", _sortingType);
        var punchListPriorityLibraryItem = new LibraryItem(TestPlantB, Guid.NewGuid(), "O", "O Desc", LibraryType.PUNCHLIST_PRIORITY);

        context.Library.Add(_punchListSortingLibraryItemC);
        context.Library.Add(_punchListSortingLibraryItemA);
        context.Library.Add(_punchListSortingLibraryItemB);
        context.Library.Add(punchListPriorityLibraryItem);

        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_IfNoneFound()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);

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
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);

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
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);

        var query = new GetLibraryItemsQuery(_sortingType);
        var dut = new GetLibraryItemsQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        AssertLibraryItem(result.Data.ElementAt(0), _punchListSortingLibraryItemA);
        AssertLibraryItem(result.Data.ElementAt(1), _punchListSortingLibraryItemB);
        AssertLibraryItem(result.Data.ElementAt(2), _punchListSortingLibraryItemC);
    }

    private void AssertLibraryItem(LibraryItemDto libraryItemDto, LibraryItem libraryItem)
    {
        Assert.AreEqual(libraryItem.Code, libraryItemDto.Code);
        Assert.AreEqual(libraryItem.Description, libraryItemDto.Description);
    }
}

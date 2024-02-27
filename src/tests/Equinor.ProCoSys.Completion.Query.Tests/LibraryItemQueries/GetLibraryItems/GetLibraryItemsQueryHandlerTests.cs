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
    // perform test in other plant than TestPlantA / TestPlantB, since base class initiate LibraryItems there
    private readonly string _testPlant = TestPlantWithoutData;

    private readonly LibraryType _sortingType = LibraryType.PUNCHLIST_SORTING;
    private readonly LibraryType _otherThanSortingType = LibraryType.PUNCHLIST_PRIORITY;
    private readonly LibraryType _typeType = LibraryType.PUNCHLIST_TYPE;

    private LibraryItem _punchListSortingLibraryItemA;
    private LibraryItem _punchListSortingLibraryItemB;
    private LibraryItem _punchListSortingLibraryItemC;
    private LibraryItem _punchListPriorityLibraryItem;
    private LibraryItem _punchListTypeLibraryItem;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
        => _plantProviderMock.Plant.Returns(_testPlant);

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_IfNoneFound()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var query = new GetLibraryItemsQuery([LibraryType.PUNCHLIST_TYPE]);
        var dut = new GetLibraryItemsQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(0, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectNumberOfLibraryItems_WhenQueryingSingleType()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        AddLibraryDataToPlant(context);

        var query = new GetLibraryItemsQuery([_sortingType]);
        var dut = new GetLibraryItemsQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(3, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectNumberOfLibraryItems_WhenQueryingMultipleTypes()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        AddLibraryDataToPlant(context);

        var query = new GetLibraryItemsQuery([_sortingType, _typeType]);
        var dut = new GetLibraryItemsQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(4, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectOrderedLibraryItems()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        AddLibraryDataToPlant(context);

        var query = new GetLibraryItemsQuery([_sortingType]);
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

    private void AddLibraryDataToPlant(CompletionContext context)
    {
        _punchListSortingLibraryItemA = new LibraryItem(_testPlant, Guid.NewGuid(), "A", "A Desc", _sortingType);
        _punchListSortingLibraryItemB = new LibraryItem(_testPlant, Guid.NewGuid(), "B", "B Desc", _sortingType);
        _punchListSortingLibraryItemC = new LibraryItem(_testPlant, Guid.NewGuid(), "C", "C Desc", _sortingType);
        _punchListPriorityLibraryItem = new LibraryItem(_testPlant, Guid.NewGuid(), "O", "O Desc", _otherThanSortingType);
        _punchListTypeLibraryItem = new LibraryItem(_testPlant, Guid.NewGuid(), "T", "T Desc", _typeType);

        context.Library.Add(_punchListSortingLibraryItemC);
        context.Library.Add(_punchListSortingLibraryItemA);
        context.Library.Add(_punchListSortingLibraryItemB);
        context.Library.Add(_punchListPriorityLibraryItem);
        context.Library.Add(_punchListTypeLibraryItem);

        context.SaveChangesAsync().Wait();
    }
}

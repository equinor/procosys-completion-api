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
public class GetPunchLibraryItemsQueryHandlerTests : ReadOnlyTestsBase
{
    // perform test in other plant than TestPlantA / TestPlantB, since base class initiate LibraryItems there
    private readonly string _testPlant = TestPlantWithoutData;

    private LibraryItem _punchListSortingLibraryItemA;
    private LibraryItem _punchListSortingLibraryItemB;
    private LibraryItem _punchListSortingLibraryItemC;
    private LibraryItem _punchListTypeLibraryItem;
    private LibraryItem _punchListPriorityLibraryItem;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
        => _plantProviderMock.Plant.Returns(_testPlant);

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_IfNoneFound()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var query = new GetPunchLibraryItemsQuery([LibraryType.PUNCHLIST_TYPE]);
        var dut = new GetPunchLibraryItemsQueryHandler(context);

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
        AddLibraryItemsInclusiveVoidedLibraryItem(context);

        var query = new GetPunchLibraryItemsQuery([LibraryType.PUNCHLIST_SORTING]);
        var dut = new GetPunchLibraryItemsQueryHandler(context);

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
        AddLibraryItemsInclusiveVoidedLibraryItem(context);

        var query = new GetPunchLibraryItemsQuery([LibraryType.PUNCHLIST_SORTING, LibraryType.PUNCHLIST_TYPE]);
        var dut = new GetPunchLibraryItemsQueryHandler(context);

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
        AddLibraryItemsInclusiveVoidedLibraryItem(context);

        var query = new GetPunchLibraryItemsQuery([LibraryType.PUNCHLIST_SORTING]);
        var dut = new GetPunchLibraryItemsQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        AssertLibraryItem(result.Data.ElementAt(0), _punchListSortingLibraryItemA);
        AssertLibraryItem(result.Data.ElementAt(1), _punchListSortingLibraryItemB);
        AssertLibraryItem(result.Data.ElementAt(2), _punchListSortingLibraryItemC);
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectNumberOfLibraryItems_WhenQueryingCommPriority()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        AddPunchPriorityLibraryItemsInclusiveNonClassifiedCommPriority(context);

        var query = new GetPunchLibraryItemsQuery([LibraryType.COMM_PRIORITY]);
        var dut = new GetPunchLibraryItemsQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(1, result.Data.Count());
        AssertLibraryItem(result.Data.ElementAt(0), _punchListPriorityLibraryItem);
    }

    private void AddPunchPriorityLibraryItemsInclusiveNonClassifiedCommPriority(CompletionContext context)
    {
        _punchListPriorityLibraryItem = new LibraryItem(_testPlant, Guid.NewGuid(), "PriA", "Pri A Desc", LibraryType.COMM_PRIORITY);
        _punchListPriorityLibraryItem.AddClassification(new Classification(Guid.Empty, Classification.PunchPriority));

        var nonClassifiedCommPriorityLibraryItem = new LibraryItem(_testPlant, Guid.NewGuid(), "PriB", "Pri B Desc", LibraryType.COMM_PRIORITY);

        context.Library.Add(_punchListPriorityLibraryItem);
        context.Library.Add(nonClassifiedCommPriorityLibraryItem);

        context.SaveChangesAsync().Wait();
    }

    private void AssertLibraryItem(LibraryItemDto libraryItemDto, LibraryItem libraryItem)
    {
        Assert.AreEqual(libraryItem.Code, libraryItemDto.Code);
        Assert.AreEqual(libraryItem.Description, libraryItemDto.Description);
    }

    private void AddLibraryItemsInclusiveVoidedLibraryItem(CompletionContext context)
    {
        _punchListSortingLibraryItemA = new LibraryItem(_testPlant, Guid.NewGuid(), "A", "A Desc", LibraryType.PUNCHLIST_SORTING);
        _punchListSortingLibraryItemB = new LibraryItem(_testPlant, Guid.NewGuid(), "B", "B Desc", LibraryType.PUNCHLIST_SORTING);
        _punchListSortingLibraryItemC = new LibraryItem(_testPlant, Guid.NewGuid(), "C", "C Desc", LibraryType.PUNCHLIST_SORTING);
        var voidedPunchListSortingLibraryItemC = new LibraryItem(_testPlant, Guid.NewGuid(), "V", "V Desc", LibraryType.PUNCHLIST_SORTING)
            {IsVoided = true};
        _punchListTypeLibraryItem = new LibraryItem(_testPlant, Guid.NewGuid(), "T", "T Desc", LibraryType.PUNCHLIST_TYPE);
        var voidedPunchListTypeLibraryItem = new LibraryItem(_testPlant, Guid.NewGuid(), "V", "V Desc", LibraryType.PUNCHLIST_TYPE)
            { IsVoided = true };

        context.Library.Add(_punchListSortingLibraryItemC);
        context.Library.Add(_punchListSortingLibraryItemA);
        context.Library.Add(_punchListSortingLibraryItemB);
        context.Library.Add(_punchListSortingLibraryItemB);
        context.Library.Add(voidedPunchListSortingLibraryItemC);
        context.Library.Add(_punchListTypeLibraryItem);
        context.Library.Add(voidedPunchListTypeLibraryItem);

        context.SaveChangesAsync().Wait();
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class ClassificationConsumerTests
{
    private readonly ConsumeContext<ClassificationEvent> _contextMock = Substitute.For<ConsumeContext<ClassificationEvent>>();
    private DbContextOptions<CompletionContext> _dbContextOptions = null!;
    private LibraryItem _priorityWithoutPunchClassification = null!;
    private LibraryItem _priorityWithPunchClassification = null!;

    [TestInitialize]
    public void Setup()
    {
        _dbContextOptions = new DbContextOptionsBuilder<CompletionContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new CompletionContext(_dbContextOptions, null!, null!, null!, null!);
        _priorityWithoutPunchClassification = new LibraryItem("P", Guid.NewGuid(), "C1", "D1", LibraryType.COMM_PRIORITY);
        _priorityWithPunchClassification = new LibraryItem("P", Guid.NewGuid(), "C2", "D2", LibraryType.COMM_PRIORITY);
        _priorityWithPunchClassification.AddClassification(new Classification(Guid.NewGuid(), Classification.PunchPriority));
        context.Library.Add(_priorityWithoutPunchClassification);
        context.Library.Add(_priorityWithPunchClassification);
        context.SaveChangesFromSyncAsync().Wait();
    }

    [TestMethod]
    public async Task Consume_ShouldAddPunchPriorityClassificationToPriority_WhenClassificationDoesNotExist()
    {
        //Arrange
        await using var context = new CompletionContext(_dbContextOptions, null!, null!, null!, null!);
        var dut = new ClassificationConsumer(Substitute.For<ILogger<ClassificationConsumer>>(), context);
        var priorityGuidUnderTest = _priorityWithoutPunchClassification.Guid;
        var bEvent = new ClassificationEvent(string.Empty, string.Empty, Guid.NewGuid(), DateTime.UtcNow, priorityGuidUnderTest, null);
        _contextMock.Message.Returns(bEvent);

        //Act
        await dut.Consume(_contextMock);

        //Assert
        var priority = context
            .Library
            .IgnoreQueryFilters()
            .Include(l => l.Classifications).SingleOrDefault(l => l.Guid == priorityGuidUnderTest);
        Assert.IsNotNull(priority);
        Assert.IsNotNull(priority.Classifications);
        Assert.AreEqual(1, priority.Classifications.Count);
        Assert.AreEqual(Classification.PunchPriority, priority.Classifications.ElementAt(0).Name);
    }

    [TestMethod]
    public async Task Consume_ShouldThrowException_WhenLibraryItemDoesNotExist()
    {
        //Arrange
        await using var context = new CompletionContext(_dbContextOptions, null!, null!, null!, null!);
        var dut = new ClassificationConsumer(Substitute.For<ILogger<ClassificationConsumer>>(), context);
        var bEvent = new ClassificationEvent(string.Empty, string.Empty, Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), null);
        _contextMock.Message.Returns(bEvent);

    //Act
    var exception = await Assert.ThrowsExceptionAsync<Exception>(() => dut.Consume(_contextMock));

        //Assert
        Assert.AreEqual($"{nameof(LibraryItem)} {bEvent.CommPriorityGuid} not found", exception.Message);
    }

    /*
    [TestMethod]
    public async Task Consume_ShouldHandleDeleteClassification_WhenClassificationNotExists()
    {
        //Arrange
        await using var context = new CompletionContext(_dbContextOptions, null!, null!, null!, null!);
        var dut = new ClassificationConsumer(Substitute.For<ILogger<ClassificationConsumer>>(), context);
        var priorityUnderTest = _priorityWithoutPunchClassification;
        var bEvent = new ClassificationEvent(
            string.Empty, 
            string.Empty,
            Guid.NewGuid(), 
            DateTime.UtcNow, 
            priorityUnderTest.Guid, "delete");
        _contextMock.Message.Returns(bEvent);

        //Act
        await dut.Consume(_contextMock);
    }

    [TestMethod]
    public async Task Consume_ShouldDeleteClassification_WhenClassificationExists()
    {
        //Arrange
        await using var context = new CompletionContext(_dbContextOptions, null!, null!, null!, null!);
        var dut = new ClassificationConsumer(Substitute.For<ILogger<ClassificationConsumer>>(), context);
        var priorityUnderTest = _priorityWithPunchClassification;
        var bEvent = new ClassificationEvent(
            string.Empty,
            string.Empty,
            priorityUnderTest.Classifications.ElementAt(0).Guid,
            DateTime.UtcNow,
            priorityUnderTest.Guid, "delete");
        _contextMock.Message.Returns(bEvent);

        //Act
        await dut.Consume(_contextMock);

        //Assert
        var priority = context
            .Library
            .IgnoreQueryFilters()
            .Include(l => l.Classifications).SingleOrDefault(l => l.Guid == priorityUnderTest.Guid);
        Assert.IsNotNull(priority);
        Assert.IsNotNull(priority.Classifications);
        Assert.AreEqual(0, priority.Classifications.Count);
    }
    */
}

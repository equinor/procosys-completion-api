using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class WorkOrderEventConsumerTests
{
    private readonly IWorkOrderRepository _workOrderRepoMock = Substitute.For<IWorkOrderRepository>();
    private readonly IPlantSetter _plantSetter = Substitute.For<IPlantSetter>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly WorkOrderEventConsumer _workOrderEventConsumer;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
    private readonly ConsumeContext<WorkOrderEvent> _contextMock = Substitute.For<ConsumeContext<WorkOrderEvent>>();
    private WorkOrder? _workOrderAddedToRepository;
    private const string WoNo = "112233";
    private const string Plant = "PCS$OSEBERG_C";

    public WorkOrderEventConsumerTests() =>
        _workOrderEventConsumer = new WorkOrderEventConsumer(Substitute.For<ILogger<WorkOrderEventConsumer>>(), _plantSetter, _workOrderRepoMock,
            _unitOfWorkMock, Substitute.For<ICurrentUserSetter>(), _applicationOptionsMock);

    [TestInitialize]
    public void Setup()
    {
        _applicationOptionsMock.CurrentValue.Returns(new ApplicationOptions { ObjectId = new Guid() });

        _workOrderRepoMock
            .When(x => x.Add(Arg.Any<WorkOrder>()))
            .Do(callInfo =>
            {
                _workOrderAddedToRepository = callInfo.Arg<WorkOrder>();
            });
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewWorkOrder_WhenWorkOrderDoesNotExist()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var bEvent = GetTestEvent(guid, Plant, WoNo, DateTime.Now);
        _contextMock.Message.Returns(bEvent);

        _workOrderRepoMock.ExistsAsync(guid, default).Returns(false);

        //Act
        await _workOrderEventConsumer.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_workOrderAddedToRepository);
        Assert.AreEqual(guid, _workOrderAddedToRepository.Guid);
        Assert.AreEqual(WoNo, _workOrderAddedToRepository.No);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldUpdateWorkOrder_WhenWorkOrderExists()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var bEvent = GetTestEvent(guid, Plant, WoNo, DateTime.Now);


        var workOrderToUpdate = new WorkOrder(Plant, guid, WoNo);

        _workOrderRepoMock.ExistsAsync(guid, default).Returns(true);
        _workOrderRepoMock.GetAsync(guid, default).Returns(workOrderToUpdate);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _workOrderEventConsumer.Consume(_contextMock);

        //Assert
        Assert.IsNull(_workOrderAddedToRepository);
        Assert.AreEqual(guid, workOrderToUpdate.Guid);
        Assert.AreEqual(WoNo, workOrderToUpdate.No);

        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldIgnoreUpdate_WhenLastUpdatedHasNotChanged()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var lastUpdated = DateTime.Now;
        var bEvent = GetTestEvent(guid, Plant, WoNo, lastUpdated);
        
        var workOrderToUpdate = new WorkOrder(Plant, guid, WoNo)
        {
            ProCoSys4LastUpdated = lastUpdated
        };

        _workOrderRepoMock.ExistsAsync(guid, default).Returns(true);
        _workOrderRepoMock.GetAsync(guid, default).Returns(workOrderToUpdate);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _workOrderEventConsumer.Consume(_contextMock);

        //Assert
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldIgnoreUpdate_WhenLastUpdatedIsOutDated()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var lastUpdated = DateTime.Now;
        var bEvent = GetTestEvent(guid, Plant, WoNo, lastUpdated);
        
        var workOrderToUpdate = new WorkOrder(Plant, guid, WoNo)
        {
            ProCoSys4LastUpdated = lastUpdated.AddMinutes(3)
        };

        _workOrderRepoMock.ExistsAsync(guid, default).Returns(true);
        _workOrderRepoMock.GetAsync(guid, default).Returns(workOrderToUpdate);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _workOrderEventConsumer.Consume(_contextMock);

        //Assert
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }
   
    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoProCoSysGuid()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.Empty, Plant, WoNo, DateTime.Now);

        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _workOrderEventConsumer.Consume(_contextMock), "Message is missing ProCoSysGuid");
    }
    
    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoPlant()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.Empty, string.Empty, WoNo, DateTime.Now);
        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _workOrderEventConsumer.Consume(_contextMock), "Message is missing Plant");
    }

    private static WorkOrderEvent GetTestEvent(Guid guid, string plant, string woNo, DateTime lastUpdated) => new (
            plant,
            guid,
            woNo, 
            false, 
            lastUpdated
        );
}

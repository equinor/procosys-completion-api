using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class SWCREventConsumerTests
{
    private readonly ISWCRRepository _swcrRepoMock = Substitute.For<ISWCRRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly SWCREventConsumer _dut;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
    private readonly ConsumeContext<SWCREvent> _contextMock = Substitute.For<ConsumeContext<SWCREvent>>();
    private SWCR? _swcrAddedToRepository;
    private const string SwcrNo = "112233";
    private const string Plant = "PCS$OSEBERG_C";

    public SWCREventConsumerTests() =>
        _dut = new SWCREventConsumer(Substitute.For<ILogger<SWCREventConsumer>>(), _swcrRepoMock,
            _unitOfWorkMock);

    [TestInitialize]
    public void Setup()
    {
        _applicationOptionsMock.CurrentValue.Returns(new ApplicationOptions { ObjectId = new Guid() });

        _swcrRepoMock
            .When(x => x.Add(Arg.Any<SWCR>()))
            .Do(callInfo =>
            {
                _swcrAddedToRepository = callInfo.Arg<SWCR>();
            });
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewSWCR_WhenSwcrDoesNotExist()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var bEvent = GetTestEvent(guid, Plant, SwcrNo, false, DateTime.Now, null);
        _contextMock.Message.Returns(bEvent);

        _swcrRepoMock.ExistsAsync(guid, default).Returns(false);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_swcrAddedToRepository);
        Assert.AreEqual(guid, _swcrAddedToRepository.Guid);
        Assert.AreEqual(false, _swcrAddedToRepository.IsVoided);
        Assert.AreEqual(int.Parse(SwcrNo), _swcrAddedToRepository.No);
        await _unitOfWorkMock.Received(1).SaveChangesFromSyncAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldUpdateSwcr_WhenSwcrExists()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var bEvent = GetTestEvent(guid, Plant, SwcrNo, true,DateTime.Now, null);


        var swcrToUpdate = new SWCR(Plant, guid, int.Parse(SwcrNo))
        {
            IsVoided = false,
            ProCoSys4LastUpdated = DateTime.Now.AddMinutes(-1)
        };

        _swcrRepoMock.ExistsAsync(guid, default).Returns(true);
        _swcrRepoMock.GetAsync(guid, default).Returns(swcrToUpdate);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNull(_swcrAddedToRepository);
        Assert.AreEqual(guid, swcrToUpdate.Guid);
        Assert.AreEqual(true, swcrToUpdate.IsVoided);
        Assert.AreEqual(int.Parse(SwcrNo), swcrToUpdate.No);

        await _unitOfWorkMock.Received(1).SaveChangesFromSyncAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldIgnoreUpdate_WhenLastUpdatedHasNotChanged()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var bEventLastUpdated = DateTime.Now;
        var bEvent = GetTestEvent(guid, Plant, SwcrNo, true, bEventLastUpdated, null);


        var swcrToUpdate = new SWCR(Plant, guid, int.Parse(SwcrNo))
        {
            IsVoided = false,
            ProCoSys4LastUpdated = bEventLastUpdated
        };

        _swcrRepoMock.ExistsAsync(guid, default).Returns(true);
        _swcrRepoMock.GetAsync(guid, default).Returns(swcrToUpdate);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        _swcrRepoMock.Received(0).Remove(Arg.Any<SWCR>());
        await _unitOfWorkMock.Received(0).SaveChangesFromSyncAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldIgnoreUpdate_WhenLastUpdatedIsOutdated()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var bEventLastUpdated = DateTime.Now;
        var bEvent = GetTestEvent(guid, Plant, SwcrNo, true, bEventLastUpdated, null);


        var swcrToUpdate = new SWCR(Plant, guid, int.Parse(SwcrNo))
        {
            IsVoided = false,
            ProCoSys4LastUpdated = bEventLastUpdated.AddMinutes(1)
        };

        _swcrRepoMock.ExistsAsync(guid, default).Returns(true);
        _swcrRepoMock.GetAsync(guid, default).Returns(swcrToUpdate);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        _swcrRepoMock.Received(0).Remove(Arg.Any<SWCR>());
        await _unitOfWorkMock.Received(0).SaveChangesFromSyncAsync();
    }
   
    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoProCoSysGuid()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.Empty, Plant, SwcrNo, false, DateTime.Now, null);

        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _dut.Consume(_contextMock), "Message is missing ProCoSysGuid");
    }

    
    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoPlant()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.Empty, string.Empty, SwcrNo, false, DateTime.Now, null);
        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _dut.Consume(_contextMock), "Message is missing Plant");
    }

    [TestMethod]
    public async Task Consume_ShouldDeleteSWCR_On_Delete_Behavior()
    {
        // Arrange
        var guid = Guid.NewGuid();

        var bEvent = GetTestEvent(guid, Plant, SwcrNo, false, DateTime.UtcNow, "delete");
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        await _swcrRepoMock.Received(1).RemoveByGuidAsync(guid, Arg.Any<CancellationToken>());
        await _swcrRepoMock.Received(0).GetAsync(guid, Arg.Any<CancellationToken>());
        _swcrRepoMock.Received(0).Add(Arg.Any<SWCR>());
        await _unitOfWorkMock.Received(1).SaveChangesFromSyncAsync();
    }

    private static SWCREvent GetTestEvent(Guid guid, string plant, string swcrNo, bool isVoided, DateTime lastUpdated, string? behavior) => new (
            string.Empty,
            plant,
            guid,
            string.Empty,
            swcrNo,
            string.Empty,
            11111,
            Guid.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            DateTime.UtcNow,
            isVoided,
            lastUpdated,
            DateOnly.MinValue,
            float.MinValue, 
            behavior
        );
}

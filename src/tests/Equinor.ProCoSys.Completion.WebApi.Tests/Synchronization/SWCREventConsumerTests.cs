using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
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
    private readonly IPlantSetter _plantSetter = Substitute.For<IPlantSetter>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly SWCREventConsumer _swcrEventConsumer;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
    private readonly ConsumeContext<SWCREvent> _contextMock = Substitute.For<ConsumeContext<SWCREvent>>();
    private SWCR? _swcrAddedToRepository;
    private const string SwcrNo = "112233";
    private const string Plant = "PCS$OSEBERG_C";

    public SWCREventConsumerTests() =>
        _swcrEventConsumer = new SWCREventConsumer(Substitute.For<ILogger<SWCREventConsumer>>(), _plantSetter, _swcrRepoMock,
            _unitOfWorkMock, Substitute.For<ICurrentUserSetter>(), _applicationOptionsMock);

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
        var bEvent = GetTestEvent(guid, Plant, SwcrNo, false);
        _contextMock.Message.Returns(bEvent);

        _swcrRepoMock.ExistsAsync(guid, default).Returns(false);

        //Act
        await _swcrEventConsumer.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_swcrAddedToRepository);
        Assert.AreEqual(guid, _swcrAddedToRepository.Guid);
        Assert.AreEqual(false, _swcrAddedToRepository.IsVoided);
        Assert.AreEqual(int.Parse(SwcrNo), _swcrAddedToRepository.No);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldUpdateSwcr_WhenSwcrExists()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var bEvent = GetTestEvent(guid, Plant, SwcrNo, true);


        var swcrToUpdate = new SWCR(Plant, guid, int.Parse(SwcrNo))
        {
            IsVoided = false
        };

        _swcrRepoMock.ExistsAsync(guid, default).Returns(true);
        _swcrRepoMock.GetAsync(guid, default).Returns(swcrToUpdate);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _swcrEventConsumer.Consume(_contextMock);

        //Assert
        Assert.IsNull(_swcrAddedToRepository);
        Assert.AreEqual(guid, swcrToUpdate.Guid);
        Assert.AreEqual(true, swcrToUpdate.IsVoided);
        Assert.AreEqual(int.Parse(SwcrNo), swcrToUpdate.No);

        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
   
    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoProCoSysGuid()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.Empty, Plant, SwcrNo, false);

        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _swcrEventConsumer.Consume(_contextMock), "Message is missing ProCoSysGuid");
    }

    
    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoPlant()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.Empty, string.Empty, SwcrNo, false);
        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _swcrEventConsumer.Consume(_contextMock), "Message is missing Plant");
    }

    private static SWCREvent GetTestEvent(Guid guid, string plant, string swcrNo, bool isVoided) => new (
            string.Empty,
            plant,
            guid,
            null,
            swcrNo,
            null,
            11111,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            DateTime.UtcNow,
            isVoided,
            DateTime.UtcNow,
            null,
            null
        );
}

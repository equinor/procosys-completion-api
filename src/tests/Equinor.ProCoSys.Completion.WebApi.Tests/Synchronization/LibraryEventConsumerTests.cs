using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class LibraryEventConsumerTests
{
    private readonly ILibraryItemRepository _libraryItemRepoMock = Substitute.For<ILibraryItemRepository>();
    private readonly IPlantSetter _plantSetter = Substitute.For<IPlantSetter>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly LibraryEventConsumer _libraryEventConsumer;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
    private readonly ConsumeContext<LibraryEvent> _contextMock = Substitute.For<ConsumeContext<LibraryEvent>>();
    private LibraryItem? _libraryAddedToRepository;
    private const string Code = "COM";
    private const string Description = "COMMISSIONING";
    private const string Plant = "PCS$OSEBERG_C";
    private const LibraryType Type = LibraryType.COMPLETION_ORGANIZATION;

    public LibraryEventConsumerTests() =>
        _libraryEventConsumer = new LibraryEventConsumer(Substitute.For<ILogger<LibraryEventConsumer>>(), _plantSetter, _libraryItemRepoMock, 
            _unitOfWorkMock, Substitute.For<ICurrentUserSetter>(), _applicationOptionsMock);

    [TestInitialize]
    public void Setup()
    {
        _applicationOptionsMock.CurrentValue.Returns(new ApplicationOptions { ObjectId = new Guid() });
        
        _libraryItemRepoMock
            .When(x => x.Add(Arg.Any<LibraryItem>()))
            .Do(callInfo =>
            {
                _libraryAddedToRepository = callInfo.Arg<LibraryItem>();
            });
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewLibraryItem_WhenLibraryItemDoesNotExist()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var bEvent = GetTestEvent(guid, Plant, Description, LibraryType.COMPLETION_ORGANIZATION.ToString(), false);
        _contextMock.Message.Returns(bEvent);
        
        _libraryItemRepoMock.ExistsAsync(guid, default).Returns(false);
        
        //Act
        await _libraryEventConsumer.Consume(_contextMock);
        
        //Assert
        Assert.IsNotNull(_libraryAddedToRepository);
        Assert.AreEqual(guid, _libraryAddedToRepository.Guid);
        Assert.AreEqual(false,_libraryAddedToRepository.IsVoided);
        Assert.AreEqual(Description,_libraryAddedToRepository.Description);
        Assert.AreEqual(Code,_libraryAddedToRepository.Code);
        Assert.AreEqual(Type, _libraryAddedToRepository.Type);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldUpdateLibraryItem_WhenLibraryItemExists()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var bEvent = GetTestEvent(guid, Plant, Description, LibraryType.COMPLETION_ORGANIZATION.ToString(), true);
        

        var libraryItemToUpdate = new LibraryItem(Plant, guid, Code, Description, Type)
        {
            IsVoided = false
        };

        _libraryItemRepoMock.ExistsAsync(guid, default).Returns(true);
        _libraryItemRepoMock.GetAsync(guid, default).Returns(libraryItemToUpdate);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _libraryEventConsumer.Consume(_contextMock);
        
        //Assert
        Assert.IsNull(_libraryAddedToRepository);
        Assert.AreEqual(guid,libraryItemToUpdate.Guid);
        Assert.AreEqual(true,libraryItemToUpdate.IsVoided);
        Assert.AreEqual(Description,libraryItemToUpdate.Description);
        Assert.AreEqual(Code,libraryItemToUpdate.Code);
        Assert.AreEqual(Type, libraryItemToUpdate.Type);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoProCoSysGuid()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.Empty, Plant, Description, LibraryType.COMPLETION_ORGANIZATION.ToString(), false);

        _contextMock.Message.Returns(bEvent);
        
        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() 
            => _libraryEventConsumer.Consume(_contextMock),"Message is missing ProCoSysGuid");
    }

    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoPlant()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.NewGuid(), string.Empty, Description, LibraryType.COMPLETION_ORGANIZATION.ToString(), false);
        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _libraryEventConsumer.Consume(_contextMock), "Message is missing Plant");
    }

    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoType()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.NewGuid(), Plant, Description, string.Empty, false);
        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _libraryEventConsumer.Consume(_contextMock), "Message is missing Type");
    }

    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoDescription()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.NewGuid(), Plant, string.Empty, LibraryType.COMPLETION_ORGANIZATION.ToString(), false);
        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _libraryEventConsumer.Consume(_contextMock), "Message is missing Description");
    }

    private static LibraryEvent GetTestEvent(Guid guid, string plant, string description, string type, bool isVoided) => new (
            plant,
            guid,
            Code,
            description,
            isVoided,
            type);
}

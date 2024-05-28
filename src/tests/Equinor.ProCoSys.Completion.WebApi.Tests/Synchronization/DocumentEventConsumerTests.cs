using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class DocumentEventConsumerTests
{
    private readonly IDocumentRepository _documentRepoMock = Substitute.For<IDocumentRepository>();
    private readonly IPlantSetter _plantSetter = Substitute.For<IPlantSetter>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly DocumentEventConsumer _documentEventConsumer;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
    private readonly ConsumeContext<DocumentEvent> _contextMock = Substitute.For<ConsumeContext<DocumentEvent>>();
    private Document? _documentAddedToRepository;

    private const string DocumentNo = "112233";
    private const string Plant = "PCS$OSEBERG_C";

    public DocumentEventConsumerTests() =>
        _documentEventConsumer = new DocumentEventConsumer(Substitute.For<ILogger<DocumentEventConsumer>>(), _plantSetter, _documentRepoMock,
            _unitOfWorkMock);

    [TestInitialize]
    public void Setup()
    {
        _applicationOptionsMock.CurrentValue.Returns(new ApplicationOptions { ObjectId = new Guid() });

        _documentRepoMock
            .When(x => x.Add(Arg.Any<Document>()))
            .Do(callInfo =>
            {
                _documentAddedToRepository = callInfo.Arg<Document>();
            });
    }
    
    [TestMethod]
    public async Task Consume_ShouldAddNewDocument_WhenDocumentDoesNotExist()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var bEvent = GetTestEvent(guid, Plant, DocumentNo, DateTime.Now);
        _contextMock.Message.Returns(bEvent);

        _documentRepoMock.ExistsAsync(guid, default).Returns(false);

        //Act
        await _documentEventConsumer.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_documentAddedToRepository);
        Assert.AreEqual(guid, _documentAddedToRepository.Guid);
        Assert.AreEqual(DocumentNo, _documentAddedToRepository.No);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldUpdateDocument_WhenDocumentExists()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var bEvent = GetTestEvent(guid, Plant, DocumentNo, DateTime.Now);


        var documentToUpdate = new Document(Plant, guid, DocumentNo)
        {
            IsVoided = false
        };

        _documentRepoMock.ExistsAsync(guid, default).Returns(true);
        _documentRepoMock.GetAsync(guid, default).Returns(documentToUpdate);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _documentEventConsumer.Consume(_contextMock);

        //Assert
        Assert.IsNull(_documentAddedToRepository);
        Assert.AreEqual(guid, documentToUpdate.Guid);
        Assert.AreEqual(DocumentNo, documentToUpdate.No);

        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldIgnoreUpdate_WhenLastUpdatedNotChanged()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var lastUpdated = DateTime.Now;
        var bEvent = GetTestEvent(guid, Plant, DocumentNo, lastUpdated);


        var documentToUpdate = new Document(Plant, guid, DocumentNo)
        {
            IsVoided = false,
            ProCoSys4LastUpdated = lastUpdated
        };

        _documentRepoMock.ExistsAsync(guid, default).Returns(true);
        _documentRepoMock.GetAsync(guid, default).Returns(documentToUpdate);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _documentEventConsumer.Consume(_contextMock);

        //Assert
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldIgnoreUpdate_WhenLastUpdatedOutDated()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var lastUpdated = DateTime.Now;
        var bEvent = GetTestEvent(guid, Plant, DocumentNo, lastUpdated);


        var documentToUpdate = new Document(Plant, guid, DocumentNo)
        {
            IsVoided = false,
            ProCoSys4LastUpdated = lastUpdated.AddMinutes(1)
        };

        _documentRepoMock.ExistsAsync(guid, default).Returns(true);
        _documentRepoMock.GetAsync(guid, default).Returns(documentToUpdate);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _documentEventConsumer.Consume(_contextMock);

        //Assert
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }
   
    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoProCoSysGuid()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.Empty, Plant, DocumentNo, DateTime.Now);

        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _documentEventConsumer.Consume(_contextMock), "Message is missing ProCoSysGuid");
    }

    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoPlant()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.Empty, string.Empty, DocumentNo, DateTime.Now);
        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _documentEventConsumer.Consume(_contextMock), "Message is missing Plant");
    }

    private static DocumentEvent GetTestEvent(Guid guid, string plant, string documentNo, DateTime lastUpdated) => new(
            plant,
            guid,
            documentNo,
            false,
            lastUpdated
        );
}

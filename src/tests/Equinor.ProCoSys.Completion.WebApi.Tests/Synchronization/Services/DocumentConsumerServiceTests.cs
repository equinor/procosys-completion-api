﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.WebApi.Synchronization.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization.Services;

[TestClass]
public class DocumentConsumerServiceTests
{
    private readonly IDocumentRepository _documentRepoMock = Substitute.For<IDocumentRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly DocumentConsumerService _dut;
    private readonly ConsumeContext<DocumentEvent> _contextMock = Substitute.For<ConsumeContext<DocumentEvent>>();
    private Document? _documentAddedToRepository;

    private const string DocumentNo = "112233";
    private const string Plant = "PCS$OSEBERG_C";

    public DocumentConsumerServiceTests() =>
        _dut = new DocumentConsumerService(
            Substitute.For<ILogger<DocumentConsumerService>>(), 
            _documentRepoMock,
            _unitOfWorkMock);

    [TestInitialize]
    public void Setup()
        => _documentRepoMock
            .When(x => x.Add(Arg.Any<Document>()))
            .Do(callInfo =>
            {
                _documentAddedToRepository = callInfo.Arg<Document>();
            });

    [TestMethod]
    public async Task Consume_ShouldAddNewDocument_WhenDocumentDoesNotExist()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var bEvent = GetTestEvent(guid, Plant, DocumentNo, DateTime.Now);
        _contextMock.Message.Returns(bEvent);

        _documentRepoMock.ExistsAsync(guid, default).Returns(false);

        //Act
        await _dut.ConsumeDocumentEvent(_contextMock,_contextMock.Message, string.Empty);

        //Assert
        Assert.IsNotNull(_documentAddedToRepository);
        Assert.AreEqual(guid, _documentAddedToRepository.Guid);
        Assert.AreEqual(DocumentNo, _documentAddedToRepository.No);
        await _unitOfWorkMock.Received(1).SaveChangesFromSyncAsync();
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
        await _dut.ConsumeDocumentEvent(_contextMock,_contextMock.Message, string.Empty);

        //Assert
        Assert.IsNull(_documentAddedToRepository);
        Assert.AreEqual(guid, documentToUpdate.Guid);
        Assert.AreEqual(DocumentNo, documentToUpdate.No);

        await _unitOfWorkMock.Received(1).SaveChangesFromSyncAsync();
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
        await _dut.ConsumeDocumentEvent(_contextMock,_contextMock.Message, string.Empty);

        //Assert
        await _unitOfWorkMock.Received(0).SaveChangesFromSyncAsync();
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
        await _dut.ConsumeDocumentEvent(_contextMock,_contextMock.Message, string.Empty);

        //Assert
        await _unitOfWorkMock.Received(0).SaveChangesFromSyncAsync();
    }
   
    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoProCoSysGuid()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.Empty, Plant, DocumentNo, DateTime.Now);

        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            =>  _dut.ConsumeDocumentEvent(_contextMock,_contextMock.Message, string.Empty), "Message is missing ProCoSysGuid");
    }

    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoPlant()
    {
        //Arrange
        var bEvent = GetTestEvent(Guid.Empty, string.Empty, DocumentNo, DateTime.Now);
        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            =>  _dut.ConsumeDocumentEvent(_contextMock,_contextMock.Message, string.Empty), "Message is missing Plant");
    }

    [TestMethod]
    public async Task Consume_ShouldRetryTwoTimes_OnDbUpdateConcurrencyException()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var bEvent = GetTestEvent(guid, Plant, DocumentNo, DateTime.Now);
        _contextMock.Message.Returns(bEvent);

        var callCount = 0;
        _unitOfWorkMock
            .When(x => x.SaveChangesFromSyncAsync(Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                callCount++;
                throw new DbUpdateConcurrencyException();
            });

        // Act
        await _dut.ConsumeDocumentEvent(_contextMock, _contextMock.Message, string.Empty);

        // Assert
        Assert.AreEqual(2, callCount);

        await _unitOfWorkMock.Received(2).SaveChangesFromSyncAsync(Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Consume_ShouldRetryTwoTimes_OnDuplicateKeyException()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var bEvent = GetTestEvent(guid, Plant, DocumentNo, DateTime.Now);
        _contextMock.Message.Returns(bEvent);

        var callCount = 0;
        _unitOfWorkMock
            .When(x => x.SaveChangesFromSyncAsync(Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                callCount++;
                throw new Exception("Cannot insert duplicate key row in object 'dbo.Documents' with unique index 'IX_Documents_Guid'");
            });

        // Act
        await _dut.ConsumeDocumentEvent(_contextMock, _contextMock.Message, string.Empty);

        // Assert
        Assert.AreEqual(2, callCount);
        // Verify that the unitOfWork.SaveChangesFromSyncAsync() is attempted MaxRetryCount times
        await _unitOfWorkMock.Received(2).SaveChangesFromSyncAsync(Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Consume_ShouldRetryTwoTimes_OnDuplicateKey_InnerException()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var bEvent = GetTestEvent(guid, Plant, DocumentNo, DateTime.Now);
        _contextMock.Message.Returns(bEvent);

        var callCount = 0;
        _unitOfWorkMock
            .When(x => x.SaveChangesFromSyncAsync(Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                callCount++;
                throw new Exception("Outer exception", new Exception("Cannot insert duplicate key row in object 'dbo.Documents' with unique index 'IX_Documents_Guid'"));
            });

        // Act
        await _dut.ConsumeDocumentEvent(_contextMock, _contextMock.Message, string.Empty);

        // Assert
        Assert.AreEqual(2, callCount);
        // Verify that the unitOfWork.SaveChangesFromSyncAsync() is attempted MaxRetryCount times
        await _unitOfWorkMock.Received(2).SaveChangesFromSyncAsync(Arg.Any<CancellationToken>());
    }


    private static DocumentEvent GetTestEvent(Guid guid, string plant, string documentNo, DateTime lastUpdated) => new(
            plant,
            guid,
            documentNo,
            false,
            lastUpdated,
            null
        );
}

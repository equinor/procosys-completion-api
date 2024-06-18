using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class PunchItemAttachmentEventConsumerTests
{
    private readonly Guid _createdByGuid = Guid.NewGuid();
    private readonly IAttachmentRepository _attachmentRepositoryMock = Substitute.For<IAttachmentRepository>();
    private readonly ILinkRepository _linkRepositoryMock = Substitute.For<ILinkRepository>();
    private readonly IPersonRepository _personRepoMock = Substitute.For<IPersonRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private PunchItemAttachmentEventConsumer _dut = null!;
    private readonly ConsumeContext<PunchItemAttachmentEvent> _contextMock = Substitute.For<ConsumeContext<PunchItemAttachmentEvent>>();
    private Attachment? _attachmentAddedToRepository;
    private Link? _linkAddedToRepository;
    private readonly DateTime _lastUpdated = DateTime.Now;
    private readonly string _lastUpdatedByUser = "someone";

    [TestInitialize]
    public void Setup()
    {
        _dut = new PunchItemAttachmentEventConsumer(
            Substitute.For<ILogger<PunchItemAttachmentEventConsumer>>(),
            _personRepoMock,
            _attachmentRepositoryMock,
            _linkRepositoryMock,
            _unitOfWorkMock);

        _attachmentRepositoryMock
            .When(x => x.Add(Arg.Any<Attachment>()))
            .Do(callInfo =>
            {
                _attachmentAddedToRepository = callInfo.Arg<Attachment>();
            });

        _linkRepositoryMock
            .When(x => x.Add(Arg.Any<Link>()))
            .Do(callInfo =>
            {
                _linkAddedToRepository = callInfo.Arg<Link>();
            });

        _personRepoMock.GetAsync(_createdByGuid, default)
            .Returns(new Person(_createdByGuid, "fn", "ln", "un", "@", false));
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewAttachment_WhenEventIsAttachmentAndAttachmentDoesNotExist()
    {
        //Arrange
        var bEvent =
            new PunchItemAttachmentEvent("Pl", "Pr", Guid.NewGuid(), Guid.NewGuid(), "fn", null, "T", 2, _createdByGuid, DateTime.UtcNow, _lastUpdated, _lastUpdatedByUser, null);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_attachmentAddedToRepository);
        Assert.IsNull(_linkAddedToRepository);
        Assert.AreEqual(bEvent.LastUpdated, _attachmentAddedToRepository.ProCoSys4LastUpdated);
        Assert.AreEqual(bEvent.LastUpdatedByUser, _attachmentAddedToRepository.ProCoSys4LastUpdatedByUser);
        Assert.AreEqual(bEvent.AttachmentGuid, _attachmentAddedToRepository.Guid);
        Assert.AreEqual(bEvent.PunchItemGuid, _attachmentAddedToRepository.ParentGuid);
        Assert.AreEqual(bEvent.FileName, _attachmentAddedToRepository.FileName);
        Assert.AreEqual(bEvent.Title, _attachmentAddedToRepository.Description);
        Assert.AreEqual(_createdByGuid, _attachmentAddedToRepository.CreatedBy.Guid);
        Assert.AreEqual(bEvent.CreatedAt, _attachmentAddedToRepository.CreatedAtUtc);
        Assert.AreEqual(bEvent.LastUpdated, _attachmentAddedToRepository.ModifiedAtUtc);
        Assert.IsTrue(_attachmentAddedToRepository.BlobPath.Contains(bEvent.ProjectName));
        await _unitOfWorkMock.Received(1).SaveChangesFromSyncAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldUpdateAttachment_WhenEventIsAttachmentAndAttachmentExist()
    {
        //Arrange
        var bEvent =
            new PunchItemAttachmentEvent("Pl", "Pr", Guid.NewGuid(), Guid.NewGuid(), "fn", null, "newTitle", 2, _createdByGuid, DateTime.UtcNow, _lastUpdated, _lastUpdatedByUser, null);
        _contextMock.Message.Returns(bEvent);
        var existingAttachment = new Attachment("Proj", "PT", Guid.NewGuid(), "FN"){ ProCoSys4LastUpdated = _lastUpdated.AddSeconds(-2)};
        _attachmentRepositoryMock.ExistsAsync(bEvent.AttachmentGuid, _contextMock.CancellationToken).Returns(true);
        _attachmentRepositoryMock.GetAsync(bEvent.AttachmentGuid, _contextMock.CancellationToken).Returns(existingAttachment);
        
        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNull(_attachmentAddedToRepository);
        Assert.IsNull(_linkAddedToRepository);
        Assert.AreEqual(bEvent.LastUpdated, existingAttachment.ProCoSys4LastUpdated);
        Assert.AreEqual(bEvent.LastUpdatedByUser, existingAttachment.ProCoSys4LastUpdatedByUser);
        Assert.AreEqual(bEvent.Title, existingAttachment.Description);
        Assert.AreEqual(bEvent.LastUpdated, existingAttachment.ModifiedAtUtc);
        await _unitOfWorkMock.Received(1).SaveChangesFromSyncAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldNotUpdateAttachment_WhenEventIsOlder()
    {
        //Arrange
        var bEvent =
            new PunchItemAttachmentEvent("Pl", "Pr", Guid.NewGuid(), Guid.NewGuid(), "fn", null, "newTitle", 2, _createdByGuid, DateTime.UtcNow, _lastUpdated, _lastUpdatedByUser, null);
        _contextMock.Message.Returns(bEvent);
        var oldProCoSys4LastUpdated = _lastUpdated.AddSeconds(2);
        var oldProCoSys4LastUpdatedByUser = "xxx";
        var existingAttachment = new Attachment("Proj", "PT", Guid.NewGuid(), "FN")
        {
            ProCoSys4LastUpdated = oldProCoSys4LastUpdated,
            ProCoSys4LastUpdatedByUser = oldProCoSys4LastUpdatedByUser,
        };
        _attachmentRepositoryMock.ExistsAsync(bEvent.AttachmentGuid, _contextMock.CancellationToken).Returns(true);
        _attachmentRepositoryMock.GetAsync(bEvent.AttachmentGuid, _contextMock.CancellationToken).Returns(existingAttachment);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNull(_attachmentAddedToRepository);
        Assert.IsNull(_linkAddedToRepository);
        Assert.AreEqual(oldProCoSys4LastUpdated, existingAttachment.ProCoSys4LastUpdated);
        Assert.AreEqual(oldProCoSys4LastUpdatedByUser, existingAttachment.ProCoSys4LastUpdatedByUser);
        await _unitOfWorkMock.Received(0).SaveChangesFromSyncAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewLink_WhenEventIsLinkAndLinkDoesNotExist()
    {
        //Arrange
        var bEvent =
            new PunchItemAttachmentEvent("Pl", "Pr", Guid.NewGuid(), Guid.NewGuid(), null, "uri", "T", null, _createdByGuid, DateTime.UtcNow, _lastUpdated, _lastUpdatedByUser, null);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNull(_attachmentAddedToRepository);
        Assert.IsNotNull(_linkAddedToRepository);
        Assert.AreEqual(bEvent.LastUpdated, _linkAddedToRepository.ProCoSys4LastUpdated);
        Assert.AreEqual(bEvent.AttachmentGuid, _linkAddedToRepository.Guid);
        Assert.AreEqual(bEvent.PunchItemGuid, _linkAddedToRepository.ParentGuid);
        Assert.AreEqual(bEvent.Title, _linkAddedToRepository.Title);
        Assert.AreEqual(bEvent.Uri, _linkAddedToRepository.Url);
        Assert.AreEqual(_createdByGuid, _linkAddedToRepository.CreatedBy.Guid);
        Assert.AreEqual(bEvent.CreatedAt, _linkAddedToRepository.CreatedAtUtc);
        Assert.AreEqual(bEvent.LastUpdated, _linkAddedToRepository.ModifiedAtUtc);
        await _unitOfWorkMock.Received(1).SaveChangesFromSyncAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldUpdateLink_WhenEventIsLinkAndLinkExist()
    {
        //Arrange
        var bEvent =
            new PunchItemAttachmentEvent("Pl", "Pr", Guid.NewGuid(), Guid.NewGuid(), null, "newUri", "newTitle", null, _createdByGuid, DateTime.UtcNow, _lastUpdated, _lastUpdatedByUser, null);
        _contextMock.Message.Returns(bEvent);
        var oldProCoSys4LastUpdated = _lastUpdated.AddSeconds(2);
        var existingLink = new Link("P", Guid.NewGuid(), "oldT", "oldU") { ProCoSys4LastUpdated = oldProCoSys4LastUpdated };
        _linkRepositoryMock.ExistsAsync(bEvent.AttachmentGuid, _contextMock.CancellationToken).Returns(true);
        _linkRepositoryMock.GetAsync(bEvent.AttachmentGuid, _contextMock.CancellationToken).Returns(existingLink);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNull(_attachmentAddedToRepository);
        Assert.IsNull(_linkAddedToRepository);
        Assert.AreEqual(oldProCoSys4LastUpdated, existingLink.ProCoSys4LastUpdated);
        await _unitOfWorkMock.Received(0).SaveChangesFromSyncAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldNotUpdateLink_WhenEventIsOlder()
    {
        //Arrange
        var bEvent =
            new PunchItemAttachmentEvent("Pl", "Pr", Guid.NewGuid(), Guid.NewGuid(), null, "newUri", "newTitle", null, _createdByGuid, DateTime.UtcNow, _lastUpdated, _lastUpdatedByUser, null);
        _contextMock.Message.Returns(bEvent);
        var oldProCoSys4LastUpdated = _lastUpdated.AddSeconds(2);
        var oldProCoSys4LastUpdatedByUser = "xxx";
        var existingLink = new Link("P", Guid.NewGuid(), "oldT", "oldU")
        {
            ProCoSys4LastUpdated = oldProCoSys4LastUpdated,
            ProCoSys4LastUpdatedByUser = oldProCoSys4LastUpdatedByUser,
        };
        _linkRepositoryMock.ExistsAsync(bEvent.AttachmentGuid, _contextMock.CancellationToken).Returns(true);
        _linkRepositoryMock.GetAsync(bEvent.AttachmentGuid, _contextMock.CancellationToken).Returns(existingLink);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNull(_attachmentAddedToRepository);
        Assert.IsNull(_linkAddedToRepository);
        Assert.AreEqual(oldProCoSys4LastUpdated, existingLink.ProCoSys4LastUpdated);
        Assert.AreEqual(oldProCoSys4LastUpdatedByUser, existingLink.ProCoSys4LastUpdatedByUser);
        await _unitOfWorkMock.Received(0).SaveChangesFromSyncAsync();
    }

    [TestMethod]
    public async Task Consume_Should_Call_Remove()
    {
        //Arrange
        var bEvent =
            new PunchItemAttachmentEvent("Pl", "Pr", Guid.NewGuid(), Guid.NewGuid(), null, null, "newTitle", null, _createdByGuid, DateTime.UtcNow, _lastUpdated, null, "delete");
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        await _attachmentRepositoryMock.Received(1).RemoveByGuidAsync(bEvent.AttachmentGuid, Arg.Any<CancellationToken>());
        await _linkRepositoryMock.Received(1).RemoveByGuidAsync(bEvent.AttachmentGuid, Arg.Any<CancellationToken>());
        await _attachmentRepositoryMock.Received(0).ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _attachmentRepositoryMock.Received(0).GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _attachmentRepositoryMock.Received(0).Add(Arg.Any<Attachment>());
        await _linkRepositoryMock.Received(0).ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _linkRepositoryMock.Received(0).GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _linkRepositoryMock.Received(0).Add(Arg.Any<Link>());
        await _unitOfWorkMock.Received(1).SaveChangesFromSyncAsync();
    }
}

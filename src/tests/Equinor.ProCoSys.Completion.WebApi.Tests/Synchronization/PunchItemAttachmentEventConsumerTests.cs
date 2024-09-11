using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
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
    private readonly IPersonRepository _personRepoMock = Substitute.For<IPersonRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private PunchItemAttachmentEventConsumer _dut = null!;
    private readonly ConsumeContext<PunchItemAttachmentEvent> _contextMock = Substitute.For<ConsumeContext<PunchItemAttachmentEvent>>();
    private Attachment? _attachmentAddedToRepository;
    private readonly DateTime _lastUpdated = DateTime.Now;
    private readonly string _lastUpdatedByUser = "someone";
    private readonly IPunchItemRepository _punchItemRepositoryMock = Substitute.For<IPunchItemRepository>();
    private PunchItem _punchItem = null!;

    [TestInitialize]
    public void Setup()
    {
        _dut = new PunchItemAttachmentEventConsumer(
            Substitute.For<ILogger<PunchItemAttachmentEventConsumer>>(),
            _personRepoMock,
            _punchItemRepositoryMock,
            _attachmentRepositoryMock,
            _unitOfWorkMock);

        _attachmentRepositoryMock
            .When(x => x.Add(Arg.Any<Attachment>()))
            .Do(callInfo =>
            {
                _attachmentAddedToRepository = callInfo.Arg<Attachment>();
            });

        var testPlant = "P";
        var org = new LibraryItem(testPlant, Guid.NewGuid(), "C", "D", LibraryType.COMPLETION_ORGANIZATION);
        _punchItem = new PunchItem(
            testPlant,
            new Project(testPlant, Guid.NewGuid(), "N", "D"),
            Guid.NewGuid(),
            Category.PA,
            "Description",
            org,
            org);


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
        Assert.AreEqual(oldProCoSys4LastUpdated, existingAttachment.ProCoSys4LastUpdated);
        Assert.AreEqual(oldProCoSys4LastUpdatedByUser, existingAttachment.ProCoSys4LastUpdatedByUser);
        await _unitOfWorkMock.Received(0).SaveChangesFromSyncAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldAddLinkToPunchDescription()
    {
        //Arrange
        var bEvent =
            new PunchItemAttachmentEvent("Pl", "Pr", Guid.NewGuid(), Guid.NewGuid(), null, "linkUri", null!, null, _createdByGuid, DateTime.UtcNow, _lastUpdated, _lastUpdatedByUser, null);
        _punchItemRepositoryMock.GetAsync(bEvent.PunchItemGuid, Arg.Any<CancellationToken>()).Returns(_punchItem);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNull(_attachmentAddedToRepository);
        Assert.IsTrue(_punchItem.Description.Contains($"\n\nLink imported from old ProCoSys punch item: {bEvent.Uri}"));
        await _unitOfWorkMock.Received(1).SaveChangesFromSyncAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldAddLinkOnceToPunchDescription()
    {
        //Arrange
        var bEvent =
            new PunchItemAttachmentEvent("Pl", "Pr", Guid.NewGuid(), Guid.NewGuid(), null, "linkUri", null!, null, _createdByGuid, DateTime.UtcNow, _lastUpdated, _lastUpdatedByUser, null);
        _punchItemRepositoryMock.GetAsync(bEvent.PunchItemGuid, Arg.Any<CancellationToken>()).Returns(_punchItem);
        _contextMock.Message.Returns(bEvent);
        await _dut.Consume(_contextMock);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNull(_attachmentAddedToRepository);
        var value = $"\n\nLink imported from old ProCoSys punch item: {bEvent.Uri}";
        var parts = _punchItem.Description.Split(value);
        Assert.IsTrue(parts.Length == 2);
    }
}

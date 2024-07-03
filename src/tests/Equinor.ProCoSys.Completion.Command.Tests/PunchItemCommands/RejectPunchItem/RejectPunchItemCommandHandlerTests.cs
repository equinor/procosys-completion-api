using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Email;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.RejectPunchItem;

[TestClass]
public class RejectPunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private readonly string _testPlant = TestPlantA;
    private RejectPunchItemCommand _command;
    private RejectPunchItemCommandHandler _dut;
    private ILabelRepository _labelRepositoryMock;
    private ICommentRepository _commentRepositoryMock;
    private IDeepLinkUtility _deepLinkUtilityMock;
    private ICompletionMailService _completionMailServiceMock;
    private IOptionsMonitor<ApplicationOptions> _optionsMock;
    private Label _rejectedLabel;
    private List<Person> _personList;

    [TestInitialize]
    public void Setup()
    {
        _existingPunchItem[_testPlant].Clear(_currentPerson);

        var person = new Person(Guid.NewGuid(), null!, null!, null!, "p1@pcs.no", false);
        _personList = [person];

        _command = new RejectPunchItemCommand(
            _existingPunchItem[_testPlant].Guid,
            Guid.NewGuid().ToString(),
            new List<Guid> { person.Guid },
            RowVersion);

        const string rejectLabelText = "Reject";
        _labelRepositoryMock = Substitute.For<ILabelRepository>();
        _rejectedLabel = new Label(rejectLabelText);
        _labelRepositoryMock.GetByTextAsync(rejectLabelText, default).Returns(_rejectedLabel);

        _commentRepositoryMock = Substitute.For<ICommentRepository>();

        _completionMailServiceMock = Substitute.For<ICompletionMailService>();

        _deepLinkUtilityMock = Substitute.For<IDeepLinkUtility>();

        _optionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
        _optionsMock.CurrentValue.Returns(
            new ApplicationOptions
            {
                RejectLabel = rejectLabelText
            });

        _personRepositoryMock.GetOrCreateManyAsync(_command.Mentions, default)
            .Returns(_personList);

        _dut = new RejectPunchItemCommandHandler(
            _punchItemRepositoryMock,
            _labelRepositoryMock,
            _commentRepositoryMock,
            _personRepositoryMock,
            _syncToPCS4ServiceMock,
            _completionMailServiceMock,
            _deepLinkUtilityMock,
            _messageProducerMock,
            _unitOfWorkMock,
            _checkListApiServiceMock,
            Substitute.For<ILogger<RejectPunchItemCommandHandler>>(),
            _optionsMock);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldRejectPunchItem()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_utcNow, _existingPunchItem[_testPlant].RejectedAtUtc);
        Assert.AreEqual(_currentPerson.Id, _existingPunchItem[_testPlant].RejectedById);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAuditData()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SetAuditDataAsync();
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAndReturnRowVersion()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_command.RowVersion, result.Data);
        Assert.AreEqual(_command.RowVersion, _existingPunchItem[_testPlant].RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddComment_WithCorrectLabel()
    {
        // Arrange 
        Comment addedComment = null!;
        _commentRepositoryMock
            .When(x => x.Add(Arg.Any<Comment>()))
            .Do(info =>
            {
                addedComment = info.Arg<Comment>();
            });

        // Act
        await _dut.Handle(_command, default);

        // Assert
        _commentRepositoryMock.Received(1)
            .Add(Arg.Any<Comment>());
        Assert.IsNotNull(addedComment);
        Assert.AreEqual(1, addedComment.Labels.Count);
        Assert.AreEqual(_rejectedLabel, addedComment.Labels.ElementAt(0));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldPublishPunchItemUpdatedIntegrationEvent()
    {
        // Arrange
        PunchItemUpdatedIntegrationEvent integrationEvent = null!;
        _messageProducerMock
            .When(x => x.PublishAsync(Arg.Any<PunchItemUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<PunchItemUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItem = _existingPunchItem[_testPlant];
        Assert.IsNotNull(integrationEvent);
        AssertNotCleared(integrationEvent);
        AssertIsRejected(punchItem, punchItem.RejectedBy, integrationEvent);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSendHistoryUpdatedIntegrationEvent()
    {
        // Arrange
        HistoryUpdatedIntegrationEvent historyEvent = null!;
        _messageProducerMock
            .When(x => x.SendHistoryAsync(Arg.Any<HistoryUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItem = _existingPunchItem[_testPlant];
        AssertHistoryUpdatedIntegrationEvent(
            historyEvent,
            punchItem.Plant,
            "Punch item rejected",
            punchItem,
            punchItem);
        Assert.IsNotNull(historyEvent.ChangedProperties);
        Assert.AreEqual(1, historyEvent.ChangedProperties.Count);
        var changedProperty = historyEvent.ChangedProperties[0];
        Assert.AreEqual(RejectPunchItemCommandHandler.RejectReasonPropertyName, changedProperty.Name);
        Assert.IsNull(changedProperty.OldValue);
        Assert.AreEqual(_command.Comment, changedProperty.Value);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSendEmailToCorrectEmails()
    {
        // Arrange
        List<string> emailSentTo = null;
        _completionMailServiceMock
            .When(x => x.SendEmailAsync(
                Arg.Any<string>(),
                Arg.Any<dynamic>(),
                Arg.Any<List<string>>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                emailSentTo = callInfo.ArgAt<List<string>>(2);
            });

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _completionMailServiceMock.Received(1)
            .SendEmailAsync(
                MailTemplateCode.PunchRejected,
                Arg.Any<dynamic>(),
                Arg.Any<List<string>>(),
                Arg.Any<CancellationToken>());
        Assert.AreEqual(1, emailSentTo.Count);
        Assert.AreEqual(_personList.ElementAt(0).Email, emailSentTo.ElementAt(0));
    }

    #region Unit Tests which can be removed when no longer sync to pcs4
    [TestMethod]
    public async Task HandlingCommand_ShouldRecalculateChecklist()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItem = _existingPunchItem[_testPlant];
        await _checkListApiServiceMock.Received(1).RecalculateCheckListStatus(_testPlant, punchItem.CheckListGuid, default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSyncWithPcs4()
    {
        // Arrange
        PunchItemUpdatedIntegrationEvent integrationEvent = null!;
        _messageProducerMock
            .When(x => x.PublishAsync(
                Arg.Any<PunchItemUpdatedIntegrationEvent>(),
                default))
            .Do(info =>
            {
                integrationEvent = info.Arg<PunchItemUpdatedIntegrationEvent>();
            });

        // Act
        await _dut.Handle(_command, default);

        // Assert
        //await _syncToPCS4ServiceMock.Received(1)
        await _syncToPCS4ServiceMock.Received(1).SyncPunchListItemUpdateAsync(integrationEvent, default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotSyncWithPcs4_WhenSavingChangesFails()
    {
        // Arrange
        _unitOfWorkMock.When(x => x.SaveChangesAsync())
            .Do(_ => throw new Exception("SaveChangesAsync error"));

        // Act
        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _dut.Handle(_command, default);
        });

        // Assert
        await _syncToPCS4ServiceMock.DidNotReceive().SyncPunchListItemUpdateAsync(Arg.Any<object>(), default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSyncCommentWithPcs4()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _syncToPCS4ServiceMock.Received(1).SyncNewCommentAsync(Arg.Any<object>(), default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotSyncCommentWithPcs4_WhenSavingChangesFails()
    {
        // Arrange
        _unitOfWorkMock.When(x => x.SaveChangesAsync())
            .Do(_ => throw new Exception("SaveChangesAsync error"));

        // Act
        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _dut.Handle(_command, default);
        });

        // Assert
        await _syncToPCS4ServiceMock.DidNotReceive().SyncNewCommentAsync(Arg.Any<object>(), default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotSyncCommentWithPcs4_WhenSyncingWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncPunchListItemUpdateAsync(Arg.Any<object>(), default))
            .Do(_ => throw new Exception("SyncPunchListItemUpdateAsync error"));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _syncToPCS4ServiceMock.DidNotReceive().SyncNewCommentAsync(Arg.Any<object>(), default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotRecalculate_WhenSavingChangesFails()
    {
        // Arrange
        _unitOfWorkMock.When(x => x.SaveChangesAsync())
            .Do(_ => throw new Exception("SaveChangesAsync error"));

        // Act
        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _dut.Handle(_command, default);
        });

        // Assert
        await _checkListApiServiceMock.DidNotReceive().RecalculateCheckListStatus(Arg.Any<string>(), Arg.Any<Guid>(), default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotRecalculate_WhenSyncingWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncPunchListItemUpdateAsync(Arg.Any<object>(), default))
            .Do(_ => throw new Exception("SyncPunchListItemUpdateAsync error"));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _checkListApiServiceMock.DidNotReceive().RecalculateCheckListStatus(Arg.Any<string>(), Arg.Any<Guid>(), default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotRecalculate_WhenSyncingCommentWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncNewCommentAsync(Arg.Any<object>(), default))
            .Do(_ => throw new Exception("SyncNewCommentAsync error"));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _checkListApiServiceMock.DidNotReceive().RecalculateCheckListStatus(Arg.Any<string>(), Arg.Any<Guid>(), default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotThrowError_WhenSyncingWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncPunchListItemUpdateAsync(Arg.Any<object>(), default))
            .Do(_ => throw new Exception("SyncPunchListItemUpdateAsync error"));

        // Act and Assert
        try
        {
            await _dut.Handle(_command, default);
        }
        catch (Exception ex)
        {
            Assert.Fail("Excepted no exception, but got: " + ex.Message);
        }
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotThrowError_WhenSyncingCommentWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncNewCommentAsync(Arg.Any<object>(), default))
            .Do(_ => throw new Exception("SyncNewCommentAsync error"));

        // Act and Assert
        try
        {
            await _dut.Handle(_command, default);
        }
        catch (Exception ex)
        {
            Assert.Fail("Excepted no exception, but got: " + ex.Message);
        }
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotThrowError_WhenRecalculatingFails()
    {
        // Arrange
        _checkListApiServiceMock.When(x => x.RecalculateCheckListStatus(Arg.Any<string>(), Arg.Any<Guid>(), default))
            .Do(_ => throw new Exception("RecalculateCheckListStatus error"));

        // Act and Assert
        try
        {
            await _dut.Handle(_command, default);
        }
        catch (Exception ex)
        {
            Assert.Fail("Excepted no exception, but got: " + ex.Message);
        }
    }
    #endregion
}

﻿using System;
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
public class RejectPunchItemCommandHandlerTests : PunchItemCommandTestsBase
{
    private const string RejectLabelText = "Reject";
    private RejectPunchItemCommand _command;
    private RejectPunchItemCommandHandler _dut;
    private ILabelRepository _labelRepositoryMock;
    private ICommentRepository _commentRepositoryMock;
    private IDeepLinkUtility _deepLinkUtilityMock;
    private ICompletionMailService _completionMailServiceMock;
    private IOptionsMonitor<ApplicationOptions> _optionsMock;
    private Label _rejectedLabel;
    private List<Person> _personList;
    private Comment _commentAddedToRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        var person = new Person(Guid.NewGuid(), null!, null!, null!, "p1@pcs.no", false);
        _personList = [person];

        _command = new RejectPunchItemCommand(
            _existingPunchItem[TestPlantA].Guid,
            Guid.NewGuid().ToString(),
            new List<Guid> { person.Guid },
            RowVersion)
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };

        _command.PunchItem.Clear(_currentPerson);

        _labelRepositoryMock = Substitute.For<ILabelRepository>();
        _rejectedLabel = new Label(RejectLabelText);
        _labelRepositoryMock.GetByTextAsync(RejectLabelText, default).Returns(_rejectedLabel);

        _commentRepositoryMock = Substitute.For<ICommentRepository>();
        _commentRepositoryMock
            .When(x => x.Add(Arg.Any<Comment>()))
            .Do(info =>
            {
                _commentAddedToRepository = info.Arg<Comment>();
                _commentAddedToRepository.SetCreated(_person);
            });

        _completionMailServiceMock = Substitute.For<ICompletionMailService>();

        _deepLinkUtilityMock = Substitute.For<IDeepLinkUtility>();

        _optionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
        _optionsMock.CurrentValue.Returns(
            new ApplicationOptions
            {
                RejectLabel = RejectLabelText
            });

        _personRepositoryMock.GetOrCreateManyAsync(_command.Mentions, default)
            .Returns(_personList);

        _dut = new RejectPunchItemCommandHandler(
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
        // Arrange
        Assert.IsTrue(_command.PunchItem.IsCleared);
        Assert.IsFalse(_command.PunchItem.IsRejected);
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsFalse(_command.PunchItem.IsCleared);
        Assert.IsTrue(_command.PunchItem.IsRejected);
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
        Assert.AreEqual(_command.RowVersion, result);
        Assert.AreEqual(_command.RowVersion, _command.PunchItem.RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddComment_WithCorrectLabel()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        _commentRepositoryMock.Received(1)
            .Add(Arg.Any<Comment>());
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(1, _commentAddedToRepository.Labels.Count);
        Assert.AreEqual(_rejectedLabel, _commentAddedToRepository.Labels.ElementAt(0));
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
        Assert.IsNotNull(integrationEvent);
        AssertNotCleared(integrationEvent);
        AssertIsRejected(_command.PunchItem, _command.PunchItem.RejectedBy, integrationEvent);
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
        AssertHistoryUpdatedIntegrationEvent(
            historyEvent,
            _command.PunchItem.Plant,
            "Punch item rejected",
            _command.PunchItem,
            _command.PunchItem);
        Assert.IsNotNull(historyEvent.ChangedProperties);
        Assert.AreEqual(1, historyEvent.ChangedProperties.Count);
        var changedProperty = historyEvent.ChangedProperties[0];
        Assert.AreEqual(RejectPunchItemCommandHandler.RejectReasonPropertyName, changedProperty.Name);
        Assert.IsNull(changedProperty.OldValue);
        Assert.AreEqual(_command.Comment, changedProperty.Value);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSendEmailEventForCorrectEmails()
    {
        // Arrange
        List<string> emailSentTo = null;
        _completionMailServiceMock
            .When(x => x.SendEmailEventAsync(
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
            .SendEmailEventAsync(
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
        await _checkListApiServiceMock.Received(1).RecalculateCheckListStatusAsync(TestPlantA, _command.PunchItem.CheckListGuid, default);
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
        await _checkListApiServiceMock.DidNotReceive().RecalculateCheckListStatusAsync(Arg.Any<string>(), Arg.Any<Guid>(), default);
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
        await _checkListApiServiceMock.DidNotReceive().RecalculateCheckListStatusAsync(Arg.Any<string>(), Arg.Any<Guid>(), default);
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
        await _checkListApiServiceMock.DidNotReceive().RecalculateCheckListStatusAsync(Arg.Any<string>(), Arg.Any<Guid>(), default);
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
        _checkListApiServiceMock.When(x => x.RecalculateCheckListStatusAsync(Arg.Any<string>(), Arg.Any<Guid>(), default))
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

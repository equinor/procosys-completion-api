﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Comments;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
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
    private ICommentService _commentServiceMock;
    private IOptionsMonitor<ApplicationOptions> _optionsMock;
    private Label _rejectedLabel;

    [TestInitialize]
    public void Setup()
    {
        _existingPunchItem[_testPlant].Clear(_currentPerson);

        _command = new RejectPunchItemCommand(_existingPunchItem[_testPlant].Guid, Guid.NewGuid().ToString(), RowVersion);

        var rejectLabelText = "Reject";
        _labelRepositoryMock = Substitute.For<ILabelRepository>();
        _rejectedLabel = new Label(rejectLabelText);
        _labelRepositoryMock.GetByTextAsync(rejectLabelText, default).Returns(_rejectedLabel);

        _commentServiceMock = Substitute.For<ICommentService>();

        _optionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
        _optionsMock.CurrentValue.Returns(
            new ApplicationOptions
            {
                RejectLabel = rejectLabelText
            });

        _dut = new RejectPunchItemCommandHandler(
            _punchItemRepositoryMock,
            _labelRepositoryMock,
            _commentServiceMock,
            _personRepositoryMock,
            _unitOfWorkMock,
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
    public async Task HandlingCommand_ShouldSave()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
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
    public async Task HandlingCommand_ShouldAddPunchItemRejectedDomainEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(_existingPunchItem[_testPlant].DomainEvents.Last(), typeof(PunchItemRejectedDomainEvent));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddRejectCommentToPunchItemRejectedEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItemRejectedDomainEventAdded = _existingPunchItem[_testPlant].DomainEvents.Last() as PunchItemRejectedDomainEvent;
        Assert.IsNotNull(punchItemRejectedDomainEventAdded);
        Assert.IsNotNull(punchItemRejectedDomainEventAdded.Changes);
        Assert.AreEqual(1, punchItemRejectedDomainEventAdded.Changes.Count);

        var change = punchItemRejectedDomainEventAdded
            .Changes
            .SingleOrDefault(c => c.Name == RejectPunchItemCommandHandler.RejectReasonPropertyName);
        Assert.IsNotNull(change);
        Assert.IsNull(change.OldValue);
        Assert.AreEqual(_command.Comment, change.NewValue);
    }

    [TestMethod]
    public async Task HandlingCommand_Should_CallAdd_OnCommentService()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _commentServiceMock.Received(1)
            .AddAsync(
                nameof(PunchItem),
                _command.PunchItemGuid,
                _command.Comment,
                _rejectedLabel,
                default);
    }
}

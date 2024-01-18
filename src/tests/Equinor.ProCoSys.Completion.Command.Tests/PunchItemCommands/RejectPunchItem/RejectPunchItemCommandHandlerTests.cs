using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Comments;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;
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
    private List<Person> _personList;

    [TestInitialize]
    public void Setup()
    {
        _existingPunchItem[_testPlant].Clear(_currentPerson);

        var person = new Person(Guid.NewGuid(), null!, null!, null!, null!, false);
        _personList = [person];

        _command = new RejectPunchItemCommand(
            _existingPunchItem[_testPlant].Guid,
            Guid.NewGuid().ToString(),
            new List<Guid> { person.Guid },
            RowVersion);

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

        _personRepositoryMock.GetOrCreateManyAsync(_command.Mentions, default)
            .Returns(_personList);

        _dut = new RejectPunchItemCommandHandler(
            _punchItemRepositoryMock,
            _labelRepositoryMock,
            _commentServiceMock,
            _personRepositoryMock,
            _syncToPCS4ServiceMock,
            _unitOfWorkMock,
            _punchEventPublisherMock,
            _historyEventPublisherMock,
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
    public async Task HandlingCommand_Should_CallAdd_OnCommentService()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        _commentServiceMock.Received(1)
            .Add(
                nameof(PunchItem),
                _command.PunchItemGuid,
                _command.Comment,
                Arg.Any<IEnumerable<Label>>(),
                Arg.Any<IEnumerable<Person>>());
    }

    [TestMethod]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task HandlingCommand_Should_CallAdd_OnCommentServiceWithCorrectLabel()
    {
        // Arrange 
        IEnumerable<Label> labelsAdded = null!;
        _commentServiceMock
            .When(x => x.Add(
                Arg.Any<string>(),
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<IEnumerable<Label>>(),
                Arg.Any<IEnumerable<Person>>()))
            .Do(info => labelsAdded = info.Arg<IEnumerable<Label>>());

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNotNull(labelsAdded);
        Assert.AreEqual(1, labelsAdded.Count());
        Assert.AreEqual(_rejectedLabel, labelsAdded.ElementAt(0));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldPublishUpdatedPunchEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _punchEventPublisherMock.Received(1).PublishUpdatedEventAsync(_existingPunchItem[_testPlant], default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldPublishUpdateToHistory()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _historyEventPublisherMock.Received(1).PublishUpdatedEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<User>(),
            Arg.Any<DateTime>(),
            Arg.Any<List<IChangedProperty>>(),
            default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldPublishCorrectHistoryEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_existingPunchItem[_testPlant].Plant, _plantPublishedToHistory);
        Assert.AreEqual("Punch item rejected", _displayNamePublishedToHistory);
        Assert.AreEqual(_existingPunchItem[_testPlant].Guid, _guidPublishedToHistory);
        Assert.IsNotNull(_userPublishedToHistory);
        Assert.AreEqual(_existingPunchItem[_testPlant].ModifiedBy!.Guid, _userPublishedToHistory.Oid);
        Assert.AreEqual(_existingPunchItem[_testPlant].ModifiedBy!.GetFullName(), _userPublishedToHistory.FullName);
        Assert.AreEqual(_existingPunchItem[_testPlant].ModifiedAtUtc, _dateTimePublishedToHistory);
        Assert.IsNotNull(_changedPropertiesPublishedToHistory);
        Assert.AreEqual(1, _changedPropertiesPublishedToHistory.Count);
        var changedProperty = _changedPropertiesPublishedToHistory[0];
        Assert.AreEqual(RejectPunchItemCommandHandler.RejectReasonPropertyName, changedProperty.Name);
        Assert.IsNull(changedProperty.OldValue);
        Assert.AreEqual(_command.Comment, changedProperty.NewValue);
    }

    #region Unit Tests which can be removed when no longer sync to pcs4
    [TestMethod]
    public async Task HandlingCommand_ShouldSyncWithPcs4()
    {
        // Arrange
        var integrationEvent = Substitute.For<IPunchItemUpdatedV1>();
        _punchEventPublisherMock
            .PublishUpdatedEventAsync(_existingPunchItem[_testPlant], default)
            .Returns(integrationEvent);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _syncToPCS4ServiceMock.Received(1).SyncObjectUpdateAsync(SyncToPCS4Service.PunchItem, integrationEvent, _testPlant, default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldBeginTransaction()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).BeginTransactionAsync(default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldCommitTransaction_WhenNoExceptions()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).CommitTransactionAsync(default);
        await _unitOfWorkMock.Received(0).RollbackTransactionAsync(default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldRollbackTransaction_WhenExceptionThrown()
    {
        // Arrange
        _unitOfWorkMock
            .When(u => u.SaveChangesAsync())
            .Do(_ => throw new Exception());

        // Act
        var exception = await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));

        // Assert
        await _unitOfWorkMock.Received(0).CommitTransactionAsync(default);
        await _unitOfWorkMock.Received(1).RollbackTransactionAsync(default);
        Assert.IsInstanceOfType(exception, typeof(Exception));
    }
    #endregion
}

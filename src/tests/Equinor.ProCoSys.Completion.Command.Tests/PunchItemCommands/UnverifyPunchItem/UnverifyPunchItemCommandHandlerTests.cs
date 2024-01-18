using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UnverifyPunchItem;

[TestClass]
public class UnverifyPunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private readonly string _testPlant = TestPlantA;
    private UnverifyPunchItemCommand _command;
    private UnverifyPunchItemCommandHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        _existingPunchItem[_testPlant].Clear(_currentPerson);
        _existingPunchItem[_testPlant].Verify(_currentPerson);

        _command = new UnverifyPunchItemCommand(_existingPunchItem[_testPlant].Guid, RowVersion);

        _dut = new UnverifyPunchItemCommandHandler(
            _punchItemRepositoryMock,
            _syncToPCS4ServiceMock,
            _unitOfWorkMock,
            _punchEventPublisherMock,
            _historyEventPublisherMock,
            Substitute.For<ILogger<UnverifyPunchItemCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUnverifyPunchItem()
    {
        // Assert
        Assert.IsTrue(_existingPunchItem[_testPlant].VerifiedAtUtc.HasValue);
        Assert.IsTrue(_existingPunchItem[_testPlant].VerifiedById.HasValue);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsFalse(_existingPunchItem[_testPlant].VerifiedAtUtc.HasValue);
        Assert.IsFalse(_existingPunchItem[_testPlant].VerifiedById.HasValue);
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
        Assert.AreEqual("Punch item unverified", _displayNamePublishedToHistory);
        Assert.AreEqual(_existingPunchItem[_testPlant].Guid, _guidPublishedToHistory);
        Assert.IsNotNull(_userPublishedToHistory);
        Assert.AreEqual(_existingPunchItem[_testPlant].ModifiedBy!.Guid, _userPublishedToHistory.Oid);
        Assert.AreEqual(_existingPunchItem[_testPlant].ModifiedBy!.GetFullName(), _userPublishedToHistory.FullName);
        Assert.AreEqual(_existingPunchItem[_testPlant].ModifiedAtUtc, _dateTimePublishedToHistory);
        Assert.IsNotNull(_changedPropertiesPublishedToHistory);
        Assert.AreEqual(0, _changedPropertiesPublishedToHistory.Count);
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

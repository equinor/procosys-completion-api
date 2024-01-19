using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.VerifyPunchItem;

[TestClass]
public class VerifyPunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private readonly string _testPlant = TestPlantA;
    private VerifyPunchItemCommand _command;
    private VerifyPunchItemCommandHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        _existingPunchItem[_testPlant].Clear(_currentPerson);

        _command = new VerifyPunchItemCommand(_existingPunchItem[_testPlant].Guid, RowVersion);

        _dut = new VerifyPunchItemCommandHandler(
            _punchItemRepositoryMock,
            _personRepositoryMock,
            _syncToPCS4ServiceMock,
            _unitOfWorkMock,
            _punchEventPublisherMock,
            _historyEventPublisherMock,
             Substitute.For<ILogger<VerifyPunchItemCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldVerifyPunchItem()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_utcNow, _existingPunchItem[_testPlant].VerifiedAtUtc);
        Assert.AreEqual(_currentPerson.Id, _existingPunchItem[_testPlant].VerifiedById);
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
        await _syncToPCS4ServiceMock.Received(1).SyncObjectUpdateAsync("PunchItem", integrationEvent, _testPlant, default);
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
        Assert.AreEqual("Punch item verified", _displayNamePublishedToHistory);
        Assert.AreEqual(_existingPunchItem[_testPlant].Guid, _guidPublishedToHistory);
        Assert.IsNotNull(_userPublishedToHistory);
        Assert.AreEqual(_existingPunchItem[_testPlant].ModifiedBy!.Guid, _userPublishedToHistory.Oid);
        Assert.AreEqual(_existingPunchItem[_testPlant].ModifiedBy!.GetFullName(), _userPublishedToHistory.FullName);
        Assert.AreEqual(_existingPunchItem[_testPlant].ModifiedAtUtc, _dateTimePublishedToHistory);
        Assert.IsNotNull(_changedPropertiesPublishedToHistory);
        Assert.AreEqual(0, _changedPropertiesPublishedToHistory.Count);
    }
}

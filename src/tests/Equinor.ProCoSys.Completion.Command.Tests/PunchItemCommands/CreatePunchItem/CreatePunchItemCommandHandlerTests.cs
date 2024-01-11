using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItem;

[TestClass]
public class CreatePunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private readonly string _testPlant = TestPlantA;
    private IPersonCache _personCacheMock;
    private readonly Guid _existingCheckListGuid = Guid.NewGuid();
    private PunchItem _punchItemAddedToRepository;
    private Person _personAddedToRepository;

    private CreatePunchItemCommandHandler _dut;
    private CreatePunchItemCommand _command;

    [TestInitialize]
    public void Setup()
    {
        _punchItemRepositoryMock
            .When(x => x.Add(Arg.Any<PunchItem>()))
            .Do(callInfo =>
            {
                _punchItemAddedToRepository = callInfo.Arg<PunchItem>();
            });

        _personRepositoryMock
            .When(x => x.Add(Arg.Any<Person>()))
            .Do(callInfo =>
            {
                _personAddedToRepository = callInfo.Arg<Person>();
            });

        _personCacheMock = Substitute.For<IPersonCache>();

        _unitOfWorkMock
            .When(x => x.SetAuditDataAsync())
            .Do(_ =>
            {
                _punchItemAddedToRepository.SetCreated(_currentPerson);
            });

        _command = new CreatePunchItemCommand(
            Category.PA,
            "P123",
            _existingProject[_testPlant].Guid,
            _existingCheckListGuid,
            _existingRaisedByOrg1[_testPlant].Guid,
            _existingClearingByOrg1[_testPlant].Guid,
            _existingPerson1.Guid,
            DateTime.UtcNow,
            _existingPriority1[_testPlant].Guid,
            _existingSorting1[_testPlant].Guid,
            _existingType1[_testPlant].Guid,
            100,
            _existingWorkOrder1[_testPlant].Guid,
            _existingWorkOrder2[_testPlant].Guid,
            _existingSWCR1[_testPlant].Guid,
            _existingDocument1[_testPlant].Guid,
            "123",
            true,
            DateTime.UtcNow,
            "123.1");

        _dut = new CreatePunchItemCommandHandler(
            _plantProviderMock,
            _punchItemRepositoryMock,
            _libraryItemRepositoryMock,
            _projectRepositoryMock,
            _personRepositoryMock,
            _personCacheMock,
            _workOrderRepositoryMock,
            _swcrRepositoryMock,
            _documentRepositoryMock,
            _syncToPCS4ServiceMock,
            _unitOfWorkMock,
            _punchEventPublisherMock,
            _historyEventPublisherMock,
            Substitute.For<ILogger<CreatePunchItemCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldReturn_GuidAndRowVersion()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(result.Data, typeof(GuidAndRowVersion));
    }

    [TestMethod]
    public async Task HandlingCommand_WithAllValues_ShouldAddCorrectPunchItem_ToPunchItemRepository()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNotNull(_punchItemAddedToRepository);
        Assert.AreEqual(_command.Description, _punchItemAddedToRepository.Description);
        Assert.AreEqual(_existingProject[_testPlant].Id, _punchItemAddedToRepository.ProjectId);
        Assert.AreEqual(_existingRaisedByOrg1[_testPlant].Id, _punchItemAddedToRepository.RaisedByOrgId);
        Assert.AreEqual(_existingClearingByOrg1[_testPlant].Id, _punchItemAddedToRepository.ClearingByOrgId);
        Assert.AreEqual(_existingPerson1.Id, _punchItemAddedToRepository.ActionById);
        Assert.AreEqual(_command.DueTimeUtc, _punchItemAddedToRepository.DueTimeUtc);
        Assert.AreEqual(_existingPriority1[_testPlant].Id, _punchItemAddedToRepository.PriorityId);
        Assert.AreEqual(_existingSorting1[_testPlant].Id, _punchItemAddedToRepository.SortingId);
        Assert.AreEqual(_existingType1[_testPlant].Id, _punchItemAddedToRepository.TypeId);
        Assert.AreEqual(_command.Estimate, _punchItemAddedToRepository.Estimate);
        Assert.AreEqual(_existingWorkOrder1[_testPlant].Id, _punchItemAddedToRepository.OriginalWorkOrderId);
        Assert.AreEqual(_existingWorkOrder2[_testPlant].Id, _punchItemAddedToRepository.WorkOrderId);
        Assert.AreEqual(_existingSWCR1[_testPlant].Id, _punchItemAddedToRepository.SWCRId);
        Assert.AreEqual(_existingDocument1[_testPlant].Id, _punchItemAddedToRepository.DocumentId);
        Assert.AreEqual(_command.ExternalItemNo, _punchItemAddedToRepository.ExternalItemNo);
        Assert.AreEqual(_command.MaterialRequired, _punchItemAddedToRepository.MaterialRequired);
        Assert.AreEqual(_command.MaterialETAUtc, _punchItemAddedToRepository.MaterialETAUtc);
        Assert.AreEqual(_command.MaterialExternalNo, _punchItemAddedToRepository.MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_WithRequiredValues_ShouldAddCorrectPunchItem_ToPunchItemRepository()
    {
        // Arrange
        var command = new CreatePunchItemCommand(
            Category.PA,
            "P123",
            _existingProject[_testPlant].Guid,
            _existingCheckListGuid,
            _existingRaisedByOrg1[_testPlant].Guid,
            _existingClearingByOrg1[_testPlant].Guid,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            null);

        // Act
        await _dut.Handle(command, default);

        // Assert
        Assert.IsNotNull(_punchItemAddedToRepository);
        Assert.AreEqual(_command.Description, _punchItemAddedToRepository.Description);
        Assert.AreEqual(_existingProject[_testPlant].Id, _punchItemAddedToRepository.ProjectId);
        Assert.AreEqual(_existingRaisedByOrg1[_testPlant].Id, _punchItemAddedToRepository.RaisedByOrgId);
        Assert.AreEqual(_existingClearingByOrg1[_testPlant].Id, _punchItemAddedToRepository.ClearingByOrgId);
        Assert.IsFalse(_punchItemAddedToRepository.ActionById.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.DueTimeUtc.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.PriorityId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.SortingId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.TypeId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.Estimate.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.OriginalWorkOrderId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.WorkOrderId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.SWCRId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.DocumentId.HasValue);
        Assert.IsNull(_punchItemAddedToRepository.ExternalItemNo);
        Assert.IsFalse(_punchItemAddedToRepository.MaterialRequired);
        Assert.IsFalse(_punchItemAddedToRepository.MaterialETAUtc.HasValue);
        Assert.IsNull(_punchItemAddedToRepository.MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_WithNonExistingActionByPerson_ShouldAddActionByPerson_ToPersonRepository()
    {
        // Arrange
        var nonExistingPersonOid = Guid.NewGuid();
        var command = new CreatePunchItemCommand(
            Category.PA,
            "P123",
            _existingProject[_testPlant].Guid,
            _existingCheckListGuid,
            _existingRaisedByOrg1[_testPlant].Guid,
            _existingClearingByOrg1[_testPlant].Guid,
            nonExistingPersonOid,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            null);
        var proCoSysPerson = new ProCoSysPerson
        {
            UserName = "YODA",
            FirstName = "YO",
            LastName = "DA",
            Email = "@",
            AzureOid = nonExistingPersonOid.ToString(),
            Super = true
        };
        _personCacheMock.GetAsync(nonExistingPersonOid)
            .Returns(proCoSysPerson);

        // Act
        await _dut.Handle(command, default);

        // Assert
        Assert.IsNotNull(_punchItemAddedToRepository);
        Assert.IsNotNull(_personAddedToRepository);
        Assert.IsNotNull(_punchItemAddedToRepository.ActionBy);
        Assert.AreEqual(nonExistingPersonOid, _punchItemAddedToRepository.ActionBy!.Guid);
        Assert.AreEqual(nonExistingPersonOid, _personAddedToRepository.Guid);
        Assert.AreEqual(proCoSysPerson.AzureOid, _personAddedToRepository.Guid.ToString());
        Assert.AreEqual(proCoSysPerson.UserName, _personAddedToRepository.UserName);
        Assert.AreEqual(proCoSysPerson.FirstName, _personAddedToRepository.FirstName);
        Assert.AreEqual(proCoSysPerson.LastName, _personAddedToRepository.LastName);
        Assert.AreEqual(proCoSysPerson.Email, _personAddedToRepository.Email);
        Assert.AreEqual(proCoSysPerson.Super, _personAddedToRepository.Superuser);
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
    public async Task HandlingCommand_ShouldSyncWithPcs4()
    {
        // Arrange
        var integrationEvent = Substitute.For<IPunchItemCreatedV1>();
        _punchEventPublisherMock
            .PublishCreatedEventAsync(Arg.Any<PunchItem>(), default)
            .Returns(integrationEvent);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _syncToPCS4ServiceMock.Received(1).SyncNewObjectAsync("PunchItem", integrationEvent, _testPlant);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldPublishCreatedEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _punchEventPublisherMock.Received(1).PublishCreatedEventAsync(_punchItemAddedToRepository, default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldPublishCreateToHistory()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _historyEventPublisherMock.Received(1).PublishCreatedEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<Guid?>(),
            Arg.Any<User>(),
            Arg.Any<DateTime>(),
            Arg.Any<List<INewProperty>>(),
            default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldPublishCorrectHistoryEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_punchItemAddedToRepository.Plant, _plantPublishedToHistory);
        Assert.AreEqual("Punch item created", _displayNamePublishedToHistory);
        Assert.AreEqual(_punchItemAddedToRepository.Guid, _guidPublishedToHistory);
        Assert.AreEqual(_punchItemAddedToRepository.CheckListGuid, _parentGuidPublishedToHistory);
        Assert.IsNotNull(_userPublishedToHistory);
        Assert.AreEqual(_punchItemAddedToRepository.CreatedBy.Guid, _userPublishedToHistory.Oid);
        Assert.AreEqual(_punchItemAddedToRepository.CreatedBy.GetFullName(), _userPublishedToHistory.FullName);
        Assert.AreEqual(_punchItemAddedToRepository.CreatedAtUtc, _dateTimePublishedToHistory);
        Assert.IsNotNull(_newPropertiesPublishedToHistory);
        // todo 109354 test published properties to history
        Assert.AreEqual(0, _newPropertiesPublishedToHistory.Count);
    }
}

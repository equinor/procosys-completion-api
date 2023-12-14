using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItem;

[TestClass]
public class CreatePunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private IPersonCache _personCacheMock;
    private IProjectRepository _projectRepositoryMock;

    private Project _existingProject;
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
        _existingProject = new Project(TestPlantA, Guid.NewGuid(), null!, null!, DateTime.Now);
        _existingProject.SetProtectedIdForTesting(10);
        _projectRepositoryMock = Substitute.For<IProjectRepository>();
        _projectRepositoryMock
            .GetAsync(_existingProject.Guid, default)
            .Returns(_existingProject);

        _personRepositoryMock
            .When(x => x.Add(Arg.Any<Person>()))
            .Do(callInfo =>
            {
                _personAddedToRepository = callInfo.Arg<Person>();
            });

        _personCacheMock = Substitute.For<IPersonCache>();

        _command = new CreatePunchItemCommand(
            Category.PA,
            "P123",
            _existingProject.Guid,
            _existingCheckListGuid,
            _existingRaisedByOrg1.Guid,
            _existingClearingByOrg1.Guid,
            _existingPerson1.Guid,
            DateTime.UtcNow, 
            _existingPriority1.Guid,
            _existingSorting1.Guid,
            _existingType1.Guid,
            100,
            _existingWorkOrder1.Guid,
            _existingWorkOrder2.Guid,
            _existingSWCR1.Guid,
            _existingDocument1.Guid,
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
            _unitOfWorkMock,
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
        Assert.AreEqual(_existingProject.Id, _punchItemAddedToRepository.ProjectId);
        Assert.AreEqual(_existingRaisedByOrg1.Id, _punchItemAddedToRepository.RaisedByOrgId);
        Assert.AreEqual(_existingClearingByOrg1.Id, _punchItemAddedToRepository.ClearingByOrgId);
        Assert.AreEqual(_existingPerson1.Id, _punchItemAddedToRepository.ActionById);
        Assert.AreEqual(_command.DueTimeUtc, _punchItemAddedToRepository.DueTimeUtc);
        Assert.AreEqual(_existingPriority1.Id, _punchItemAddedToRepository.PriorityId);
        Assert.AreEqual(_existingSorting1.Id, _punchItemAddedToRepository.SortingId);
        Assert.AreEqual(_existingType1.Id, _punchItemAddedToRepository.TypeId);
        Assert.AreEqual(_command.Estimate, _punchItemAddedToRepository.Estimate);
        Assert.AreEqual(_existingWorkOrder1.Id, _punchItemAddedToRepository.OriginalWorkOrderId);
        Assert.AreEqual(_existingWorkOrder2.Id, _punchItemAddedToRepository.WorkOrderId);
        Assert.AreEqual(_existingSWCR1.Id, _punchItemAddedToRepository.SWCRId);
        Assert.AreEqual(_existingDocument1.Id, _punchItemAddedToRepository.DocumentId);
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
            _existingProject.Guid,
            _existingCheckListGuid, 
            _existingRaisedByOrg1.Guid,
            _existingClearingByOrg1.Guid,
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
        Assert.AreEqual(_existingProject.Id, _punchItemAddedToRepository.ProjectId);
        Assert.AreEqual(_existingRaisedByOrg1.Id, _punchItemAddedToRepository.RaisedByOrgId);
        Assert.AreEqual(_existingClearingByOrg1.Id, _punchItemAddedToRepository.ClearingByOrgId);
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
            _existingProject.Guid,
            _existingCheckListGuid,
            _existingRaisedByOrg1.Guid,
            _existingClearingByOrg1.Guid,
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
            AzureOid = nonExistingPersonOid.ToString()
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
    public async Task HandlingCommand_ShouldAddPunchItemCreatedEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(_punchItemAddedToRepository.DomainEvents.First(), typeof(PunchItemCreatedDomainEvent));
    }
}

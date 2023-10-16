using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItem;

[TestClass]
public class CreatePunchItemCommandHandlerTests : TestsBase
{
    private IPunchItemRepository _punchItemRepositoryMock;
    private IProjectRepository _projectRepositoryMock;
    private ILibraryItemRepository _libraryItemRepositoryMock;
    private IPersonRepository _personRepositoryMock;
    private IPersonCache _personCacheMock;
    private IWorkOrderRepository _woRepositoryMock;
    private ISWCRRepository _swcrRepositoryMock;
    private IDocumentRepository _documentRepositoryMock;

    private Project _existingProject;
    private readonly Guid _existingCheckListGuid = Guid.NewGuid();
    private LibraryItem _existingRaisedByOrg;
    private LibraryItem _existingClearingByOrg;
    private LibraryItem _existingPriority;
    private LibraryItem _existingSorting;
    private LibraryItem _existingType;
    private PunchItem _punchItemAddedToRepository;
    private Person _personAddedToRepository;
    private Person _existingPerson;
    private WorkOrder _existingOriginalWorkOrder;
    private WorkOrder _existingWorkOrder;
    private SWCR _existingSWCR;
    private Document _existingDocument;

    private CreatePunchItemCommandHandler _dut;
    private CreatePunchItemCommand _command;
    private readonly Guid _existingPersonOid = Guid.NewGuid();
    private readonly Guid _nonExistingPersonOid = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        var projectIdOnExisting = 10;
        var raisedByOrgIdOnExisting = 11;
        var clearingByOrgIdOnExisting = 12;
        var priorityIdOnExisting = 13;
        var sortingIdOnExisting = 14;
        var typeIdOnExisting = 15;
        var personIdOnExisting = 16;
        var wo1IdOnExisting = 17;
        var wo2IdOnExisting = 18;
        var swcrIdOnExisting = 19;
        var documentIdOnExisting = 20;
        _punchItemRepositoryMock = Substitute.For<IPunchItemRepository>();
        _punchItemRepositoryMock
            .When(x => x.Add(Arg.Any<PunchItem>()))
            .Do(callInfo =>
            {
                _punchItemAddedToRepository = callInfo.Arg<PunchItem>();
            });
        _existingProject = new Project(TestPlantA, Guid.NewGuid(), null!, null!);
        _existingProject.SetProtectedIdForTesting(projectIdOnExisting);
        _projectRepositoryMock = Substitute.For<IProjectRepository>();
        _projectRepositoryMock
            .GetByGuidAsync(_existingProject.Guid)
            .Returns(_existingProject);

        _existingRaisedByOrg = new LibraryItem(TestPlantA, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        _existingRaisedByOrg.SetProtectedIdForTesting(raisedByOrgIdOnExisting);

        _existingClearingByOrg = new LibraryItem(TestPlantA, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        _existingClearingByOrg.SetProtectedIdForTesting(clearingByOrgIdOnExisting);
        
        _existingPriority = new LibraryItem(TestPlantA, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_PRIORITY);
        _existingPriority.SetProtectedIdForTesting(priorityIdOnExisting);
        
        _existingSorting = new LibraryItem(TestPlantA, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_SORTING);
        _existingSorting.SetProtectedIdForTesting(sortingIdOnExisting);
        
        _existingType = new LibraryItem(TestPlantA, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_TYPE);
        _existingType.SetProtectedIdForTesting(typeIdOnExisting);

        _existingPerson = new Person(_existingPersonOid, null!, null!, null!, null!);
        _existingPerson.SetProtectedIdForTesting(personIdOnExisting);

        _existingOriginalWorkOrder = new WorkOrder(TestPlantA, Guid.NewGuid(), null!);
        _existingOriginalWorkOrder.SetProtectedIdForTesting(wo1IdOnExisting);

        _existingWorkOrder = new WorkOrder(TestPlantA, Guid.NewGuid(), null!);
        _existingWorkOrder.SetProtectedIdForTesting(wo2IdOnExisting);

        _existingSWCR = new SWCR(TestPlantA, Guid.NewGuid(), 1);
        _existingSWCR.SetProtectedIdForTesting(swcrIdOnExisting);

        _existingDocument = new Document(TestPlantA, Guid.NewGuid(), null!);
        _existingDocument.SetProtectedIdForTesting(documentIdOnExisting);

        _libraryItemRepositoryMock = Substitute.For<ILibraryItemRepository>();
        _libraryItemRepositoryMock
            .GetByGuidAndTypeAsync(_existingRaisedByOrg.Guid, LibraryType.COMPLETION_ORGANIZATION)
            .Returns(_existingRaisedByOrg);

        _libraryItemRepositoryMock
            .GetByGuidAndTypeAsync(_existingClearingByOrg.Guid, LibraryType.COMPLETION_ORGANIZATION)
            .Returns(_existingClearingByOrg);

        _libraryItemRepositoryMock
            .GetByGuidAndTypeAsync(_existingPriority.Guid, LibraryType.PUNCHLIST_PRIORITY)
            .Returns(_existingPriority);

        _libraryItemRepositoryMock
            .GetByGuidAndTypeAsync(_existingSorting.Guid, LibraryType.PUNCHLIST_SORTING)
            .Returns(_existingSorting);

        _libraryItemRepositoryMock
            .GetByGuidAndTypeAsync(_existingType.Guid, LibraryType.PUNCHLIST_TYPE)
            .Returns(_existingType);

        _personRepositoryMock = Substitute.For<IPersonRepository>();
        _personRepositoryMock
            .GetByGuidAsync(_existingPerson.Guid)
            .Returns(_existingPerson);
        _personRepositoryMock
            .When(x => x.Add(Arg.Any<Person>()))
            .Do(callInfo =>
            {
                _personAddedToRepository = callInfo.Arg<Person>();
            });

        _personCacheMock = Substitute.For<IPersonCache>();

        _woRepositoryMock = Substitute.For<IWorkOrderRepository>();
        _woRepositoryMock
            .GetByGuidAsync(_existingOriginalWorkOrder.Guid)
            .Returns(_existingOriginalWorkOrder);
        _woRepositoryMock
            .GetByGuidAsync(_existingWorkOrder.Guid)
            .Returns(_existingWorkOrder);

        _swcrRepositoryMock = Substitute.For<ISWCRRepository>();
        _swcrRepositoryMock
            .GetByGuidAsync(_existingSWCR.Guid)
            .Returns(_existingSWCR);

        _documentRepositoryMock = Substitute.For<IDocumentRepository>();
        _documentRepositoryMock
            .GetByGuidAsync(_existingDocument.Guid)
            .Returns(_existingDocument);

        _command = new CreatePunchItemCommand(
            Category.PA,
            "P123",
            _existingProject.Guid,
            _existingCheckListGuid,
            _existingRaisedByOrg.Guid,
            _existingClearingByOrg.Guid,
            _existingPerson.Guid,
            DateTime.UtcNow, 
            _existingPriority.Guid,
            _existingSorting.Guid,
            _existingType.Guid,
            100,
            _existingOriginalWorkOrder.Guid,
            _existingWorkOrder.Guid,
            _existingSWCR.Guid,
            _existingDocument.Guid,
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
            _woRepositoryMock,
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
        Assert.AreEqual(_existingRaisedByOrg.Id, _punchItemAddedToRepository.RaisedByOrgId);
        Assert.AreEqual(_existingClearingByOrg.Id, _punchItemAddedToRepository.ClearingByOrgId);
        Assert.AreEqual(_existingPerson.Id, _punchItemAddedToRepository.ActionById);
        Assert.AreEqual(_command.DueTimeUtc, _punchItemAddedToRepository.DueTimeUtc);
        Assert.AreEqual(_existingPriority.Id, _punchItemAddedToRepository.PriorityId);
        Assert.AreEqual(_existingSorting.Id, _punchItemAddedToRepository.SortingId);
        Assert.AreEqual(_existingType.Id, _punchItemAddedToRepository.TypeId);
        Assert.AreEqual(_command.Estimate, _punchItemAddedToRepository.Estimate);
        Assert.AreEqual(_existingOriginalWorkOrder.Id, _punchItemAddedToRepository.OriginalWorkOrderId);
        Assert.AreEqual(_existingWorkOrder.Id, _punchItemAddedToRepository.WorkOrderId);
        Assert.AreEqual(_existingSWCR.Id, _punchItemAddedToRepository.SWCRId);
        Assert.AreEqual(_existingDocument.Id, _punchItemAddedToRepository.DocumentId);
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
            _existingRaisedByOrg.Guid,
            _existingClearingByOrg.Guid,
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
        Assert.AreEqual(_existingRaisedByOrg.Id, _punchItemAddedToRepository.RaisedByOrgId);
        Assert.AreEqual(_existingClearingByOrg.Id, _punchItemAddedToRepository.ClearingByOrgId);
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
        var command = new CreatePunchItemCommand(
            Category.PA,
            "P123",
            _existingProject.Guid,
            _existingCheckListGuid,
            _existingRaisedByOrg.Guid,
            _existingClearingByOrg.Guid,
            _nonExistingPersonOid,
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
        _personRepositoryMock
            .GetByGuidAsync(_nonExistingPersonOid)
            .Returns((Person)null);
        var proCoSysPerson = new ProCoSysPerson
        {
            UserName = "YODA",
            FirstName = "YO",
            LastName = "DA",
            Email = "@",
            AzureOid = _nonExistingPersonOid.ToString()
        };
        _personCacheMock.GetAsync(_nonExistingPersonOid)
            .Returns(proCoSysPerson);

        // Act
        await _dut.Handle(command, default);

        // Assert
        Assert.IsNotNull(_punchItemAddedToRepository);
        Assert.IsNotNull(_personAddedToRepository);
        Assert.IsNotNull(_punchItemAddedToRepository.ActionBy);
        Assert.AreEqual(_nonExistingPersonOid, _punchItemAddedToRepository.ActionBy!.Guid);
        Assert.AreEqual(_nonExistingPersonOid, _personAddedToRepository.Guid);
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
    public async Task HandlingCommand_ShouldThrowException_WhenProjectNotExists()
    {
        // Arrange
        _projectRepositoryMock
            .GetByGuidAsync(_existingProject.Guid)
            .Returns((Project)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));
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

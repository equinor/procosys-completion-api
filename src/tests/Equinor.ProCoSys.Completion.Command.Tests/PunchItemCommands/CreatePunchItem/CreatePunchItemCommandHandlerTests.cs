using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
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

    private Project _existingProject;
    private readonly Guid _existingCheckListGuid = Guid.NewGuid();
    private LibraryItem _existingRaisedByOrg;
    private LibraryItem _existingClearingByOrg;
    private LibraryItem _existingPriority;
    private LibraryItem _existingSorting;
    private LibraryItem _existingType;
    private PunchItem _punchItemAddedToRepository;

    private CreatePunchItemCommandHandler _dut;
    private CreatePunchItemCommand _command;

    [TestInitialize]
    public void Setup()
    {
        var projectIdOnExisting = 10;
        var raisedByOrgIdOnExisting = 11;
        var clearingByOrgIdOnExisting = 12;
        var priorityIdOnExisting = 13;
        var sortingIdOnExisting = 14;
        var typeIdOnExisting = 15;
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

        _libraryItemRepositoryMock =Substitute.For<ILibraryItemRepository>();
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


        _command = new CreatePunchItemCommand(
            Category.PA,
            "P123",
            _existingProject.Guid,
            _existingCheckListGuid,
            _existingRaisedByOrg.Guid,
            _existingClearingByOrg.Guid,
            _existingPriority.Guid,
            _existingSorting.Guid,
            _existingType.Guid);

        _dut = new CreatePunchItemCommandHandler(
            _plantProviderMock,
            _punchItemRepositoryMock,
            _libraryItemRepositoryMock,
            _unitOfWorkMock,
            _projectRepositoryMock,
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
    public async Task HandlingCommand_WithAllValues_ShouldAddCorrectPunchItemToRepository()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNotNull(_punchItemAddedToRepository);
        Assert.AreEqual(_command.Description, _punchItemAddedToRepository.Description);
        Assert.AreEqual(_existingProject.Id, _punchItemAddedToRepository.ProjectId);
        Assert.AreEqual(_existingRaisedByOrg.Id, _punchItemAddedToRepository.RaisedByOrgId);
        Assert.AreEqual(_existingClearingByOrg.Id, _punchItemAddedToRepository.ClearingByOrgId);
        Assert.AreEqual(_existingPriority.Id, _punchItemAddedToRepository.PriorityId);
        Assert.AreEqual(_existingSorting.Id, _punchItemAddedToRepository.SortingId);
        Assert.AreEqual(_existingType.Id, _punchItemAddedToRepository.TypeId);
    }

    [TestMethod]
    public async Task HandlingCommand_WithRequiredValues_ShouldAddCorrectPunchItemToRepository()
    {
        // Arrange
        var command = new CreatePunchItemCommand(
            Category.PA,
            "P123",
            _existingProject.Guid,
            _existingCheckListGuid, 
            _existingRaisedByOrg.Guid,
            _existingClearingByOrg.Guid);

        // Act
        await _dut.Handle(command, default);

        // Assert
        Assert.IsNotNull(_punchItemAddedToRepository);
        Assert.AreEqual(_command.Description, _punchItemAddedToRepository.Description);
        Assert.AreEqual(_existingProject.Id, _punchItemAddedToRepository.ProjectId);
        Assert.AreEqual(_existingRaisedByOrg.Id, _punchItemAddedToRepository.RaisedByOrgId);
        Assert.AreEqual(_existingClearingByOrg.Id, _punchItemAddedToRepository.ClearingByOrgId);
        Assert.IsFalse(_punchItemAddedToRepository.PriorityId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.SortingId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.TypeId.HasValue);
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
    public async Task HandlingCommand_ShouldThrowException_WhenRaisedByOrgNotExists()
    {
        // Arrange
        _libraryItemRepositoryMock
            .GetByGuidAndTypeAsync(_existingRaisedByOrg.Guid, LibraryType.COMPLETION_ORGANIZATION)
            .Returns((LibraryItem)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldThrowException_WhenClearingByOrgNotExists()
    {
        // Arrange
        _libraryItemRepositoryMock
            .GetByGuidAndTypeAsync(_existingClearingByOrg.Guid, LibraryType.COMPLETION_ORGANIZATION)
            .Returns((LibraryItem)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldThrowException_WhenPriorityNotExists()
    {
        // Arrange
        _libraryItemRepositoryMock
            .GetByGuidAndTypeAsync(_existingPriority.Guid, LibraryType.PUNCHLIST_PRIORITY)
            .Returns((LibraryItem)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldThrowException_WhenSortingNotExists()
    {
        // Arrange
        _libraryItemRepositoryMock
            .GetByGuidAndTypeAsync(_existingSorting.Guid, LibraryType.PUNCHLIST_SORTING)
            .Returns((LibraryItem)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldThrowException_WhenTypeNotExists()
    {
        // Arrange
        _libraryItemRepositoryMock
            .GetByGuidAndTypeAsync(_existingType.Guid, LibraryType.PUNCHLIST_TYPE)
            .Returns((LibraryItem)null);

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

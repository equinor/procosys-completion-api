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
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItem;

[TestClass]
public class CreatePunchItemCommandHandlerTests : TestsBase
{
    private Mock<IPunchItemRepository> _punchItemRepositoryMock;
    private Mock<IProjectRepository> _projectRepositoryMock;
    private Mock<ILibraryItemRepository> _libraryItemRepositoryMock;

    private Project _existingProject;
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
        _punchItemRepositoryMock = new Mock<IPunchItemRepository>();
        _punchItemRepositoryMock
            .Setup(x => x.Add(It.IsAny<PunchItem>()))
            .Callback<PunchItem>(punchItem =>
            {
                _punchItemAddedToRepository = punchItem;
            });
        _existingProject = new Project(TestPlantA, Guid.NewGuid(), null!, null!);
        _existingProject.SetProtectedIdForTesting(projectIdOnExisting);
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _projectRepositoryMock
            .Setup(x => x.GetByGuidAsync(_existingProject.Guid))
            .ReturnsAsync(_existingProject);

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

        _libraryItemRepositoryMock = new Mock<ILibraryItemRepository>();
        _libraryItemRepositoryMock
            .Setup(x => x.GetByGuidAndTypeAsync(_existingRaisedByOrg.Guid, LibraryType.COMPLETION_ORGANIZATION))
            .ReturnsAsync(_existingRaisedByOrg);
        _libraryItemRepositoryMock
            .Setup(x => x.GetByGuidAndTypeAsync(_existingClearingByOrg.Guid, LibraryType.COMPLETION_ORGANIZATION))
            .ReturnsAsync(_existingClearingByOrg);
        _libraryItemRepositoryMock
            .Setup(x => x.GetByGuidAndTypeAsync(_existingPriority.Guid, LibraryType.PUNCHLIST_PRIORITY))
            .ReturnsAsync(_existingPriority);
        _libraryItemRepositoryMock
            .Setup(x => x.GetByGuidAndTypeAsync(_existingSorting.Guid, LibraryType.PUNCHLIST_SORTING))
            .ReturnsAsync(_existingSorting);
        _libraryItemRepositoryMock
            .Setup(x => x.GetByGuidAndTypeAsync(_existingType.Guid, LibraryType.PUNCHLIST_TYPE))
            .ReturnsAsync(_existingType);

        _command = new CreatePunchItemCommand(
            "P123",
            _existingProject.Guid,
            _existingRaisedByOrg.Guid,
            _existingClearingByOrg.Guid,
            _existingPriority.Guid,
            _existingSorting.Guid,
            _existingType.Guid);

        _dut = new CreatePunchItemCommandHandler(
            _plantProviderMock.Object,
            _punchItemRepositoryMock.Object,
            _libraryItemRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _projectRepositoryMock.Object,
            new Mock<ILogger<CreatePunchItemCommandHandler>>().Object);
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
            "P123",
            _existingProject.Guid,
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
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldThrowException_WhenProjectNotExists()
    {
        // Arrange
        _projectRepositoryMock
            .Setup(x => x.GetByGuidAsync(_existingProject.Guid))
            .ReturnsAsync((Project)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldThrowException_WhenRaisedByOrgNotExists()
    {
        // Arrange
        _libraryItemRepositoryMock
            .Setup(x => x.GetByGuidAndTypeAsync(_existingRaisedByOrg.Guid, LibraryType.COMPLETION_ORGANIZATION))
            .ReturnsAsync((LibraryItem)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldThrowException_WhenClearingByOrgNotExists()
    {
        // Arrange
        _libraryItemRepositoryMock
            .Setup(x => x.GetByGuidAndTypeAsync(_existingClearingByOrg.Guid, LibraryType.COMPLETION_ORGANIZATION))
            .ReturnsAsync((LibraryItem)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldThrowException_WhenPriorityNotExists()
    {
        // Arrange
        _libraryItemRepositoryMock
            .Setup(x => x.GetByGuidAndTypeAsync(_existingPriority.Guid, LibraryType.PUNCHLIST_PRIORITY))
            .ReturnsAsync((LibraryItem)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldThrowException_WhenSortingNotExists()
    {
        // Arrange
        _libraryItemRepositoryMock
            .Setup(x => x.GetByGuidAndTypeAsync(_existingSorting.Guid, LibraryType.PUNCHLIST_SORTING))
            .ReturnsAsync((LibraryItem)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldThrowException_WhenTypeNotExists()
    {
        // Arrange
        _libraryItemRepositoryMock
            .Setup(x => x.GetByGuidAndTypeAsync(_existingType.Guid, LibraryType.PUNCHLIST_TYPE))
            .ReturnsAsync((LibraryItem)null);

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

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

    private readonly int _projectIdOnExisting = 10;
    private readonly int _raisedByOrgIdOnExisting = 11;
    private readonly int _clearingByOrgIdOnExisting = 12;

    private Project _existingProject;
    private LibraryItem _existingRaisedByOrg;
    private LibraryItem _existingClearingByOrg;
    private PunchItem _punchItemAddedToRepository;

    private CreatePunchItemCommandHandler _dut;
    private CreatePunchItemCommand _command;

    [TestInitialize]
    public void Setup()
    {
        _punchItemRepositoryMock = new Mock<IPunchItemRepository>();
        _punchItemRepositoryMock
            .Setup(x => x.Add(It.IsAny<PunchItem>()))
            .Callback<PunchItem>(punchItem =>
            {
                _punchItemAddedToRepository = punchItem;
            });
        _existingProject = new Project(TestPlantA, Guid.NewGuid(), null!, null!);
        _existingProject.SetProtectedIdForTesting(_projectIdOnExisting);
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _projectRepositoryMock
            .Setup(x => x.GetByGuidAsync(_existingProject.Guid))
            .ReturnsAsync(_existingProject);

        _existingRaisedByOrg = new LibraryItem(TestPlantA, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        _existingRaisedByOrg.SetProtectedIdForTesting(_raisedByOrgIdOnExisting);
        _existingClearingByOrg = new LibraryItem(TestPlantA, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        _existingClearingByOrg.SetProtectedIdForTesting(_clearingByOrgIdOnExisting);

        _libraryItemRepositoryMock = new Mock<ILibraryItemRepository>();
        _libraryItemRepositoryMock
            .Setup(x => x.GetByGuidAndTypeAsync(_existingRaisedByOrg.Guid, LibraryType.COMPLETION_ORGANIZATION))
            .ReturnsAsync(_existingRaisedByOrg);
        _libraryItemRepositoryMock
            .Setup(x => x.GetByGuidAndTypeAsync(_existingClearingByOrg.Guid, LibraryType.COMPLETION_ORGANIZATION))
            .ReturnsAsync(_existingClearingByOrg);

        _command = new CreatePunchItemCommand(
            "P123",
            _existingProject.Guid,
            _existingRaisedByOrg.Guid,
            _existingClearingByOrg.Guid);

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
    public async Task HandlingCommand_ShouldAddPunchItemToRepository()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNotNull(_punchItemAddedToRepository);
        Assert.AreEqual(_command.Description, _punchItemAddedToRepository.Description);
        Assert.AreEqual(_projectIdOnExisting, _punchItemAddedToRepository.ProjectId);
        Assert.AreEqual(_raisedByOrgIdOnExisting, _punchItemAddedToRepository.RaisedByOrgId);
        Assert.AreEqual(_clearingByOrgIdOnExisting, _punchItemAddedToRepository.ClearingByOrgId);
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
    public async Task HandlingCommand_ShouldAddPunchItemCreatedEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(_punchItemAddedToRepository.DomainEvents.First(), typeof(PunchItemCreatedDomainEvent));
    }
}

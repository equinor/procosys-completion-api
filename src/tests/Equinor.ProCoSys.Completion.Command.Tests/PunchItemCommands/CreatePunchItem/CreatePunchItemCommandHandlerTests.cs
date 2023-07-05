using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
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

    private readonly Guid _projectGuid = Guid.NewGuid();
    private readonly int _projectIdOnExisting = 10;

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
        var project = new Project(TestPlantA, _projectGuid, null!, null!);
        project.SetProtectedIdForTesting(_projectIdOnExisting);
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _projectRepositoryMock
            .Setup(x => x.GetByGuidAsync(_projectGuid))
            .ReturnsAsync(project);

        _command = new CreatePunchItemCommand("P123", _projectGuid);

        _dut = new CreatePunchItemCommandHandler(
            _plantProviderMock.Object,
            _punchItemRepositoryMock.Object,
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
        Assert.AreEqual(_command.ItemNo, _punchItemAddedToRepository.ItemNo);
        Assert.AreEqual(_projectIdOnExisting, _punchItemAddedToRepository.ProjectId);
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
    public async Task HandlingCommand_ShouldThrewException_WhenProjectNotExists()
    {
        // Arrange
        _projectRepositoryMock
            .Setup(x => x.GetByGuidAsync(_projectGuid))
            .ReturnsAsync((Project)null);

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

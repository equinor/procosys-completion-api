using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunch;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.CreatePunch;

[TestClass]
public class CreatePunchCommandHandlerTests : TestsBase
{
    private Mock<IPunchRepository> _punchRepositoryMock;
    private Mock<IProjectRepository> _projectRepositoryMock;

    private readonly Guid _projectGuid = Guid.NewGuid();
    private readonly int _projectIdOnExisting = 10;

    private Punch _punchAddedToRepository;

    private CreatePunchCommandHandler _dut;
    private CreatePunchCommand _command;

    [TestInitialize]
    public void Setup()
    {
        _punchRepositoryMock = new Mock<IPunchRepository>();
        _punchRepositoryMock
            .Setup(x => x.Add(It.IsAny<Punch>()))
            .Callback<Punch>(punch =>
            {
                _punchAddedToRepository = punch;
            });
        var project = new Project(TestPlantA, _projectGuid, null!, null!);
        project.SetProtectedIdForTesting(_projectIdOnExisting);
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _projectRepositoryMock
            .Setup(x => x.GetByGuidAsync(_projectGuid))
            .ReturnsAsync(project);

        _command = new CreatePunchCommand("Punch", _projectGuid);

        _dut = new CreatePunchCommandHandler(
            _plantProviderMock.Object,
            _punchRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _projectRepositoryMock.Object,
            new Mock<ILogger<CreatePunchCommandHandler>>().Object);
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
    public async Task HandlingCommand_ShouldAddPunchToRepository()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNotNull(_punchAddedToRepository);
        Assert.AreEqual(_command.ItemNo, _punchAddedToRepository.ItemNo);
        Assert.AreEqual(_projectIdOnExisting, _punchAddedToRepository.ProjectId);
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
    public async Task HandlingCommand_ShouldAddPunchCreatedEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(_punchAddedToRepository.DomainEvents.First(), typeof(PunchCreatedEvent));
    }
}

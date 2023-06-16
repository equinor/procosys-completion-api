using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Telemetry;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using Equinor.ProCoSys.PcsServiceBus.Topics;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class BusReceiverServiceTests
{
    private BusReceiverService _dut;
    private Mock<IUnitOfWork> _unitOfWork;
    private Mock<IPlantSetter> _plantSetter;
    private Mock<IProjectRepository> _projectRepository;
    private readonly string _plant = "Plant";
    private readonly Guid _projectGuid = Guid.NewGuid();
    private Project _project1;
    private Project _projectedAddedToRepository;

    [TestInitialize]
    public void Setup()
    {
        _plantSetter = new Mock<IPlantSetter>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _project1 = new Project(_plant, _projectGuid, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        _projectRepository = new Mock<IProjectRepository>();
        _projectRepository.Setup(p => p.TryGetByGuidAsync(_projectGuid))
            .ReturnsAsync(_project1);
        _projectRepository
            .Setup(x => x.Add(It.IsAny<Project>()))
            .Callback<Project>(project =>
            {
                _projectedAddedToRepository = project;
            });

        _dut = new BusReceiverService(
            _plantSetter.Object,
            _unitOfWork.Object,
            new Mock<ITelemetryClient>().Object,
            _projectRepository.Object);
    }

    #region Project
    [TestMethod]
    public async Task HandlingProjectTopic_ShouldUpdateProject_WhenKnownGuid()
    {
        // Arrange
        var message = new ProjectTopic
        {
            Behavior = "",
            ProjectName = Guid.NewGuid().ToString(),
            Description = Guid.NewGuid().ToString(),
            IsClosed = true,
            Plant = _plant,
            ProCoSysGuid = _projectGuid
        };
        var messageJson = JsonSerializer.Serialize(message);
        Assert.IsFalse(_project1.IsClosed);

        // Act
        await _dut.ProcessMessageAsync(ProjectTopic.TopicName, messageJson, default);

        // Assert
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _plantSetter.Verify(p => p.SetPlant(_plant), Times.Once);
        _projectRepository.Verify(i => i.TryGetByGuidAsync(_projectGuid), Times.Once);
        Assert.AreEqual(message.ProjectName, _project1.Name);
        Assert.AreEqual(message.Description, _project1.Description);
        Assert.IsTrue(_project1.IsClosed);
    }

    [TestMethod]
    public async Task HandlingProjectTopic_ShouldCreateOpenProject_WhenUnknownGuid()
    {
        // Arrange
        var message = new ProjectTopic
        {
            Behavior = "",
            ProjectName = Guid.NewGuid().ToString(),
            Description = Guid.NewGuid().ToString(),
            Plant = _plant,
            ProCoSysGuid = _projectGuid
        };
        var messageJson = JsonSerializer.Serialize(message);
        _projectRepository.Setup(p => p.TryGetByGuidAsync(_projectGuid))
            .ReturnsAsync((Project)null);
        Assert.IsFalse(_project1.IsClosed);

        // Act
        await _dut.ProcessMessageAsync(ProjectTopic.TopicName, messageJson, default);

        // Assert
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _plantSetter.Verify(p => p.SetPlant(_plant), Times.Once);
        _projectRepository.Verify(i => i.TryGetByGuidAsync(_projectGuid), Times.Once);
        Assert.IsNotNull(_projectedAddedToRepository);
        Assert.AreEqual(message.ProCoSysGuid, _projectedAddedToRepository.Guid);
        Assert.AreEqual(message.ProjectName, _projectedAddedToRepository.Name);
        Assert.AreEqual(message.Description, _projectedAddedToRepository.Description);
        Assert.IsFalse(_projectedAddedToRepository.IsClosed);
    }

    [TestMethod]
    public async Task HandlingProjectTopic_ShouldMarkProjectedAsDeletedInSource()
    {
        // Arrange
        var message = new ProjectTopic
        {
            Behavior = "delete",
            Plant = _plant,
            ProCoSysGuid = _projectGuid
        };
        var messageJson = JsonSerializer.Serialize(message);
        Assert.IsFalse(_project1.IsClosed);
        Assert.IsFalse(_project1.IsDeletedInSource);
        var oldName = _project1.Name;
        var oldDescription = _project1.Description;

        // Act
        await _dut.ProcessMessageAsync(ProjectTopic.TopicName, messageJson, default);

        // Assert
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _plantSetter.Verify(p => p.SetPlant(_plant), Times.Once);
        _projectRepository.Verify(i => i.TryGetByGuidAsync(_projectGuid), Times.Once);
        Assert.AreEqual(oldName, _project1.Name);
        Assert.AreEqual(oldDescription, _project1.Description);
        Assert.IsTrue(_project1.IsDeletedInSource);
        Assert.IsTrue(_project1.IsClosed);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task HandlingProjectTopic_ShouldFail_WhenMissingPlant()
    {
        // Arrange
        var message = new ProjectTopic
        {
            Behavior = "",
            ProjectName = Guid.NewGuid().ToString(),
            Description = Guid.NewGuid().ToString(),
            IsClosed = true,
            ProCoSysGuid = _projectGuid
        };
        var messageJson = JsonSerializer.Serialize(message);

        // Act
        await _dut.ProcessMessageAsync(ProjectTopic.TopicName, messageJson, default);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task HandlingProjectTopic_ShouldFailIfEmptyMessage()
    {
        // Arrange
        var messageJson = "{}";

        // Act
        await _dut.ProcessMessageAsync(ProjectTopic.TopicName, messageJson, default);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public async Task HandlingProjectTopic_ShouldFailIfBlankMessage()
    {
        // Arrange
        var messageJson = "";

        // Act
        await _dut.ProcessMessageAsync(ProjectTopic.TopicName, messageJson, default);
    }
    #endregion
}

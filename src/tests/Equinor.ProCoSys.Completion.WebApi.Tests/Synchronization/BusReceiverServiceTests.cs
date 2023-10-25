using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Telemetry;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using Equinor.ProCoSys.PcsServiceBus.Topics;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class BusReceiverServiceTests
{
    private BusReceiverService _dut = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IPlantSetter _plantSetter = null!;
    private IProjectRepository _projectRepository = null!;
    private const string Plant = "Plant";
    private readonly Guid _projectGuid = Guid.NewGuid();
    private Project _project1 = null!;
    private Project _projectedAddedToRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _plantSetter = Substitute.For<IPlantSetter>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _project1 = new Project(Plant, _projectGuid, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        _projectRepository = Substitute.For<IProjectRepository>();
        _projectRepository.GetByGuidAsync(_projectGuid)
            .Returns(_project1);
        _projectRepository.ExistsAsync(_projectGuid)
            .Returns(true);
        _projectRepository
            .When(x => x.Add(Arg.Any<Project>()))
            .Do(info =>
            {
                _projectedAddedToRepository = info.Arg<Project>();
            });

        _dut = new BusReceiverService(
            _plantSetter,
            _unitOfWork,
            Substitute.For<ITelemetryClient>(),
            _projectRepository);
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
            Plant = Plant,
            ProCoSysGuid = _projectGuid
        };
        var messageJson = JsonSerializer.Serialize(message);
        Assert.IsFalse(_project1.IsClosed);

        // Act
        await _dut.ProcessMessageAsync(ProjectTopic.TopicName, messageJson, default);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _plantSetter.Received(1).SetPlant(Plant);
        await _projectRepository.Received(1).GetByGuidAsync(_projectGuid);
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
            Plant = Plant,
            ProCoSysGuid = _projectGuid
        };
        var messageJson = JsonSerializer.Serialize(message);
        _projectRepository.ExistsAsync(_projectGuid)
            .Returns(false);
        Assert.IsFalse(_project1.IsClosed);

        // Act
        await _dut.ProcessMessageAsync(ProjectTopic.TopicName, messageJson, default);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _plantSetter.Received(1).SetPlant(Plant);
        await _projectRepository.Received(0).GetByGuidAsync(_projectGuid);
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
            Plant = Plant,
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
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _plantSetter.Received(1).SetPlant(Plant);
        await _projectRepository.Received(1).GetByGuidAsync(_projectGuid);
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
        const string MessageJson = "{}";

        // Act
        await _dut.ProcessMessageAsync(ProjectTopic.TopicName, MessageJson, default);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public async Task HandlingProjectTopic_ShouldFailIfBlankMessage() =>
        //Arrange and  Act
        await _dut.ProcessMessageAsync(ProjectTopic.TopicName, "", default);

    #endregion
}

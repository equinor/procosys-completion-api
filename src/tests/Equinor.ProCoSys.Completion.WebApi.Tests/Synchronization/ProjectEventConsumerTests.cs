using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class ProjectEventConsumerTests
{
    private readonly IProjectRepository _projectRepoMock = Substitute.For<IProjectRepository>();
    private readonly IPlantSetter _plantSetter = Substitute.For<IPlantSetter>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly ProjectEventConsumer _projectEventConsumer;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
    private readonly ConsumeContext<ProjectEvent> _contextMock = Substitute.For<ConsumeContext<ProjectEvent>>();
    private Project? _projectAddedToRepository;

    public ProjectEventConsumerTests() =>
        _projectEventConsumer = new ProjectEventConsumer(Substitute.For<ILogger<ProjectEventConsumer>>(), _plantSetter, _projectRepoMock, 
            _unitOfWorkMock, Substitute.For<ICurrentUserSetter>(), _applicationOptionsMock);

    [TestInitialize]
    public void Setup()
    {
        _applicationOptionsMock.CurrentValue.Returns(new ApplicationOptions { ObjectId = new Guid() });
        
        _projectRepoMock
            .When(x => x.Add(Arg.Any<Project>()))
            .Do(callInfo =>
            {
                _projectAddedToRepository = callInfo.Arg<Project>();
            });
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewProject_WhenProjectDoesNotExist()
    {
        //Arrange
        var guid = Guid.NewGuid();
        
        var lastUpdated = DateTime.Now;
        const string Description = "projectDescription";
        const string ProjectName = "ProjectName";
        var testEvent = new ProjectEvent("", Description, false, lastUpdated, "plant", guid, ProjectName, null);
        
        _contextMock.Message.Returns(testEvent);
        
        _projectRepoMock.ExistsAsync(guid, default).Returns(false);
        
        //Act
        await _projectEventConsumer.Consume(_contextMock);
        
        //Assert
        Assert.IsNotNull(_projectAddedToRepository);
        Assert.AreEqual(guid,_projectAddedToRepository.Guid);
        Assert.AreEqual(false,_projectAddedToRepository.IsClosed);
        Assert.AreEqual(lastUpdated,_projectAddedToRepository.ProCoSys4LastUpdated);
        Assert.AreEqual(Description,_projectAddedToRepository.Description);
        Assert.AreEqual(ProjectName,_projectAddedToRepository.Name);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldUpdateProject_WhenProjectExists()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var lastUpdated = DateTime.Now;
        const string Description = "projectDescription";
        const string ProjectName = "ProjectName";
        var testEvent = new ProjectEvent("", Description, false, lastUpdated, "plant", guid, ProjectName, null);
        //simulate project received from db to be one hour "older" than event coming in.
        var projectToUpdate = new Project("AnyPlant",guid, "AnyProjectName", "AnyDescription");
        projectToUpdate.ProCoSys4LastUpdated =DateTime.Now.Subtract(TimeSpan.FromHours(1));
        _projectRepoMock.ExistsAsync(guid, default).Returns(true);
        _projectRepoMock.GetAsync(guid, default).Returns(projectToUpdate);
        _contextMock.Message.Returns(testEvent);

        //Act
        await _projectEventConsumer.Consume(_contextMock);
        
        //Assert
        Assert.IsNull(_projectAddedToRepository);
        Assert.AreEqual(guid,projectToUpdate.Guid);
        Assert.AreEqual(false,projectToUpdate.IsClosed);
        Assert.AreEqual(lastUpdated,projectToUpdate.ProCoSys4LastUpdated);
        Assert.AreEqual(Description,projectToUpdate.Description);
        Assert.AreEqual(ProjectName,projectToUpdate.Name);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task Consume_UsedProjectNameAsDescription_WhenDescriptionIsNull()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var lastUpdated = DateTime.Now;
        const string ProjectName = "ProjectName";
        var testEvent = new ProjectEvent("", null, false, lastUpdated, "plant", guid, ProjectName, null);
        _contextMock.Message.Returns(testEvent);
        _projectRepoMock.ExistsAsync(guid, default).Returns(false);
        
        //Act
        await _projectEventConsumer.Consume(_contextMock);
        
        //Assert
        Assert.IsNotNull(_projectAddedToRepository);
        Assert.AreEqual(ProjectName,_projectAddedToRepository.Description);
        Assert.AreEqual(ProjectName,_projectAddedToRepository.Name);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoProCoSysGuid()
    {
        //Arrange
        var testEvent = new ProjectEvent("", 
            "project", 
            false, 
            DateTime.MinValue, 
            "plant", Guid.Empty, 
            "ProjectName", 
            null);
        _contextMock.Message.Returns(testEvent);
        
        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() 
            => _projectEventConsumer.Consume(_contextMock),"Message is missing ProCoSysGuid");
    }
    
    [TestMethod]
    public async Task Consume_ShouldRemoveProject_IfMessageHasDeleteBehavior()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var testEvent = new ProjectEvent(
            "", 
            "project", 
            false, 
            DateTime.Now.AddDays(1),
            "plant", guid,
            "ProjectName", 
            "delete");

        var toDelete = new Project("AnyPlant",guid,"AnyProjectName","AnyDescription");
        toDelete.ProCoSys4LastUpdated = DateTime.Now;
        _projectRepoMock.GetAsync(guid, default).Returns(toDelete);
        _contextMock.Message.Returns(testEvent);

        //Act
        await _projectEventConsumer.Consume(_contextMock);
        
        //Assert
        _projectRepoMock.Received(1).Remove(toDelete);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
        
    [TestMethod]
    public async Task Consume_ShouldIgnoreMessage_IfMessageIsOutdated()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var testEvent = new ProjectEvent("", "project", false, DateTime.Now, "plant", guid,
            "ProjectName", null);

        var project = new Project("AnyPlant",guid,"AnyProjectName","AnyDescription");
        project.ProCoSys4LastUpdated = DateTime.Now.AddDays(1);
        _projectRepoMock.ExistsAsync(guid, default).Returns(true);
        _projectRepoMock.GetAsync(guid, default).Returns(project);
        _contextMock.Message.Returns(testEvent);

        //Act
        await _projectEventConsumer.Consume(_contextMock);
        
        //Assert
        await _projectRepoMock.Received(1).GetAsync(guid, default);
        _projectRepoMock.Received(0).Remove(project);
        _projectRepoMock.Received(0).Add(project);
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldIgnoreMessage_IfLastUpdatedHasNotChanged()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var lastUpdated = DateTime.Now;
        var testEvent = new ProjectEvent("", "project", false, lastUpdated, "plant", guid,
            "ProjectName", null);

        var project = new Project("AnyPlant",guid,"AnyProjectName","AnyDescription")
        {
            ProCoSys4LastUpdated = lastUpdated
        };
        _projectRepoMock.ExistsAsync(guid, default).Returns(true);
        _projectRepoMock.GetAsync(guid, default).Returns(project);
        _contextMock.Message.Returns(testEvent);

        //Act
        await _projectEventConsumer.Consume(_contextMock);
        
        //Assert
        await _projectRepoMock.Received(1).GetAsync(guid, default);
        _projectRepoMock.Received(0).Remove(project);
        _projectRepoMock.Received(0).Add(project);
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }
}

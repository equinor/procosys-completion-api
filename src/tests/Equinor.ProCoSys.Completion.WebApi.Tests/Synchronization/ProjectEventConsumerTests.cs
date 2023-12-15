using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.WebApi.Authentication;
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
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly ProjectEventConsumer _projectEventConsumer;
    private readonly IOptions<CompletionAuthenticatorOptions> _optionsMock = Substitute.For<IOptions<CompletionAuthenticatorOptions>>();
    private readonly ConsumeContext<ProjectEvent> _contextMock = Substitute.For<ConsumeContext<ProjectEvent>>();

    public ProjectEventConsumerTests() =>
        _projectEventConsumer = new ProjectEventConsumer(Substitute.For<ILogger<ProjectEventConsumer>>(), _projectRepoMock, 
            _unitOfWorkMock, Substitute.For<ICurrentUserSetter>(), _optionsMock);

    [TestInitialize]
    public void Setup() =>
        _optionsMock.Value.Returns(new CompletionAuthenticatorOptions
        {
            CompletionApiObjectId = new Guid()
        });

    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoProCoSysGuid()
    {
        //Arrange
        var testEvent = new ProjectEvent("", "project", false, DateTime.MinValue, "plant", Guid.Empty,
                "ProjectName", null);
        var contextMock = Substitute.For<ConsumeContext<ProjectEvent>>();
        contextMock.Message.Returns(testEvent);
        
        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _projectEventConsumer.Consume(contextMock),"Message is missing ProCoSysGuid");
    }
    
    [TestMethod]
    public async Task Consume_ShouldRemoveProject_IfMessageContainsDelete()
    {
        //Arrange
        var proCoSysGuidForDeletedProject = Guid.NewGuid();
        var testEvent = new ProjectEvent("", "project", false, DateTime.Now.AddDays(1), "plant", proCoSysGuidForDeletedProject,
            "ProjectName", "delete");

        var projectToDelete = new Project("AnyPlant",proCoSysGuidForDeletedProject,"AnyProjectName","AnyDescription",DateTime.Now);
        _projectRepoMock.GetAsync(proCoSysGuidForDeletedProject, default).Returns(projectToDelete);
        _contextMock.Message.Returns(testEvent);

        //Act
        await _projectEventConsumer.Consume(_contextMock);
        
        //Assert
        _projectRepoMock.Received(1).Remove(projectToDelete);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }
        
    [TestMethod]
    public async Task Consume_ShouldIgnoreMessage_IfMessageIsOutdated()
    {
        //Arrange
        var proCoSysGuidForProjectToUpdate = Guid.NewGuid();
        var testEvent = new ProjectEvent("", "project", false, DateTime.Now, "plant", proCoSysGuidForProjectToUpdate,
            "ProjectName", null);

        var project = new Project("AnyPlant",proCoSysGuidForProjectToUpdate,"AnyProjectName","AnyDescription",DateTime.Now.AddDays(1));
        _projectRepoMock.ExistsAsync(proCoSysGuidForProjectToUpdate, default).Returns(true);
        _projectRepoMock.GetAsync(proCoSysGuidForProjectToUpdate, default).Returns(project);
        _contextMock.Message.Returns(testEvent);

        //Act
        await _projectEventConsumer.Consume(_contextMock);
        
        //Assert
        await _projectRepoMock.Received(1).GetAsync(proCoSysGuidForProjectToUpdate, default);
        _projectRepoMock.Received(0).Remove(project);
        _projectRepoMock.Received(0).Add(project);
        await _unitOfWorkMock.Received(0).SaveChangesAsync(default);
    }
    
}

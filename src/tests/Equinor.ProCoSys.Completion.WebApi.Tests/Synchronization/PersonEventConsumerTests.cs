using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.WebApi.Authentication;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Person = Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate.Person;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class PersonEventConsumerTests
{
    private readonly IPersonRepository _personRepoMock = Substitute.For<IPersonRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly PersonEventConsumer _personEventConsumer;
    private readonly IOptionsMonitor<CompletionAuthenticatorOptions> _optionsMock = Substitute.For<IOptionsMonitor<CompletionAuthenticatorOptions>>();
    private readonly ConsumeContext<PersonEvent> _contextMock = Substitute.For<ConsumeContext<PersonEvent>>();
    private Project? _projectAddedToRepository;

    public PersonEventConsumerTests() =>
        _personEventConsumer = new PersonEventConsumer(Substitute.For<ILogger<PersonEventConsumer>>(), _personRepoMock, 
            _unitOfWorkMock, Substitute.For<ICurrentUserSetter>(), _optionsMock);

    [TestInitialize]
    public void Setup()
    {
        _optionsMock.CurrentValue.Returns(new CompletionAuthenticatorOptions { CompletionApiObjectId = new Guid() });
        
        _personRepoMock
            .When(x => x.Add(Arg.Any<Person>()))
            .Do(callInfo =>
            {
                _projectAddedToRepository = callInfo.Arg<Project>();
            });
    }
    
    [TestMethod]
    public async Task Consume_ShouldUpdatePerson_WhenPersonExists()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var testEvent = new PersonEvent(
            "", 
            guid, 
            "Average Max", 
            "Joe", 
            "AJOE",
            "average.joe@equinor.com", 
            true, 
            DateTime.Now.AddDays(1), 
            null);
        //simulate person received from completion db to be one hour "older" than event coming in.
        var personToUpdate = new Person(
            guid, 
            "Average", 
            "Joe", 
            "AJOE", 
            "average.joe@equinor.com", 
            false, 
            DateTime.Now);
        
        _personRepoMock.ExistsAsync(guid, default).Returns(true);
        _personRepoMock.GetAsync(guid, default).Returns(personToUpdate);
        _contextMock.Message.Returns(testEvent);

        //Act
        await _personEventConsumer.Consume(_contextMock);
        
        //Assert
        Assert.IsNull(_projectAddedToRepository);
        Assert.AreEqual(guid, personToUpdate.Guid);
        Assert.AreEqual("Average Max", personToUpdate.FirstName);
        Assert.AreEqual(true, personToUpdate.Superuser);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }
    
    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoProCoSysGuid()
    {
        //Arrange
        var testEvent = new PersonEvent("", Guid.Empty, "Average Max", "Joe", "AJOE", "average.joe@equinor.com", true, DateTime.Now, null);
        _contextMock.Message.Returns(testEvent);
        
        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() 
            => _personEventConsumer.Consume(_contextMock),"Message is missing ProCoSysGuid");
    }

    [TestMethod]
    public async Task Consume_ShouldIgnoreMessage_IfMessageIsOutdated()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var testEvent = new PersonEvent(
            "",
            guid,
            "Average Max",
            "Joe",
            "AJOE",
            "average.joe@equinor.com",
            true,
            DateTime.Now,
            null);

        var person = new Person(
            guid, 
            "Average", 
            "Joe", 
            "AJOE", 
            "average.joe@equinor.com", 
            false, 
            DateTime.Now.AddDays(1));
        _personRepoMock.ExistsAsync(guid, default).Returns(true);
        _personRepoMock.GetAsync(guid, default).Returns(person);
        _contextMock.Message.Returns(testEvent);

        //Act
        await _personEventConsumer.Consume(_contextMock);

        //Assert
        await _personRepoMock.Received(1).GetAsync(guid, default);
        _personRepoMock.Received(0).Remove(person);
        _personRepoMock.Received(0).Add(person);
        await _unitOfWorkMock.Received(0).SaveChangesAsync(default);
    }
}

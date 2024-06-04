using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class PersonEventConsumerTests
{
    private readonly IPersonRepository _personRepoMock = Substitute.For<IPersonRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly PersonEventConsumer _personEventConsumer;
    private readonly IOptionsMonitor<ApplicationOptions> _azureAdOptionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
    private readonly ConsumeContext<PersonEvent> _contextMock = Substitute.For<ConsumeContext<PersonEvent>>();

    public PersonEventConsumerTests() =>
        _personEventConsumer = new PersonEventConsumer(Substitute.For<ILogger<PersonEventConsumer>>(), _personRepoMock, 
            _unitOfWorkMock);

    [TestInitialize]
    public void Setup() => _azureAdOptionsMock.CurrentValue.Returns(new ApplicationOptions { ObjectId = new Guid() });

    [TestMethod]
    public async Task Consume_ShouldUpdatePerson_WhenPersonExists()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var testEvent = new PersonEvent(
            "",
            Guid.NewGuid(),
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
            false);
        
        _personRepoMock.ExistsAsync(guid, default).Returns(true);
        _personRepoMock.GetAsync(guid, default).Returns(personToUpdate);
        _contextMock.Message.Returns(testEvent);

        //Act
        await _personEventConsumer.Consume(_contextMock);
        
        //Assert
        Assert.AreEqual(guid, personToUpdate.Guid);
        Assert.AreEqual("Average Max", personToUpdate.FirstName);
        Assert.AreEqual(true, personToUpdate.Superuser);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldIgnoreMessage_IfMessageIsOutdated()
    {
        //Arrange
        var azureOid = Guid.NewGuid();
        var testEvent = new PersonEvent(
            "",
            Guid.NewGuid(),
            azureOid,
            "Average Max",
            "Joe",
            "AJOE",
            "average.joe@equinor.com",
            true,
            DateTime.Now,
            null);

        var person = new Person(
            azureOid, 
            "Average", 
            "Joe", 
            "AJOE", 
            "average.joe@equinor.com", 
            false);

        person.ProCoSys4LastUpdated = DateTime.Now.AddDays(1);
        _personRepoMock.ExistsAsync(azureOid, default).Returns(true);
        _personRepoMock.GetAsync(azureOid, default).Returns(person);
        _contextMock.Message.Returns(testEvent);

        //Act
        await _personEventConsumer.Consume(_contextMock);

        //Assert
        await _personRepoMock.Received(1).GetAsync(azureOid, default);
        _personRepoMock.Received(0).Remove(person);
        _personRepoMock.Received(0).Add(person);
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task Consume_ShouldIgnoreMessage_IfLastUpdatedHasNotChanged()
    {
        //Arrange
        var azureOid = Guid.NewGuid();
        var lastUpdated = DateTime.Now;
        var testEvent = new PersonEvent("", Guid.Empty, azureOid, "Average Max", "Joe", "AJOE", "average.joe@equinor.com", true,
            lastUpdated,
            null);
        var person = new Person(azureOid, "Average", "Joe", "AJOE", "average.joe@equinor.com", false)
        {
            ProCoSys4LastUpdated = lastUpdated
        };

        _personRepoMock.ExistsAsync(azureOid, default).Returns(true);
        _personRepoMock.GetAsync(azureOid, default).Returns(person);
        _contextMock.Message.Returns(testEvent);

        //Act
        await _personEventConsumer.Consume(_contextMock);

        //Assert
        await _personRepoMock.Received(1).GetAsync(azureOid, default);
        _personRepoMock.Received(0).Remove(person);
        _personRepoMock.Received(0).Add(person);
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldIgnoreMessage_WhenPersonDoesNotExist()
    {
        //Arrange
        var azureOid = Guid.NewGuid();
        var testEvent = new PersonEvent(
            "",
            Guid.Empty,
            azureOid,
            "Average Max",
            "Joe",
            "AJOE",
            "average.joe@equinor.com",
            true,
            DateTime.Now,
            null);

        _personRepoMock.ExistsAsync(azureOid, default).Returns(false);
        _contextMock.Message.Returns(testEvent);

        //Act
        await _personEventConsumer.Consume(_contextMock);

        //Assert
        await _personRepoMock.Received(1).ExistsAsync(azureOid, default);
        await _personRepoMock.Received(0).GetAsync(azureOid, default);
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldIgnoreMessage_WhenBehaviorDelete()
    {
        //Arrange
        var azureOid = Guid.NewGuid();
        var testEvent = new PersonEvent(
            "",
            Guid.Empty,
            azureOid,
            "Average Max",
            "Joe",
            "AJOE",
            "average.joe@equinor.com",
            true,
            DateTime.Now,
            "delete");

        _contextMock.Message.Returns(testEvent);

        //Act
        await _personEventConsumer.Consume(_contextMock);

        //Assert
        await _personRepoMock.Received(0).ExistsAsync(azureOid, default);
    }

}

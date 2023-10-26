using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.Completion.Command.PersonCommands.CreatePerson;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PersonCommands.CreatePerson;

[TestClass]
public class CreatePersonCommandHandlerTests : TestsBase
{
    private IPersonCache _personCacheMock;
    private IPersonRepository _personRepositoryMock;
    private IOptionsMonitor<ApplicationOptions> _optionsMock;
    private const string UserName = "VP";
    private const string FistName = "Vippe";
    private const string LastName = "Tangen";
    private const string Email = "vp@pcs.com";
    private const string SpEmail = "noreply@pcs.com";
    private const string AzureOid = "8d508aa7-b753-4cb7-b084-cf5508c8ac17";
    private readonly Guid _azureOid = new (AzureOid);

    private const int PersonIdOnNew = 1;

    private Person _personAddedToRepository;
    private ProCoSysPerson _proCoSysPerson;

    private CreatePersonCommandHandler _dut;
    private CreatePersonCommand _command;

    [TestInitialize]
    public void Setup()
    { 
        _personRepositoryMock = Substitute.For<IPersonRepository>();
        _personRepositoryMock
            .When(x => x.Add(Arg.Any<Person>()))
            .Do(info =>
            {
                var person = info.Arg<Person>();
                _personAddedToRepository = person;
                person.SetProtectedIdForTesting(PersonIdOnNew);
            });

            
        _proCoSysPerson = new ProCoSysPerson
        {
            UserName = UserName,
            FirstName = FistName,
            LastName = LastName,
            Email = Email,
            AzureOid = AzureOid,
            ServicePrincipal = false
        };
        _personCacheMock = Substitute.For<IPersonCache>();
        _personCacheMock
            .GetAsync(_azureOid)
            .Returns(_proCoSysPerson);

        _optionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
        _optionsMock.CurrentValue.Returns(
            new ApplicationOptions
            {
                ServicePrincipalMail = SpEmail
            });

        _command = new CreatePersonCommand(_azureOid);

        _dut = new CreatePersonCommandHandler(
            _personCacheMock,
            _personRepositoryMock,
            _unitOfWorkMock,
            _optionsMock,
            Substitute.For<ILogger<CreatePersonCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddPersonToRepository_WhenPersonNotExists()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNotNull(_personAddedToRepository);
        Assert.AreEqual(PersonIdOnNew, _personAddedToRepository.Id);
        Assert.AreEqual(UserName, _personAddedToRepository.UserName);
        Assert.AreEqual(FistName, _personAddedToRepository.FirstName);
        Assert.AreEqual(LastName, _personAddedToRepository.LastName);
        Assert.AreEqual(Email, _personAddedToRepository.Email);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddServicePrincipalToRepository_WhenPersonNotExists()
    {
        // Arrange
        _proCoSysPerson.Email = null;
        _proCoSysPerson.ServicePrincipal = true;

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNotNull(_personAddedToRepository);
        Assert.AreEqual(PersonIdOnNew, _personAddedToRepository.Id);
        Assert.AreEqual(UserName, _personAddedToRepository.UserName);
        Assert.AreEqual(FistName, _personAddedToRepository.FirstName);
        Assert.AreEqual(LastName, _personAddedToRepository.LastName);
        Assert.AreEqual(SpEmail, _personAddedToRepository.Email);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPersonToRepository_WhenPersonAlreadyExists()
    {
        // Arrange
        _personRepositoryMock.ExistsAsync(_azureOid).Returns(true);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNull(_personAddedToRepository);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await  _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldThrowException_WhenPersonNotInCache()
    {
        // Arrange
        _personCacheMock.GetAsync(_azureOid).Returns((ProCoSysPerson)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));
    }
}

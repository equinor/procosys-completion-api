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
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PersonCommands.CreatePerson;

[TestClass]
public class CreatePersonCommandHandlerTests : TestsBase
{
    private Mock<IPersonCache> _personCacheMock;
    private Mock<IPersonRepository> _personRepositoryMock;
    private Mock<IOptionsMonitor<ApplicationOptions>> _optionsMock;
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
        _personRepositoryMock = new Mock<IPersonRepository>();
        _personRepositoryMock
            .Setup(x => x.Add(It.IsAny<Person>()))
            .Callback<Person>(person =>
            {
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
        _personCacheMock = new Mock<IPersonCache>();
        _personCacheMock
            .Setup(x => x.GetAsync(_azureOid))
            .ReturnsAsync(_proCoSysPerson);

        _optionsMock = new Mock<IOptionsMonitor<ApplicationOptions>>();
        _optionsMock.Setup(o => o.CurrentValue).Returns(
            new ApplicationOptions
            {
                ServicePrincipalMail = SpEmail
            });

        _command = new CreatePersonCommand(_azureOid);

        _dut = new CreatePersonCommandHandler(
            _personCacheMock.Object,
            _personRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _optionsMock.Object,
            new Mock<ILogger<CreatePersonCommandHandler>>().Object);
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
        _personRepositoryMock.Setup(p => p.GetByGuidAsync(_azureOid))
            .ReturnsAsync(new Person(_azureOid, FistName, LastName, UserName, Email));

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
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldThrowException_WhenPersonNotInCache()
    {
        // Arrange
        _personCacheMock.Setup(x => x.GetAsync(_azureOid)).ReturnsAsync((ProCoSysPerson)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));
    }
}

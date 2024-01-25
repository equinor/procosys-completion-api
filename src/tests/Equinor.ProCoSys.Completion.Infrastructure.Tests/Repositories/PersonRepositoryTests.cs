using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class PersonRepositoryTests : EntityWithGuidRepositoryTestBase<Person>
{
    private readonly ICurrentUserProvider _userProviderMock = Substitute.For<ICurrentUserProvider>();
    private readonly IPersonCache _personCacheMock = Substitute.For<IPersonCache>();
    private readonly Guid _nonExistingPersonOid = Guid.NewGuid();
    private Person _existingPerson;
    private ProCoSysPerson _proCoSysPerson;
    private Person _personAddedToRepository;

    protected override EntityWithGuidRepository<Person> GetDut()
        => new PersonRepository(_contextHelper.ContextMock, _userProviderMock, _personCacheMock);

    protected override void SetupRepositoryWithOneKnownItem()
    {
        _proCoSysPerson = new ProCoSysPerson
        {
            UserName = "YODA",
            FirstName = "YO",
            LastName = "DA",
            Email = "@",
            AzureOid = _nonExistingPersonOid.ToString(),
            Super = true
        };

        _existingPerson = new Person(
            Guid.NewGuid(), 
            "FirstName",
            "LastName",
            "UNAME",
            "email@test.com", 
            false);
        _knownGuid = _existingPerson.Guid;
        _existingPerson.SetProtectedIdForTesting(_knownId);

        var persons = new List<Person> { _existingPerson };

        _dbSetMock = persons.AsQueryable().BuildMockDbSet();
        _dbSetMock
            .When(x => x.Add(Arg.Any<Person>()))
            .Do(callInfo =>
            {
                _personAddedToRepository = callInfo.Arg<Person>();
            });

        _contextHelper
            .ContextMock
            .Persons
            .Returns(_dbSetMock);
    }

    protected override Person GetNewEntity() => new (Guid.NewGuid(), "New", "Person", "NP", "@", false);

    [TestMethod]
    public async Task GetOrCreateAsync_WithExistingPerson_ShouldReturnExistingPerson()
    {
        // Arrange
        var dut = new PersonRepository(_contextHelper.ContextMock, _userProviderMock, _personCacheMock);

        // Act
        var result = await dut.GetOrCreateAsync(_knownGuid, default);

        // Assert
        Assert.AreEqual(_existingPerson, result);
        _dbSetMock.Received(0).Add(Arg.Any<Person>());
        Assert.IsNull(_personAddedToRepository);
    }

    [TestMethod]
    public async Task GetOrCreateAsync_WithNonExistingPerson_ShouldReturnCreatedPerson()
    {
        // Arrange
        var dut = new PersonRepository(_contextHelper.ContextMock, _userProviderMock, _personCacheMock);
        _personCacheMock.GetAsync(_nonExistingPersonOid)
            .Returns(_proCoSysPerson);

        // Act
        var result = await dut.GetOrCreateAsync(_nonExistingPersonOid, default);

        // Assert
        Assert.AreEqual(_nonExistingPersonOid, result.Guid);
        Assert.AreEqual(_proCoSysPerson.AzureOid, result.Guid.ToString());
        Assert.AreEqual(_proCoSysPerson.UserName, result.UserName);
        Assert.AreEqual(_proCoSysPerson.FirstName, result.FirstName);
        Assert.AreEqual(_proCoSysPerson.LastName, result.LastName);
        Assert.AreEqual(_proCoSysPerson.Email, result.Email);
        Assert.AreEqual(_proCoSysPerson.Super, result.Superuser);
    }

    [TestMethod]
    public async Task GetOrCreateAsync_WithNonExistingPerson_ShouldAddPersonToRepository()
    {
        // Arrange
        var dut = new PersonRepository(_contextHelper.ContextMock, _userProviderMock, _personCacheMock);
        _personCacheMock.GetAsync(_nonExistingPersonOid)
            .Returns(_proCoSysPerson);

        // Act
        await dut.GetOrCreateAsync(_nonExistingPersonOid, default);

        // Assert
        Assert.IsNotNull(_personAddedToRepository);
        Assert.AreEqual(_nonExistingPersonOid, _personAddedToRepository.Guid);
        Assert.AreEqual(_proCoSysPerson.AzureOid, _personAddedToRepository.Guid.ToString());
        Assert.AreEqual(_proCoSysPerson.UserName, _personAddedToRepository.UserName);
        Assert.AreEqual(_proCoSysPerson.FirstName, _personAddedToRepository.FirstName);
        Assert.AreEqual(_proCoSysPerson.LastName, _personAddedToRepository.LastName);
        Assert.AreEqual(_proCoSysPerson.Email, _personAddedToRepository.Email);
        Assert.AreEqual(_proCoSysPerson.Super, _personAddedToRepository.Superuser);
        _dbSetMock.Received(1).Add(Arg.Any<Person>());
    }

    [TestMethod]
    public async Task GetOrCreateManyAsync_WithExistingPerson_ShouldReturnExistingPerson()
    {
        // Arrange
        var dut = new PersonRepository(_contextHelper.ContextMock, _userProviderMock, _personCacheMock);

        // Act
        var result = await dut.GetOrCreateManyAsync(new List<Guid> { _knownGuid }, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        var person = result.ElementAt(0);
        Assert.IsNotNull(person);
        Assert.AreEqual(_existingPerson, person);
        _dbSetMock.Received(0).Add(Arg.Any<Person>());
        Assert.IsNull(_personAddedToRepository);
    }

    [TestMethod]
    public async Task GetOrCreateManyAsync_WithNonExistingPerson_ShouldReturnCreatedPerson()
    {
        // Arrange
        var dut = new PersonRepository(_contextHelper.ContextMock, _userProviderMock, _personCacheMock);
        _personCacheMock.GetAsync(_nonExistingPersonOid)
            .Returns(_proCoSysPerson);

        // Act
        var result = await dut.GetOrCreateManyAsync(new List<Guid> { _nonExistingPersonOid }, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        var person = result.ElementAt(0);
        Assert.IsNotNull(person);
        Assert.AreEqual(_nonExistingPersonOid, person.Guid);
        Assert.AreEqual(_proCoSysPerson.AzureOid, person.Guid.ToString());
        Assert.AreEqual(_proCoSysPerson.UserName, person.UserName);
        Assert.AreEqual(_proCoSysPerson.FirstName, person.FirstName);
        Assert.AreEqual(_proCoSysPerson.LastName, person.LastName);
        Assert.AreEqual(_proCoSysPerson.Email, person.Email);
        Assert.AreEqual(_proCoSysPerson.Super, person.Superuser);
    }

    [TestMethod]
    public async Task GetOrCreateManyAsync_WithNonExistingPerson_ShouldAddPersonToRepository()
    {
        // Arrange
        var dut = new PersonRepository(_contextHelper.ContextMock, _userProviderMock, _personCacheMock);
        _personCacheMock.GetAsync(_nonExistingPersonOid)
            .Returns(_proCoSysPerson);

        // Act
        var result = await dut.GetOrCreateManyAsync(new List<Guid> { _nonExistingPersonOid }, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(_personAddedToRepository);
        Assert.AreEqual(_nonExistingPersonOid, _personAddedToRepository.Guid);
        Assert.AreEqual(_proCoSysPerson.AzureOid, _personAddedToRepository.Guid.ToString());
        Assert.AreEqual(_proCoSysPerson.UserName, _personAddedToRepository.UserName);
        Assert.AreEqual(_proCoSysPerson.FirstName, _personAddedToRepository.FirstName);
        Assert.AreEqual(_proCoSysPerson.LastName, _personAddedToRepository.LastName);
        Assert.AreEqual(_proCoSysPerson.Email, _personAddedToRepository.Email);
        Assert.AreEqual(_proCoSysPerson.Super, _personAddedToRepository.Superuser);
        _dbSetMock.Received(1).Add(Arg.Any<Person>());
    }

    [TestMethod]
    public async Task GetOrCreateManyAsync_WithExistingAndNonExistingPersons_ShouldReturnMultiplePersons()
    {
        // Arrange
        var dut = new PersonRepository(_contextHelper.ContextMock, _userProviderMock, _personCacheMock);
        _personCacheMock.GetAsync(_nonExistingPersonOid)
            .Returns(_proCoSysPerson);

        // Act
        var result = await dut.GetOrCreateManyAsync(new List<Guid> { _knownGuid, _nonExistingPersonOid }, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(p => p.Guid == _knownGuid));
        Assert.IsTrue(result.Any(p => p.Guid == _nonExistingPersonOid));
    }
}

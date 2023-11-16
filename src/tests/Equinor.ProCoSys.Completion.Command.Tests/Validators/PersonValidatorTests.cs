using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Command.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.Validators;

[TestClass]
public class PersonValidatorTests : ReadOnlyTestsBase
{

    private Person _person = null!;
    private IPersonCache _personCacheMock = null!;
    private readonly Guid _localPersonOid = Guid.NewGuid();
    private readonly Guid _pcsPersonOid = Guid.NewGuid();

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        _person = new Person(_localPersonOid, "A", "B", "C", "@");
        context.Persons.Add(_person);

        context.SaveChangesAsync().Wait();

        _personCacheMock = Substitute.For<IPersonCache>();
    }

    #region ExistsLocalOrInProCoSysAsync
    [TestMethod]
    public async Task ExistsLocalOrInProCoSysAsync_ShouldReturnTrue_WhenLocalPersonExists()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);            
        var dut = new PersonValidator(context, _personCacheMock);

        // Act
        var result = await dut.ExistsLocalOrInProCoSysAsync(_localPersonOid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsLocalOrInProCoSysAsync_ShouldReturnTrue_WhenLocalPersonNotExists_ButExistsInProCoSysAsPerson()
    {
        // Arrange
        _personCacheMock.GetAsync(_pcsPersonOid).Returns(new ProCoSysPerson
        {
            AzureOid = _pcsPersonOid.ToString(),
            ServicePrincipal = false
        });
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PersonValidator(context, _personCacheMock);

        // Act
        var result = await dut.ExistsLocalOrInProCoSysAsync(_pcsPersonOid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsLocalOrInProCoSysAsync_ShouldReturnFalse_WhenLocalPersonNotExists_ButExistsInProCoSysAsServicePrincipal()
    {
        // Arrange
        _personCacheMock.GetAsync(_pcsPersonOid).Returns(new ProCoSysPerson
        {
            AzureOid = _pcsPersonOid.ToString(),
            ServicePrincipal = true
        });
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PersonValidator(context, _personCacheMock);

        // Act
        var result = await dut.ExistsLocalOrInProCoSysAsync(_pcsPersonOid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ExistsLocalOrInProCoSysAsync_ShouldReturnFalse_WhenPersonNotExist_NeitherLocalOrInProCoSys()
    {
        // Arrange
        var unknownOid = Guid.NewGuid();
        _personCacheMock.GetAsync(unknownOid).Returns((ProCoSysPerson)null);
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PersonValidator(context, _personCacheMock);

        // Act
        var result = await dut.ExistsLocalOrInProCoSysAsync(unknownOid, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}

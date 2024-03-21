using Equinor.ProCoSys.Common.Caches;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Test.Common;
using System;
using System.Collections;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.Persons;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.ForeignApiTests.MainApi.Persons;

[TestClass]
public class PersonCacheTests
{
    private PersonCache _dut;
    private IPersonApiService _personApiServiceMock;
    private IOptionsMonitor<CacheOptions> _optionsMock;
    private readonly string _testPlant = "PA";

    [TestInitialize]
    public void Setup()
    {
        TimeService.SetProvider(new ManualTimeProvider(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

        _personApiServiceMock = Substitute.For<IPersonApiService>();
        _personApiServiceMock.GetAllPersonsAsync(_testPlant, CancellationToken.None).Returns([
            new() {
                AzureOid = "asdf-fghj-qwer-tyui",
                Email = "test@email.com",
                FirstName = "Ola",
                LastName = "Hansen",
                Username = "oha@mail.com",
                Id = 5
            },
            new() {
                AzureOid = "1234-4567-6789-5432",
                Email = "test2@email.com",
                FirstName = "Hans",
                LastName = "Olsen",
                Username = "hans@mail.com",
                Id = 5
            }
        ]);

        _optionsMock = Substitute.For<IOptionsMonitor<CacheOptions>>();
        _optionsMock.CurrentValue.Returns(new CacheOptions {  PersonCacheMinutes = 300 });

        _dut = new PersonCache(new CacheManager(), _personApiServiceMock, _optionsMock);
    }

    [TestMethod]
    public async Task GetAllPersons_ShouldReturnPersonListFromPersonApiServiceFirstTime()
    {
        // Act
        var result = await _dut.GetAllPersonsAsync(_testPlant, CancellationToken.None);

        // Assert
        AssertResult(result);
        await _personApiServiceMock.Received(1).GetAllPersonsAsync(_testPlant, CancellationToken.None);
    }

    [TestMethod]
    public async Task GetAllPersons_ShouldReturnPersonListsFromCacheSecondTime()
    {
        await _dut.GetAllPersonsAsync(_testPlant, CancellationToken.None);

        // Act
        var result = await _dut.GetAllPersonsAsync(_testPlant, CancellationToken.None);

        // Assert
        AssertResult(result);
        // since GetCheckListAsync has been called twice, but TryGetCheckListByOidAsync has been called once, the second Get uses cache
        await _personApiServiceMock.Received(1).GetAllPersonsAsync(_testPlant, CancellationToken.None);
    }

    private static void AssertResult(ICollection list) => Assert.IsTrue(list.Count == 2);
}

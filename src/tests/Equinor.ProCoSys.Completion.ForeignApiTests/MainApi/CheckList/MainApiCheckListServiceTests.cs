using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.ForeignApiTests.MainApi.CheckList;

[TestClass]
public class MainApiCheckListServiceTests
{
    private MainApiCheckListService _dut;
    private IOptionsMonitor<ApplicationOptions> _applicationOptionsMock;
    private IMainApiClientForApplication _mainApiClientForApplication;
    private IOptionsMonitor<MainApiOptions> _mainApiOptionsMock;

    [TestInitialize]
    public void Setup()
    {
        _applicationOptionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
        _applicationOptionsMock.CurrentValue.Returns(new ApplicationOptions { CheckListCacheExpirationMinutes = 1 });
        _mainApiOptionsMock = Substitute.For<IOptionsMonitor<MainApiOptions>>();
        _mainApiOptionsMock.CurrentValue.Returns(new MainApiOptions
        {
            BaseAddress = "https://backend-xxxxxxxx-yyyyyyyyyy-api-test.radix.zzzzzzz.com/api",
            ApiVersion = "4.1"
        });

        _mainApiClientForApplication = Substitute.For<IMainApiClientForApplication>();
        _mainApiClientForApplication
            .TryQueryAndDeserializeAsync<List<ProCoSys4CheckList>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([new ProCoSys4CheckList(Guid.Empty, "FT", "FG", "R", "TRC", "TRD", "TFC", "TFD", false, Guid.Empty)]);
        _dut = new MainApiCheckListService(
            _mainApiClientForApplication,
            _mainApiOptionsMock,
            _applicationOptionsMock);
    }

    [TestMethod]
    public async Task GetManyCheckListsAsync_With1CheckListGuid_ShouldCall_MainApiClientOnce()
    {
        // Act
        await _dut.GetManyCheckListsAsync([Guid.NewGuid()], default);

        // Assert
        await _mainApiClientForApplication.Received(1)
            .TryQueryAndDeserializeAsync<List<ProCoSys4CheckList>>(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task GetManyCheckListsAsync_With42CheckListGuids_ShouldCall_MainApiClientTwice()
    {
        // Arrange
        List<Guid> checkListGuids = [];
        for (var i = 0; i < 42; i++)
        {
            checkListGuids.Add(Guid.NewGuid());
        }

        // Act
        await _dut.GetManyCheckListsAsync(checkListGuids, default);

        // Assert
        await _mainApiClientForApplication.Received(2)
            .TryQueryAndDeserializeAsync<List<ProCoSys4CheckList>>(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task GetManyCheckListsAsync_With42CheckListGuids_ShouldCall_MainApiClientWithCorrectUrls()
    {
        // Arrange
        _mainApiClientForApplication
            .When(x => x.TryQueryAndDeserializeAsync<List<ProCoSys4CheckList>>(Arg.Any<string>(), Arg.Any<CancellationToken>()))
            .Do(Callback.Always(callbackInfo =>
            {
                var url = callbackInfo.Arg<string>();
                Assert.IsTrue(url.Length < 2000);
            }));

        List<Guid> checkListGuids = [];
        for (var i = 0; i < 42; i++)
        {
            checkListGuids.Add(Guid.NewGuid());
        }

        // Act
        await _dut.GetManyCheckListsAsync(checkListGuids, default);

        // Assert
        await _mainApiClientForApplication.Received(2)
            .TryQueryAndDeserializeAsync<List<ProCoSys4CheckList>>(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task GetManyCheckListsAsync_With42CheckListGuids_ShouldReturnResultFromMainApiClientTwice()
    {
        // Arrange
        List<Guid> checkListGuids = [];
        for (var i = 0; i < 42; i++)
        {
            checkListGuids.Add(Guid.NewGuid());
        }

        // Act
        var result = await _dut.GetManyCheckListsAsync(checkListGuids, default);

        // Assert
        Assert.AreEqual(2, result.Count);
    }
}

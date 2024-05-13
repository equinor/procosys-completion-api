using System;
using Equinor.ProCoSys.Completion.Command.Email;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.Comments;

[TestClass]
public class DeepLinkUtilityTests : TestsBase
{
    private readonly string _baseUrl = "http://localhost/";
    private DeepLinkUtility _dut;
    private IOptionsMonitor<ApplicationOptions> _optionsMock;

    [TestInitialize]
    public void Setup()
    {
        _optionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
        _optionsMock.CurrentValue.Returns(new ApplicationOptions { BaseUrl = _baseUrl});
        _dut = new DeepLinkUtility(_plantProviderMock, _optionsMock);
    }

    [TestMethod]
    public void CreateUrl_ShouldCreateUrl_ForPunchType()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = _dut.CreateUrl(nameof(PunchItem), guid);

        // Assert
        var expected = $"{_baseUrl.TrimEnd('/')}/{TestPlantA[4..]}/Completion/PunchListItem/RedirectToPunchListItemView?punchListItemGuid={guid}";
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void CreateUrl_ShouldCreateValidUrl_WithoutAnyDoubleSlash()
    {
        // Act
        var result = _dut.CreateUrl(nameof(PunchItem), Guid.NewGuid());

        // Assert
        var uri = new Uri(result);
        var doubleSlashPos = uri.PathAndQuery.IndexOf("//", StringComparison.InvariantCulture);
        Assert.AreEqual(-1, doubleSlashPos);
    }

    [TestMethod]
    public void CreateUrl_ShouldThrowNotImplementedException_WhenUnknownType() =>
        // Act and Assert
        Assert.ThrowsException<NotImplementedException>(() => _dut.CreateUrl("UnknownEntityType", Guid.NewGuid()));
}

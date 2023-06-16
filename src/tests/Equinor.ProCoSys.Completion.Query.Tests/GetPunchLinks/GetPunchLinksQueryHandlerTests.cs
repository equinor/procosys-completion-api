using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchLinks;
using Equinor.ProCoSys.Completion.Query.Links;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Query.Tests.GetPunchLinks;

[TestClass]
public class GetPunchLinksQueryHandlerTests : TestsBase
{
    private GetPunchLinksQueryHandler _dut;
    private Mock<ILinkService> _linkServiceMock;
    private GetPunchLinksQuery _query;
    private LinkDto _linkDto;

    [TestInitialize]
    public void Setup()
    {
        _query = new GetPunchLinksQuery(Guid.NewGuid());

        _linkDto = new LinkDto(_query.PunchGuid, Guid.NewGuid(), "T", "U", "R");
        var linkDtos = new List<LinkDto>
        {
            _linkDto
        };
        _linkServiceMock = new Mock<ILinkService>();
        _linkServiceMock.Setup(l => l.GetAllForSourceAsync(_query.PunchGuid, default))
            .ReturnsAsync(linkDtos);

        _dut = new GetPunchLinksQueryHandler(_linkServiceMock.Object);
    }

    [TestMethod]
    public async Task HandlingQuery_ShouldReturn_Links()
    {
        // Act
        var result = await _dut.Handle(_query, default);

        // Assert
        Assert.IsInstanceOfType(result.Data, typeof(IEnumerable<LinkDto>));
        var link = result.Data.Single();
        Assert.AreEqual(_linkDto.SourceGuid, link.SourceGuid);
        Assert.AreEqual(_linkDto.Guid, link.Guid);
        Assert.AreEqual(_linkDto.Title, link.Title);
        Assert.AreEqual(_linkDto.Url, link.Url);
        Assert.AreEqual(_linkDto.RowVersion, link.RowVersion);
    }

    [TestMethod]
    public async Task HandlingQuery_Should_CallGetAllForSource_OnLinkService()
    {
        // Act
        await _dut.Handle(_query, default);

        // Assert
        _linkServiceMock.Verify(u => u.GetAllForSourceAsync(
            _query.PunchGuid,
            default), Times.Exactly(1));
    }
}

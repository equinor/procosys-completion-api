using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemLinks;
using Equinor.ProCoSys.Completion.Query.Links;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Query.Tests.GetPunchItemLinks;

[TestClass]
public class GetPunchItemLinksQueryHandlerTests : TestsBase
{
    private GetPunchItemLinksQueryHandler _dut;
    private Mock<ILinkService> _linkServiceMock;
    private GetPunchItemLinksQuery _query;
    private LinkDto _linkDto;

    [TestInitialize]
    public void Setup()
    {
        _query = new GetPunchItemLinksQuery(Guid.NewGuid());

        _linkDto = new LinkDto(_query.PunchItemGuid, Guid.NewGuid(), "T", "U", "R");
        var linkDtos = new List<LinkDto>
        {
            _linkDto
        };
        _linkServiceMock = new Mock<ILinkService>();
        _linkServiceMock.Setup(l => l.GetAllForSourceAsync(_query.PunchItemGuid, default))
            .ReturnsAsync(linkDtos);

        _dut = new GetPunchItemLinksQueryHandler(_linkServiceMock.Object);
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
            _query.PunchItemGuid,
            default), Times.Exactly(1));
    }
}

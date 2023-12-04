using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemLinks;
using Equinor.ProCoSys.Completion.Query.Links;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItemLinks;

[TestClass]
public class GetPunchItemLinksQueryHandlerTests : TestsBase
{
    private GetPunchItemLinksQueryHandler _dut;
    private ILinkService _linkServiceMock;
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
        _linkServiceMock = Substitute.For<ILinkService>();
        _linkServiceMock.GetAllForParentAsync(_query.PunchItemGuid, default)
            .Returns(linkDtos);

        _dut = new GetPunchItemLinksQueryHandler(_linkServiceMock);
    }

    [TestMethod]
    public async Task HandlingQuery_ShouldReturn_Links()
    {
        // Act
        var result = await _dut.Handle(_query, default);

        // Assert
        Assert.IsInstanceOfType(result.Data, typeof(IEnumerable<LinkDto>));
        var link = result.Data.Single();
        Assert.AreEqual(_linkDto.ParentGuid, link.ParentGuid);
        Assert.AreEqual(_linkDto.Guid, link.Guid);
        Assert.AreEqual(_linkDto.Title, link.Title);
        Assert.AreEqual(_linkDto.Url, link.Url);
        Assert.AreEqual(_linkDto.RowVersion, link.RowVersion);
    }

    [TestMethod]
    public async Task HandlingQuery_Should_CallGetAllForParent_OnLinkService()
    {
        // Act
        await _dut.Handle(_query, default);

        // Assert
        await _linkServiceMock.Received(1).GetAllForParentAsync(
            _query.PunchItemGuid,
            default);
    }
}

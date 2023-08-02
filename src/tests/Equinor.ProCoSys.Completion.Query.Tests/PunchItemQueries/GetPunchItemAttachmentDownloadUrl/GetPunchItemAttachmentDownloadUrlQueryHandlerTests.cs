using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.Attachments;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;

[TestClass]
public class GetPunchItemAttachmentDownloadUrlQueryHandlerTests : TestsBase
{
    private GetPunchItemAttachmentDownloadUrlQueryHandler _dut;
    private Mock<IAttachmentService> _attachmentServiceMock;
    private GetPunchItemAttachmentDownloadUrlQuery _query;
    private Uri _uri
        ;

    [TestInitialize]
    public void Setup()
    {
        _query = new GetPunchItemAttachmentDownloadUrlQuery(Guid.NewGuid(), Guid.NewGuid());

        _uri = new Uri("http://blah.blah.com");
        _attachmentServiceMock = new Mock<IAttachmentService>();
        _attachmentServiceMock.Setup(l => l.GetDownloadUriAsync(_query.AttachmentGuid, default))
            .ReturnsAsync(_uri);

        _dut = new GetPunchItemAttachmentDownloadUrlQueryHandler(_attachmentServiceMock.Object);
    }

    [TestMethod]
    public async Task HandlingQuery_ShouldReturnUri_WhenKnownAttachment()
    {
        // Act
        var result = await _dut.Handle(_query, default);

        // Assert
        Assert.IsInstanceOfType(result.Data, typeof(Uri));
        Assert.AreEqual(_uri, result.Data);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
    }

    [TestMethod]
    public async Task HandlingQuery_ShouldReturnNull_WhenUnknownAttachment()
    {
        // Arrange
        var query = new GetPunchItemAttachmentDownloadUrlQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _dut.Handle(query, default);

        // Assert
        Assert.IsNull(result.Data);
        Assert.AreEqual(ResultType.NotFound, result.ResultType);
    }

    [TestMethod]
    public async Task HandlingQuery_Should_CallGetDownloadUriAsync_OnAttachmentService()
    {
        // Act
        await _dut.Handle(_query, default);

        // Assert
        _attachmentServiceMock.Verify(u => u.GetDownloadUriAsync(
            _query.AttachmentGuid,
            default), Times.Exactly(1));
    }
}

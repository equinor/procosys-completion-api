using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.Attachments;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchAttachmentDownloadUrl;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.GetPunchAttachmentDownloadUrl;

[TestClass]
public class GetPunchAttachmentDownloadUrlQueryHandlerTests : TestsBase
{
    private GetPunchAttachmentDownloadUrlQueryHandler _dut;
    private Mock<IAttachmentService> _attachmentServiceMock;
    private GetPunchAttachmentDownloadUrlQuery _query;
    private Uri _uri
        ;

    [TestInitialize]
    public void Setup()
    {
        _query = new GetPunchAttachmentDownloadUrlQuery(Guid.NewGuid(), Guid.NewGuid());

        _uri = new Uri("http://blah.blah.com");
        _attachmentServiceMock = new Mock<IAttachmentService>();
        _attachmentServiceMock.Setup(l => l.TryGetDownloadUriAsync(_query.AttachmentGuid, default))
            .ReturnsAsync(_uri);

        _dut = new GetPunchAttachmentDownloadUrlQueryHandler(_attachmentServiceMock.Object);
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
        var query = new GetPunchAttachmentDownloadUrlQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _dut.Handle(query, default);

        // Assert
        Assert.IsNull(result.Data);
        Assert.AreEqual(ResultType.NotFound, result.ResultType);
    }

    [TestMethod]
    public async Task HandlingQuery_Should_CallTryGetDownloadUriAsync_OnAttachmentService()
    {
        // Act
        await _dut.Handle(_query, default);

        // Assert
        _attachmentServiceMock.Verify(u => u.TryGetDownloadUriAsync(
            _query.AttachmentGuid,
            default), Times.Exactly(1));
    }
}

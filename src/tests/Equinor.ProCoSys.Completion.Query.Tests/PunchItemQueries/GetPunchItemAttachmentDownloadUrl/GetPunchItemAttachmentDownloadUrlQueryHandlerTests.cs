using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Query.Attachments;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;

[TestClass]
public class GetPunchItemAttachmentDownloadUrlQueryHandlerTests : TestsBase
{
    private GetPunchItemAttachmentDownloadUrlQueryHandler _dut;
    private IAttachmentService _attachmentServiceMock;
    private GetPunchItemAttachmentDownloadUrlQuery _query;
    private Uri _uri;

    [TestInitialize]
    public void Setup()
    {
        _query = new GetPunchItemAttachmentDownloadUrlQuery(Guid.NewGuid(), Guid.NewGuid());

        _uri = new Uri("http://blah.blah.com");
        _attachmentServiceMock = Substitute.For<IAttachmentService>();
        _attachmentServiceMock.GetDownloadUriAsync(_query.AttachmentGuid, default)
            .Returns(_uri);

        _dut = new GetPunchItemAttachmentDownloadUrlQueryHandler(_attachmentServiceMock);
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
    public async Task HandlingQuery_ShouldThrowException_WhenUnknownAttachment()
    {
        // Arrange
        var query = new GetPunchItemAttachmentDownloadUrlQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act and Assert
        await Assert.ThrowsExceptionAsync<EntityNotFoundException>(()
            => _dut.Handle(query, default));
    }

    [TestMethod]
    public async Task HandlingQuery_Should_CallGetDownloadUriAsync_OnAttachmentService()
    {
        // Act
        await _dut.Handle(_query, default);

        // Assert
        await _attachmentServiceMock.Received(1).GetDownloadUriAsync(
            _query.AttachmentGuid,
            default);
    }
}

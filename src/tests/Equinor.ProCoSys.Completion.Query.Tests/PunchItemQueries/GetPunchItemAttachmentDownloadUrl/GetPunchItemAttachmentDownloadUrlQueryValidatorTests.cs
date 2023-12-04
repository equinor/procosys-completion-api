using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.Query.Attachments;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;

[TestClass]
public class GetPunchItemAttachmentDownloadUrlQueryValidatorTests
{
    private GetPunchItemAttachmentDownloadUrlQueryValidator _dut;
    private GetPunchItemAttachmentDownloadUrlQuery _query;
    private IPunchItemValidator _punchItemValidatorMock;
    private IAttachmentService _attachmentServiceMock;

    [TestInitialize]
    public void Setup_OkState()
    {
        _query = new GetPunchItemAttachmentDownloadUrlQuery(Guid.NewGuid(), Guid.NewGuid());
        _punchItemValidatorMock = Substitute.For<IPunchItemValidator>();
        _punchItemValidatorMock.ExistsAsync(_query.PunchItemGuid, default).Returns(true);
        _attachmentServiceMock = Substitute.For<IAttachmentService>();
        _attachmentServiceMock.ExistsAsync(_query.AttachmentGuid, default).Returns(true);

        _dut = new GetPunchItemAttachmentDownloadUrlQueryValidator(
            _punchItemValidatorMock,
            _attachmentServiceMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Act
        var result = await _dut.ValidateAsync(_query);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchItemNotExists()
    {
        // Arrange
        _punchItemValidatorMock.ExistsAsync(_query.PunchItemGuid, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_query);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        var validationFailure = result.Errors[0];
        Assert.IsTrue(validationFailure.ErrorMessage.StartsWith("Punch item with this guid does not exist!"));
        Assert.IsInstanceOfType(validationFailure.CustomState, typeof(EntityNotFoundException));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_AttachmentNotExists()
    {
        // Arrange
        _attachmentServiceMock.ExistsAsync(_query.AttachmentGuid, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_query);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        var validationFailure = result.Errors[0];
        Assert.IsTrue(validationFailure.ErrorMessage.StartsWith("Attachment with this guid does not exist!"));
        Assert.IsInstanceOfType(validationFailure.CustomState, typeof(EntityNotFoundException));
    }
}

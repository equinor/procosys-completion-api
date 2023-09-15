using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;
using Equinor.ProCoSys.Completion.WebApi.InputValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.Attachments;

[TestClass]
public class OverwriteAttachmentDtoValidatorTests : UploadBaseDtoValidatorTests<OverwriteAttachmentDto>
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private IRowVersionValidator _rowVersionValidatorMock = null!;

    protected override void SetupDut()
    {
        _rowVersionValidatorMock = Substitute.For<IRowVersionValidator>();
        _rowVersionValidatorMock.IsValid(_rowVersion)
            .Returns(true);
        _dut = new OverwriteAttachmentDtoValidator(
            _rowVersionValidatorMock,
            _blobStorageOptionsMock);
    }

    protected override OverwriteAttachmentDto GetValidDto() =>
        new()
        {
            File = new TestableFormFile("picture.gif", 1000),
            RowVersion = _rowVersion
        };

    [TestMethod]
    public async Task Validate_ShouldFail_WhenRowVersionNotGiven()
    {
        // Arrange
        var dto = new OverwriteAttachmentDto
        {
            File = new TestableFormFile("picture.gif", 1000)
        };

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Row Version' must not be empty."));
    }
}

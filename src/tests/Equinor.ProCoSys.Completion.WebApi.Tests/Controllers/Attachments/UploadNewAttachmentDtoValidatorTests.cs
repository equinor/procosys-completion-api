using Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.Attachments;

[TestClass]
public class UploadNewAttachmentDtoValidatorTests : UploadBaseDtoValidatorTests<UploadNewAttachmentDto>
{
    protected override void SetupDut() =>
        _dut = new(_blobStorageOptionsMock.Object);

    protected override UploadNewAttachmentDto GetValidDto() =>
        new()
        {
            File = new TestableFormFile("picture.gif", 1000)
        };
}

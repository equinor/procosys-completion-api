using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.Attachments;

[TestClass]
public abstract class UploadBaseDtoValidatorTests<T> where T : UploadBaseDto, new()
{
    protected UploadBaseDtoValidator<T> _dut;
    protected Mock<IOptionsSnapshot<BlobStorageOptions>> _blobStorageOptionsMock;
    private BlobStorageOptions _blobStorageOptions;

    [TestInitialize]
    public void Setup()
    {
        _blobStorageOptionsMock = new Mock<IOptionsSnapshot<BlobStorageOptions>>();
        _blobStorageOptions = new BlobStorageOptions
        {
            MaxSizeMb = 2,
            BlobContainer = "bc",
            BlockedFileSuffixes = new[] { ".exe", ".zip" }
        };
        _blobStorageOptionsMock
            .Setup(x => x.Value)
            .Returns(_blobStorageOptions);

        SetupDut();
    }

    protected abstract void SetupDut();
    protected abstract T GetValidDto();

    [TestMethod]
    public void Validate_ShouldBeValid_WhenOkState()
    {
        var result = _dut.Validate(GetValidDto());

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldValidate_ShouldFail_WhenFileNotGiven()
    {
        var uploadAttachmentDto = new T();

        var result = _dut.Validate(uploadAttachmentDto);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual(result.Errors[0].ErrorMessage, "'File' must not be empty.");
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenFileNameNotExists()
    {
        var uploadAttachmentDto = new T
        {
            File = new TestableFormFile(null, 1000)
        };

        var result = _dut.Validate(uploadAttachmentDto);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual(result.Errors[0].ErrorMessage, "File name not given!");
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenFileNameIsTooLong()
    {
        var uploadAttachmentDto = new T
        {
            File = new TestableFormFile(new string('x', Attachment.FileNameLengthMax + 1), 1000)
        };

        var result = _dut.Validate(uploadAttachmentDto);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("File name to long! Max"));
    }

    [TestMethod]
    public void Validate_ShouldValidate_ShouldFail_WhenFileToBig()
    {
        var uploadAttachmentDto = new T
        {
            File = new TestableFormFile("picture.gif", _blobStorageOptions.MaxSizeMb * 1024 * 1024 + 1)
        };

        var result = _dut.Validate(uploadAttachmentDto);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual(result.Errors[0].ErrorMessage, $"Maximum file size is {_blobStorageOptions.MaxSizeMb}MB!");
    }

    [TestMethod]
    public void Validate_ShouldValidate_ShouldFail_WhenIllegalFileType()
    {
        var uploadAttachmentDto = new T
        {
            File = new TestableFormFile("picture.exe", 500)
        };

        var result = _dut.Validate(uploadAttachmentDto);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual(result.Errors[0].ErrorMessage, $"File {uploadAttachmentDto.File.FileName} is not a valid file for upload!");
    }
}
internal class TestableFormFile : IFormFile
{
    public TestableFormFile(string fileName, long lengthInBytes)
    {
        ContentDisposition = null;
        ContentType = null;
        FileName = fileName;
        Headers = null;
        Length = lengthInBytes;
        Name = null;
    }

    public void CopyTo(Stream target) => throw new System.NotImplementedException();

    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotImplementedException();

    public Stream OpenReadStream() => throw new System.NotImplementedException();

    public string ContentDisposition { get; }
    public string ContentType { get; }
    public string FileName { get; }
    public IHeaderDictionary Headers { get; }
    public long Length { get; }
    public string Name { get; }
}

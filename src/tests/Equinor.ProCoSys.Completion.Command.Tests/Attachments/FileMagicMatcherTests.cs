using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.Attachments;

[TestClass]
public sealed class FileMagicMatcherTests : TestsBase
{
    [TestMethod]
    public async Task GetMimeForFileAsync_ShouldReturnCorrectMimeTypeForFiles()
    {
        var mimeByFiles = new Dictionary<string, string>
        {
            { "ExampleBmp.bmp", FileMagicConstants.MimeTypes.Bmp },
            { "ExampleWord.docx", FileMagicConstants.MimeTypes.Docx },
            { "ExampleGif.gif", FileMagicConstants.MimeTypes.Gif },
            { "ExampleJpg.jpg", FileMagicConstants.MimeTypes.Jpg },
            { "ExamplePdf.pdf", FileMagicConstants.MimeTypes.Pdf },
            { "ExamplePng.png", FileMagicConstants.MimeTypes.Png },
            { "ExampleTif.tif", FileMagicConstants.MimeTypes.Tiff },
            { "ExampleWebp.webp", FileMagicConstants.MimeTypes.Webp },
            { "ExampleExcel.xlsx", FileMagicConstants.MimeTypes.Xlsx },
            { "ExamplePowerPoint.pptx", FileMagicConstants.MimeTypes.Pptx }
        };

        foreach (var file in mimeByFiles)
        {
            var filePath = Path.Combine("Attachments", "Files", file.Key);
            var fileExtension = Path.GetExtension(filePath);
            var stream = File.OpenRead(filePath);
            var result = await FileMagicMatcher.GetMimeForFileAsync(stream, fileExtension, CancellationToken.None);
            Assert.AreEqual(file.Value, result,
                $"Failed for file '{file.Key}', expected '{file.Value}' but got '{result}'.");
        }
    }

    [TestMethod]
    public async Task GetMimeForFileAsync_ShouldReturnUnknownForBadFiles()
    {
        string[] files =
        [
            "BadExampleBmp.bmp",
            "BadExampleWord.docx",
            "BadExampleGif.gif",
            "BadExampleJpg.jpg",
            "BadExamplePdf.pdf",
            "BadExamplePng.png",
            "BadExampleTif.tif",
            "BadExampleWebp.webp",
            "BadExampleExcel.xlsx",
            "BadExamplePowerPoint.pptx"
        ];
        
        foreach (var file in files)
        {
            var filePath = Path.Combine("Attachments", "Files", file);
            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            var stream = File.OpenRead(filePath);
            var result = await FileMagicMatcher.GetMimeForFileAsync(stream, fileExtension, CancellationToken.None);
            Assert.AreEqual(FileMagicConstants.MimeTypes.Unknown, result,
                $"Failed for file '{file}', expected '{FileMagicConstants.MimeTypes.Unknown}' but got '{result}'.");
        }
    }
}

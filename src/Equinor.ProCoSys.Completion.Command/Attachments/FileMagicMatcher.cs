using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Attachments;

public static class FileMagicConstants
{
    public static class FileTypes
    {
        public const string Pdf = ".pdf";
        public const string Jpg = ".jpg";
        public const string Png = ".png";
        public const string Gif = ".gif";
        public const string Bmp = ".bmp";
        public const string Tiff = ".tiff";
        public const string Tif = ".tif";
        public const string Webp = ".webp";
        public const string Docx = ".docx";
        public const string Xlsx = ".xlsx";
        public const string Pptx = ".pptx";
    }

    public static class MimeTypes
    {
        public const string Pdf = "application/pdf";
        public const string Jpg = "image/jpeg";
        public const string Png = "image/png";
        public const string Gif = "image/gif";
        public const string Bmp = "image/bmp";
        public const string Tiff = "image/tiff";
        public const string Tif = "image/tiff"; // both .tiff and .tif are valid extensions for TIFF images
        public const string Webp = "image/webp";
        public const string Docx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string Pptx = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        public const string Unknown = "application/octet-stream";
    }
}

public static class FileMagicMatcher
{
    private static readonly Dictionary<string, byte[]> s_fileSignature = new()
    {
        { FileMagicConstants.FileTypes.Pdf, [0x25, 0x50, 0x44, 0x46] },
        { FileMagicConstants.FileTypes.Jpg, [0xFF, 0xD8, 0xFF] },
        { FileMagicConstants.FileTypes.Png, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A] },
        { FileMagicConstants.FileTypes.Gif, [0x47, 0x49, 0x46, 0x38] },
        { FileMagicConstants.FileTypes.Bmp, [0x42, 0x4D] },
        { FileMagicConstants.FileTypes.Tiff, [0x49, 0x49, 0x2A, 0x00] }, // TIFF image (little-endian)
        { FileMagicConstants.FileTypes.Tif, [0x4D, 0x4D, 0x00, 0x2A] }, // TIFF image (big-endian)
        { FileMagicConstants.FileTypes.Webp, [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50] },
        {
            FileMagicConstants.FileTypes.Docx, [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00]
        }, // DOCX (also valid for other Office Open XML formats like .xlsx and .pptx), Has multiple values
        {
            FileMagicConstants.FileTypes.Xlsx, [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00]
        }, // XLSX (also valid for other Office Open XML formats), Has Multiple values
        {
            FileMagicConstants.FileTypes.Pptx, [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00]
        } // PPTX (also valid for other Office Open XML formats), Has Multiple values
    };

    private static readonly Dictionary<string, string> s_contentTypeMappings = new()
    {
        { FileMagicConstants.FileTypes.Pdf, FileMagicConstants.MimeTypes.Pdf },
        { FileMagicConstants.FileTypes.Jpg, FileMagicConstants.MimeTypes.Jpg },
        { FileMagicConstants.FileTypes.Png, FileMagicConstants.MimeTypes.Png },
        { FileMagicConstants.FileTypes.Gif, FileMagicConstants.MimeTypes.Gif },
        { FileMagicConstants.FileTypes.Bmp, FileMagicConstants.MimeTypes.Bmp },
        { FileMagicConstants.FileTypes.Tiff, FileMagicConstants.MimeTypes.Tiff },
        { FileMagicConstants.FileTypes.Tif, FileMagicConstants.MimeTypes.Tif },
        { FileMagicConstants.FileTypes.Webp, FileMagicConstants.MimeTypes.Webp },
        { FileMagicConstants.FileTypes.Docx, FileMagicConstants.MimeTypes.Docx },
        { FileMagicConstants.FileTypes.Xlsx, FileMagicConstants.MimeTypes.Xlsx },
        { FileMagicConstants.FileTypes.Pptx, FileMagicConstants.MimeTypes.Pptx }
    };

    private static readonly Dictionary<string, Func<byte[], bool>> s_magicMatcherByFileType = new()
    {
        {
            FileMagicConstants.FileTypes.Pdf,
            actual => MatchSequences(actual, s_fileSignature[FileMagicConstants.FileTypes.Pdf])
        },
        {
            FileMagicConstants.FileTypes.Jpg,
            actual => MatchSequences(actual, s_fileSignature[FileMagicConstants.FileTypes.Jpg])
        },
        {
            FileMagicConstants.FileTypes.Png,
            actual => MatchSequences(actual, s_fileSignature[FileMagicConstants.FileTypes.Png])
        },
        {
            FileMagicConstants.FileTypes.Gif,
            actual => MatchSequences(actual, s_fileSignature[FileMagicConstants.FileTypes.Gif])
        },
        {
            FileMagicConstants.FileTypes.Bmp,
            actual => MatchSequences(actual, s_fileSignature[FileMagicConstants.FileTypes.Bmp])
        },
        { FileMagicConstants.FileTypes.Tiff, MatchTiff },
        { FileMagicConstants.FileTypes.Tif, MatchTiff },
        { FileMagicConstants.FileTypes.Webp, MatchWebp },
        { FileMagicConstants.FileTypes.Docx, MatchOfficeDocuments },
        { FileMagicConstants.FileTypes.Xlsx, MatchOfficeDocuments },
        { FileMagicConstants.FileTypes.Pptx, MatchOfficeDocuments }
    };

    private static bool MatchSequences(IReadOnlyCollection<byte> actual, IReadOnlyCollection<byte> expected)
        => actual.Count >= expected.Count && actual.SequenceEqual(expected);

    private static bool MatchOfficeDocuments(IReadOnlyCollection<byte> actual)
    {
        IReadOnlyCollection<byte> sig1 = [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00];
        IReadOnlyCollection<byte> sig2 = [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00];

        return actual.Count >= sig1.Count && (actual.SequenceEqual(sig1) || actual.SequenceEqual(sig2));
    }

    private static bool MatchTiff(IReadOnlyCollection<byte> actual)
    {
        IReadOnlyCollection<byte> sig1 = [0x4D, 0x4D, 0x00, 0x2A];
        IReadOnlyCollection<byte> sig2 = [0x49, 0x49, 0x2A, 0x00];

        return actual.Count >= sig1.Count && (actual.SequenceEqual(sig1) || actual.SequenceEqual(sig2));
    }

    private static bool MatchWebp(IReadOnlyCollection<byte> actual)
    {
        IReadOnlyCollection<byte> start = [0x52, 0x49, 0x46, 0x46];
        IReadOnlyCollection<byte> end = [0x57, 0x45, 0x42, 0x50];
        var length = start.Count + end.Count + 4;

        return actual.Count >= length && actual.Take(start.Count).SequenceEqual(start) &&
               actual.Skip(actual.Count - end.Count).SequenceEqual(end);
    }

    public static async Task<string> GetMimeForFileAsync(Stream stream, string extension,
        CancellationToken cancellationToken)
    {
        if (!s_fileSignature.TryGetValue(extension, out var expectedSignature))
        {
            return FileMagicConstants.MimeTypes.Unknown;
        }

        if (!s_contentTypeMappings.TryGetValue(extension, out var expectedContentType))
        {
            return FileMagicConstants.MimeTypes.Unknown;
        }

        if (!s_magicMatcherByFileType.TryGetValue(extension, out var expectedMagicMatcher))
        {
            return FileMagicConstants.MimeTypes.Unknown;
        }

        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        var actualSignature = new byte[expectedSignature.Length];
        _ = await stream.ReadAsync(actualSignature, cancellationToken);

        if (expectedMagicMatcher(actualSignature))
        {
            return expectedContentType;
        }

        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        return FileMagicConstants.MimeTypes.Unknown;
    }
}

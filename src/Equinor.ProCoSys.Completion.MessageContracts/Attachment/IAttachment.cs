using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Attachment;

public interface IAttachment
{
    Guid SourceGuid { get; }
    string SourceType { get; }
    string FileName { get; }
    string BlobPath { get; }
}

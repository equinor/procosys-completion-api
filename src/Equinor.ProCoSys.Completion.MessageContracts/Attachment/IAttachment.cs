using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Attachment;

public interface IAttachment
{
    // Guid of the entity owning the Attachment
    Guid ParentGuid { get; }
    // Type of the entity owning the Attachment
    string ParentType { get; }
    string FileName { get; }
    string BlobPath { get; }
}

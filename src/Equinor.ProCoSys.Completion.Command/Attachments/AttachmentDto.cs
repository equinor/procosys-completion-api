using System;

namespace Equinor.ProCoSys.Completion.Command.Attachments;

public record AttachmentDto(Guid Guid, string RowVersion);

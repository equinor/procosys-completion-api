using System;

namespace Equinor.ProCoSys.Completion.Query.Links;

public record LinkDto(Guid SourceGuid, Guid Guid, string Title, string Url, string RowVersion);

using System;

namespace Equinor.ProCoSys.Completion.Domain.Imports;

public readonly record struct PunchItemImportReferences(
    Guid ProjectGuid,
    Guid CheckListGuid,
    Guid RaisedByOrgGuid,
    Guid ClearingByOrgGuid,
    Guid? TypeGuid
);

using System;

namespace Equinor.ProCoSys.Completion.Domain.Imports;

public sealed record TagCheckList(
    int Id,
    int TagId,
    string TagNo,
    string FormularType,
    Guid ProCoSysGuid,
    string Plant,
    string ResponsibleCode);

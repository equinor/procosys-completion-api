using System;

namespace Equinor.ProCoSys.Completion.Domain.Imports;

public sealed record TagCheckList(string TagNo, Guid ProCoSysGuid, string Plant);

using System;

namespace Equinor.ProCoSys.Completion.Domain.Imports;

public readonly record struct ActionByPerson(Guid PersonOid, DateTime Date);

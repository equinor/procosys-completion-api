﻿using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed record PunchItemImportMessage(
    TIObject TiObject,
    string TagNo,
    string ExternalPunchItemNo,
    string FormType,
    string Responsible,
    Optional<string?> PunchClass,
    Optional<string?> PunchItemNo,
    Optional<string?> Description,
    Optional<string?> RaisedByOrganization,
    Category? Category,
    Optional<string?> PunchListType,
    Optional<DateTime?> DueDate,
    Optional<DateTime?> ClearedDate,
    Optional<string?> ClearedBy,
    Optional<string?> ClearedByOrganization,
    Optional<DateTime?> VerifiedDate,
    Optional<string?> VerifiedBy,
    Optional<DateTime?> RejectedDate,
    Optional<string?> RejectedBy,
    Optional<string?> MaterialRequired,
    Optional<DateTime?> MaterialEta,
    Optional<string?> MaterialNo
)
{
    public ImportError ToImportError(string message) =>
        new(
            TiObject.Guid,
            TiObject.Method,
            TiObject.Project,
            TiObject.Site,
            message
        );
};
using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportUpdatePunchItem;

public class CreatePunchItemCommandForImport(
    Category category,
    string description,
    Guid checkListGuid,
    Guid raisedByOrgGuid,
    Guid clearingByOrgGuid,
    Guid? actionByPersonOid,
    DateTime? dueTimeUtc,
    Guid? priorityGuid,
    Guid? sortingGuid,
    Guid? typeGuid,
    int? estimate,
    Guid? originalWorkOrderGuid,
    Guid? workOrderGuid,
    Guid? swcrGuid,
    Guid? documentGuid,
    string? externalItemNo,
    bool materialRequired,
    DateTime? materialETAUtc,
    string? materialExternalNo)
    : CreatePunchItemCommand(category, description, checkListGuid, raisedByOrgGuid, clearingByOrgGuid,
        actionByPersonOid, dueTimeUtc, priorityGuid, sortingGuid, typeGuid, estimate, originalWorkOrderGuid,
        workOrderGuid, swcrGuid, documentGuid, externalItemNo, materialRequired, materialETAUtc, materialExternalNo) , IImportCommand;

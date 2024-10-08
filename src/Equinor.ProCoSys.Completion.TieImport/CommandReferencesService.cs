﻿using Equinor.ProCoSys.Completion.TieImport.Models;
using static Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate.LibraryType;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class CommandReferencesService(ImportDataBundle bundle)
{
    public CommandReferences GetAndValidatePunchItemReferencesForImport(PunchItemImportMessage message)
    {
        var references = new CommandReferences();
        references = references with { ProjectGuid = bundle.Project.Guid };
        references = ValidateAndSetCheckList(message, references);
        references = ValidateAndSetRaisedByOrg(message, references);
        references = ValidateAndSetClearedByOrg(message, references);
        references = SetPunchType(message, references);
        return references;
    }
    
    private CommandReferences ValidateAndSetCheckList(PunchItemImportMessage message, CommandReferences references)
    {
        if (bundle.CheckListGuid is null)
        {
            return references with {Errors = 
            [
                ..references.Errors,
                message.ToImportError($"CheckList '{message.FormType}' for Tag '{message.TagNo}' and responsible {message.Responsible} not found")
            ]};
        }
        return references with{ CheckListGuid = bundle.CheckListGuid ?? Guid.Empty };
    }

    private CommandReferences ValidateAndSetRaisedByOrg(PunchItemImportMessage message, CommandReferences references)
    {
        var raisedByOrg = bundle.Library.SingleOrDefault(x =>
            x.Code == message.RaisedByOrganization.Value && x.Type == COMPLETION_ORGANIZATION);
        if (raisedByOrg is null)
        {
            return references with {Errors = 
            [
                ..references.Errors,
                message.ToImportError($"RaisedByOrganization '{message.RaisedByOrganization.Value}' not found")
            ]};
        }
        return references with { RaisedByOrgGuid = raisedByOrg.Guid};
    }

    private CommandReferences ValidateAndSetClearedByOrg(PunchItemImportMessage message, CommandReferences references)
    {
        var clearingByOrg = bundle.Library.SingleOrDefault(x =>
            x.Code == message.ClearedByOrganization.Value && x.Type == COMPLETION_ORGANIZATION);
        if (clearingByOrg is null)
        {
            return references with {Errors =
            [
                ..references.Errors, message.ToImportError(
                    $"ClearedByOrganization '{message.ClearedByOrganization.Value}' not found")
            ]};
        }
        return references with{ ClearedByOrgGuid = clearingByOrg.Guid};
    }

    private CommandReferences SetPunchType(PunchItemImportMessage message, CommandReferences references)
    {
        var type = bundle.Library.SingleOrDefault(x =>
            x.Code == message.PunchListType.Value && x.Type == PUNCHLIST_TYPE);
        return references with { TypeGuid = type?.Guid};
    }
}

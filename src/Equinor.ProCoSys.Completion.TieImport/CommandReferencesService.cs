using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class CommandReferencesService(PlantScopedImportDataContext context)
{
    public CreatePunchReferences GetCreatePunchItemReferences(PunchItemImportMessage message)
    {
        var references = new CreatePunchReferences();


        references = GetProject(message, references);
        references = GetCheckList(message, references);
        references = GetRaisedByOrg(message, references);
        references = GetClearedByOrg(message, references);
        references = GetPunchType(message, references);

        return references;
    }

    private CreatePunchReferences GetProject(PunchItemImportMessage message, CreatePunchReferences references)
    {
        var project = context.Projects.FirstOrDefault(x => x.Name == message.ProjectName);
        if (project is null)
        {
            references.Errors =
                [..references.Errors, message.ToImportError($"Project '{message.ProjectName}' not found")];
        }

        references.ProjectGuid = project?.Guid ?? Guid.Empty;

        return references;
    }
    
    private CreatePunchReferences GetCheckList(PunchItemImportMessage message, CreatePunchReferences references)
    {
        var checkList = context.CheckLists.FirstOrDefault(x => x.FormularType == message.FormType && x.TagNo == message.TagNo);
        if (checkList is null)
        {
            references.Errors =
                [..references.Errors, message.ToImportError($"CheckList '{message.FormType}' for Tag '{message.TagNo}' not found")];
        }

        references.CheckListGuid = checkList?.ProCoSysGuid ?? Guid.Empty;

        return references;
    }
    
    private CreatePunchReferences GetRaisedByOrg(PunchItemImportMessage message, CreatePunchReferences references)
    {
        var raisedByOrg = context.Library.FirstOrDefault(x =>
            x.Code == message.RaisedByOrganization.Value && x.Type == LibraryType.COMPLETION_ORGANIZATION);
        if (raisedByOrg is null)
        {
            references.Errors =
                [..references.Errors, message.ToImportError($"RaisedByOrganization '{message.RaisedByOrganization.Value}' not found")];
        }

        references.RaisedByOrgGuid = raisedByOrg?.Guid ?? Guid.Empty;

        return references;
    }
    
    private CreatePunchReferences GetClearedByOrg(PunchItemImportMessage message, CreatePunchReferences references)
    {
        var clearingByOrg = context.Library.FirstOrDefault(x =>
            x.Code == message.ClearedByOrganization.Value && x.Type == LibraryType.COMPLETION_ORGANIZATION);
        if (clearingByOrg is null)
        {
            references.Errors =
            [..references.Errors,message.ToImportError(
                $"ClearedByOrganization '{message.ClearedByOrganization.Value}' not found")];
        }
        
        references.ClearedByOrgGuid = clearingByOrg?.Guid ?? Guid.Empty;

        return references;
    }
    
    private CreatePunchReferences GetPunchType(PunchItemImportMessage message, CreatePunchReferences references)
    {
        var type = context.Library.FirstOrDefault(x =>
            x.Code == message.PunchListType.Value && x.Type == LibraryType.PUNCHLIST_TYPE);

        references.TypeGuid = type?.Guid;

        return references;
    }
}

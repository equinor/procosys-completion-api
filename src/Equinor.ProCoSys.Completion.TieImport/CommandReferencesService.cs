using System.Linq.Expressions;
using System.Reflection;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;
using static Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate.LibraryType;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class CommandReferencesService(ImportDataBundle bundle)
{
    public ICommandReferences GetAndValidatePunchItemReferencesForImport(PunchItemImportMessage message)
    {
        var references = new ImportPunchReferences();
        return SetInterfaceReferences(message, references);
    }
    
    private ICommandReferences SetInterfaceReferences(PunchItemImportMessage message, ICommandReferences references)
    {
        references = GetProject(message, references);
        references = GetCheckList(message, references);
        references = GetRaisedByOrg(message, references);
        references = GetClearedByOrg(message, references);
        references = GetPunchType(message, references);
        //TODO removed this bit until we implement Update
        // references = GetPerson(references, x => x.ClearedBy, message.ClearedBy, message.ClearedDate, message);
        // references = GetPerson(references, x => x.VerifiedBy, message.VerifiedBy, message.VerifiedDate, message);
        // references = GetPerson(references, x => x.RejectedBy, message.RejectedBy, message.RejectedDate, message);

        return references;
    }

    private ICommandReferences GetPerson(
        ICommandReferences references,
        Expression<Func<ICommandReferences, Optional<ActionByPerson?>>> propertySelector,
        Optional<string?> personEmail,
        Optional<DateTime?> date,
        PunchItemImportMessage message)
    {
        var newValue = new Optional<ActionByPerson?>();

        if (personEmail is { HasValue: true, Value: not null } && date is not { HasValue: true, Value: not null })
        {
            references.Errors =
            [
                ..references.Errors,
                message.ToImportError(
                    $"Date is required for action (Clear, Verify, Reject) by person '{personEmail.Value}'")
            ];
        }

        if (date is { HasValue: true, Value: not null } && personEmail is not { HasValue: true, Value: not null })
        {
            references.Errors =
            [
                ..references.Errors,
                message.ToImportError($"Person is required for action (Clear, Verify, Reject) on date '{date.Value}'")
            ];
        }

        if (personEmail is { HasValue: true, Value: not null } && date is { HasValue: true, Value: not null })
        {
            var person = bundle.Persons.SingleOrDefault(x =>
                string.Equals(x.Email, personEmail.Value, StringComparison.InvariantCultureIgnoreCase));
            if (person is null)
            {
                references.Errors =
                [
                    ..references.Errors,
                    message.ToImportError(
                        $"Person '{personEmail.Value}' for action (Clear, Verify, Reject) on '{date.Value}' not found")
                ];
            }
            else
            {
                newValue = new Optional<ActionByPerson?>(new ActionByPerson(person.Guid, date.Value.Value));
            }
        }

        var member = propertySelector.Body as MemberExpression;
        var property = (PropertyInfo)member?.Member!;
        property.SetValue(references, newValue);

        return references;
    }


    private ICommandReferences GetProject(PunchItemImportMessage message, ICommandReferences references)
    {
        var project = bundle.Projects.SingleOrDefault(x => x.Name == message.TiObject.Project);
        if (project is null)
        {
            references.Errors =
                [..references.Errors, message.ToImportError($"Project '{message.TiObject.Project}' not found")];
        }

        references.ProjectGuid = project?.Guid ?? Guid.Empty;

        return references;
    }

    private ICommandReferences GetCheckList(PunchItemImportMessage message, ICommandReferences references)
    {
        var checkList =
            bundle.CheckLists.SingleOrDefault(x 
                => x.FormularType == message.FormType 
                   && x.TagNo == message.TagNo 
                   && message.Responsible == x.ResponsibleCode);
        if (checkList is null)
        {
            references.Errors =
            [
                ..references.Errors,
                message.ToImportError($"CheckList '{message.FormType}' for Tag '{message.TagNo}' not found")
            ];
        }

        references.CheckListGuid = checkList?.ProCoSysGuid ?? Guid.Empty;

        return references;
    }

    private ICommandReferences GetRaisedByOrg(PunchItemImportMessage message, ICommandReferences references)
    {
        var raisedByOrg = bundle.Library.SingleOrDefault(x =>
            x.Code == message.RaisedByOrganization.Value && x.Type == COMPLETION_ORGANIZATION);
        if (raisedByOrg is null)
        {
            references.Errors =
            [
                ..references.Errors,
                message.ToImportError($"RaisedByOrganization '{message.RaisedByOrganization.Value}' not found")
            ];
        }

        references.RaisedByOrgGuid = raisedByOrg?.Guid ?? Guid.Empty;

        return references;
    }

    private ICommandReferences GetClearedByOrg(PunchItemImportMessage message, ICommandReferences references)
    {
        var clearingByOrg = bundle.Library.SingleOrDefault(x =>
            x.Code == message.ClearedByOrganization.Value && x.Type == COMPLETION_ORGANIZATION);
        if (clearingByOrg is null)
        {
            references.Errors =
            [
                ..references.Errors, message.ToImportError(
                    $"ClearedByOrganization '{message.ClearedByOrganization.Value}' not found")
            ];
        }

        references.ClearedByOrgGuid = clearingByOrg?.Guid ?? Guid.Empty;

        return references;
    }

    private ICommandReferences GetPunchType(PunchItemImportMessage message, ICommandReferences references)
    {
        var type = bundle.Library.SingleOrDefault(x =>
            x.Code == message.PunchListType.Value && x.Type == PUNCHLIST_TYPE);

        references.TypeGuid = type?.Guid;

        return references;
    }

    public PunchItem? GetPunchItem(PunchItemImportMessage message)
    {
        if (message.PunchItemNo is { HasValue: true, Value: not null })
        {
            return bundle.PunchItems.SingleOrDefault(item => item.ItemNo == int.Parse(message.PunchItemNo.Value));
        }
        return bundle.PunchItems
            .SingleOrDefault(x =>
                x.ExternalItemNo == message.ExternalPunchItemNo &&
                x.Plant == message.TiObject.Site &&
                x.Project.Name == message.TiObject.Project &&
                bundle.CheckLists.Any(c => c.ResponsibleCode == message.Responsible &&
                                            c.Plant == x.Plant &&
                                            c.ProCoSysGuid == x.CheckListGuid)
            );
    }
}

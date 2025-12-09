using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.TieImport.Models;
using static Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate.LibraryType;

namespace Equinor.ProCoSys.Completion.TieImport;

public class CommandReferencesService
{
    private readonly ImportDataBundle _bundle;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ISWCRRepository _swcrRepository;
    private readonly IPersonRepository _personRepository;
    
    public CommandReferencesService(
        ImportDataBundle bundle,
        IWorkOrderRepository workOrderRepository,
        IDocumentRepository documentRepository,
        ISWCRRepository swcrRepository,
        IPersonRepository personRepository)
    {
        _bundle = bundle;
        _workOrderRepository = workOrderRepository;
        _documentRepository = documentRepository;
        _swcrRepository = swcrRepository;
        _personRepository = personRepository;
    }
    
    public async Task<CommandReferences> GetAndValidatePunchItemReferencesForImportAsync(
        PunchItemImportMessage message,
        CancellationToken cancellationToken)
    {
        var references = new CommandReferences();
        
        references = references with { ProjectGuid = _bundle.Project.Guid };
        references = ValidateAndSetCheckList(message, references);
        references = ValidateAndSetRaisedByOrg(message, references);
        references = ValidateAndSetClearedByOrg(message, references);
        references = SetPunchType(message, references);
        references = SetPunchPriority(message, references);
        references = SetPunchSorting(message, references);
        
        // Validate related entities
        references = await ValidateAndSetWorkOrderAsync(message, references, cancellationToken);
        references = await ValidateAndSetOriginalWorkOrderAsync(message, references, cancellationToken);
        references = await ValidateAndSetDocumentAsync(message, references, cancellationToken);
        references = await ValidateAndSetSWCRAsync(message, references, cancellationToken);
        references = await ValidateAndSetActionByAsync(message, references, cancellationToken);
        references = await ValidateAndSetClearedByAsync(message, references, cancellationToken);
        references = await ValidateAndSetVerifiedByAsync(message, references, cancellationToken);
        references = await ValidateAndSetRejectedByAsync(message, references, cancellationToken);
        return references;
    }
    
    private CommandReferences ValidateAndSetCheckList(PunchItemImportMessage message, CommandReferences references)
    {
        if (_bundle.CheckListGuid is null)
        {
            return references with {Errors = 
            [
                ..references.Errors,
                message.ToImportError($"CheckList '{message.FormType}' for Tag '{message.TagNo}' and responsible {message.Responsible} not found")
            ]};
        }
        return references with{ CheckListGuid = _bundle.CheckListGuid ?? Guid.Empty };
    }

    private CommandReferences ValidateAndSetRaisedByOrg(PunchItemImportMessage message, CommandReferences references)
    {
        var raisedByOrg = _bundle.Library.SingleOrDefault(x =>
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
        var clearingByOrg = _bundle.Library.SingleOrDefault(x =>
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
        // If marked for deletion or no value, skip lookup
        if (!message.PunchListType.HasValue || message.PunchListType.ShouldBeNull || string.IsNullOrEmpty(message.PunchListType.Value))
        {
            return references;
        }

        var type = _bundle.Library.SingleOrDefault(x =>
            x.Code == message.PunchListType.Value && x.Type == PUNCHLIST_TYPE);
        return references with { TypeGuid = type?.Guid };
    }

    private CommandReferences SetPunchPriority(PunchItemImportMessage message, CommandReferences references)
    {
        // If marked for deletion or no value, skip lookup
        if (!message.Priority.HasValue || message.Priority.ShouldBeNull || string.IsNullOrEmpty(message.Priority.Value))
        {
            return references;
        }

        var priority = _bundle.Library.SingleOrDefault(x =>
            x.Code == message.Priority.Value && x.Type == COMM_PRIORITY);
        return references with { PriorityGuid = priority?.Guid};
    }

    private CommandReferences SetPunchSorting(PunchItemImportMessage message, CommandReferences references)
    {
        // If marked for deletion or no value, skip lookup
        if (!message.Sorting.HasValue || message.Sorting.ShouldBeNull || string.IsNullOrEmpty(message.Sorting.Value))
        {
            return references;
        }

        var sorting = _bundle.Library.SingleOrDefault(x =>
            x.Code == message.Sorting.Value && x.Type == PUNCHLIST_SORTING);
        return references with { SortingGuid = sorting?.Guid };
    }

    private async Task<CommandReferences> ValidateAndSetWorkOrderAsync(
        PunchItemImportMessage message, 
        CommandReferences references, 
        CancellationToken cancellationToken)
    {
        // If marked for deletion or no value, skip lookup
        if (!message.WorkOrderNo.HasValue || message.WorkOrderNo.ShouldBeNull || string.IsNullOrEmpty(message.WorkOrderNo.Value))
        {
            return references;
        }

        var workOrder = await _workOrderRepository.GetByNoAsync(
            message.WorkOrderNo.Value, 
            cancellationToken);
            
        if (workOrder == null)
        {
            return references with {Errors =
            [
                ..references.Errors,
                message.ToImportError($"WorkOrder '{message.WorkOrderNo.Value}' not found in plant '{_bundle.Plant}'")
            ]};
        }
        
        if (workOrder.IsVoided)
        {
            return references with {Errors =
            [
                ..references.Errors,
                message.ToImportError($"WorkOrder '{message.WorkOrderNo.Value}' is voided")
            ]};
        }
        
        return references with { WorkOrderGuid = workOrder.Guid };
    }

    private async Task<CommandReferences> ValidateAndSetOriginalWorkOrderAsync(
        PunchItemImportMessage message, 
        CommandReferences references, 
        CancellationToken cancellationToken)
    {
        // If marked for deletion or no value, skip lookup
        if (!message.OriginalWorkOrderNo.HasValue || message.OriginalWorkOrderNo.ShouldBeNull || string.IsNullOrEmpty(message.OriginalWorkOrderNo.Value))
        {
            return references;
        }

        var workOrder = await _workOrderRepository.GetByNoAsync(
            message.OriginalWorkOrderNo.Value, 
            cancellationToken);
            
        if (workOrder == null)
        {
            return references with {Errors =
            [
                ..references.Errors,
                message.ToImportError($"OriginalWorkOrder '{message.OriginalWorkOrderNo.Value}' not found in plant '{_bundle.Plant}'")
            ]};
        }
        
        if (workOrder.IsVoided)
        {
            return references with {Errors =
            [
                ..references.Errors,
                message.ToImportError($"OriginalWorkOrder '{message.OriginalWorkOrderNo.Value}' is voided")
            ]};
        }
        
        return references with { OriginalWorkOrderGuid = workOrder.Guid };
    }

    private async Task<CommandReferences> ValidateAndSetDocumentAsync(
        PunchItemImportMessage message, 
        CommandReferences references, 
        CancellationToken cancellationToken)
    {
        // If marked for deletion or no value, skip lookup
        if (!message.DocumentNo.HasValue || message.DocumentNo.ShouldBeNull || string.IsNullOrEmpty(message.DocumentNo.Value))
        {
            return references;
        }

        var document = await _documentRepository.GetByNoAsync(
            message.DocumentNo.Value, 
            cancellationToken);
            
        if (document == null)
        {
            return references with {Errors =
            [
                ..references.Errors,
                message.ToImportError($"Document '{message.DocumentNo.Value}' not found in plant '{_bundle.Plant}'")
            ]};
        }
        
        if (document.IsVoided)
        {
            return references with {Errors =
            [
                ..references.Errors,
                message.ToImportError($"Document '{message.DocumentNo.Value}' is voided")
            ]};
        }
        
        return references with { DocumentGuid = document.Guid };
    }

    private async Task<CommandReferences> ValidateAndSetSWCRAsync(
        PunchItemImportMessage message, 
        CommandReferences references, 
        CancellationToken cancellationToken)
    {
        // If not present in message, marked as {NULL}, or value is null - skip
        if (!message.SwcrNo.HasValue || message.SwcrNo.ShouldBeNull || !message.SwcrNo.Value.HasValue)
        {
            return references;
        }

        var swcrNo = message.SwcrNo.Value.Value;

        var swcr = await _swcrRepository.GetByNoAsync(
            swcrNo, 
            cancellationToken);
            
        if (swcr == null)
        {
            return references with {Errors =
            [
                ..references.Errors,
                message.ToImportError($"SWCR '{swcrNo}' not found in plant '{_bundle.Plant}'")
            ]};
        }
        
        if (swcr.IsVoided)
        {
            return references with {Errors =
            [
                ..references.Errors,
                message.ToImportError($"SWCR '{swcrNo}' is voided")
            ]};
        }
        
        return references with { SWCRGuid = swcr.Guid };
    }

    private async Task<CommandReferences> ValidateAndSetActionByAsync(
        PunchItemImportMessage message, 
        CommandReferences references, 
        CancellationToken cancellationToken)
    {
        // If marked for deletion or no value, skip lookup
        if (!message.ActionBy.HasValue || message.ActionBy.ShouldBeNull || string.IsNullOrEmpty(message.ActionBy.Value))
        {
            return references;
        }

        var person = await _personRepository.GetByUserNameAsync(
            message.ActionBy.Value, 
            cancellationToken);
            
        if (person == null)
        {
            return references with {Errors =
            [
                ..references.Errors,
                message.ToImportError($"ActionBy person '{message.ActionBy.Value}' not found")
            ]};
        }
        
        return references with { ActionByPersonOid = person.Guid };
    }

    private async Task<CommandReferences> ValidateAndSetClearedByAsync(
        PunchItemImportMessage message, 
        CommandReferences references, 
        CancellationToken cancellationToken)
    {
        var result = await ValidateAndCreateActionByPersonAsync(
            message,
            message.ClearedBy,
            message.ClearedDate,
            "ClearedBy",
            "ClearedDate",
            cancellationToken);

        if (result.Error is not null)
        {
            return references with { Errors = [..references.Errors, result.Error] };
        }

        return result.ActionByPerson is not null 
            ? references with { ClearedBy = result.ActionByPerson } 
            : references;
    }

    private async Task<CommandReferences> ValidateAndSetVerifiedByAsync(
        PunchItemImportMessage message, 
        CommandReferences references, 
        CancellationToken cancellationToken)
    {
        var result = await ValidateAndCreateActionByPersonAsync(
            message,
            message.VerifiedBy,
            message.VerifiedDate,
            "VerifiedBy",
            "VerifiedDate",
            cancellationToken);

        if (result.Error is not null)
        {
            return references with { Errors = [..references.Errors, result.Error] };
        }

        return result.ActionByPerson is not null 
            ? references with { VerifiedBy = result.ActionByPerson } 
            : references;
    }

    private async Task<CommandReferences> ValidateAndSetRejectedByAsync(
        PunchItemImportMessage message, 
        CommandReferences references, 
        CancellationToken cancellationToken)
    {
        var result = await ValidateAndCreateActionByPersonAsync(
            message,
            message.RejectedBy,
            message.RejectedDate,
            "RejectedBy",
            "RejectedDate",
            cancellationToken);

        if (result.Error is not null)
        {
            return references with { Errors = [..references.Errors, result.Error] };
        }

        return result.ActionByPerson is not null 
            ? references with { RejectedBy = result.ActionByPerson } 
            : references;
    }

    private async Task<(ActionByPerson? ActionByPerson, ImportError? Error)> ValidateAndCreateActionByPersonAsync(
        PunchItemImportMessage message,
        Optional<string?> personUsername,
        Optional<DateTime?> actionDate,
        string personFieldName,
        string dateFieldName,
        CancellationToken cancellationToken)
    {
        // If no person specified, nothing to do
        if (!personUsername.HasValue || string.IsNullOrEmpty(personUsername.Value))
        {
            return (null, null);
        }

        // Validate that date is provided when person is set
        if (!actionDate.HasValue || !actionDate.Value.HasValue)
        {
            return (null, message.ToImportError($"{dateFieldName} is required when {personFieldName} is specified"));
        }

        // Look up the person
        var person = await _personRepository.GetByUserNameAsync(personUsername.Value, cancellationToken);
        if (person is null)
        {
            return (null, message.ToImportError($"{personFieldName} person '{personUsername.Value}' not found"));
        }

        return (new ActionByPerson(person.Guid, actionDate.Value.Value), null);
    }
}

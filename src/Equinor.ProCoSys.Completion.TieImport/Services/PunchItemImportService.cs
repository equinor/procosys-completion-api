using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.TieImport.Mappers;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.TieImport.References;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using static Equinor.ProCoSys.Completion.TieImport.PunchObjectAttributes;

namespace Equinor.ProCoSys.Completion.TieImport.Services;

public class PunchItemImportService(
    IMediator mediator,
    IPlantSetter plantSetter,
    ICurrentUserSetter currentUserSetter,
    IImportDataFetcher importDataFetcher,
    IPunchItemRepository punchItemRepository,
    ICommandReferencesServiceFactory commandReferencesServiceFactory,
    ILogger<PunchItemImportService> logger) : IPunchItemImportService
{

    public async Task<List<ImportError>> HandlePunchImportMessageAsync(PunchItemImportMessage message)
    {
        // Route to appropriate handler based on method
        return message.Method switch
        {
            Methods.Create => await HandleCreateAsync(message),
            Methods.Update => await HandleUpdateAsync(message),
            Methods.Append => await HandleAppendAsync(message),
            _ => [new ImportError(message.MessageGuid, message.Method, message.ProjectName, message.Plant,
                $"Unsupported method: {message.Method}")]
        };
    }

    private async Task<List<ImportError>> HandleCreateAsync(PunchItemImportMessage message)
    {
        plantSetter.SetPlant(message.Plant);
        var importBundle = await CreateImportDataBundleAsync(message);
        return await ExecuteCreateAsync(message, importBundle);
    }

    private async Task<List<ImportError>> ExecuteCreateAsync(PunchItemImportMessage message, ImportDataBundle importBundle)
    {
        SetImportUser(importBundle);
        
        var referencesService = commandReferencesServiceFactory.Create(importBundle);
        var references = await referencesService.GetAndValidatePunchItemReferencesForImportAsync(message, null, CancellationToken.None);
        if (references.Errors.Length != 0)
        {
            return references.Errors.ToList();
        }
        var createCommand = CreateCommand(message, references);

        try
        {
            await mediator.Send(createCommand, CancellationToken.None);
            return [];
        }
        catch (ValidationException ve)
        {
            var validationErrors = ve.Errors
                .Select(x => new ImportError(message.MessageGuid, message.Method, message.ProjectName, message.Plant, x.ErrorMessage)).ToArray();
            return validationErrors.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CREATE import failed unexpectedly");
            return [new ImportError(message.MessageGuid, message.Method, message.ProjectName, message.Plant, ex.Message)];
        }
    }

    private async Task<List<ImportError>> HandleUpdateAsync(PunchItemImportMessage message)
    {
        try
        {
            var context = await PrepareOperationContextAsync(message, requireExistingPunchItem: true);
            if (!context.IsSuccess)
            {
                return context.Errors!;
            }

            return await ExecuteUpdateAsync(message, context.PunchItem!, context.ImportBundle!);
        }
        catch (ValidationException ve)
        {
            var validationErrors = ve.Errors
                .Select(x => new ImportError(message.MessageGuid, message.Method, message.ProjectName, message.Plant, x.ErrorMessage)).ToArray();
            return validationErrors.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UPDATE import failed unexpectedly for PunchItemNo {PunchItemNo}",
                message.PunchItemNo.HasValue ? message.PunchItemNo.Value : "N/A");
            return [new ImportError(message.MessageGuid, message.Method, message.ProjectName, message.Plant, ex.Message)];
        }
    }

    private async Task<List<ImportError>> HandleAppendAsync(PunchItemImportMessage message)
    {
        try
        {
            var context = await PrepareOperationContextAsync(message, requireExistingPunchItem: false);
            if (!context.IsSuccess)
            {
                return context.Errors!;
            }

            // If punch item exists, update it; otherwise, create it
            if (context.PunchItemExists)
            {
                return await ExecuteUpdateAsync(message, context.PunchItem!, context.ImportBundle!);
            }
            
            return await ExecuteCreateAsync(message, context.ImportBundle!);
        }
        catch (ValidationException ve)
        {
            var validationErrors = ve.Errors
                .Select(x => new ImportError(message.MessageGuid, message.Method, message.ProjectName, message.Plant, x.ErrorMessage)).ToArray();
            return validationErrors.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "APPEND import failed unexpectedly for PunchItem with ExternalItemNo {ExternalItemNo}",
                message.ExternalPunchItemNo);
            return [new ImportError(message.MessageGuid, message.Method, message.ProjectName, message.Plant, ex.Message)];
        }
    }

    private async Task<List<ImportError>> ExecuteUpdateAsync(PunchItemImportMessage message, PunchItem punchItem, ImportDataBundle importBundle)
    {
        // Fetch and Validate references
        var referencesService = commandReferencesServiceFactory.Create(importBundle);
        var references = await referencesService.GetAndValidatePunchItemReferencesForImportAsync(message, punchItem, CancellationToken.None);

        if (references.Errors.Length != 0)
        {
            return references.Errors.ToList();
        }

        // Build JsonPatchDocument for field updates
        var patchDocument = ImportUpdateHelper.CreateJsonPatchDocument(message, punchItem, references);

        // Create and send UPDATE command
        // ActionByPerson objects are already validated and populated in references
        var updateCommand = new ImportUpdatePunchItemCommand(
            message.MessageGuid,
            punchItem.Project.Guid,
            message.Plant,
            punchItem.Guid,
            patchDocument,
            ToOptional(references.ClearedBy),
            ToOptional(references.VerifiedBy),
            ToOptional(references.RejectedBy),
            punchItem.RowVersion.ConvertToString());

        // Send command and return any validation errors
        var errors = await mediator.Send(updateCommand, CancellationToken.None);
        return errors;
    }

    private static Optional<ActionByPerson?> ToOptional(ActionByPerson? actionByPerson) =>
        actionByPerson is not null ? new Optional<ActionByPerson?>(actionByPerson) : new Optional<ActionByPerson?>();
    
    /// <summary>
    /// Prepares the context for UPDATE and APPEND operations.
    /// Builds import bundle, validates CheckListGuid, sets current user context, and retrieves the PunchItem.
    /// PunchItem lookup priority: PunchItemNo first (if provided), then ExternalPunchItemNo as fallback.
    /// </summary>
    /// <param name="message">The import message containing punch item data</param>
    /// <param name="requireExistingPunchItem">
    /// If true (UPDATE), returns an error when PunchItem is not found.
    /// If false (APPEND), allows PunchItem to be null for create scenario.
    /// </param>
    /// <returns>
    /// PunchItemOperationContext containing:
    /// - PunchItem (if found, or null for APPEND when not found)
    /// - ImportBundle (for subsequent operations)
    /// - Errors (if validation failed)
    /// </returns>
    private async Task<PunchItemOperationContext> PrepareOperationContextAsync(
        PunchItemImportMessage message, 
        bool requireExistingPunchItem)
    {
        // Build import data bundle
        plantSetter.SetPlant(message.Plant);
        var importBundle = await CreateImportDataBundleAsync(message);

        // Validate CheckListGuid is present
        if (!importBundle.CheckListGuid.HasValue)
        {
            return new PunchItemOperationContext(null, null,
                [new ImportError(message.MessageGuid, message.Method, message.ProjectName, message.Plant,
                    $"CheckList not found for Tag '{message.TagNo}', FormType '{message.FormType}', Responsible '{message.Responsible}' in plant '{message.Plant}'")]);
        }

        // Prepare execution context
        SetImportUser(importBundle);

        // Retrieve existing PunchItem - try PunchItemNo first, then fall back to ExternalPunchItemNo
        var punchItem = await GetPunchItemAsync(message, importBundle.CheckListGuid.Value);
        
        // If punch item is required but not found, return error
        if (requireExistingPunchItem && punchItem is null)
        {
            var identifier = message.PunchItemNo.HasValue && message.PunchItemNo.Value.HasValue
                ? $"PunchItemNo '{message.PunchItemNo.Value}'" 
                : $"ExternalItemNo '{message.ExternalPunchItemNo}'";
            return new PunchItemOperationContext(null, null,
                [new ImportError(message.MessageGuid, message.Method, message.ProjectName, message.Plant,
                    $"PunchItem with {identifier} not found in plant '{message.Plant}'")]);
        }

        return new PunchItemOperationContext(punchItem, importBundle, null);
    }

    private async Task<PunchItem?> GetPunchItemAsync(PunchItemImportMessage message, Guid checkListGuid)
    {
        // Try PunchItemNo first if provided
        if (message.PunchItemNo.HasValue && message.PunchItemNo.Value.HasValue)
        {
            var punchItem = await punchItemRepository.GetByItemNoAsync(
                message.PunchItemNo.Value.Value, 
                checkListGuid, 
                CancellationToken.None);
            
            if (punchItem is not null)
            {
                return punchItem;
            }
        }

        // Fall back to ExternalPunchItemNo (always present - validated as required)
        return await punchItemRepository.GetByExternalItemNoAsync(
            message.ExternalPunchItemNo, 
            checkListGuid, 
            CancellationToken.None);
    }
     
    private void SetImportUser(ImportDataBundle dataBundle)
    {
        var importUser = dataBundle.Persons.First(x => x.UserName == ImportUserOptions.UserName);
        currentUserSetter.SetCurrentUserOid(importUser.Guid);
    }
    
    private async Task<ImportDataBundle> CreateImportDataBundleAsync(PunchItemImportMessage punchItemImportMessage)
    {
        var contextBuilder = new ImportBundleBuilder(importDataFetcher);
        var scopedContext = await contextBuilder
            .BuildAsync(punchItemImportMessage, CancellationToken.None);
        return scopedContext;
    }
    
    private static CreatePunchItemCommandForImport CreateCommand(PunchItemImportMessage message, CommandReferences references)
    {
        var materialRequired = message.MaterialRequired.HasValue && message.MaterialRequired.Value == true;
        var command = new CreatePunchItemCommandForImport(
            message.Category!.Value,
            message.Description.Value!,
            references.CheckListGuid,
            references.RaisedByOrgGuid,
            references.ClearedByOrgGuid,
            null,
            message.DueDate.Value,
            null,
            null,
            references.TypeGuid,
            null,
            null,
            null,
            null,
            null,
            message.ExternalPunchItemNo,
            materialRequired,
            message.MaterialEta.Value,
            message.MaterialNo.Value
        );
        return command;
    }

    /// <summary>
    /// Result of preparing context for operations on an existing PunchItem.
    /// For UPDATE: PunchItem must exist (requireExisting=true).
    /// For APPEND: PunchItem may or may not exist (requireExisting=false).
    /// </summary>
    internal sealed record PunchItemOperationContext(
        PunchItem? PunchItem,
        ImportDataBundle? ImportBundle,
        List<ImportError>? Errors)
    {
        public bool IsSuccess => Errors is null || Errors.Count == 0;
        public bool PunchItemExists => PunchItem is not null;
    }
}

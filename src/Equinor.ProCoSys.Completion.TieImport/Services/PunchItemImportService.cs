using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.TieImport.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.TieImport.Services;

public class PunchItemImportService(
    IMediator mediator,
    IPlantSetter plantSetter,
    ICurrentUserSetter currentUserSetter,
    IImportDataFetcher importDataFetcher,
    ILogger<PunchItemImportService> logger) : IPunchItemImportService
{

    public async Task<List<ImportError>> HandlePunchImportMessageAsync(PunchItemImportMessage message)
    {
        var importBundle = await CreateImportDataBundleAsync(message);
        var referencesService = new CommandReferencesService(importBundle);
        var references = referencesService.GetAndValidatePunchItemReferencesForImport(message);
        if(references.Errors.Length != 0)
        {
            return references.Errors.ToList();
        }
        var createCommand = CreateCommand(message, references);
        
        try
        { 
            await PrepareAndSendCommand(createCommand, importBundle, CancellationToken.None);
            return [];

        }
        catch (ValidationException ve)
        {
            var validationErrors = ve.Errors
                .Select(x => new ImportError(message.MessageGuid, message.Method, message.ProjectName,message.Plant,x.ErrorMessage)).ToArray();
            return validationErrors.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,"ImportObject failed unexpectedly");
            return [new ImportError(message.MessageGuid, message.Method, message.ProjectName, message.Plant, ex.Message)];
        }
    }
     
    private async Task PrepareAndSendCommand(CreatePunchItemCommandForImport importResult,
        ImportDataBundle dataBundle, CancellationToken cancellationToken)
    {
        plantSetter.SetPlant(dataBundle.Plant);
        var importUser = dataBundle.Persons.First(x => x.UserName == ImportUserOptions.UserName);
        currentUserSetter.SetCurrentUserOid(importUser.Guid);
        
        await mediator.Send(importResult, cancellationToken);
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
        bool.TryParse(message.MaterialRequired.Value, out var materialRequired);
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
}

using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportUpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.TieImport.Services;

public class PunchImportService(
    IMediator mediator,
    IPlantSetter plantSetter,
    ICurrentUserSetter currentUserSetter,
    IOptionsMonitor<TieImportOptions> tieConfig,
    IImportDataFetcher importDataFetcher,
    ILogger<PunchImportService> logger) : IPunchImportService
{

    public async Task<ImportResult> HandlePunchImportMessage(PunchItemImportMessage message)
    {
        var importBundle = await CreateImportDataBundleContexts(message);
        var referencesService = new CommandReferencesService(importBundle);
        var references = referencesService.GetAndValidatePunchItemReferencesForImport(message);
        if(references.Errors.Length != 0)
        {
            return new ImportResult(message.MessageGuid, references.Errors);
        }
        var createCommand = GetAndValidateCreateCommand(message, references);
        var importResult = new ImportResult(message.MessageGuid,[]);
        try
        { 
            await PrepareAndSendCommand(createCommand, importBundle, CancellationToken.None);
            return importResult;

        }
        catch (ValidationException ve)
        {
            var validationErrors = ve.Errors
                .Select(x => new ImportError(message.MessageGuid, message.Method, message.ProjectName,message.Plant,x.ErrorMessage)).ToArray();
            return importResult with { Errors = [..importResult.Errors, ..validationErrors] };
        }
        catch (Exception ex)
        {
            logger.LogError(ex,"ImportObject failed unexpectedly");
            return importResult with { Errors = [..importResult.Errors, 
                new ImportError(message.MessageGuid, message.Method, message.ProjectName,message.Plant,ex.Message)]};
        }
    }
     
    private async Task PrepareAndSendCommand(CreatePunchItemCommandForImport importResult,
        ImportDataBundle dataBundle, CancellationToken cancellationToken)
    {
        plantSetter.SetPlant(dataBundle.Plant);
        var importUser = dataBundle.Persons.First(x => x.UserName == tieConfig.CurrentValue.ImportUserName);
        currentUserSetter.SetCurrentUserOid(importUser.Guid);
        
        await mediator.Send(importResult, cancellationToken);
    }
    
    private async Task<ImportDataBundle> CreateImportDataBundleContexts(PunchItemImportMessage punchItemImportMessage)
    {
        var contextBuilder = new ImportBundleBuilder(importDataFetcher);
        var scopedContext = await contextBuilder
            .BuildAsync(punchItemImportMessage, CancellationToken.None);
        return scopedContext;
    }
    
    private static CreatePunchItemCommandForImport GetAndValidateCreateCommand(PunchItemImportMessage message, CommandReferences references)
    {
        bool.TryParse(message.MaterialRequired.Value, out var materialRequired);
        var command = new CreatePunchItemCommandForImport(
            message.Category!.Value,
            message.Description.Value ?? string.Empty,
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

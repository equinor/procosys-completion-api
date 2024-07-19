using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ClearPunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.Domain.Validators;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportUpdatePunchItem;

public sealed record ImportUpdatePunchItemCommand(
    Guid ImportGuid,
    Guid ProjectGuid,
    string Plant,
    Guid PunchItemGuid,
    JsonPatchDocument<PatchablePunchItem> PatchDocument,
    Category? Category,
    Optional<ActionByPerson?> ClearedBy,
    Optional<ActionByPerson?> VerifiedBy,
    Optional<ActionByPerson?> RejectedBy,
    string RowVersion) : IRequest<Result<ImportError[]>>, IIsPunchItemCommand;

public sealed class ImportUpdatePunchItemHandler(
    IPunchItemValidator punchItemValidator,
    ILabelValidator labelValidator,
    IOptionsMonitor<ApplicationOptions> options,
    ILibraryItemValidator libraryItemValidator,
    IWorkOrderValidator workOrderValidator,
    ISWCRValidator swcrValidator,
    IDocumentValidator documentValidator)
    : IRequestHandler<ImportUpdatePunchItemCommand, Result<ImportError[]>>
{
    private ImportError ToImportError(ImportUpdatePunchItemCommand request, string message) =>
        new(
            request.ImportGuid,
            "UPDATE",
            $"Project with GUID {request.ProjectGuid}",
            request.Plant,
            message);

    private async Task<ImportError[]> Validate(ImportUpdatePunchItemCommand request,
        CancellationToken cancellationToken)
    {
        var clearValidator = new ClearPunchItemCommandValidator(punchItemValidator);
        var verifyValidator = new VerifyPunchItemCommandValidator(punchItemValidator);
        var rejectValidator = new RejectPunchItemCommandValidator(punchItemValidator, labelValidator, options);
        var updateValidator = new UpdatePunchItemCommandValidator(punchItemValidator, libraryItemValidator,
            workOrderValidator, swcrValidator, documentValidator);
        var categoryValidator = new UpdatePunchItemCategoryCommandValidator(punchItemValidator);

        var errors = new List<ImportError>();

        if (request.ClearedBy.HasValue)
        {
            var results =
                await clearValidator.ValidateAsync(
                    new ClearPunchItemCommand(
                        request.PunchItemGuid,
                        request.RowVersion),
                    cancellationToken);
            errors.AddRange(results.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        if (request.VerifiedBy.HasValue)
        {
            var results =
                await verifyValidator.ValidateAsync(
                    new VerifyPunchItemCommand(
                        request.PunchItemGuid,
                        request.RowVersion),
                    cancellationToken);
            errors.AddRange(results.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        if (request.RejectedBy.HasValue)
        {
            var results =
                await rejectValidator.ValidateAsync(
                    new RejectPunchItemCommand(
                        request.PunchItemGuid,
                        string.Empty,
                        [],
                        request.RowVersion),
                    cancellationToken);
            errors.AddRange(results.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        if (request.Category.HasValue)
        {
            var results =
                await categoryValidator.ValidateAsync(
                    new UpdatePunchItemCategoryCommand(
                        request.PunchItemGuid,
                        request.Category.Value,
                        request.RowVersion),
                    cancellationToken);
            errors.AddRange(results.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        var updateResults =
            await updateValidator.ValidateAsync(
                new UpdatePunchItemCommand(
                    request.PunchItemGuid,
                    request.PatchDocument,
                    request.RowVersion),
                cancellationToken);
        errors.AddRange(updateResults.Errors.Select(x => ToImportError(request, x.ErrorMessage)));

        return errors.ToArray();
    }

    public async Task<Result<ImportError[]>> Handle(ImportUpdatePunchItemCommand request,
        CancellationToken cancellationToken)
    {
        var errors = await Validate(request, cancellationToken);

        return new SuccessResult<ImportError[]>([]);
    }
}

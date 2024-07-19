using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportUpdatePunchItem;

public sealed record ImportUpdatePunchItemCommand(
    Guid PunchItemGuid,
    JsonPatchDocument<PatchablePunchItem> PatchDocument,
    Category? Category,
    Optional<Guid?> ClearedByGuid,
    Optional<Guid?> VerifiedByGuid,
    Optional<Guid?> RejectedByGuid,
    string RowVersion) : IRequest<Result<string>>, IIsPunchItemCommand;

public sealed class ImportUpdatePunchItemHandler : IRequestHandler<ImportUpdatePunchItemCommand, Result<string>>
{
    public Task<Result<string>> Handle(ImportUpdatePunchItemCommand request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}

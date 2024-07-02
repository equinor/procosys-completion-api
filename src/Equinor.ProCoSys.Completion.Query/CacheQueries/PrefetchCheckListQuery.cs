using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.CacheQueries;

public readonly record struct PrefetchCheckListQuery(Guid CheckListGuid) : IRequest;

public sealed class PrefetchCheckListQueryHandler(ICheckListApiService checkListApiService)
    : IRequestHandler<PrefetchCheckListQuery>
{
    public async Task Handle(PrefetchCheckListQuery request, CancellationToken cancellationToken) =>
        await checkListApiService.GetCheckListAsync(request.CheckListGuid, cancellationToken);
}
